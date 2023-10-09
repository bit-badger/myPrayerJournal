/// The data model for myPrayerJournal
[<AutoOpen>]
module MyPrayerJournal.Domain

open System
open Cuid
open NodaTime

/// An identifier for a request
type RequestId = RequestId of Cuid

/// Functions to manipulate request IDs
module RequestId =
    
    /// The string representation of the request ID
    let toString = function RequestId x -> Cuid.toString x
    
    /// Create a request ID from a string representation
    let ofString = Cuid >> RequestId


/// The identifier of a user (the "sub" part of the JWT)
type UserId = UserId of string

/// Functions to manipulate user IDs
module UserId =
    
    /// The string representation of the user ID
    let toString = function UserId x -> x


/// How frequently a request should reappear after it is marked "Prayed"
type Recurrence =
    
    /// A request should reappear immediately at the bottom of the list
    | Immediate
    
    /// A request should reappear in the given number of hours
    | Hours of int16
    
    /// A request should reappear in the given number of days
    | Days  of int16
    
    /// A request should reappear in the given number of weeks (7-day increments)
    | Weeks of int16

/// Functions to manipulate recurrences
module Recurrence =
    
    /// Create a string representation of a recurrence
    let toString =
        function
        | Immediate -> "Immediate"
        | Hours   h -> $"{h} Hours"
        | Days    d -> $"{d} Days"
        | Weeks   w -> $"{w} Weeks"
    
    /// Create a recurrence value from a string
    let ofString =
        function
        | "Immediate" -> Immediate
        | it when it.Contains " " ->
            let parts = it.Split " "
            let length = Convert.ToInt16 parts[0]
            match parts[1] with
            | "Hours" -> Hours length
            | "Days"  -> Days  length
            | "Weeks" -> Weeks length
            | _       -> invalidOp $"{parts[1]} is not a valid recurrence"
        | it -> invalidOp $"{it} is not a valid recurrence"
    
    /// An hour's worth of seconds
    let private oneHour = 3_600L
    
    /// The duration of the recurrence (in milliseconds)
    let duration =
        function
        | Immediate -> 0L
        | Hours   h -> int64 h * oneHour
        | Days    d -> int64 d * oneHour * 24L
        | Weeks   w -> int64 w * oneHour * 24L * 7L


/// The action taken on a request as part of a history entry
type RequestAction =
    | Created
    | Prayed
    | Updated
    | Answered

/// Functions to manipulate request actions
module RequestAction =
  
    /// Create a string representation of an action
    let toString =
        function
        | Created  -> "Created"
        | Prayed   -> "Prayed"
        | Updated  -> "Updated"
        | Answered -> "Answered"
    
    /// Create a RequestAction from a string
    let ofString =
        function
        | "Created"  -> Created
        | "Prayed"   -> Prayed
        | "Updated"  -> Updated
        | "Answered" -> Answered
        | it         -> invalidOp $"Bad request action {it}"
    


/// History is a record of action taken on a prayer request, including updates to its text
[<CLIMutable; NoComparison; NoEquality>]
type History =
    {   /// The time when this history entry was made
        AsOf : Instant
        
        /// The status for this history entry
        Status : RequestAction
        
        /// The text of the update, if applicable
        Text : string option
    }

/// Functions to manipulate history entries
module History =
  
    /// Determine if a history's status is `Created`
    let isCreated hist = hist.Status = Created
    
    /// Determine if a history's status is `Prayed`
    let isPrayed hist = hist.Status = Prayed
    
    /// Determine if a history's status is `Answered`
    let isAnswered hist = hist.Status = Answered


/// Note is a note regarding a prayer request that does not result in an update to its text
[<CLIMutable; NoComparison; NoEquality>]
type Note =
    {   /// The time when this note was made
        AsOf : Instant
        
        /// The text of the notes
        Notes : string
    }


