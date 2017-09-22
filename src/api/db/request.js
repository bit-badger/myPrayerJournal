'use strict'

import { Pool } from 'pg'
import cuid from 'cuid'

export default function (pool) {
  return {
    /**
     * Get the current requests for a user (i.e., their complete current journal)
     * @param {string} userId The Id of the user
     * @return The requests that make up the current journal
     */
    journal: async userId =>
      (await pool.query({
        name: 'journal',
        text: `
          SELECT
            request."requestId",
            (SELECT "text"
               FROM mpj.history
              WHERE history."requestId" = request."requestId"
                AND "text" IS NOT NULL
              ORDER BY "asOf" DESC
              LIMIT 1) AS "text",
            (SELECT "asOf"
               FROM mpj.history
              WHERE history."requestId" = request."requestId"
              ORDER BY "asOf" DESC
              LIMIT 1) AS "asOf"
            FROM mpj.request
           WHERE "userId" = $1
           GROUP BY request."requestId"
           ORDER BY "asOf" DESC`
        }, [userId])).rows,
    
    /**
     * Add a new prayer request
     * @param {string} userId The Id of the user
     * @param {string} requestText The text of the request
     * @return The created request
     */
    addNew: async (userId, requestText) => {
      const id = cuid()
      const enteredOn = Date.now()
      return (async () => {
        const client = await pool.connect()
        try {
          await client.query('BEGIN')
          await client.query(
            'INSERT INTO mpj.request ("requestId", "enteredOn", "userId") VALUES ($1, $2, $3)',
            [ id, enteredOn, userId ])
          await client.query(
            `INSERT INTO mpj.history ("requestId", "asOf", "status", "text") VALUES ($1, $2, 'Created', $3)`,
            [ id, enteredOn, requestText ])
          await client.query('COMMIT')
        } catch (e) {
          await client.query('ROLLBACK')
          throw e
        } finally {
          client.release()
        }
        return { requestId: id, text: requestText, asOf: enteredOn }
      })().catch(e => {
        console.error(e.stack)
        return { requestId: '', text: 'error', asOf: 0 }
      })
    }
  }
}
