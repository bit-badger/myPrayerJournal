
open Microsoft.FSharpLu.Json
open MyPrayerJournal
open Npgsql
open Raven.Client.Documents

type NpgsqlDataReader with
  member this.getShort = this.GetOrdinal >> this.GetInt16
  member this.getString = this.GetOrdinal >> this.GetString
  member this.getTicks = this.GetOrdinal >> this.GetInt64 >> Ticks
  member this.isNull = this.GetOrdinal >> this.IsDBNull

let pgConn () =
  let c = new NpgsqlConnection "Host=severus-server;Database=mpj;Username=mpj;Password=devpassword;Application Name=myPrayerJournal"
  c.Open ()
  c

let isValidStatus stat =
  try
    (RequestAction.fromString >> ignore) stat
    true
  with _ -> false

let getHistory reqId =
  use conn = pgConn ()
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

let getNotes reqId =
  use conn = pgConn ()
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

let migrateRequests (store : IDocumentStore) =
  use sess = store.OpenSession ()
  use conn = pgConn ()
  use cmd = conn.CreateCommand ()
  cmd.CommandText <-
    """SELECT "requestId", "enteredOn", "userId", "snoozedUntil", "showAfter", "recurType", "recurCount" FROM mpj.request"""
  use rdr = cmd.ExecuteReader ()
  while rdr.Read () do
    let reqId = rdr.getString "requestId"
    sess.Store (
      { Id           = (RequestId.fromIdString >> RequestId.toString) reqId
        enteredOn    = rdr.getTicks "enteredOn"
        userId       = (rdr.getString >> UserId) "userId"
        snoozedUntil = rdr.getTicks "snoozedUntil"
        showAfter    = rdr.getTicks "showAfter"
        recurType    = (rdr.getString >> Recurrence.fromString) "recurType"
        recurCount   = rdr.getShort "recurCount"
        history      = getHistory reqId
        notes        = getNotes reqId
        })
  sess.SaveChanges ()

[<EntryPoint>]
let main argv =
  let raven = new DocumentStore (Urls = [| "http://localhost:8080" |], Database = "myPrayerJournal")
  raven.Conventions.CustomizeJsonSerializer <-
    fun x ->
        x.Converters.Add (RequestIdJsonConverter ())
        x.Converters.Add (TicksJsonConverter ())
        x.Converters.Add (UserIdJsonConverter ())
        x.Converters.Add (CompactUnionJsonConverter ())
  let store = raven.Initialize ()
  migrateRequests store
  printfn "fin"
  0 // return an integer exit code
