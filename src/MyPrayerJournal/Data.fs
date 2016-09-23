[<AutoOpen>]
module Data

open Newtonsoft.Json
open RethinkDb.Driver
open RethinkDb.Driver.Net
open System

let private r = RethinkDB.R

/// Tables for data storage
module DataTable =
  /// The table for prayer requests
  [<Literal>]
  let Request = "Request"
  /// The table for users
  [<Literal>]
  let User = "User"

/// Extensions for the RethinkDB connection
type IConnection with
  
  /// Set up the environment for MyPrayerJournal
  member this.EstablishEnvironment () =
    /// Shorthand for the database
    let db () = r.Db("MyPrayerJournal")
    /// Log a step in the database environment set up
    let logStep step = sprintf "[MyPrayerJournal] %s" step |> Console.WriteLine
    /// Ensure the database exists
    let checkDatabase () =
      async {
        logStep "|> Checking database..."
        let! dbList = r.DbList().RunResultAsync<string list>(this) |> Async.AwaitTask 
        match dbList |> List.contains "MyPrayerJournal" with
        | true -> ()
        | _ -> logStep "     Database not found - creating..."
               do! r.DbCreate("MyPrayerJournal").RunResultAsync(this) |> Async.AwaitTask |> Async.Ignore
               logStep "       ...done"
      }
    /// Ensure all tables exit
    let checkTables () =
      async {
        logStep "|> Checking tables..."
        let! tables = db().TableList().RunResultAsync<string list>(this) |> Async.AwaitTask
        [ DataTable.Request; DataTable.User ]
        |> List.filter (fun tbl -> not (tables |> List.contains tbl))
        |> List.map (fun tbl ->
            async {
              logStep <| sprintf "     %s table not found - creating..." tbl
              do! db().TableCreate(tbl).RunResultAsync(this) |> Async.AwaitTask |> Async.Ignore
              logStep "       ...done"
            })
        |> List.iter Async.RunSynchronously
      }
    /// Ensure the proper indexes exist
    let checkIndexes () =
      async {
        logStep "|> Checking indexes..."
        let! reqIdx = db().Table(DataTable.Request).IndexList().RunResultAsync<string list>(this) |> Async.AwaitTask
        match reqIdx |> List.contains "UserId" with
        | true -> ()
        | _ -> logStep <| sprintf "     %s.UserId index not found - creating..." DataTable.Request
               do! db().Table(DataTable.Request).IndexCreate("UserId").RunResultAsync(this) |> Async.AwaitTask |> Async.Ignore
               logStep "       ...done"
        let! usrIdx = db().Table(DataTable.User).IndexList().RunResultAsync<string list>(this) |> Async.AwaitTask
        match usrIdx |> List.contains "Email" with
        | true -> ()
        | _ -> logStep <| sprintf "     %s.Email index not found - creating..." DataTable.User
               do! db().Table(DataTable.User).IndexCreate("Email").RunResultAsync(this) |> Async.AwaitTask |> Async.Ignore
               logStep "       ...done"
      }
    async {
      logStep "Database checks starting"
      do! checkDatabase ()
      do! checkTables ()
      do! checkIndexes ()
      logStep "Database checks complete"
    }
