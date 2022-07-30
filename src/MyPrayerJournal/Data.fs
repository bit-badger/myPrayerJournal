module MyPrayerJournal.Data

open LiteDB
open MyPrayerJournal
open NodaTime
open System.Threading.Tasks

// fsharplint:disable MemberNames

/// LiteDB extensions
[<AutoOpen>]
module Extensions =
  
    /// Extensions on the LiteDatabase class
    type LiteDatabase with
        
        /// The Request collection
        member this.requests
          with get () = this.GetCollection<Request> "request"
        
        /// Async version of the checkpoint command (flushes log)
        member this.saveChanges () =
            this.Checkpoint ()
            Task.CompletedTask


/// Map domain to LiteDB
//  It does mapping, but since we're so DU-heavy, this gives us control over the JSON representation
[<RequireQualifiedAccess>]
module Mapping =
  
    /// Mapping for NodaTime's Instant type
    module Instant =
        let fromBson (value : BsonValue) = Instant.FromUnixTimeMilliseconds value.AsInt64
        let toBson (value : Instant) : BsonValue = value.ToUnixTimeMilliseconds ()
    
    /// Mapping for option types
    module Option =
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
        BsonMapper.Global.RegisterType<Recurrence>(Recurrence.toBson, Recurrence.fromBson)
        BsonMapper.Global.RegisterType<RequestAction>(RequestAction.toBson, RequestAction.fromBson)
        BsonMapper.Global.RegisterType<RequestId>(RequestId.toBson, RequestId.fromBson)
        BsonMapper.Global.RegisterType<string option>(Option.stringToBson, Option.stringFromBson)
        BsonMapper.Global.RegisterType<UserId>(UserId.toBson, UserId.fromBson)

/// Code to be run at startup
module Startup =
  
    /// Ensure the database is set up
    let ensureDb (db : LiteDatabase) =
        db.requests.EnsureIndex (fun it -> it.userId) |> ignore
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
        db.requests.Update req |> ignore
        Task.CompletedTask


/// Retrieve a request, including its history and notes, by its ID and user ID
let tryFullRequestById reqId userId (db : LiteDatabase) = backgroundTask {
    let! req = db.requests.Find (Query.EQ ("_id", RequestId.toString reqId)) |> firstAsync
    return match box req with null -> None | _ when req.userId = userId -> Some req | _ -> None
}

/// Add a history entry
let addHistory reqId userId hist db = backgroundTask {
    match! tryFullRequestById reqId userId db with
    | Some req -> do! doUpdate db { req with history = hist :: req.history }
    | None     -> invalidOp $"{RequestId.toString reqId} not found"
}

/// Add a note
let addNote reqId userId note db = backgroundTask {
    match! tryFullRequestById reqId userId db with
    | Some req -> do! doUpdate db { req with notes = note :: req.notes }
    | None     -> invalidOp $"{RequestId.toString reqId} not found"
}

/// Add a request
let addRequest (req : Request) (db : LiteDatabase) =
    db.requests.Insert req |> ignore

// FIXME: make a common function here

/// Retrieve all answered requests for the given user
let answeredRequests userId (db : LiteDatabase) = backgroundTask {
    let! reqs = db.requests.Find (Query.EQ ("userId", UserId.toString userId)) |> toListAsync
    return
        reqs
        |> Seq.map JournalRequest.ofRequestFull
        |> Seq.filter (fun it -> it.lastStatus = Answered)
        |> Seq.sortByDescending (fun it -> it.asOf)
        |> List.ofSeq
}

/// Retrieve the user's current journal
let journalByUserId userId (db : LiteDatabase) = backgroundTask {
    let! jrnl = db.requests.Find (Query.EQ ("userId", UserId.toString userId)) |> toListAsync
    return
        jrnl
        |> Seq.map JournalRequest.ofRequestLite
        |> Seq.filter (fun it -> it.lastStatus <> Answered)
        |> Seq.sortBy (fun it -> it.asOf)
        |> List.ofSeq
}

/// Does the user have any snoozed requests?
let hasSnoozed userId now (db : LiteDatabase) = backgroundTask {
    let! jrnl = journalByUserId userId db
    return jrnl |> List.exists (fun r -> r.snoozedUntil > now)
}

/// Retrieve a request by its ID and user ID (without notes and history)
let tryRequestById reqId userId db = backgroundTask {
    let! req = tryFullRequestById reqId userId db
    return req |> Option.map (fun r -> { r with history = []; notes = [] })
}

/// Retrieve notes for a request by its ID and user ID
let notesById reqId userId (db : LiteDatabase) = backgroundTask {
    match! tryFullRequestById reqId userId db with | Some req -> return req.notes | None -> return []
}
    
/// Retrieve a journal request by its ID and user ID
let tryJournalById reqId userId (db : LiteDatabase) = backgroundTask {
    let! req = tryFullRequestById reqId userId db
    return req |> Option.map JournalRequest.ofRequestLite
}
    
/// Update the recurrence for a request
let updateRecurrence reqId userId recurType db = backgroundTask {
    match! tryFullRequestById reqId userId db with
    | Some req -> do! doUpdate db { req with recurrence = recurType }
    | None     -> invalidOp $"{RequestId.toString reqId} not found"
}

/// Update a snoozed request
let updateSnoozed reqId userId until db = backgroundTask {
    match! tryFullRequestById reqId userId db with
    | Some req -> do! doUpdate db { req with snoozedUntil = until; showAfter = until }
    | None     -> invalidOp $"{RequestId.toString reqId} not found"
}

/// Update the "show after" timestamp for a request
let updateShowAfter reqId userId showAfter db = backgroundTask {
    match! tryFullRequestById reqId userId db with
    | Some req -> do! doUpdate db { req with showAfter = showAfter }
    | None     -> invalidOp $"{RequestId.toString reqId} not found"
}
