namespace MyPrayerJournal

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.EntityFrameworkCore

/// Helpers for this file
[<AutoOpen>]
module private Helpers =
  
  /// Convert any item to an option (Option.ofObj does not work for non-nullable types)
  let toOption<'T> (x : 'T) = match box x with null -> None | _ -> Some x


/// Entities for use in the data model for myPrayerJournal
[<AutoOpen>]
module Entities =
  
  open FSharp.EFCore.OptionConverter
  open System.Collections.Generic

  /// Type alias for a Collision-resistant Unique IDentifier
  type Cuid = string

  /// Request ID is a CUID
  type RequestId = Cuid

  /// User ID is a string (the "sub" part of the JWT)
  type UserId = string

  /// History is a record of action taken on a prayer request, including updates to its text
  type [<CLIMutable; NoComparison; NoEquality>] History =
    { /// The ID of the request to which this history entry applies
      requestId : RequestId
      /// The time when this history entry was made
      asOf      : int64
      /// The status for this history entry
      status    : string
      /// The text of the update, if applicable
      text      : string option
      }
  with
    /// An empty history entry
    static member empty =
      { requestId = ""
        asOf      = 0L
        status    = ""
        text      = None
        }

    static member configureEF (mb : ModelBuilder) =
      mb.Entity<History> (
        fun m ->
          m.ToTable "history" |> ignore
          m.HasKey ("requestId", "asOf") |> ignore
          m.Property(fun e -> e.requestId).IsRequired () |> ignore
          m.Property(fun e -> e.asOf).IsRequired () |> ignore
          m.Property(fun e -> e.status).IsRequired() |> ignore
          m.Property(fun e -> e.text) |> ignore)
      |> ignore
      let typ = mb.Model.FindEntityType(typeof<History>)
      let prop = typ.FindProperty("text")
      mb.Model.FindEntityType(typeof<History>).FindProperty("text").SetValueConverter (OptionConverter<string> ())

  /// Note is a note regarding a prayer request that does not result in an update to its text
  and [<CLIMutable; NoComparison; NoEquality>] Note =
    { /// The ID of the request to which this note applies
      requestId : RequestId
      /// The time when this note was made
      asOf      : int64
      /// The text of the notes
      notes     : string
      }
  with
    /// An empty note
    static member empty =
      { requestId = ""
        asOf      = 0L
        notes     = ""
        }

    static member configureEF (mb : ModelBuilder) =
      mb.Entity<Note> (
        fun m ->
          m.ToTable "note" |> ignore
          m.HasKey ("requestId", "asOf") |> ignore
          m.Property(fun e -> e.requestId).IsRequired () |> ignore
          m.Property(fun e -> e.asOf).IsRequired () |> ignore
          m.Property(fun e -> e.notes).IsRequired () |> ignore)
      |> ignore

  /// Request is the identifying record for a prayer request
  and [<CLIMutable; NoComparison; NoEquality>] Request =
    { /// The ID of the request
      requestId    : RequestId
      /// The time this request was initially entered
      enteredOn    : int64
      /// The ID of the user to whom this request belongs ("sub" from the JWT)
      userId       : string
      /// The time at which this request should reappear in the user's journal by manual user choice
      snoozedUntil : int64
      /// The time at which this request should reappear in the user's journal by recurrence
      showAfter    : int64
      /// The type of recurrence for this request
      recurType    : string
      /// How many of the recurrence intervals should occur between appearances in the journal
      recurCount   : int16
      /// The history entries for this request
      history      : ICollection<History>
      /// The notes for this request
      notes        : ICollection<Note>
      }
  with
    /// An empty request
    static member empty =
      { requestId    = ""
        enteredOn    = 0L
        userId       = ""
        snoozedUntil = 0L
        showAfter    = 0L
        recurType    = "immediate"
        recurCount   = 0s
        history      = List<History> ()
        notes        = List<Note> ()
        }

    static member configureEF (mb : ModelBuilder) =
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

  /// JournalRequest is the form of a prayer request returned for the request journal display. It also contains
  /// properties that may be filled for history and notes
  [<CLIMutable; NoComparison; NoEquality>]
  type JournalRequest =
    { /// The ID of the request
      requestId    : RequestId
      /// The ID of the user to whom the request belongs
      userId       : string
      /// The current text of the request
      text         : string
      /// The last time action was taken on the request
      asOf         : int64
      /// The last status for the request
      lastStatus   : string
      /// The time that this request should reappear in the user's journal
      snoozedUntil : int64
      /// The time after which this request should reappear in the user's journal by configured recurrence
      showAfter    : int64
      /// The type of recurrence for this request
      recurType    : string
      /// How many of the recurrence intervals should occur between appearances in the journal
      recurCount   : int16
      /// History entries for the request
      history      : History list
      /// Note entries for the request
      notes        : Note list
      }
  with
    static member configureEF (mb : ModelBuilder) =
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
    [ History.configureEF
      Note.configureEF
      Request.configureEF
      JournalRequest.configureEF
      ]
    |> List.iter (fun x -> x mb)
  
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
      return toOption req
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
      return toOption req
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
          match toOption fullReq with
          | Some _ -> return Some { req with history = List.ofSeq fullReq.history; notes = List.ofSeq fullReq.notes }
          | None -> return None
      | None -> return None
      }
