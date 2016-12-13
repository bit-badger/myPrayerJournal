namespace MyPrayerJournal

open Chiron
open RethinkDb.Driver
open RethinkDb.Driver.Net
open System

type ConfigParameter =
  | Hostname of string
  | Port     of int
  | AuthKey  of string
  | Timeout  of int
  | Database of string

type DataConfig = { Parameters : ConfigParameter list }
with
  member this.CreateConnection () : IConnection =
    let folder (builder : Connection.Builder) block =
      match block with
      | Hostname x -> builder.Hostname x
      | Port     x -> builder.Port     x
      | AuthKey  x -> builder.AuthKey  x
      | Timeout  x -> builder.Timeout  x
      | Database x -> builder.Db       x
    let bldr =
      this.Parameters
      |> Seq.fold folder (RethinkDB.R.Connection ())
    upcast bldr.Connect()
  member this.Database =
    match this.Parameters
          |> List.filter (fun x -> match x with Database _ -> true | _ -> false)
          |> List.tryHead with
    | Some (Database x) -> x
    | _ -> RethinkDBConstants.DefaultDbName
  static member FromJson json =
    match Json.parse json with
    | Object config ->
        let options =
          config
          |> Map.toList
          |> List.map (fun item ->
              match item with
              | "Hostname", String x -> Hostname x
              | "Port",     Number x -> Port <| int x
              | "AuthKey",  String x -> AuthKey x
              | "Timeout",  Number x -> Timeout <| int x
              | "Database", String x -> Database x
              | key, value ->
                  invalidOp <| sprintf "Unrecognized RethinkDB configuration parameter %s (value %A)" key value)
        { Parameters = options }
    | _ -> { Parameters = [] }


/// Tables for data storage
module DataTable =
  /// The table for prayer requests
  [<Literal>]
  let Request = "Request"
  /// The table for users
  [<Literal>]
  let User = "User"

/// Extensions for the RethinkDB connection
[<RequireQualifiedAccess>]
module Data =
  
  let private r = RethinkDB.R

  /// Set up the environment for MyPrayerJournal
  let establishEnvironment (conn : IConnection) =
    /// Shorthand for the database
    let db () = r.Db "myPrayerJournal"
    // Be chatty about what we're doing
    let mkStep = sprintf "[MyPrayerJournal] %s"
    let logStep = mkStep >> Console.WriteLine
    let logStepStart = mkStep >> Console.Write
    let logStepEnd () = Console.WriteLine " done"
    /// Ensure the database exists
    let checkDatabase () =
      async {
        logStep "|> Checking database"
        let! dbList = r.DbList().RunResultAsync<string list> conn 
        match dbList |> List.contains "myPrayerJournal" with
        | true -> ()
        | _ ->
            logStepStart "   Database not found - creating..."
            do! r.DbCreate("myPrayerJournal").RunResultAsync conn
            logStepEnd ()
        }
    /// Ensure all tables exit
    let checkTables () =
      async {
        logStep "|> Checking tables"
        let! tables = db().TableList().RunResultAsync<string list> conn
        [ DataTable.Request; DataTable.User ]
        |> List.filter (fun tbl -> not (tables |> List.contains tbl))
        |> List.map (fun tbl ->
            async {
              logStepStart <| sprintf "   %s table not found - creating..." tbl
              do! db().TableCreate(tbl).RunResultAsync conn
              logStepEnd()
              })
        |> List.iter Async.RunSynchronously
      }
    /// Ensure the proper indexes exist
    let checkIndexes () =
      async {
        logStep "|> Checking indexes"
        let! reqIdx = db().Table(DataTable.Request).IndexList().RunResultAsync<string list> conn
        match reqIdx |> List.contains "UserId" with
        | true -> ()
        | _ ->
            logStepStart <| sprintf "   %s.UserId index not found - creating..." DataTable.Request
            do! db().Table(DataTable.Request).IndexCreate("UserId").RunResultAsync conn
            logStepEnd ()
        let! usrIdx = db().Table(DataTable.User).IndexList().RunResultAsync<string list> conn
        match usrIdx |> List.contains "Email" with
        | true -> ()
        | _ ->
            logStepStart <| sprintf "   %s.Email index not found - creating..." DataTable.User
            do! db().Table(DataTable.User).IndexCreate("Email").RunResultAsync conn
            logStepEnd ()
        }
    async {
      logStep "Database checks starting"
      do! checkDatabase ()
      do! checkTables ()
      do! checkIndexes ()
      logStep "Database checks complete"
      }
