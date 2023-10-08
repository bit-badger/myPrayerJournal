open LiteDB
open MyPrayerJournal.Data
open MyPrayerJournal.Domain
open MyPrayerJournal.LiteData
open Microsoft.Extensions.Configuration


let lite = new LiteDatabase "Filename=./mpj.db"
Startup.ensureDb lite

let cfg = (ConfigurationBuilder().AddJsonFile "appsettings.json").Build ()
Connection.setUp cfg |> Async.AwaitTask |> Async.RunSynchronously

let reqs = lite.Requests.FindAll ()

reqs
|> Seq.map (fun old ->
    { Request.empty with
        Id           = old.Id
        EnteredOn    = old.EnteredOn
        UserId       = old.UserId
        SnoozedUntil = old.SnoozedUntil
        ShowAfter    = old.ShowAfter
        Recurrence   = old.Recurrence
        History      = old.History |> Array.sortByDescending (fun it -> it.AsOf) |> List.ofArray
        Notes        = old.Notes   |> Array.sortByDescending (fun it -> it.AsOf) |> List.ofArray
    })
|> Seq.map Request.add
|> List.ofSeq
|> List.iter (Async.AwaitTask >> Async.RunSynchronously)

System.Console.WriteLine $"Migration complete - {Seq.length reqs} requests migrated"

