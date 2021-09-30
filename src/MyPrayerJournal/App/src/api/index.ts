import { JournalRequest, NotesEntry } from "./types"

/**
 * Create a URL that will access the API
 * @param url The partial URL for the API
 * @returns A full URL for the API
 */
const apiUrl = (url : string) : string => `/api/${url}`

/** The bearer token */
let bearer : string | undefined

/**
 * Create request init parameters
 *
 * @param method The method by which the request should be executed
 * @param user The currently logged-on user
 * @returns RequestInit parameters
 */
// eslint-disable-next-line
const reqInit = (method : string, body : any | undefined = undefined) : RequestInit => {
  const headers = new Headers()
  if (bearer) headers.append("Authorization", bearer)
  if (body) {
    headers.append("Content-Type", "application/json")
    return {
      headers,
      method,
      cache: "no-cache",
      body: JSON.stringify(body)
    }
  }
  return {
    headers,
    method
  }
}

/**
 * Retrieve a result for an API call
 *
 * @param resp The response received from the API
 * @param action The action being performed (used in error messages)
 * @returns The expected result (if found), undefined (if not found), or an error string
 */
async function apiResult<T> (resp : Response, action : string) : Promise<T | undefined | string> {
  if (resp.status === 200) return await resp.json() as T
  if (resp.status === 404) return undefined
  return `Error ${action} - ${await resp.text()}`
}

/**
 * Run an API action that does not return a result
 *
 * @param resp The response received from the API call
 * @param action The action being performed (used in error messages)
 * @returns Undefined (if successful), or an error string
 */
const apiAction = async (resp : Response, action : string) : Promise<string | undefined> => {
  if (resp.status === 200) return undefined
  return `Error ${action} - ${await resp.text()}`
}

/**
 * API access for myPrayerJournal
 */
export default {

  /**
   * Set the bearer token for all future requests
   * @param token The token to use to identify the user to the server
   */
  setBearer: (token: string) => {
    console.info(`Setting bearer token to ${token}`)
    bearer = `Bearer ${token}`
  },

  /**
   * Remove the bearer token
   */
  removeBearer: () => { bearer = undefined },

  notes: {
    /**
     * Add a note for a prayer request
     * @param requestId The Id of the request to which the note applies
     * @param notes The notes to be added
     */
    add: async (requestId : string, notes : string) =>
      apiAction(await fetch(apiUrl(`request/${requestId}/note`), reqInit("POST", { notes })), "adding note"),

    /**
     * Get past notes for a prayer request
     * @param requestId The Id of the request for which notes should be retrieved
     */
    getForRequest: async (requestId : string) =>
      apiResult<NotesEntry[]>(await fetch(apiUrl(`request/${requestId}/notes`), reqInit("GET")), "getting notes")
  },

  request: {
    /**
     * Add a new prayer request
     * @param requestText The text of the request to be added
     * @param recurType The type of recurrence for this request
     * @param recurCount The number of intervals of recurrence
     */
    add: async (requestText : string, recurType : string, recurCount : number) =>
      apiAction(await fetch(apiUrl("request"), reqInit("POST", { requestText, recurType, recurCount })),
        "adding prayer request"),

    /**
     * Get a prayer request (full; includes all history and notes)
     * @param requestId The Id of the request to retrieve
     */
    full: async (requestId : string) =>
      apiResult<JournalRequest>(await fetch(apiUrl(`request/${requestId}/full`), reqInit("GET")),
        "retrieving full request"),

    /**
     * Get a prayer request (journal-style; only latest update)
     * @param requestId The Id of the request to retrieve
     */
    get: async (requestId : string) =>
      apiResult<JournalRequest>(await fetch(apiUrl(`request/${requestId}`), reqInit("GET")), "retrieving request"),

    /**
     * Get all answered requests, along with the text they had when it was answered
     */
    getAnswered: async () =>
      apiResult<JournalRequest[]>(await fetch(apiUrl("requests/answered"), reqInit("GET")),
        "retrieving answered requests"),

    /**
     * Get all prayer requests and their most recent updates
     */
    journal: async () =>
      apiResult<JournalRequest[]>(await fetch(apiUrl('journal'), reqInit("GET")), "retrieving journal"),

    /**
     * Show a request after the given date (used for "show now")
     * @param requestId The ID of the request which should be shown
     * @param showAfter The ticks after which the request should be shown
     */
    show: async (requestId : string, showAfter : number) =>
      apiAction(await fetch(apiUrl(`request/${requestId}/show`), reqInit("PATCH", { showAfter })), "showing request"),

    /**
     * Snooze a request until the given time
     * @param requestId The ID of the prayer request to be snoozed
     * @param until The ticks until which the request should be snoozed
     */
    snooze: async (requestId : string, until : number) =>
      apiAction(await fetch(apiUrl(`request/${requestId}/snooze`), reqInit("PATCH", { until })), "snoozing request"),

    /**
     * Update a prayer request
     * @param requestId The ID of the request to be updated
     * @param status The status of the update
     * @param updateText The text of the update (optional)
     */
    update: async (requestId : string, status : string, updateText : string) =>
      apiAction(await fetch(apiUrl(`request/${requestId}/history`), reqInit("POST", { status, updateText })),
        "updating request"),

    /**
     * Update recurrence for a prayer request
     * @param requestId The ID of the prayer request for which recurrence is being updated
     * @param recurType The type of recurrence to set
     * @param recurCount The number of recurrence intervals to set
     */
    updateRecurrence: async (requestId : string, recurType : string, recurCount : number) =>
      apiAction(await fetch(apiUrl(`request/${requestId}/recurrence`), reqInit("PATCH", { recurType, recurCount })),
        "updating request recurrence")
  }
}

export * from './types'
