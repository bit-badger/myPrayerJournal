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
            snoozedUntil = req.snoozedUntil,
            showAfter = req.showAfter,
            recurType = req.recurType,
            recurCount = req.recurCount
        })"
        ]
      this.Fields <-
        [ "text", IndexFieldOptions (Storage = Nullable FieldStorage.Yes)
          "asOf", IndexFieldOptions (Storage = Nullable FieldStorage.Yes)
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
    
  // Extensions for the RavenDB session type
  type IAsyncDocumentSession with
    
    /// Add a history entry
    member this.AddHistory (reqId : RequestId) (hist : History) =
      listPush "history" (RequestId.toString reqId) hist
      |> this.Advanced.Defer

    /// Add a note
    member this.AddNote (reqId : RequestId) (note : Note) =
      listPush "notes" (RequestId.toString reqId) note
      |> this.Advanced.Defer

    /// Add a request
    member this.AddRequest req =
      this.StoreAsync (req, req.Id)

    /// Retrieve all answered requests for the given user
    // TODO: not right
    member this.AnsweredRequests (userId : UserId) =
      sprintf "%s where userId = '%s' and lastStatus = 'Answered' order by asOf as long desc"
        (fromIndex typeof<Requests_AsJournal>) (UserId.toString userId)
      |> this.Advanced.AsyncRawQuery<JournalRequest>
    
    /// Retrieve the user's current journal
    // TODO: probably not right either
    member this.JournalByUserId (userId : UserId) =
      sprintf "%s where userId = '%s' and lastStatus <> 'Answered' order by showAfter as long"
        (fromIndex typeof<Requests_AsJournal>) (UserId.toString userId)
      |> this.Advanced.AsyncRawQuery<JournalRequest>
    
    /// Retrieve a request, including its history and notes, by its ID and user ID
    member this.TryFullRequestById (reqId : RequestId) userId =
      task {
        let! req = RequestId.toString reqId |> this.LoadAsync
        match Option.fromObject req with
        | Some r when r.userId = userId -> return Some r
        | _ -> return None
        }

    /// Retrieve a request by its ID and user ID (without notes and history)
    member this.TryRequestById reqId userId =
      task {
        match! this.TryFullRequestById reqId userId with
        | Some r -> return Some { r with history = []; notes = [] }
        | _ -> return None
        }

    /// Retrieve notes for a request by its ID and user ID
    member this.NotesById reqId userId =
      task {
        match! this.TryRequestById reqId userId with
        | Some req -> return req.notes
        | None -> return []
        }

    /// Retrieve a journal request by its ID and user ID
    member this.TryJournalById (reqId : RequestId) userId =
      task {
        let! req =
          this.Query<Request, Requests_AsJournal>()
            .Where(fun x -> x.Id = (RequestId.toString reqId) && x.userId = userId)
            .ProjectInto<JournalRequest>()
            .FirstOrDefaultAsync ()
        return Option.fromObject req
        }
        
    /// Update the recurrence for a request
    member this.UpdateRecurrence (reqId : RequestId) (recurType : Recurrence) (recurCount : int16) =
      let r = PatchRequest()
      r.Script           <- "this.recurType = args.Type; this.recurCount = args.Count"
      r.Values.["Type"]  <- string recurType
      r.Values.["Count"] <- recurCount
      PatchCommandData (RequestId.toString reqId, null, r, null) |> this.Advanced.Defer

    /// Update the "show after" timestamp for a request
    member this.UpdateShowAfter (reqId : RequestId) (showAfter : Ticks) =
      fieldUpdate "showAfter" (RequestId.toString reqId) (Ticks.toLong showAfter)
      |> this.Advanced.Defer

    /// Update a snoozed request
    member this.UpdateSnoozed (reqId : RequestId) (until : Ticks) =
      let r = PatchRequest()
      r.Script          <- "this.snoozedUntil = args.Item; this.showAfter = args.Item"
      r.Values.["Item"] <- Ticks.toLong until
      PatchCommandData (RequestId.toString reqId, null, r, null) |> this.Advanced.Defer



    (*
/// Entity Framework configuration for myPrayerJournal
module internal EFConfig =
  
  open FSharp.EFCore.OptionConverter
  open System.Collections.Generic

  /// Configure EF properties for all entity types
  let configure (mb : ModelBuilder) =
    mb.Entity<History> (
      fun m ->
        m.ToTable "history" |> ignore
        m.HasKey ("requestId", "asOf") |> ignore
        m.Property(fun e -> e.requestId).IsRequired () |> ignore
        m.Property(fun e -> e.asOf).IsRequired () |> ignore
        m.Property(fun e -> e.status).IsRequired() |> ignore
        m.Property(fun e -> e.text) |> ignore)
    |> ignore
    mb.Model.FindEntityType(typeof<History>).FindProperty("text").SetValueConverter (OptionConverter<string> ())

    mb.Entity<Note> (
      fun m ->
        m.ToTable "note" |> ignore
        m.HasKey ("requestId", "asOf") |> ignore
        m.Property(fun e -> e.requestId).IsRequired () |> ignore
        m.Property(fun e -> e.asOf).IsRequired () |> ignore
        m.Property(fun e -> e.notes).IsRequired () |> ignore)
    |> ignore

    mb.Entity<Request> (
      fun m ->
        m.ToTable "request" |> ignore
        m.HasKey(fun e -> e.requestId :> obj) |> ignore
        m.Property(fun e -> e.requestId).IsRequired () |> ignore
        m.Property(fun e -> e.enteredOn).IsRequired () |> ignore
        m.Property(fun e -> e.userId).IsRequired () |> ignore
        m.Property(fun e -> e.snoozedUntil).IsRequired () |> ignore
        m.Property(fun e -> e.showAfter).IsRequired () |> ignore
        m.Property(fun e -> e.recurType).IsRequired() |> ignore
        m.Property(fun e -> e.recurCount).IsRequired() |> ignore
        m.HasMany(fun e -> e.history :> IEnumerable<History>)
          .WithOne()
          .HasForeignKey(fun e -> e.requestId :> obj)
        |> ignore
        m.HasMany(fun e -> e.notes :> IEnumerable<Note>)
          .WithOne()
          .HasForeignKey(fun e -> e.requestId :> obj)
        |> ignore)
    |> ignore

    mb.Query<JournalRequest> (
      fun m ->
        m.ToView "journal" |> ignore
        m.Ignore(fun e -> e.history :> obj) |> ignore
        m.Ignore(fun e -> e.notes :> obj) |> ignore)
    |> ignore


open System.Linq

/// Data context
type AppDbContext (opts : DbContextOptions<AppDbContext>) =
  inherit DbContext (opts)

  [<DefaultValue>]
  val mutable private history  : DbSet<History>
  [<DefaultValue>]
  val mutable private notes    : DbSet<Note>
  [<DefaultValue>]
  val mutable private requests : DbSet<Request>
  [<DefaultValue>]
  val mutable private journal  : DbQuery<JournalRequest>

  member this.History
    with get () = this.history
      and set v = this.history <- v
  member this.Notes
    with get () = this.notes
      and set v = this.notes <- v
  member this.Requests
    with get () = this.requests
      and set v = this.requests <- v
  member this.Journal
    with get () = this.journal
      and set v = this.journal <- v
  
  override __.OnModelCreating (mb : ModelBuilder) =
    base.OnModelCreating mb
    EFConfig.configure mb
  
  /// Register a disconnected entity with the context, having the given state
  member private this.RegisterAs<'TEntity when 'TEntity : not struct> state e =
    this.Entry<'TEntity>(e).State <- state

  /// Add an entity instance to the context
  member this.AddEntry e =
    this.RegisterAs EntityState.Added e

  /// Update the entity instance's values
  member this.UpdateEntry e =
    this.RegisterAs EntityState.Modified e

  /// Retrieve all answered requests for the given user
  member this.AnsweredRequests userId : JournalRequest seq =
    upcast this.Journal
      .Where(fun r -> r.userId = userId && r.lastStatus = "Answered")
      .OrderByDescending(fun r -> r.asOf)
  
  /// Retrieve the user's current journal
  member this.JournalByUserId userId : JournalRequest seq =
    upcast this.Journal
      .Where(fun r -> r.userId = userId && r.lastStatus <> "Answered")
      .OrderBy(fun r -> r.showAfter)
  
  /// Retrieve a request by its ID and user ID
  member this.TryRequestById reqId userId =
    task {
      let! req = this.Requests.AsNoTracking().FirstOrDefaultAsync(fun r -> r.requestId = reqId && r.userId = userId)
      return Option.fromObject req
      }

  /// Retrieve notes for a request by its ID and user ID
  member this.NotesById reqId userId =
    task {
      match! this.TryRequestById reqId userId with
      | Some _ -> return this.Notes.AsNoTracking().Where(fun n -> n.requestId = reqId) |> List.ofSeq
      | None -> return []
      }

  /// Retrieve a journal request by its ID and user ID
  member this.TryJournalById reqId userId =
    task {
      let! req = this.Journal.FirstOrDefaultAsync(fun r -> r.requestId = reqId && r.userId = userId)
      return Option.fromObject req
      }
  
  /// Retrieve a request, including its history and notes, by its ID and user ID
  member this.TryFullRequestById requestId userId =
    task {
      match! this.TryJournalById requestId userId with
      | Some req ->
          let! fullReq =
            this.Requests.AsNoTracking()
              .Include(fun r -> r.history)
              .Include(fun r -> r.notes)
              .FirstOrDefaultAsync(fun r -> r.requestId = requestId && r.userId = userId)
          match Option.fromObject fullReq with
          | Some _ -> return Some { req with history = List.ofSeq fullReq.history; notes = List.ofSeq fullReq.notes }
          | None -> return None
      | None -> return None
      }
*)