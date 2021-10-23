module MyPrayerJournal.Data

open LiteDB
open System
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
  
  /// Map a history entry to BSON
  let historyToBson (hist : History) : BsonValue =
    let doc = BsonDocument ()
    doc["asOf"]   <- Ticks.toLong hist.asOf
    doc["status"] <- RequestAction.toString hist.status
    doc["text"]   <- match hist.text with Some t -> t | None -> ""
    upcast doc

  /// Map a BSON document to a history entry
  let historyFromBson (doc : BsonValue) =
    { asOf   = Ticks doc["asOf"].AsInt64
      status = RequestAction.ofString doc["status"].AsString
      text   = match doc["text"].AsString with "" -> None | txt -> Some txt
      }

  /// Map a note entry to BSON
  let noteToBson (note : Note) : BsonValue =
    let doc = BsonDocument ()
    doc["asOf"]  <- Ticks.toLong note.asOf
    doc["notes"] <- note.notes
    upcast doc

  /// Map a BSON document to a note entry
  let noteFromBson (doc : BsonValue) =
    { asOf  = Ticks doc["asOf"].AsInt64
      notes = doc["notes"].AsString
      }

  /// Map a request to its BSON representation
  let requestToBson req : BsonValue =
    let doc = BsonDocument ()
    doc["_id"]          <- RequestId.toString req.id
    doc["enteredOn"]    <- Ticks.toLong req.enteredOn
    doc["userId"]       <- UserId.toString req.userId
    doc["snoozedUntil"] <- Ticks.toLong req.snoozedUntil
    doc["showAfter"]    <- Ticks.toLong req.showAfter
    doc["recurType"]    <- Recurrence.toString req.recurType
    doc["recurCount"]   <- BsonValue req.recurCount
    doc["history"]      <- BsonArray (req.history |> List.map historyToBson |> Seq.ofList)
    doc["notes"]        <- BsonArray (req.notes   |> List.map noteToBson    |> Seq.ofList)
    upcast doc
  
  /// Map a BSON document to a request
  let requestFromBson (doc : BsonValue) =
    { id           = RequestId.ofString doc["_id"].AsString
      enteredOn    = Ticks doc["enteredOn"].AsInt64
      userId       = UserId doc["userId"].AsString
      snoozedUntil = Ticks doc["snoozedUntil"].AsInt64
      showAfter    = Ticks doc["showAfter"].AsInt64
      recurType    = Recurrence.ofString doc["recurType"].AsString
      recurCount   = int16 doc["recurCount"].AsInt32
      history      = doc["history"].AsArray |> Seq.map historyFromBson |> List.ofSeq
      notes        = doc["notes"].AsArray   |> Seq.map noteFromBson    |> List.ofSeq
      }
  
  /// Set up the mapping
  let register () = 
    BsonMapper.Global.RegisterType<Request>(
      Func<Request, BsonValue> requestToBson, Func<BsonValue, Request> requestFromBson)

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

/// Retrieve all answered requests for the given user
let answeredRequests userId (db : LiteDatabase) = backgroundTask {
  let! reqs = db.requests.Find (Query.EQ ("userId", UserId.toString userId)) |> toListAsync
  return
    reqs
    |> Seq.map JournalRequest.ofRequestFull
    |> Seq.filter (fun it -> it.lastStatus = Answered)
    |> Seq.sortByDescending (fun it -> Ticks.toLong it.asOf)
    |> List.ofSeq
  }

/// Retrieve the user's current journal
let journalByUserId userId (db : LiteDatabase) = backgroundTask {
  let! jrnl = db.requests.Find (Query.EQ ("userId", UserId.toString userId)) |> toListAsync
  return
    jrnl
    |> Seq.map JournalRequest.ofRequestLite
    |> Seq.filter (fun it -> it.lastStatus <> Answered)
    |> Seq.sortBy (fun it -> Ticks.toLong it.asOf)
    |> List.ofSeq
  }

/// Does the user have any snoozed requests?
let hasSnoozed userId now (db : LiteDatabase) = backgroundTask {
  let! jrnl = journalByUserId userId db
  return jrnl |> List.exists (fun r -> Ticks.toLong r.snoozedUntil > Ticks.toLong now)
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
let updateRecurrence reqId userId recurType recurCount db = backgroundTask {
  match! tryFullRequestById reqId userId db with
  | Some req -> do! doUpdate db { req with recurType = recurType; recurCount = recurCount }
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
