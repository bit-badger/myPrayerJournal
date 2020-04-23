namespace MyPrayerJournal

open System
open System.Collections.Generic

/// JSON converters for various DUs
module Converters =
  
  open Microsoft.FSharpLu.Json
  open Newtonsoft.Json

  /// JSON converter for request IDs
  type RequestIdJsonConverter () =
    inherit JsonConverter<RequestId> ()
    override __.WriteJson(writer : JsonWriter, value : RequestId, _ : JsonSerializer) =
      (RequestId.toString >> writer.WriteValue) value
    override __.ReadJson(reader: JsonReader, _ : Type, _ : RequestId, _ : bool, _ : JsonSerializer) =
      (string >> RequestId.fromIdString) reader.Value

  /// JSON converter for user IDs
  type UserIdJsonConverter () =
    inherit JsonConverter<UserId> ()
    override __.WriteJson(writer : JsonWriter, value : UserId, _ : JsonSerializer) =
      (UserId.toString >> writer.WriteValue) value
    override __.ReadJson(reader: JsonReader, _ : Type, _ : UserId, _ : bool, _ : JsonSerializer) =
      (string >> UserId) reader.Value

  /// JSON converter for Ticks
  type TicksJsonConverter () =
    inherit JsonConverter<Ticks> ()
    override __.WriteJson(writer : JsonWriter, value : Ticks, _ : JsonSerializer) =
      (Ticks.toLong >> writer.WriteValue) value
    override __.ReadJson(reader: JsonReader, _ : Type, _ : Ticks, _ : bool, _ : JsonSerializer) =
      (string >> int64 >> Ticks) reader.Value

  /// A sequence of all custom converters needed for myPrayerJournal
  let all : JsonConverter seq =
    seq {
      yield RequestIdJsonConverter ()
      yield UserIdJsonConverter ()
      yield TicksJsonConverter ()
      yield CompactUnionJsonConverter true
      }


/// RavenDB index declarations
module Indexes =
  
  open Raven.Client.Documents.Indexes

  /// Index requests for a journal view
  type Requests_AsJournal () as this =
    inherit AbstractJavaScriptIndexCreationTask ()
    do
      this.Maps <- HashSet<string> [
        """docs.Requests.Select(req => new {
            requestId = req.Id.Replace("Requests/", ""),
            userId = req.userId,
            text = req.history.Where(hist => hist.text != null).OrderByDescending(hist => hist.asOf).First().text,
            asOf = req.history.OrderByDescending(hist => hist.asOf).First().asOf,
            lastStatus = req.history.OrderByDescending(hist => hist.asOf).First().status,
            snoozedUntil = req.snoozedUntil,
            showAfter = req.showAfter,
            recurType = req.recurType,
            recurCount = req.recurCount
        })"""
        ]
      this.Fields <-
        [ "requestId",  IndexFieldOptions (Storage = Nullable FieldStorage.Yes)
          "text",       IndexFieldOptions (Storage = Nullable FieldStorage.Yes)
          "asOf",       IndexFieldOptions (Storage = Nullable FieldStorage.Yes)
          "lastStatus", IndexFieldOptions (Storage = Nullable FieldStorage.Yes)
          ]
        |> dict
        |> Dictionary<string, IndexFieldOptions>


