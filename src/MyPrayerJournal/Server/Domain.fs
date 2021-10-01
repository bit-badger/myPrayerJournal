[<AutoOpen>]
/// The data model for myPrayerJournal
module MyPrayerJournal.Domain

// fsharplint:disable RecordFieldNames

open Cuid

/// An identifier for a request
type RequestId =
  | RequestId of Cuid

/// Functions to manipulate request IDs
module RequestId =
  /// The string representation of the request ID
  let toString = function RequestId x -> Cuid.toString x
  /// Create a request ID from a string representation
  let ofString = Cuid >> RequestId


/// The identifier of a user (the "sub" part of the JWT)
type UserId =
  | UserId of string

/// Functions to manipulate user IDs
module UserId =
  /// The string representation of the user ID
  let toString = function UserId x -> x


/// A long integer representing seconds since the epoch
type Ticks =
  | Ticks of int64

/// Functions to manipulate Ticks
module Ticks =
  /// The int64 (long) representation of ticks
  let toLong = function Ticks x -> x


/// How frequently a request should reappear after it is marked "Prayed"
type Recurrence =
  | Immediate
  | Hours
  | Days
  | Weeks

/// Functions to manipulate recurrences
module Recurrence =
  /// Create a string representation of a recurrence
  let toString =
    function
    | Immediate -> "Immediate"
    | Hours     -> "Hours"
    | Days      -> "Days"
    | Weeks     -> "Weeks"
  /// Create a recurrence value from a string
  let fromString =
    function
    | "Immediate" -> Immediate
    | "Hours"     -> Hours
    | "Days"      -> Days
    | "Weeks"     -> Weeks
    | it          -> invalidOp $"{it} is not a valid recurrence"
  /// An hour's worth of seconds
  let private oneHour = 3_600L
  /// The duration of the recurrence (in milliseconds)
  let duration x =
    (match x with
    | Immediate -> 0L
    | Hours     -> oneHour
    | Days      -> oneHour * 24L
    | Weeks     -> oneHour * 24L * 7L)
    |> ( * ) 1000L


/// The action taken on a request as part of a history entry
type RequestAction =
  | Created
  | Prayed
  | Updated
  | Answered


/// History is a record of action taken on a prayer request, including updates to its text
[<CLIMutable; NoComparison; NoEquality>]
type History = {
  /// The time when this history entry was made
  asOf   : Ticks
  /// The status for this history entry
  status : RequestAction
  /// The text of the update, if applicable
  text   : string option
  }

/// Note is a note regarding a prayer request that does not result in an update to its text
[<CLIMutable; NoComparison; NoEquality>]
type Note = {
  /// The time when this note was made
  asOf  : Ticks
  /// The text of the notes
  notes : string
  }

/// Request is the identifying record for a prayer request
[<CLIMutable; NoComparison; NoEquality>]
type Request = {
  /// The ID of the request
  id           : RequestId
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
    { id           = Cuid.generate () |> RequestId
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
/// properties that may be filled for history and notes.
[<NoComparison; NoEquality>]
type JournalRequest =
  { /// The ID of the request (just the CUID part)
    requestId    : RequestId
    /// The ID of the user to whom the request belongs
    userId       : UserId
    /// The current text of the request
    text         : string
    /// The last time action was taken on the request
    asOf         : Ticks
    /// The last status for the request
    lastStatus   : RequestAction
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
  let fromString =
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
