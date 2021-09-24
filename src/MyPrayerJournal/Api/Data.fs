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
      with get () = this.GetCollection<Request>("request")
    /// Async version of the checkpoint command (flushes log)
    member this.saveChanges () =
      this.Checkpoint()
      Task.CompletedTask


/// Map domain to LiteDB
//  It does mapping, but since we're so DU-heavy, this gives us control over the JSON representation
[<RequireQualifiedAccess>]
module Mapping =
  
  /// Map a history entry to BSON
  let historyToBson (hist : History) : BsonValue =
    let doc = BsonDocument ()
    doc.["asOf"]   <- BsonValue (Ticks.toLong hist.asOf)
    doc.["status"] <- BsonValue (RequestAction.toString hist.status)
    doc.["text"]   <- BsonValue (match hist.text with Some t -> t | None -> "")
    upcast doc

  /// Map a BSON document to a history entry
  let historyFromBson (doc : BsonValue) =
    { asOf   = Ticks doc.["asOf"].AsInt64
      status = RequestAction.fromString doc.["status"].AsString
      text   = match doc.["text"].AsString with "" -> None | txt -> Some txt
      }

  /// Map a note entry to BSON
  let noteToBson (note : Note) : BsonValue =
    let doc = BsonDocument ()
    doc.["asOf"]  <- BsonValue (Ticks.toLong note.asOf)
    doc.["notes"] <- BsonValue note.notes
    upcast doc

  /// Map a BSON document to a note entry
  let noteFromBson (doc : BsonValue) =
    { asOf  = Ticks doc.["asOf"].AsInt64
      notes = doc.["notes"].AsString
      }

  /// Map a request to its BSON representation
  let requestToBson req : BsonValue =
    let doc = BsonDocument ()
    doc.["_id"]          <- BsonValue (RequestId.toString req.id)
    doc.["enteredOn"]    <- BsonValue (Ticks.toLong req.enteredOn)
    doc.["userId"]       <- BsonValue (UserId.toString req.userId)
    doc.["snoozedUntil"] <- BsonValue (Ticks.toLong req.snoozedUntil)
    doc.["showAfter"]    <- BsonValue (Ticks.toLong req.showAfter)
    doc.["recurType"]    <- BsonValue (Recurrence.toString req.recurType)
    doc.["recurCount"]   <- BsonValue req.recurCount
    doc.["history"]      <- BsonArray (req.history |> List.map historyToBson |> Seq.ofList)
    doc.["notes"]        <- BsonArray (req.notes   |> List.map noteToBson    |> Seq.ofList)
    upcast doc
  
  /// Map a BSON document to a request
  let requestFromBson (doc : BsonValue) =
    { id           = RequestId.ofString doc.["_id"].AsString
      enteredOn    = Ticks doc.["enteredOn"].AsInt64
      userId       = UserId doc.["userId"].AsString
      snoozedUntil = Ticks doc.["snoozedUntil"].AsInt64
      showAfter    = Ticks doc.["showAfter"].AsInt64
      recurType    = Recurrence.fromString doc.["recurType"].AsString
      recurCount   = int16 doc.["recurCount"].AsInt32
      history      = doc.["history"].AsArray |> Seq.map historyFromBson |> List.ofSeq
      notes        = doc.["notes"].AsArray   |> Seq.map noteFromBson    |> List.ofSeq
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

  /// Async wrapper around a LiteDB query that returns multiple results
  let doListQuery<'T> (q : ILiteQueryable<'T>) =
    q.ToList () |> Task.FromResult

  /// Async wrapper around a LiteDB query that returns 0 or 1 results
  let doSingleQuery<'T> (q : ILiteQueryable<'T>) =
    q.FirstOrDefault () |> Task.FromResult

  /// Async wrapper around a request update
  let doUpdate (db : LiteDatabase) (req : Request) =
    db.requests.Update req |> ignore
    Task.CompletedTask

  /// Convert a request to the form used for the journal (precomputed values, no notes or history)
  let toJournalLite (req : Request) =
    let hist = req.history |> List.sortByDescending (fun it -> Ticks.toLong it.asOf) |> List.head
    { requestId    = req.id
      userId       = req.userId
      text         = (req.history
                       |> List.filter (fun it -> Option.isSome it.text)
                       |> List.sortByDescending (fun it -> Ticks.toLong it.asOf)
                       |> List.head).text
                     |> Option.get
      asOf         = hist.asOf
      lastStatus   = hist.status
      snoozedUntil = req.snoozedUntil
      showAfter    = req.showAfter
      recurType    = req.recurType
      recurCount   = req.recurCount
      history      = []
      notes        = []
      }

  /// Same as above, but with notes and history
  let toJournalFull req =
    { toJournalLite req with 
        history = req.history
        notes   = req.notes
      }


