'use strict'

export default {
  /** Mutation for when the user's prayer journal is being loaded */
  LOADING_JOURNAL: 'loading-journal',
  /** Mutation for when the user's prayer journal has been loaded */
  LOADED_JOURNAL: 'journal-loaded',
  /** Mutation for adding a new prayer request (pass text) */
  REQUEST_ADDED: 'request-added',
  /** Mutation for logging a user off */
  USER_LOGGED_OFF: 'user-logged-off',
  /** Mutation for logging a user on (pass user) */
  USER_LOGGED_ON: 'user-logged-on'
}
