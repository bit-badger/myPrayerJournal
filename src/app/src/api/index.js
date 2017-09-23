import axios from 'axios'

const http = axios.create({
  baseURL: 'http://localhost:3000/api/'
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
   * Get all prayer requests and their most recent updates
   */
  journal: () => http.get('journal/'),

  /**
   * Add a new prayer request
   * @param {string} requestText The text of the request to be added
   */
  addRequest: requestText => http.post('request/', { requestText }),

  /**
   * Update a prayer request
   * @param request The request (should have requestId, status, and updateText properties)
   */
  updateRequest: request => http.post(`request/${request.requestId}/history`, {
    status: request.status,
    updateText: request.updateText
  }),

  /**
   * Get a prayer request (journal-style; only latest update)
   * @param {string} requestId The Id of the request to retrieve
   */
  getRequest: requestId => http.get(`request/${requestId}`),

  /**
   * Get a prayer request (full; includes all history)
   * @param {string} requestId The Id of the request to retrieve
   */
  getFullRequest: requestId => http.get(`request/${requestId}/full`)

}
