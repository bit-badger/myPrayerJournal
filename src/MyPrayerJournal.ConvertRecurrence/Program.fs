open MyPrayerJournal.Domain
open NodaTime

/// Request is the identifying record for a prayer request
[<CLIMutable; NoComparison; NoEquality>]
type OldRequest = {
  /// The ID of the request
  id           : RequestId
  /// The time this request was initially entered
  enteredOn    : Instant
  /// The ID of the user to whom this request belongs ("sub" from the JWT)
  userId       : UserId
  /// The time at which this request should reappear in the user's journal by manual user choice
  snoozedUntil : Instant
  /// The time at which this request should reappear in the user's journal by recurrence
  showAfter    : Instant
  /// The type of recurrence for this request
  recurType    : string
  /// How many of the recurrence intervals should occur between appearances in the journal
  recurCount   : int16
  /// The history entries for this request
  history      : History array
  /// The notes for this request
  notes        : Note array
  }

open LiteDB
open MyPrayerJournal.Data

let db = new LiteDatabase ("Filename=./mpj.db")
Startup.ensureDb db

/// Map the old recurrence to the new style
let mapRecurrence old =
  match old.recurType with
  | "Days" -> Days old.recurCount
  | "Hours" -> Hours old.recurCount
  | "Weeks" -> Weeks old.recurCount
  | _ -> Immediate

/// Map the old request to the new request
let convert old = {
  id           = old.id
  enteredOn    = old.enteredOn
  userId       = old.userId
  snoozedUntil = old.snoozedUntil
  showAfter    = old.showAfter
  recurrence   = mapRecurrence old
  history      = Array.toList old.history
  notes        = Array.toList old.notes
  }

/// Remove the old request, add the converted one (removes recurType / recurCount fields)
let replace (req : Request) =
  db.requests.Delete(Mapping.RequestId.toBson req.id) |> ignore
  db.requests.Insert(req) |> ignore
  db.Checkpoint()

let reqs = db.GetCollection<OldRequest>("request").FindAll()
let rList = reqs |> Seq.toList
let mapped = rList |> List.map convert
//let reqList = mapped |> List.ofSeq

mapped |> List.iter replace

// For more information see https://aka.ms/fsharp-console-apps
printfn "Done"
