namespace MyPrayerJournal.Migrations

open System
open System.Collections.Generic
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Infrastructure
open Microsoft.EntityFrameworkCore.Metadata
open Microsoft.EntityFrameworkCore.Migrations
open Microsoft.EntityFrameworkCore.Migrations.Operations
open Microsoft.EntityFrameworkCore.Migrations.Operations.Builders
open MyPrayerJournal

type RequestTable = {
  RequestId : OperationBuilder<AddColumnOperation>
  EnteredOn : OperationBuilder<AddColumnOperation>
  UserId : OperationBuilder<AddColumnOperation>
  }

type HistoryTable = {
  RequestId : OperationBuilder<AddColumnOperation>
  AsOf : OperationBuilder<AddColumnOperation>
  Status : OperationBuilder<AddColumnOperation>
  Text : OperationBuilder<AddColumnOperation>
  }

[<DbContext (typeof<DataContext>)>]
[<Migration "20170104023341_InitialDb">]
type InitialDb () =
  inherit Migration ()
  
  override this.Up migrationBuilder =
    migrationBuilder.EnsureSchema(
      name = "mpj")
    |> ignore

    migrationBuilder.CreateTable(
      name = "Request",
      schema = "mpj",
      columns =
        (fun table ->
          { RequestId = table.Column<Guid>(nullable = false)
            EnteredOn = table.Column<int64>(nullable = false)
            UserId = table.Column<Guid>(nullable = false)
            }
          ),
      constraints =
        fun table ->
          table.PrimaryKey("PK_Request", fun x -> x.RequestId :> obj) |> ignore
      )
    |> ignore

    migrationBuilder.CreateTable(
      name = "History",
      schema = "mpj",
      columns =
        (fun table ->
          { RequestId = table.Column<Guid>(nullable = false)
            AsOf = table.Column<int64>(nullable = false)
            Status = table.Column<string>(nullable = true)
            Text = table.Column<string>(nullable = true)
            }
          ),
      constraints =
        fun table ->
          table.PrimaryKey("PK_History", fun x -> (x.RequestId, x.AsOf) :> obj)
          |> ignore
          table.ForeignKey(
            name = "FK_History_Request_RequestId",
            column = (fun x -> x.RequestId :> obj),
            principalSchema = "mpj",
            principalTable = "Request",
            principalColumn = "RequestId",
            onDelete = ReferentialAction.Cascade)
          |> ignore
      )
    |> ignore

  override this.Down migrationBuilder =
    migrationBuilder.DropTable(
      name = "History",
      schema = "mpj")
    |> ignore

    migrationBuilder.DropTable(
      name = "Request",
      schema = "mpj")
    |> ignore
