'use strict'

import { Pool } from 'pg'
import cuid from 'cuid'

const currentRequestSql = `
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
    FROM mpj.request`

const journalSql = `${currentRequestSql}
  WHERE "userId" = $1
  GROUP BY request."requestId"`

const requestNotFound = {
  requestId: '',
  text: 'Not Found',
  asOf: 0
}

export default function (pool) {
  return {
    /**
     * Get the current requests for a user (i.e., their complete current journal)
     * @param {string} userId The Id of the user
     * @return The requests that make up the current journal
     */
    journal: async userId => (await pool.query(`${journalSql} ORDER BY "asOf" DESC`, [ userId ])).rows,

    /**
     * Get the least-recently-updated prayer request for the given user
     * @param {string} userId The Id of the current user
     * @return The least-recently-updated request for the given user
     */
    oldest: async userId => (await pool.query(`${journalSql} ORDER BY "asOf" LIMIT 1`, [ userId ])).rows[0],
    
    /**
     * Get the "current" version of a request by its Id
     * @param {string} requestId The Id of the request to retrieve
     * @param {string} userId The Id of the user to which the request belongs
     * @return The request, or a request-like object indicating that the request was not found
     */
    byId: async (userId, requestId) => {
      const reqs = await pool.query(`${currentRequestSql}
        WHERE "requestId" = $1
          AND "userId" = $2
        GROUP BY request."requestId"`,
        [ requestId, userId ])
      return (0 < req.rowCount) ? reqs.rows[0] : requestNotFound
    },

    /**
     * Get a prayer request, including its full history, by its Id
     * @param {string} userId The Id of the user to which the request belongs
     * @param {string} requestId The Id of the request to retrieve
     * @return The request, or a request-like object indicating that the request was not found
     */
    fullById: async (userId, requestId) => {
      const reqResults = await pool.query(`
        SELECT "requestId", "enteredOn"
          FROM mpj.request
         WHERE "requestId" = $1
           AND "userId" = $2`,
        [ requestId, userId ])
      if (0 === reqResults.rowCount) {
        return requestNotFound
      }
      const req = reqResults.rows[0]
      const history = await pool.query(`
        SELECT "asOf", "status", COALESCE("text", '') AS "text"
          FROM mpj.history
         WHERE "requestId" = $1
         ORDER BY "asOf"`,
        [ requestId ])
      req.history = history.rows
      return req
    },

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
    },

    /**
     * Add a history entry for this request
     * @param {string} requestId The Id of the request
     * @param {string} status The status for this history entry
     * @param {string} updateText The updated text for the request (pass blank if no update)
     */
    addHistory: async (requestId, status, updateText) => {
      const asOf = Date.now()
      await pool.query(`
        INSERT INTO mpj.history ("requestId", "asOf", "status", "text") VALUES ($1, $2, $3, NULLIF($4, ''))`,
      [ requestId, asOf, status, updateText ])
    }

  }
}
