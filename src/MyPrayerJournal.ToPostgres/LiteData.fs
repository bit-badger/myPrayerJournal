module MyPrayerJournal.LiteData

open LiteDB
open MyPrayerJournal
open NodaTime

/// Request is the identifying record for a prayer request
[<CLIMutable; NoComparison; NoEquality>]
type OldRequest =
    {   /// The ID of the request
        Id : RequestId
        
        /// The time this request was initially entered
        EnteredOn : Instant
        
        /// The ID of the user to whom this request belongs ("sub" from the JWT)
        UserId : UserId
        
        /// The time at which this request should reappear in the user's journal by manual user choice
        SnoozedUntil : Instant option
        
        /// The time at which this request should reappear in the user's journal by recurrence
        ShowAfter : Instant option
        
        /// The recurrence for this request
        Recurrence : Recurrence
        
        /// The history entries for this request
        History : History[]
        
        /// The notes for this request
        Notes : Note[]
    }


/// LiteDB extensions
[<AutoOpen>]
module Extensions =
  
    /// Extensions on the LiteDatabase class
    type LiteDatabase with
        
        /// The Request collection
        member this.Requests = this.GetCollection<OldRequest> "request"


/// Map domain to LiteDB
//  It does mapping, but since we're so DU-heavy, this gives us control over the JSON representation
[<RequireQualifiedAccess>]
module Mapping =
    
    open NodaTime.Text
    
    /// A NodaTime instant pattern to use for parsing instants from the database
    let instantPattern = InstantPattern.CreateWithInvariantCulture "g"
    
    /// Mapping for NodaTime's Instant type
    module Instant =
        let fromBson (value : BsonValue) = (instantPattern.Parse value.AsString).Value
        let toBson (value : Instant) : BsonValue = value.ToString ("g", null)
    
    /// Mapping for option types
    module Option =
        let instantFromBson (value : BsonValue) = if value.IsNull then None else Some (Instant.fromBson value)
        let instantToBson (value : Instant option) = match value with Some it -> Instant.toBson it | None -> null
        
        let stringFromBson (value : BsonValue) = match value.AsString with "" -> None | x -> Some x
        let stringToBson (value : string option) : BsonValue = match value with Some txt -> txt | None -> ""
    
    /// Mapping for Recurrence
    module Recurrence =
        let fromBson (value : BsonValue) = Recurrence.ofString value
        let toBson (value : Recurrence) : BsonValue = Recurrence.toString value
    
    /// Mapping for RequestAction
    module RequestAction =
        let fromBson (value : BsonValue) = RequestAction.ofString value.AsString
        let toBson (value : RequestAction) : BsonValue = RequestAction.toString value
    
    /// Mapping for RequestId
    module RequestId =
        let fromBson (value : BsonValue) = RequestId.ofString value.AsString
        let toBson (value : RequestId) : BsonValue = RequestId.toString value
    
    /// Mapping for UserId
    module UserId =
        let fromBson (value : BsonValue) = UserId value.AsString
        let toBson (value : UserId) : BsonValue = UserId.toString value
    
    /// Set up the mapping
    let register () = 
        BsonMapper.Global.RegisterType<Instant>(Instant.toBson, Instant.fromBson)
        BsonMapper.Global.RegisterType<Instant option>(Option.instantToBson, Option.instantFromBson)
        BsonMapper.Global.RegisterType<Recurrence>(Recurrence.toBson, Recurrence.fromBson)
        BsonMapper.Global.RegisterType<RequestAction>(RequestAction.toBson, RequestAction.fromBson)
        BsonMapper.Global.RegisterType<RequestId>(RequestId.toBson, RequestId.fromBson)
        BsonMapper.Global.RegisterType<string option>(Option.stringToBson, Option.stringFromBson)
        BsonMapper.Global.RegisterType<UserId>(UserId.toBson, UserId.fromBson)

/// Code to be run at startup
module Startup =
  
    /// Ensure the database is set up
    let ensureDb (db : LiteDatabase) =
        db.Requests.EnsureIndex (fun it -> it.UserId) |> ignore
        Mapping.register ()
