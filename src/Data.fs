namespace MyPrayerJournal

open Microsoft.EntityFrameworkCore
open System.Linq
open System.Runtime.CompilerServices

/// Data context for myPrayerJournal
type DataContext =
  inherit DbContext

  (*--- CONSTRUCTORS ---*)

  new () = { inherit DbContext () }
  new (options : DbContextOptions<DataContext>) = { inherit DbContext (options) }

  (*--- DbSet FIELDS ---*)

  [<DefaultValue>]
  val mutable private requests : DbSet<Request>
  [<DefaultValue>]
  val mutable private history : DbSet<History>
  
  (*--- DbSet PROPERTIES ---*)

  /// Prayer Requests
  member this.Requests with get () = this.requests and set v = this.requests <- v

  /// History
  member this.History with get () = this.history and set v = this.history <- v

  override this.OnModelCreating (modelBuilder) =
    base.OnModelCreating modelBuilder

    modelBuilder.HasDefaultSchema "mpj"
    |> Request.ConfigureEF
    |> History.ConfigureEF
    |> ignore

/// Data access
module Data =

  /// Data access for prayer requests
  module Requests =

    /// Get all prayer requests for a user
    let allForUser userId (ctx : DataContext) =
      query {
        for req in ctx.Requests do
          where (req.UserId = userId)
          select req
      }
      |> Seq.sortBy
        (fun req ->
          match req.History |> Seq.sortBy (fun hist -> hist.AsOf) |> Seq.tryLast with
          | Some hist -> hist.AsOf
          | _ -> 0L)
      |> List.ofSeq
