'use strict'

import { Pool } from 'pg'
import cuid from 'cuid'

export default function (query) {
  return {
    /**
     * Get the current requests for a user (i.e., their complete current journal)
     * @param {string} userId The Id of the user
     * @return The requests that make up the current journal
     */
    journal: async userId =>
      (await query('SELECT "requestId" FROM request WHERE "userId" = $1', [userId])).rows,
    
    /**
     * Add a new prayer request
     * @param {string} userId The Id of the user
     * @param {string} requestText The text of the request
     * @return {string} The Id of the created request
     */
    addNew: async (userId, requestText) => {
      const id = cuid()
      const enteredOn = Date.now()
      await query(`
        BEGIN;
        INSERT INTO request ("requestId", "enteredOn", "userId") VALUES ($1, $2, $3);
        INSERT INTO history ("requestId", "asOf", "status", "text") VALUES ($1, $2, 'Created', $4);
        COMMIT;`,
        [ id, enteredOn, userId, requestText ])
      return id
    }
  }
}
