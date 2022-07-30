/// The data model for myPrayerJournal
[<AutoOpen>]
module MyPrayerJournal.Domain

// fsharplint:disable RecordFieldNames

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


/// History is a record of action taken on a prayer request, including updates to its text
[<CLIMutable; NoComparison; NoEquality>]
type History =
    {   /// The time when this history entry was made
        asOf   : Instant
        
        /// The status for this history entry
        status : RequestAction
        
        /// The text of the update, if applicable
        text   : string option
    }


/// Note is a note regarding a prayer request that does not result in an update to its text
[<CLIMutable; NoComparison; NoEquality>]
type Note =
    {   /// The time when this note was made
        asOf  : Instant
        
        /// The text of the notes
        notes : string
    }


/// Request is the identifying record for a prayer request
[<CLIMutable; NoComparison; NoEquality>]
type Request =
    {   /// The ID of the request
        id           : RequestId
        
        /// The time this request was initially entered
        enteredOn    : Instant
        
        /// The ID of the user to whom this request belongs ("sub" from the JWT)
        userId       : UserId
        
        /// The time at which this request should reappear in the user's journal by manual user choice
        snoozedUntil : Instant
        
        /// The time at which this request should reappear in the user's journal by recurrence
        showAfter    : Instant
        
        /// The recurrence for this request
        recurrence   : Recurrence
        
        /// The history entries for this request
        history      : History list
        
        /// The notes for this request
        notes        : Note list
    }

/// Functions to support requests
module Request =
    
    /// An empty request
    let empty =
        {   id           = Cuid.generate () |> RequestId
            enteredOn    = Instant.MinValue
            userId       = UserId ""
            snoozedUntil = Instant.MinValue
            showAfter    = Instant.MinValue
            recurrence   = Immediate
            history      = []
            notes        = []
        }


/// JournalRequest is the form of a prayer request returned for the request journal display. It also contains
/// properties that may be filled for history and notes.
[<NoComparison; NoEquality>]
type JournalRequest =
    {   /// The ID of the request (just the CUID part)
        requestId    : RequestId
        
        /// The ID of the user to whom the request belongs
        userId       : UserId
        
        /// The current text of the request
        text         : string
        
        /// The last time action was taken on the request
        asOf         : Instant
        
        /// The last status for the request
        lastStatus   : RequestAction
        
        /// The time that this request should reappear in the user's journal
        snoozedUntil : Instant
        
        /// The time after which this request should reappear in the user's journal by configured recurrence
        showAfter    : Instant
        
        /// The recurrence for this request
        recurrence   : Recurrence
        
        /// History entries for the request
        history      : History list
        
        /// Note entries for the request
        notes        : Note list
    }

/// Functions to manipulate journal requests
module JournalRequest =

    /// Convert a request to the form used for the journal (precomputed values, no notes or history)
    let ofRequestLite (req : Request) =
        let hist = req.history |> List.sortByDescending (fun it -> it.asOf) |> List.tryHead
        {   requestId    = req.id
            userId       = req.userId
            text         = req.history
                           |> List.filter (fun it -> Option.isSome it.text)
                           |> List.sortByDescending (fun it -> it.asOf)
                           |> List.tryHead
                           |> Option.map (fun h -> Option.get h.text)
                           |> Option.defaultValue ""
            asOf         = match hist with Some h -> h.asOf   | None -> Instant.MinValue
            lastStatus   = match hist with Some h -> h.status | None -> Created
            snoozedUntil = req.snoozedUntil
            showAfter    = req.showAfter
            recurrence   = req.recurrence
            history      = []
            notes        = []
          }

    /// Same as `ofRequestLite`, but with notes and history
    let ofRequestFull req =
        { ofRequestLite req with 
            history = req.history
            notes   = req.notes
        }


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
    
    /// Determine if a history's status is `Created`
    let isCreated hist = hist.status = Created
    
    /// Determine if a history's status is `Prayed`
    let isPrayed hist = hist.status = Prayed
    
    /// Determine if a history's status is `Answered`
    let isAnswered hist = hist.status = Answered
