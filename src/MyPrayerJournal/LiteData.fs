module MyPrayerJournal.LiteData

open LiteDB
open MyPrayerJournal
open System.Threading.Tasks

/// LiteDB extensions
[<AutoOpen>]
module Extensions =
  
    /// Extensions on the LiteDatabase class
    type LiteDatabase with
        
        /// The Request collection
        member this.Requests = this.GetCollection<Request> "request"
        
        /// Async version of the checkpoint command (flushes log)
        member this.SaveChanges () =
            this.Checkpoint ()
            Task.CompletedTask


/// Map domain to LiteDB
//  It does mapping, but since we're so DU-heavy, this gives us control over the JSON representation
[<RequireQualifiedAccess>]
module Mapping =
    
    open NodaTime
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


/// Async wrappers for LiteDB, and request -> journal mappings
[<AutoOpen>]
module private Helpers =
    
    open System.Linq

    /// Convert a sequence to a list asynchronously (used for LiteDB IO)
    let toListAsync<'T> (q : 'T seq) =
        (q.ToList >> Task.FromResult) ()

    /// Convert a sequence to a list asynchronously (used for LiteDB IO)
    let firstAsync<'T> (q : 'T seq) =
        q.FirstOrDefault () |> Task.FromResult

    /// Async wrapper around a request update
    let doUpdate (db : LiteDatabase) (req : Request) =
        db.Requests.Update req |> ignore
        Task.CompletedTask


/// Retrieve a request, including its history and notes, by its ID and user ID
let tryFullRequestById reqId userId (db : LiteDatabase) = backgroundTask {
    let! req = db.Requests.Find (Query.EQ ("_id", RequestId.toString reqId)) |> firstAsync
    return match box req with null -> None | _ when req.UserId = userId -> Some req | _ -> None
}

/// Add a history entry
let addHistory reqId userId hist db = backgroundTask {
    match! tryFullRequestById reqId userId db with
    | Some req -> do! doUpdate db { req with History = Array.append [| hist |] req.History }
    | None     -> invalidOp $"{RequestId.toString reqId} not found"
}

/// Add a note
let addNote reqId userId note db = backgroundTask {
    match! tryFullRequestById reqId userId db with
    | Some req -> do! doUpdate db { req with Notes = Array.append [| note |] req.Notes }
    | None     -> invalidOp $"{RequestId.toString reqId} not found"
}

/// Add a request
let addRequest (req : Request) (db : LiteDatabase) =
    db.Requests.Insert req |> ignore

/// Find all requests for the given user
let private getRequestsForUser (userId : UserId) (db : LiteDatabase) = backgroundTask {
    return! db.Requests.Find (Query.EQ (nameof Request.empty.UserId, Mapping.UserId.toBson userId)) |> toListAsync
}

/// Retrieve all answered requests for the given user
let answeredRequests userId db = backgroundTask {
    let! reqs = getRequestsForUser userId db
    return
        reqs
        |> Seq.map JournalRequest.ofRequestFull
        |> Seq.filter (fun it -> it.LastStatus = Answered)
        |> Seq.sortByDescending (fun it -> it.AsOf)
        |> List.ofSeq
}

/// Retrieve the user's current journal
let journalByUserId userId db = backgroundTask {
    let! reqs = getRequestsForUser userId db
    return
        reqs
        |> Seq.map JournalRequest.ofRequestLite
        |> Seq.filter (fun it -> it.LastStatus <> Answered)
        |> Seq.sortBy (fun it -> it.AsOf)
        |> List.ofSeq
}

/// Does the user have any snoozed requests?
let hasSnoozed userId now (db : LiteDatabase) = backgroundTask {
    let! jrnl = journalByUserId userId db
    return jrnl |> List.exists (fun r -> defaultArg (r.SnoozedUntil |> Option.map (fun it -> it > now)) false)
}

/// Retrieve a request by its ID and user ID (without notes and history)
let tryRequestById reqId userId db = backgroundTask {
    let! req = tryFullRequestById reqId userId db
    return req |> Option.map (fun r -> { r with History = [||]; Notes = [||] })
}

/// Retrieve notes for a request by its ID and user ID
let notesById reqId userId (db : LiteDatabase) = backgroundTask {
    match! tryFullRequestById reqId userId db with | Some req -> return req.Notes | None -> return [||]
}
    
/// Retrieve a journal request by its ID and user ID
let tryJournalById reqId userId (db : LiteDatabase) = backgroundTask {
    let! req = tryFullRequestById reqId userId db
    return req |> Option.map JournalRequest.ofRequestLite
}
    
/// Update the recurrence for a request
let updateRecurrence reqId userId recurType db = backgroundTask {
    match! tryFullRequestById reqId userId db with
    | Some req -> do! doUpdate db { req with Recurrence = recurType }
    | None     -> invalidOp $"{RequestId.toString reqId} not found"
}

/// Update a snoozed request
let updateSnoozed reqId userId until db = backgroundTask {
    match! tryFullRequestById reqId userId db with
    | Some req -> do! doUpdate db { req with SnoozedUntil = until; ShowAfter = until }
    | None     -> invalidOp $"{RequestId.toString reqId} not found"
}

/// Update the "show after" timestamp for a request
let updateShowAfter reqId userId showAfter db = backgroundTask {
    match! tryFullRequestById reqId userId db with
    | Some req -> do! doUpdate db { req with ShowAfter = showAfter }
    | None     -> invalidOp $"{RequestId.toString reqId} not found"
}
