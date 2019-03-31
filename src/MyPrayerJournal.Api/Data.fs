namespace MyPrayerJournal

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.EntityFrameworkCore
open Microsoft.FSharpLu

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
