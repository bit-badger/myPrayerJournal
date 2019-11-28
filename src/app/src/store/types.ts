import { ProgressProps } from '@/types'

/** A history entry for a prayer request */
export class HistoryEntry {
  /** The status for this history entry */
  status = '' // TODO string union?

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
  notes = ''
}

/** A prayer request that is part of the user's journal */
export class JournalRequest {
  /** The ID of the request (just the CUID part) */
  requestId = ''

  /** The ID of the user to whom the request belongs */
  userId = ''

  /** The current text of the request */
  text = ''

  /** The last time action was taken on the request */
  asOf = 0

  /** The last status for the request */
  lastStatus = '' // TODO string union?

  /** The time that this request should reappear in the user's journal */
  snoozedUntil = 0

  /** The time after which this request should reappear in the user's journal by configured recurrence */
  showAfter = 0

  /** The type of recurrence for this request */
  recurType = '' // TODO Recurrence union?

  /** How many of the recurrence intervals should occur between appearances in the journal */
  recurCount = 0

  /** History entries for the request */
  history: HistoryEntry[] = []

  /** Note entries for the request */
  notes: NotesEntry[] = []
}

/** The state of myPrayerJournal */
export interface AppState {
  /** The user's profile */
  user: any,

  /** Whether there is a user signed in */
  isAuthenticated: boolean,

  /** The current set of prayer requests */
  journal: JournalRequest[],

  /** Whether the journal is currently being loaded */
  isLoadingJournal: boolean
}

const actions = {
  /** Action to add a prayer request (pass request text) */
  AddRequest: 'add-request',

  /** Action to check if a user is authenticated, refreshing the session first if it exists */
  CheckAuthentication: 'check-authentication',

  /** Action to load the user's prayer journal */
  LoadJournal: 'load-journal',

  /** Action to update a request */
  UpdateRequest: 'update-request',

  /** Action to skip the remaining recurrence period */
  ShowRequestNow: 'show-request-now',

  /** Action to snooze a request */
  SnoozeRequest: 'snooze-request'
}
export { actions as Actions }

const mutations = {
  /** Mutation for when the user's prayer journal is being loaded */
  LoadingJournal: 'loading-journal',

  /** Mutation for when the user's prayer journal has been loaded */
  LoadedJournal: 'journal-loaded',

  /** Mutation for adding a new prayer request (pass text) */
  RequestAdded: 'request-added',

  /** Mutation to replace a prayer request at the top of the current journal */
  RequestUpdated: 'request-updated',

  /** Mutation for setting the authentication state */
  SetAuthentication: 'set-authentication',

  /** Mutation for logging a user off */
  UserLoggedOff: 'user-logged-off',

  /** Mutation for logging a user on (pass user) */
  UserLoggedOn: 'user-logged-on'
}
export { mutations as Mutations }

/** The shape of the parameter to the add request action */
export interface AddRequestAction {
  /** The progress bar component properties */
  progress: ProgressProps
  /** The text of the request */
  requestText: string
  /** The recurrence type */
  recurType: string
  /** The number of intervals for non-immediate recurrence */
  recurCount: number
}

/** The shape of the parameter to the show request action */
export interface ShowRequestAction {
  /** The progress bar component properties */
  progress: ProgressProps
  /** The ID of the prayer request being shown */
  requestId: string
  /** The date/time after which the request will be once again shown */
  showAfter: number
}

/** The shape of the parameter to the snooze request action */
export interface SnoozeRequestAction {
  /** The progress bar component properties */
  progress: ProgressProps
  /** The ID of the prayer request being snoozed/unsnoozed */
  requestId: string
  /** The date/time after which the request will be once again shown */
  until: number
}

/** The shape of the parameter to the update request action */
export interface UpdateRequestAction {
  /** The progress bar component properties */
  progress: ProgressProps
  /** The ID of the prayer request */
  requestId: string
  /** The text of the update */
  updateText: string
  /** The status associated with the update */
  status: string
  /** The type of recurrence for the request */
  recurType: string
  /** The number of intervals for non-immediate recurrence */
  recurCount: number
}
