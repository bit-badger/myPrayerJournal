[<AutoOpen>]
/// Entities for use in the data model for myPrayerJournal
module MyPrayerJournal.Entities

open System.Collections.Generic

/// Type alias for a Collision-resistant Unique IDentifier
type Cuid = string

/// Request ID is a CUID
type RequestId = Cuid

/// User ID is a string (the "sub" part of the JWT)
type UserId = string

/// History is a record of action taken on a prayer request, including updates to its text
type [<CLIMutable; NoComparison; NoEquality>] History =
  { /// The ID of the request to which this history entry applies
    requestId : RequestId
    /// The time when this history entry was made
    asOf      : int64
    /// The status for this history entry
    status    : string
    /// The text of the update, if applicable
    text      : string option
    }
with
  /// An empty history entry
  static member empty =
    { requestId = ""
      asOf      = 0L
      status    = ""
      text      = None
      }

/// Note is a note regarding a prayer request that does not result in an update to its text
and [<CLIMutable; NoComparison; NoEquality>] Note =
  { /// The ID of the request to which this note applies
    requestId : RequestId
    /// The time when this note was made
    asOf      : int64
    /// The text of the notes
    notes     : string
    }
with
  /// An empty note
  static member empty =
    { requestId = ""
      asOf      = 0L
      notes     = ""
      }

/// Request is the identifying record for a prayer request
and [<CLIMutable; NoComparison; NoEquality>] Request =
  { /// The ID of the request
    requestId    : RequestId
    /// The time this request was initially entered
    enteredOn    : int64
    /// The ID of the user to whom this request belongs ("sub" from the JWT)
    userId       : string
    /// The time at which this request should reappear in the user's journal by manual user choice
    snoozedUntil : int64
    /// The time at which this request should reappear in the user's journal by recurrence
    showAfter    : int64
    /// The type of recurrence for this request
    recurType    : string
    /// How many of the recurrence intervals should occur between appearances in the journal
    recurCount   : int16
    /// The history entries for this request
    history      : ICollection<History>
    /// The notes for this request
    notes        : ICollection<Note>
    }
with
  /// An empty request
  static member empty =
    { requestId    = ""
      enteredOn    = 0L
      userId       = ""
      snoozedUntil = 0L
      showAfter    = 0L
      recurType    = "immediate"
      recurCount   = 0s
      history      = List<History> ()
      notes        = List<Note> ()
      }

/// JournalRequest is the form of a prayer request returned for the request journal display. It also contains
/// properties that may be filled for history and notes
[<CLIMutable; NoComparison; NoEquality>]
type JournalRequest =
  { /// The ID of the request
    requestId    : RequestId
    /// The ID of the user to whom the request belongs
    userId       : string
    /// The current text of the request
    text         : string
    /// The last time action was taken on the request
    asOf         : int64
    /// The last status for the request
    lastStatus   : string
    /// The time that this request should reappear in the user's journal
    snoozedUntil : int64
    /// The time after which this request should reappear in the user's journal by configured recurrence
    showAfter    : int64
    /// The type of recurrence for this request
    recurType    : string
    /// How many of the recurrence intervals should occur between appearances in the journal
    recurCount   : int16
    /// History entries for the request
    history      : History list
    /// Note entries for the request
    notes        : Note list
    }