/// Request is the identifying record for a prayer request
[<CLIMutable; NoComparison; NoEquality>]
type Request =
    {   /// The ID of the request
        Id : RequestId
        
        /// The time this request was initially entered
        EnteredOn : Instant
        
        /// The ID of the user to whom this request belongs ("sub" from the JWT)
        UserId : UserId
        
        /// The time at which this request should reappear in the user's journal by manual user choice
        SnoozedUntil : Instant option
        
        /// The time at which this request should reappear in the user's journal by recurrence
        ShowAfter : Instant option
        
        /// The recurrence for this request
        Recurrence : Recurrence
        
        /// The history entries for this request
        History : History list
        
        /// The notes for this request
        Notes : Note list
    }

/// Functions to support requests
module Request =
    
    /// An empty request
    let empty =
        {   Id           = Cuid.generate () |> RequestId
            EnteredOn    = Instant.MinValue
            UserId       = UserId ""
            SnoozedUntil = None
            ShowAfter    = None
            Recurrence   = Immediate
            History      = []
            Notes        = []
        }


/// JournalRequest is the form of a prayer request returned for the request journal display. It also contains
/// properties that may be filled for history and notes.
[<NoComparison; NoEquality>]
type JournalRequest =
    {   /// The ID of the request (just the CUID part)
        RequestId : RequestId
        
        /// The ID of the user to whom the request belongs
        UserId : UserId
        
        /// The current text of the request
        Text : string
        
        /// The last time action was taken on the request
        AsOf : Instant
        
        /// The last time a request was marked as prayed
        LastPrayed : Instant option
        
        /// The last status for the request
        LastStatus : RequestAction
        
        /// The time that this request should reappear in the user's journal
        SnoozedUntil : Instant option
        
        /// The time after which this request should reappear in the user's journal by configured recurrence
        ShowAfter : Instant option
        
        /// The recurrence for this request
        Recurrence : Recurrence
        
        /// History entries for the request
        History : History list
        
        /// Note entries for the request
        Notes : Note list
    }

/// Functions to manipulate journal requests
module JournalRequest =

    /// Convert a request to the form used for the journal (precomputed values, no notes or history)
    let ofRequestLite (req : Request) =
        let history = Seq.ofList req.History
        let lastHistory = Seq.tryHead history
        // Requests are sorted by the "as of" field in this record; for sorting to work properly, we will put the
        // largest of the last prayed date, the "snoozed until". or the "show after" date; if none of those are filled,
        // we will use the last activity date. This will mean that:
        //  - Immediately shown requests will be at the top of the list, in order from least recently prayed to most.
        //  - Non-immediate requests will enter the list as if they were marked as prayed at that time; this will put
        //    them at the bottom of the list.
        //  - Snoozed requests will reappear at the bottom of the list when they return.
        //  - New requests will go to the bottom of the list, but will rise as others are marked as prayed.
        let lastActivity = lastHistory |> Option.map (fun it -> it.AsOf) |> Option.defaultValue Instant.MinValue
        let showAfter    = defaultArg req.ShowAfter    Instant.MinValue
        let snoozedUntil = defaultArg req.SnoozedUntil Instant.MinValue
        let lastPrayed   =
            history
            |> Seq.filter History.isPrayed
            |> Seq.tryHead
            |> Option.map (fun it -> it.AsOf)
            |> Option.defaultValue Instant.MinValue
        let asOf = List.max [ lastPrayed; showAfter; snoozedUntil ]
        {   RequestId    = req.Id
            UserId       = req.UserId
            Text         = history
                           |> Seq.filter (fun it -> Option.isSome it.Text)
                           |> Seq.tryHead
                           |> Option.map (fun h -> Option.get h.Text)
                           |> Option.defaultValue ""
            AsOf         = if asOf > Instant.MinValue then asOf else lastActivity
            LastPrayed   = if lastPrayed = Instant.MinValue then None else Some lastPrayed
            LastStatus   = match lastHistory with Some h -> h.Status | None -> Created
            SnoozedUntil = req.SnoozedUntil
            ShowAfter    = req.ShowAfter
            Recurrence   = req.Recurrence
            History      = []
            Notes        = []
        }

    /// Same as `ofRequestLite`, but with notes and history
    let ofRequestFull req =
        { ofRequestLite req with 
            History = req.History
            Notes   = req.Notes
        }
