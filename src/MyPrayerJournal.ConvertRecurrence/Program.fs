open MyPrayerJournal.Domain
open NodaTime

/// The old definition of the history entry
[<CLIMutable; NoComparison; NoEquality>]
type OldHistory =
    {   /// The time when this history entry was made
        asOf   : int64
        /// The status for this history entry
        status : RequestAction
        /// The text of the update, if applicable
        text   : string option
    }

/// The old definition of of the note entry
[<CLIMutable; NoComparison; NoEquality>]
type OldNote =
    {   /// The time when this note was made
        asOf  : int64
        
        /// The text of the notes
        notes : string
    }

/// Request is the identifying record for a prayer request
[<CLIMutable; NoComparison; NoEquality>]
type OldRequest =
    {   /// The ID of the request
        id           : RequestId
        
        /// The time this request was initially entered
        enteredOn    : int64
        
        /// The ID of the user to whom this request belongs ("sub" from the JWT)
        userId       : UserId
        
        /// The time at which this request should reappear in the user's journal by manual user choice
        snoozedUntil : int64
        
        /// The time at which this request should reappear in the user's journal by recurrence
        showAfter    : int64
        
        /// The type of recurrence for this request
        recurType    : string
        
        /// How many of the recurrence intervals should occur between appearances in the journal
        recurCount   : int16
        
        /// The history entries for this request
        history      : OldHistory[]
        
        /// The notes for this request
        notes        : OldNote[]
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

/// Convert an old history entry to the new form
let convertHistory (old : OldHistory) =
    {   AsOf   = Instant.FromUnixTimeMilliseconds old.asOf
        Status = old.status
        Text   = old.text
    }

/// Convert an old note to the new form
let convertNote (old : OldNote) =
    {   AsOf  = Instant.FromUnixTimeMilliseconds old.asOf
        Notes = old.notes
    }

/// Map the old request to the new request
let convert old =
    {   Id           = old.id
        EnteredOn    = Instant.FromUnixTimeMilliseconds old.enteredOn
        UserId       = old.userId
        SnoozedUntil = Instant.FromUnixTimeMilliseconds old.snoozedUntil
        ShowAfter    = Instant.FromUnixTimeMilliseconds old.showAfter
        Recurrence   = mapRecurrence old
        History      = old.history |> Array.map convertHistory |> List.ofArray
        Notes        = old.notes   |> Array.map convertNote    |> List.ofArray
    }

/// Remove the old request, add the converted one (removes recurType / recurCount fields)
let replace (req : Request) =
    db.requests.Delete (Mapping.RequestId.toBson req.Id) |> ignore
    db.requests.Insert req |> ignore
    db.Checkpoint ()

db.GetCollection<OldRequest>("request").FindAll ()
|> Seq.map convert
|> List.ofSeq
|> List.iter replace

// For more information see https://aka.ms/fsharp-console-apps
printfn "Done"