/// Retrieve a request, including its history and notes, by its ID and user ID
let tryFullRequestById reqId userId (db : LiteDatabase) = task {
  let! req = doSingleQuery (db.requests.Query().Where (fun it -> it.id = reqId && it.userId = userId))
  return match box req with null -> None | _ -> Some req
  }

/// Add a history entry
let addHistory reqId userId hist db = task {
  match! tryFullRequestById reqId userId db with
  | Some req -> do! doUpdate db { req with history = hist :: req.history }
  | None -> invalidOp $"{RequestId.toString reqId} not found"
  }

/// Add a note
let addNote reqId userId note db = task {
  match! tryFullRequestById reqId userId db with
  | Some req -> do! doUpdate db { req with notes = note :: req.notes }
  | None -> invalidOp $"{RequestId.toString reqId} not found"
  }

/// Add a request
let addRequest (req : Request) (db : LiteDatabase) =
  db.requests.Insert req |> ignore

/// Retrieve all answered requests for the given user
let answeredRequests userId (db : LiteDatabase) = task {
  let! reqs = doListQuery (db.requests.Query().Where(fun req -> req.userId = userId))
  return
    reqs
    |> Seq.map toJournalFull
    |> Seq.filter (fun it -> it.lastStatus = Answered)
    |> Seq.sortByDescending (fun it -> Ticks.toLong it.asOf)
    |> List.ofSeq
  }
  
/// Retrieve the user's current journal
let journalByUserId userId (db : LiteDatabase) = task {
  let! jrnl = doListQuery (db.requests.Query().Where(fun req -> req.userId = userId))
  return
    jrnl
    |> Seq.map toJournalLite
    |> Seq.filter (fun it -> it.lastStatus <> Answered)
    |> Seq.sortBy (fun it -> Ticks.toLong it.asOf)
    |> List.ofSeq
  }

/// Retrieve a request by its ID and user ID (without notes and history)
let tryRequestById reqId userId db = task {
  match! tryFullRequestById reqId userId db with
  | Some r -> return Some { r with history = []; notes = [] }
  | _ -> return None
  }

/// Retrieve notes for a request by its ID and user ID
let notesById reqId userId (db : LiteDatabase) = task {
  match! tryFullRequestById reqId userId db with | Some req -> return req.notes | None -> return []
  }
    
/// Retrieve a journal request by its ID and user ID
let tryJournalById reqId userId (db : LiteDatabase) = task {
  match! tryFullRequestById reqId userId db with
  | Some req -> return req |> (toJournalLite >> Some)
  | None -> return None
  }
    
/// Update the recurrence for a request
let updateRecurrence reqId userId recurType recurCount db = task {
  match! tryFullRequestById reqId userId db with
  | Some req -> do! doUpdate db { req with recurType = recurType; recurCount = recurCount }
  | None -> invalidOp $"{RequestId.toString reqId} not found"
  }

/// Update a snoozed request
let updateSnoozed reqId userId until db = task {
  match! tryFullRequestById reqId userId db with
  | Some req -> do! doUpdate db { req with snoozedUntil = until; showAfter = until }
  | None -> invalidOp $"{RequestId.toString reqId} not found"
  }

/// Update the "show after" timestamp for a request
let updateShowAfter reqId userId showAfter db = task {
  match! tryFullRequestById reqId userId db with
  | Some req -> do! doUpdate db { req with showAfter = showAfter }
  | None -> invalidOp $"{RequestId.toString reqId} not found"
  }
