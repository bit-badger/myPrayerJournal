import axios from 'axios'

const http = axios.create({
  baseURL: `${location.protocol}//${location.host}/api/`
})

/**
 * API access for myPrayerJournal
 */
export default {

  /**
   * Set the bearer token for all future requests
   * @param {string} token The token to use to identify the user to the server
   */
  setBearer: token => { http.defaults.headers.common['authorization'] = `Bearer ${token}` },

  /**
   * Remove the bearer token
   */
  removeBearer: () => delete http.defaults.headers.common['authorization'],

  /**
   * Add a note for a prayer request
   * @param {string} requestId The Id of the request to which the note applies
   * @param {string} notes The notes to be added
   */
  addNote: (requestId, notes) => http.post(`request/${requestId}/note`, { notes }),

  /**
   * Add a new prayer request
   * @param {string} requestText The text of the request to be added
   */
  addRequest: requestText => http.post('request/', { requestText }),

  /**
   * Get all answered requests, along with the text they had when it was answered
   */
  getAnsweredRequests: () => http.get('request/answered'),

  /**
   * Get a prayer request (full; includes all history)
   * @param {string} requestId The Id of the request to retrieve
   */
  getFullRequest: requestId => http.get(`request/${requestId}/full`),

  /**
   * Get past notes for a prayer request
   * @param {string} requestId The Id of the request for which notes should be retrieved
   */
  getNotes: requestId => http.get(`request/${requestId}/notes`),

  /**
   * Get a prayer request (journal-style; only latest update)
   * @param {string} requestId The Id of the request to retrieve
   */
  getRequest: requestId => http.get(`request/${requestId}`),

  /**
   * Get a complete request; equivalent of "full" and "notes" combined
   */
  getRequestComplete: requestId => http.get(`request/${requestId}/complete`),

  /**
   * Get all prayer requests and their most recent updates
   */
  journal: () => http.get('journal/'),

  /**
   * Update a prayer request
   * @param request The request (should have requestId, status, and updateText properties)
   */
  updateRequest: request => http.post(`request/${request.requestId}/history`, {
    status: request.status,
    updateText: request.updateText
  })

}
