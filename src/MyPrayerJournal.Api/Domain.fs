[<AutoOpen>]
/// The data model for myPrayerJournal
module MyPrayerJournal.Domain

/// A Collision-resistant Unique IDentifier
type Cuid =
  | Cuid of string
module Cuid =
  /// The string value of the CUID
  let toString x = match x with Cuid y -> y


/// Request ID is a CUID
type RequestId =
  | RequestId of Cuid
module RequestId =
  /// The string representation of the request ID
  let toString x = match x with RequestId y -> (Cuid.toString >> sprintf "Requests/%s") y
  /// Create a request ID from a string representation
  let fromIdString (y : string) = (Cuid >> RequestId) <| y.Replace("Requests/", "")


/// User ID is a string (the "sub" part of the JWT)
type UserId =
  | UserId of string
module UserId =
  /// The string representation of the user ID
  let toString x = match x with UserId y -> y


/// A long integer representing seconds since the epoch
type Ticks =
  | Ticks of int64
module Ticks =
  /// The int64 (long) representation of ticks
  let toLong x = match x with Ticks y -> y


/// How frequently a request should reappear after it is marked "Prayed"
type Recurrence =
  | Immediate
  | Hours
  | Days
  | Weeks
module Recurrence =
  /// The string reprsentation used in the database and the web app
  let toString x =
    match x with
    | Immediate -> "immediate"
    | Hours -> "hours"
    | Days -> "days"
    | Weeks -> "weeks"
  /// Create a recurrence value from a string
  let fromString x =
    match x with
    | "immediate" -> Immediate
    | "hours" -> Hours
    | "days" -> Days
    | "weeks" -> Weeks
    | _ -> invalidOp (sprintf "%s is not a valid recurrence" x)
  /// The duration of the recurrence
  let duration x =
    match x with
    | Immediate ->     0L
    | Hours ->   3600000L
    | Days ->   86400000L
    | Weeks -> 604800000L


/// History is a record of action taken on a prayer request, including updates to its text
[<CLIMutable; NoComparison; NoEquality>]
type History =
  { /// The time when this history entry was made
    asOf   : Ticks
    /// The status for this history entry
    status : string
    /// The text of the update, if applicable
    text   : string option
    }
with
  /// An empty history entry
  static member empty =
    { asOf   = Ticks 0L
      status = ""
      text   = None
      }

/// Note is a note regarding a prayer request that does not result in an update to its text
[<CLIMutable; NoComparison; NoEquality>]
type Note =
  { /// The time when this note was made
    asOf  : Ticks
    /// The text of the notes
    notes : string
    }
with
  /// An empty note
  static member empty =
    { asOf  = Ticks 0L
      notes = ""
      }

/// Request is the identifying record for a prayer request
[<CLIMutable; NoComparison; NoEquality>]
type Request =
  { /// The ID of the request
    Id           : string
    /// The time this request was initially entered
    enteredOn    : Ticks
    /// The ID of the user to whom this request belongs ("sub" from the JWT)
    userId       : UserId
    /// The time at which this request should reappear in the user's journal by manual user choice
    snoozedUntil : Ticks
    /// The time at which this request should reappear in the user's journal by recurrence
    showAfter    : Ticks
    /// The type of recurrence for this request
    recurType    : Recurrence
    /// How many of the recurrence intervals should occur between appearances in the journal
    recurCount   : int16
    /// The history entries for this request
    history      : History list
    /// The notes for this request
    notes        : Note list
    }
with
  /// An empty request
  static member empty =
    { Id           = ""
      enteredOn    = Ticks 0L
      userId       = UserId ""
      snoozedUntil = Ticks 0L
      showAfter    = Ticks 0L
      recurType    = Immediate
      recurCount   = 0s
      history      = []
      notes        = []
      }

/// JournalRequest is the form of a prayer request returned for the request journal display. It also contains
/// properties that may be filled for history and notes
[<CLIMutable; NoComparison; NoEquality>]
type JournalRequest =
  { /// The ID of the request
    requestId    : RequestId
    /// The ID of the user to whom the request belongs
    userId       : UserId
    /// The current text of the request
    text         : string
    /// The last time action was taken on the request
    asOf         : Ticks
    /// The last status for the request
    lastStatus   : string
    /// The time that this request should reappear in the user's journal
    snoozedUntil : Ticks
    /// The time after which this request should reappear in the user's journal by configured recurrence
    showAfter    : Ticks
    /// The type of recurrence for this request
    recurType    : Recurrence
    /// How many of the recurrence intervals should occur between appearances in the journal
    recurCount   : int16
    /// History entries for the request
    history      : History list
    /// Note entries for the request
    notes        : Note list
    }
