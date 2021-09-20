/** A history entry for a prayer request */
export class HistoryEntry {
  /** The status for this history entry */
  status = "" // TODO string union?
  /** The date/time for this history entry */
  asOf = 0
  /** The text of this history entry */
  text?: string = undefined
}

/** An entry with notes for a request */
export class NotesEntry {
  /** The date/time the notes were recorded */
  asOf = 0
  /** The notes */
  notes = ""
}

/** A prayer request that is part of the user's journal */
export class JournalRequest {
  /** The ID of the request (just the CUID part) */
  requestId = ""
  /** The ID of the user to whom the request belongs */
  userId = ""
  /** The current text of the request */
  text = ""
  /** The last time action was taken on the request */
  asOf = 0
  /** The last status for the request */
  lastStatus = "" // TODO string union?
  /** The time that this request should reappear in the user's journal */
  snoozedUntil = 0
  /** The time after which this request should reappear in the user's journal by configured recurrence */
  showAfter = 0
  /** The type of recurrence for this request */
  recurType = "" // TODO Recurrence union?
  /** How many of the recurrence intervals should occur between appearances in the journal */
  recurCount = 0
  /** History entries for the request */
  history: HistoryEntry[] = []
  /** Note entries for the request */
  notes: NotesEntry[] = []
}
