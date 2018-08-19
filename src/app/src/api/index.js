'use strict'

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
   * @param {string} recurType The type of recurrence for this request
   * @param {number} recurCount The number of intervals of recurrence
   */
  addRequest: (requestText, recurType, recurCount) => http.post('request', { requestText, recurType, recurCount }),

  /**
   * Get all answered requests, along with the text they had when it was answered
   */
  getAnsweredRequests: () => http.get('requests/answered'),

  /**
   * Get a prayer request (full; includes all history and notes)
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
   * Get all prayer requests and their most recent updates
   */
  journal: () => http.get('journal'),

  /**
   * Show a request after the given date (used for "show now")
   * @param {string} requestId The ID of the request which should be shown
   * @param {number} showAfter The ticks after which the request should be shown
   */
  showRequest: (requestId, showAfter) => http.patch(`request/${requestId}/show`, { showAfter }),

  /**
   * Snooze a request until the given time
   * @param {string} requestId The ID of the prayer request to be snoozed
   * @param {number} until The ticks until which the request should be snoozed
   */
  snoozeRequest: (requestId, until) => http.patch(`request/${requestId}/snooze`, { until }),

  /**
   * Update recurrence for a prayer request
   * @param {string} requestId The ID of the prayer request for which recurrence is being updated
   * @param {string} recurType The type of recurrence to set
   * @param {number} recurCount The number of recurrence intervals to set
   */
  updateRecurrence: (requestId, recurType, recurCount) =>
    http.patch(`request/${requestId}/recurrence`, { recurType, recurCount }),

  /**
   * Update a prayer request
   * @param {string} requestId The ID of the request to be updated
   * @param {string} status The status of the update
   * @param {string} updateText The text of the update (optional)
   */
  updateRequest: (requestId, status, updateText) => http.post(`request/${requestId}/history`, { status, updateText })
}