/// All data manipulations within myPrayerJournal
module Data =
  
  open FSharp.Control.Tasks.V2.ContextInsensitive
  open Indexes
  open Microsoft.FSharpLu
  open Raven.Client.Documents
  open Raven.Client.Documents.Linq
  open Raven.Client.Documents.Session

  /// Add a history entry
  let addHistory reqId (hist : History) (sess : IAsyncDocumentSession) =
    sess.Advanced.Patch<Request, History> (
      RequestId.toString reqId,
      (fun r -> r.history :> IEnumerable<History>),
      fun (h : JavaScriptArray<History>) -> h.Add (hist) :> obj)
  
  /// Add a note
  let addNote reqId (note : Note) (sess : IAsyncDocumentSession) =
    sess.Advanced.Patch<Request, Note> (
      RequestId.toString reqId,
      (fun r -> r.notes :> IEnumerable<Note>),
      fun (h : JavaScriptArray<Note>) -> h.Add (note) :> obj)

  /// Add a request
  let addRequest req (sess : IAsyncDocumentSession) =
    sess.StoreAsync (req, req.Id)

  /// Retrieve all answered requests for the given user
  let answeredRequests userId (sess : IAsyncDocumentSession) =
    task {
      let! reqs =
        sess.Query<JournalRequest, Requests_AsJournal>()
          .Where(fun r -> r.userId = userId && r.lastStatus = "Answered")
          .OrderByDescending(fun r -> r.asOf)
          .ProjectInto<JournalRequest>()
          .ToListAsync ()
      return List.ofSeq reqs
      }
    
  /// Retrieve the user's current journal
  let journalByUserId userId (sess : IAsyncDocumentSession) =
    task {
      let! jrnl =
        sess.Query<JournalRequest, Requests_AsJournal>()
          .Where(fun r -> r.userId = userId && r.lastStatus <> "Answered")
          .OrderBy(fun r -> r.asOf)
          .ProjectInto<JournalRequest>()
          .ToListAsync()
      return
        jrnl
        |> Seq.map (fun r -> r.history <- []; r.notes <- []; r)
        |> List.ofSeq
      }

  /// Save changes in the current document session
  let saveChanges (sess : IAsyncDocumentSession) =
    sess.SaveChangesAsync ()

  /// Retrieve a request, including its history and notes, by its ID and user ID
  let tryFullRequestById reqId userId (sess : IAsyncDocumentSession) =
    task {
      let! req = RequestId.toString reqId |> sess.LoadAsync
      return match Option.fromObject req with Some r when r.userId = userId -> Some r | _ -> None
      }


  /// Retrieve a request by its ID and user ID (without notes and history)
  let tryRequestById reqId userId (sess : IAsyncDocumentSession) =
    task {
      match! tryFullRequestById reqId userId sess with
      | Some r -> return Some { r with history = []; notes = [] }
      | _ -> return None
      }
  
  /// Retrieve notes for a request by its ID and user ID
  let notesById reqId userId (sess : IAsyncDocumentSession) =
    task {
      match! tryFullRequestById reqId userId sess with
      | Some req -> return req.notes
      | None -> return []
      }
      
  /// Retrieve a journal request by its ID and user ID
  let tryJournalById reqId userId (sess : IAsyncDocumentSession) =
    task {
      let! req =
        sess.Query<Request, Requests_AsJournal>()
          .Where(fun x -> x.Id = (RequestId.toString reqId) && x.userId = userId)
          .ProjectInto<JournalRequest>()
          .FirstOrDefaultAsync ()
      return
        Option.fromObject req
        |> Option.map (fun r -> r.history <- []; r.notes <- []; r)
      }
      
  /// Update the recurrence for a request
  let updateRecurrence reqId recurType recurCount (sess : IAsyncDocumentSession) =
    sess.Advanced.Patch<Request, Recurrence> (RequestId.toString reqId, (fun r -> r.recurType),  recurType)
    sess.Advanced.Patch<Request, int16>      (RequestId.toString reqId, (fun r -> r.recurCount), recurCount)

  /// Update a snoozed request
  let updateSnoozed reqId until (sess : IAsyncDocumentSession) =
    sess.Advanced.Patch<Request, Ticks> (RequestId.toString reqId, (fun r -> r.snoozedUntil),  until)
    sess.Advanced.Patch<Request, Ticks> (RequestId.toString reqId, (fun r -> r.showAfter),     until)

  /// Update the "show after" timestamp for a request
  let updateShowAfter reqId showAfter (sess : IAsyncDocumentSession) =
    sess.Advanced.Patch<Request, Ticks> (RequestId.toString reqId, (fun r -> r.showAfter), showAfter)
