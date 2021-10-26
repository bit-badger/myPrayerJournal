open FSharp.Data
open FSharp.Data.CsvExtensions
open LiteDB
open MyPrayerJournal.Domain
open NodaTime

module Subdocs =
  
  open FSharp.Data.JsonExtensions

  let history json =
    match JsonValue.Parse json with
    | JsonValue.Array hist ->
      hist
      |> Array.map (fun h ->
          { asOf   = (h?asOf.AsInteger64 >> Instant.FromUnixTimeMilliseconds) ()
            status = h?status.AsString () |> RequestAction.ofString
            text   = match h?text.AsString () with "" -> None | txt -> Some txt
            })
      |> List.ofArray
    | _ -> []
  
  let notes json =
    match JsonValue.Parse json with
    | JsonValue.Array notes ->
        notes
        |> Array.map (fun n ->
            { asOf = (n?asOf.AsInteger64 >> Instant.FromUnixTimeMilliseconds) ()
              notes = n?notes.AsString ()
              })
        |> List.ofArray
    | _ -> []

let oldData = CsvFile.Load("data.csv")

let db = new LiteDatabase("Filename=./mpj.db")

MyPrayerJournal.Data.Startup.ensureDb db

let migrated =
  oldData.Rows
  |> Seq.map (fun r ->
      { id           = r["@id"].Replace ("Requests/", "") |> RequestId.ofString
        enteredOn    = (r?enteredOn.AsInteger64 >> Instant.FromUnixTimeMilliseconds) ()
        userId       = UserId r?userId
        snoozedUntil = (r?snoozedUntil.AsInteger64 >> Instant.FromUnixTimeMilliseconds) ()
        showAfter    = (r?showAfter.AsInteger64 >> Instant.FromUnixTimeMilliseconds) ()
        recurType    = r?recurType |> Recurrence.ofString
        recurCount   = (r?recurCount.AsInteger >> int16) ()
        history      = Subdocs.history r?history
        notes        = Subdocs.notes r?notes
      })
  |> db.GetCollection<Request>("request").Insert

db.Checkpoint ()

printfn $"Migrated {migrated} requests"
