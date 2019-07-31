namespace MyPrayerJournal

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.FSharpLu
open Newtonsoft.Json
open Raven.Client.Documents
open Raven.Client.Documents.Indexes
open Raven.Client.Documents.Linq
open System
open System.Collections.Generic

/// JSON converters for various DUs
module Converters =
  
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

  /// A sequence of all custom converters for myPrayerJournal
  let all : JsonConverter seq =
    seq {
      yield RequestIdJsonConverter ()
      yield UserIdJsonConverter ()
      yield TicksJsonConverter ()
      }

/// RavenDB index declarations
module Indexes =
  
  /// Index requests by user ID
  type Requests_ByUserId () as this =
    inherit AbstractJavaScriptIndexCreationTask ()
    do
      this.Maps <- HashSet<string> [ "docs.Requests.Select(req => new { userId = req.userId })" ]

  /// Index requests for a journal view
  type Requests_AsJournal () as this =
    inherit AbstractJavaScriptIndexCreationTask ()
    do
      this.Maps <- HashSet<string> [
        "docs.Requests.Select(req => new {
            requestId = req.Id,
            userId = req.userId,
            text = req.history.Where(hist => hist.text != null).OrderByDescending(hist => hist.asOf).First().text,
            asOf = req.history.OrderByDescending(hist => hist.asOf).First().asOf,
            lastStatus = req.history.OrderByDescending(hist => hist.asOf).First().status,
            snoozedUntil = req.snoozedUntil,
            showAfter = req.showAfter,
            recurType = req.recurType,
            recurCount = req.recurCount
        })"
        ]
      this.Fields <-
        [ "text",       IndexFieldOptions (Storage = Nullable FieldStorage.Yes)
          "asOf",       IndexFieldOptions (Storage = Nullable FieldStorage.Yes)
          "lastStatus", IndexFieldOptions (Storage = Nullable FieldStorage.Yes)
          ]
        |> dict
        |> Dictionary<string, IndexFieldOptions>


/// Extensions on the IAsyncDocumentSession interface to support our data manipulation needs
[<AutoOpen>]
module Extensions =
  
  open Indexes
  open Raven.Client.Documents.Commands.Batches
  open Raven.Client.Documents.Operations
  open Raven.Client.Documents.Session

  /// Format an RQL query by a strongly-typed index
  let fromIndex (typ : Type) =
    typ.Name.Replace ("_", "/") |> sprintf "from index '%s'"

  /// Utility method to create a patch request to push an item on the end of a list
  let listPush<'T> listName docId (item : 'T) =
    let r = PatchRequest()
    r.Script          <- sprintf "this.%s.push(args.Item)" listName
    r.Values.["Item"] <- item
    PatchCommandData (docId, null, r, null)

  /// Utility method to create a patch to update a single field
  // TODO: think we need to include quotes if it's a string
  let fieldUpdate<'T> fieldName docId (item : 'T) =
    let r = PatchRequest()
    r.Script          <- sprintf "this.%s = args.Item" fieldName
    r.Values.["Item"] <- item
    PatchCommandData (docId, null, r, null)
    

/// All data manipulations within myPrayerJournal
module Data =
  
  open Indexes
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
    sess.Query<JournalRequest, Requests_AsJournal>()
      .Where(fun r -> r.userId = userId && r.lastStatus = "Answered")
      .OrderByDescending(fun r -> r.asOf)
      .ProjectInto<JournalRequest>()
      .ToListAsync()
    
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
        |> List.ofSeq
        |> List.map (fun r -> r.history <- []; r.notes <- []; r)
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
      return Option.fromObject req
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
