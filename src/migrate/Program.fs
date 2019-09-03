
open Microsoft.FSharpLu.Json
open MyPrayerJournal
open Npgsql
open Raven.Client.Documents

type NpgsqlDataReader with
  member this.getShort = this.GetOrdinal >> this.GetInt16
  member this.getString = this.GetOrdinal >> this.GetString
  member this.getTicks = this.GetOrdinal >> this.GetInt64 >> Ticks
  member this.isNull = this.GetOrdinal >> this.IsDBNull

let pgConn connStr =
  let c = new NpgsqlConnection (connStr)
  c.Open ()
  c

let isValidStatus stat =
  try
    (RequestAction.fromString >> ignore) stat
    true
  with _ -> false

let getHistory reqId connStr =
  use conn = pgConn connStr
  use cmd = conn.CreateCommand ()
  cmd.CommandText <- """SELECT "asOf", status, text FROM mpj.history WHERE "requestId" = @reqId ORDER BY "asOf" """
  (cmd.Parameters.Add >> ignore) (NpgsqlParameter ("@reqId", reqId :> obj))
  use rdr = cmd.ExecuteReader ()
  seq {
    while rdr.Read () do
      match (rdr.getString >> isValidStatus) "status" with
      | true ->
          yield
            { asOf   = rdr.getTicks "asOf"
              status = (rdr.getString >> RequestAction.fromString) "status"
              text   = match rdr.isNull "text" with true -> None | false -> (rdr.getString >> Some) "text"
              }
      | false ->
          printf "Invalid status %s; skipped history entry %s/%i\n" (rdr.getString "status") reqId
            ((rdr.getTicks >> Ticks.toLong) "asOf")
    }
  |> List.ofSeq

let getNotes reqId connStr =
  use conn = pgConn connStr
  use cmd = conn.CreateCommand ()
  cmd.CommandText <- """SELECT "asOf", notes FROM mpj.note WHERE "requestId" = @reqId"""
  (cmd.Parameters.Add >> ignore) (NpgsqlParameter ("@reqId", reqId :> obj))
  use rdr = cmd.ExecuteReader ()
  seq {
    while rdr.Read () do
      yield
        { asOf  = rdr.getTicks "asOf"
          notes = rdr.getString "notes"
          }
    }
  |> List.ofSeq

let migrateRequests (store : IDocumentStore) connStr =
  use sess = store.OpenSession ()
  use conn = pgConn connStr
  use cmd = conn.CreateCommand ()
  cmd.CommandText <-
    """SELECT "requestId", "enteredOn", "userId", "snoozedUntil", "showAfter", "recurType", "recurCount" FROM mpj.request"""
  use rdr = cmd.ExecuteReader ()
  while rdr.Read () do
    let reqId      = rdr.getString "requestId"
    let recurrence =
      match rdr.getString "recurType" with
      | "immediate" -> Immediate
      | "hours" -> Hours
      | "days" -> Days
      | "weeks" -> Weeks
      | x -> invalidOp (sprintf "%s is not a valid recurrence" x)
    sess.Store (
      { Id           = (RequestId.fromIdString >> RequestId.toString) reqId
        enteredOn    = rdr.getTicks "enteredOn"
        userId       = (rdr.getString >> UserId) "userId"
        snoozedUntil = rdr.getTicks "snoozedUntil"
        showAfter    = match recurrence with Immediate -> Ticks 0L | _ -> rdr.getTicks "showAfter"
        recurType    = recurrence
        recurCount   = rdr.getShort "recurCount"
        history      = getHistory reqId connStr
        notes        = getNotes   reqId connStr
        })
  sess.SaveChanges ()

open Converters
open System
open System.Security.Cryptography.X509Certificates

[<EntryPoint>]
let main argv =
  match argv.Length with
  | 4 ->
      let clientCert = new X509Certificate2 (argv.[1], argv.[2])
      let raven = new DocumentStore (Urls = [| argv.[0] |], Database = "myPrayerJournal", Certificate = clientCert)
      raven.Conventions.CustomizeJsonSerializer <-
        fun x ->
            x.Converters.Add (RequestIdJsonConverter ())
            x.Converters.Add (TicksJsonConverter ())
            x.Converters.Add (UserIdJsonConverter ())
            x.Converters.Add (CompactUnionJsonConverter ())
      let store = raven.Initialize ()
      migrateRequests store argv.[3]
      printfn "fin"
  | _ ->
      Console.WriteLine "Usage: dotnet migrate.dll [raven-url] [raven-cert-file] [raven-cert-pw] [postgres-conn-str]"
  0
