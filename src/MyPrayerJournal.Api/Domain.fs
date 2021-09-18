[<AutoOpen>]
/// The data model for myPrayerJournal
module MyPrayerJournal.Domain

// fsharplint:disable RecordFieldNames

open Cuid

/// Request ID is a CUID
type RequestId =
  | RequestId of Cuid
module RequestId =
  /// The string representation of the request ID
  let toString = function RequestId x -> $"Requests/{Cuid.toString x}"
  /// Create a request ID from a string representation
  let fromIdString (x : string) = x.Replace ("Requests/", "") |> (Cuid >> RequestId)


/// User ID is a string (the "sub" part of the JWT)
type UserId =
  | UserId of string
module UserId =
  /// The string representation of the user ID
  let toString = function UserId x -> x


/// A long integer representing seconds since the epoch
type Ticks =
  | Ticks of int64
module Ticks =
  /// The int64 (long) representation of ticks
  let toLong = function Ticks x -> x


/// How frequently a request should reappear after it is marked "Prayed"
type Recurrence =
  | Immediate
  | Hours
  | Days
  | Weeks
module Recurrence =
  /// Create a recurrence value from a string
  let fromString =
    function
    | "Immediate" -> Immediate
    | "Hours" -> Hours
    | "Days" -> Days
    | "Weeks" -> Weeks
    | it -> invalidOp $"{it} is not a valid recurrence"
  /// The duration of the recurrence
  let duration =
    function
    | Immediate ->     0L
    | Hours ->   3600000L
    | Days ->   86400000L
    | Weeks -> 604800000L


/// The action taken on a request as part of a history entry
type RequestAction =
  | Created
  | Prayed
  | Updated
  | Answered
module RequestAction =
  /// Create a RequestAction from a string
  let fromString =
    function
    | "Created" -> Created
    | "Prayed" -> Prayed
    | "Updated" -> Updated
    | "Answered" -> Answered
    | it -> invalidOp $"Bad request action {it}"


/// History is a record of action taken on a prayer request, including updates to its text
[<CLIMutable; NoComparison; NoEquality>]
type History =
  { /// The time when this history entry was made
    asOf   : Ticks
    /// The status for this history entry
    status : RequestAction
    /// The text of the update, if applicable
    text   : string option
    }
with
  /// An empty history entry
  static member empty =
    { asOf   = Ticks 0L
      status = Created
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
/// properties that may be filled for history and notes.
// RavenDB doesn't like the "@"-suffixed properties from record types in a ProjectInto clause
[<NoComparison; NoEquality>]
type JournalRequest () =
  /// The ID of the request (just the CUID part)
  [<DefaultValue>] val mutable requestId : string
  /// The ID of the user to whom the request belongs
  [<DefaultValue>] val mutable userId : UserId
  /// The current text of the request
  [<DefaultValue>] val mutable text : string
  /// The last time action was taken on the request
  [<DefaultValue>] val mutable asOf : Ticks
  /// The last status for the request
  [<DefaultValue>] val mutable lastStatus : string
  /// The time that this request should reappear in the user's journal
  [<DefaultValue>] val mutable snoozedUntil : Ticks
  /// The time after which this request should reappear in the user's journal by configured recurrence
  [<DefaultValue>] val mutable showAfter : Ticks
  /// The type of recurrence for this request
  [<DefaultValue>] val mutable recurType : Recurrence
  /// How many of the recurrence intervals should occur between appearances in the journal
  [<DefaultValue>] val mutable recurCount : int16
  /// History entries for the request
  [<DefaultValue>] val mutable history : History list
  /// Note entries for the request
  [<DefaultValue>] val mutable notes : Note list
