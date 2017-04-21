namespace MyPrayerJournal.Migrations

open System
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Infrastructure
open Microsoft.EntityFrameworkCore.Metadata
open Microsoft.EntityFrameworkCore.Migrations
open MyPrayerJournal

[<DbContext (typeof<DataContext>)>]
type DataContextModelSnapshot () =
  inherit ModelSnapshot ()
  override this.BuildModel modelBuilder =
    modelBuilder
      .HasDefaultSchema("mpj")
      .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
      .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
    |> ignore

    modelBuilder.Entity("MyPrayerJournal.History",
      fun b ->
        b.Property<Guid>("RequestId")
        |> ignore
        b.Property<int64>("AsOf")
        |> ignore
        b.Property<string>("Status")
        |> ignore
        b.Property<string>("Text")
        |> ignore
        b.HasKey("RequestId", "AsOf")
        |> ignore
        b.ToTable("History")
        |> ignore
      )
    |> ignore

    modelBuilder.Entity("MyPrayerJournal.Request",
      fun b ->
        b.Property<Guid>("RequestId")
          .ValueGeneratedOnAdd()
        |> ignore
        b.Property<int64>("EnteredOn")
        |> ignore
        b.Property<Guid>("UserId")
        |> ignore
        b.HasKey("RequestId")
        |> ignore
        b.ToTable("Request")
        |> ignore
      )
    |> ignore

    modelBuilder.Entity("MyPrayerJournal.History",
      fun b ->
        b.HasOne("MyPrayerJournal.Request", "Request")
          .WithMany("History")
          .HasForeignKey("RequestId")
          .OnDelete(DeleteBehavior.Cascade)
        |> ignore
      )
    |> ignore