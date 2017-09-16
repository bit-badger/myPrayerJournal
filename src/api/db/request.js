'use strict'

const { Pool } = require('pg')

module.exports = query => {
  return {
    /**
     * Get the current requests for a user (i.e., their complete current journal)
     * @param {string} userId The Id of the user
     * @return The requests that make up the current journal
     */
    journal: async userId =>
      (await query('SELECT "requestId" FROM request WHERE "userId" = $1', [userId])).rows
  }
}
