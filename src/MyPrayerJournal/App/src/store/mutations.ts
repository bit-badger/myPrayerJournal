/** Mutation for when the user's prayer journal is being loaded */
export const LoadingJournal = "loading-journal"

/** Mutation for when the user's prayer journal has been loaded */
export const LoadedJournal = "journal-loaded"

/** Mutation for adding a new prayer request (pass text) */
export const RequestAdded = "request-added"

/** Mutation to replace a prayer request at the top of the current journal */
export const RequestUpdated = "request-updated"

/** Mutation for setting the authentication state */
export const SetAuthentication = "set-authentication"

/** Mutation for setting the page title */
export const SetTitle = "set-title"

/** Mutation for logging a user off */
export const UserLoggedOff = "user-logged-off"

/** Mutation for logging a user on (pass user) */
export const UserLoggedOn = "user-logged-on"
