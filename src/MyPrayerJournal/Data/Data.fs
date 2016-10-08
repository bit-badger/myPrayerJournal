[<AutoOpen>]
module MyPrayerJournal.Data

open Newtonsoft.Json
open RethinkDb.Driver
open RethinkDb.Driver.Ast
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
  
  /// Log on a user
  member this.LogOnUser (email : string) (passwordHash : string) =
    async {
      let! user = r.Table(DataTable.User)
                    .GetAll(email).OptArg("index", "Email")
                    .Filter(ReqlFunction1(fun usr -> upcast usr.["PasswordHash"].Eq(passwordHash)))
                    .RunResultAsync<User list>(this)
      return user |> List.tryHead
      }

  /// Set up the environment for MyPrayerJournal
  member this.EstablishEnvironment (cfg : AppConfig) =
    /// Shorthand for the database
    let db () = r.Db("MyPrayerJournal")
    // Be chatty about what we're doing
    let mkStep step = sprintf "[MyPrayerJournal] %s" step
    let logStep step = mkStep step |> Console.WriteLine
    let logStepStart step = mkStep step |> Console.Write
    let logStepEnd () = Console.WriteLine " done"
    /// Ensure the database exists
    let checkDatabase () =
      async {
        logStep "|> Checking database"
        let! dbList = r.DbList().RunResultAsync<string list>(this) 
        match dbList |> List.contains "MyPrayerJournal" with
        | true -> ()
        | _ -> logStepStart "   Database not found - creating..."
               do! r.DbCreate("MyPrayerJournal").RunResultAsync(this)
               logStepEnd ()
        }
    /// Ensure all tables exit
    let checkTables () =
      async {
        logStep "|> Checking tables"
        let! tables = db().TableList().RunResultAsync<string list>(this)
        [ DataTable.Request; DataTable.User ]
        |> List.filter (fun tbl -> not (tables |> List.contains tbl))
        |> List.map (fun tbl ->
            async {
              logStepStart <| sprintf "   %s table not found - creating..." tbl
              do! db().TableCreate(tbl).RunResultAsync(this)
              logStepEnd()
              })
        |> List.iter Async.RunSynchronously
        // Seed the user table if it is empty
        let! userCount = db().Table(DataTable.User).Count().RunResultAsync<int64>(this)
        match int64 0 = userCount with
        | true -> logStepStart "   No users found - seeding..."
                  do! db().Table(DataTable.User).Insert(
                        { User.Empty with
                            Id = Guid.NewGuid().ToString ()
                            Email = "test@example.com"
                            PasswordHash = User.HashPassword "password" cfg.PasswordSaltBytes
                            Name = "Default User"
                            TimeZone = "America/Chicago"
                          }).RunResultAsync(this)
                  logStepEnd ()
        | _ -> ()
      }
    /// Ensure the proper indexes exist
    let checkIndexes () =
      async {
        logStep "|> Checking indexes"
        let! reqIdx = db().Table(DataTable.Request).IndexList().RunResultAsync<string list>(this)
        match reqIdx |> List.contains "UserId" with
        | true -> ()
        | _ -> logStepStart <| sprintf "   %s.UserId index not found - creating..." DataTable.Request
               do! db().Table(DataTable.Request).IndexCreate("UserId").RunResultAsync(this)
               logStepEnd ()
        let! usrIdx = db().Table(DataTable.User).IndexList().RunResultAsync<string list>(this)
        match usrIdx |> List.contains "Email" with
        | true -> ()
        | _ -> logStepStart <| sprintf "   %s.Email index not found - creating..." DataTable.User
               do! db().Table(DataTable.User).IndexCreate("Email").RunResultAsync(this)
               logStepEnd ()
        }
    async {
      logStep "Database checks starting"
      do! checkDatabase ()
      do! checkTables ()
      do! checkIndexes ()
      logStep "Database checks complete"
      }
