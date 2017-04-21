namespace MyPrayerJournal

open Microsoft.EntityFrameworkCore
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

  override this.OnConfiguring (optionsBuilder) =
    base.OnConfiguring optionsBuilder
    optionsBuilder.UseNpgsql
      "Host=severus-server;Database=mpj;Username=mpj;Password=devpassword;Application Name=myPrayerJournal"
    |> ignore
  
  override this.OnModelCreating (modelBuilder) =
    base.OnModelCreating modelBuilder

    modelBuilder.HasDefaultSchema "mpj"
    |> Request.ConfigureEF
    |> History.ConfigureEF
    |> ignore
