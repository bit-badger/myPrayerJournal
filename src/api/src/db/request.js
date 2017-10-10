'use strict'

import { Pool } from 'pg'
import cuid from 'cuid'

const currentRequestSql = `
  SELECT "requestId", "text", "asOf", "lastStatus"
    FROM mpj.journal`

const journalSql = `${currentRequestSql}
  WHERE "userId" = $1
    AND "lastStatus" <> 'Answered'`

const requestNotFound = {
  requestId: '',
  text: 'Not Found',
  asOf: 0
}

export default function (pool) {
  
  /**
   * Retrieve basic information about a single request
   * @param {string} requestId The Id of the request to retrieve
   * @param {string} userId The Id of the user to whom the request belongs
   */
  let retrieveRequest = (requestId, userId) =>
    pool.query(`
      SELECT "requestId", "enteredOn"
        FROM mpj.request
      WHERE "requestId" = $1
        AND "userId" = $2`,
      [ requestId, userId ])
  
  return {
    /**
     * Add a history entry for this request
     * @param {string} userId The Id of the user to whom this request belongs
     * @param {string} requestId The Id of the request to which the update applies
     * @param {string} status The status for this history entry
     * @param {string} updateText The updated text for the request (pass blank if no update)
     * @return {number} 404 if the request is not found or does not belong to the given user, 204 if successful
     */
    addHistory: async (userId, requestId, status, updateText) => {
      const req = retrieveRequest(requestId, userId)
      if (req.rowCount === 0) {
        return 404
      }
      await pool.query(`
        INSERT INTO mpj.history
          ("requestId", "asOf", "status", "text")
        VALUES
          ($1, $2, $3, NULLIF($4, ''))`,
        [ requestId, Date.now(), status, updateText ])
      return 204
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
        return { requestId: id, text: requestText, asOf: enteredOn, lastStatus: 'Created' }
      })().catch(e => {
        console.error(e.stack)
        return { requestId: '', text: 'error', asOf: 0, lastStatus: 'Errored' }
      })
    },

    /**
     * Add a note about a prayer request
     * @param {string} userId The Id of the user to whom the request belongs
     * @param {string} requestId The Id of the request to which the note applies
     * @param {string} note The notes to add
     * @return {number} 404 if the request is not found or does not belong to the given user, 204 if successful
     */
    addNote: async (userId, requestId, note) => {
      const req = retrieveRequest(requestId, userId)
      if (req.rowCount === 0) {
        return 404
      }
      await pool.query(`
        INSERT INTO mpj.note
          ("requestId", "asOf", "notes")
        VALUES
          ($1, $2, $3)`,
        [ requestId, Date.now(), note ])
      return 204
    },

    /**
     * Get all answered requests with their text as of the "Answered" status
     * @param {string} userId The Id of the user for whom requests should be retrieved
     * @return All requests
     */
    answered: async (userId) => 
      (await pool.query(`${currentRequestSql}
        WHERE "userId" = $1
          AND "lastStatus" = 'Answered'
        ORDER BY "asOf" DESC`,
        [ userId ])).rows,

    /**
     * Get the "current" version of a request by its Id
     * @param {string} requestId The Id of the request to retrieve
     * @param {string} userId The Id of the user to which the request belongs
     * @return The request, or a request-like object indicating that the request was not found
     */
    byId: async (userId, requestId) => {
      const reqs = await pool.query(`${currentRequestSql}
        WHERE "requestId" = $1
          AND "userId" = $2`,
        [ requestId, userId ])
      return (0 < reqs.rowCount) ? reqs.rows[0] : requestNotFound
    },

    /**
     * Get a prayer request, including its full history, by its Id
     * @param {string} userId The Id of the user to which the request belongs
     * @param {string} requestId The Id of the request to retrieve
     * @return The request, or a request-like object indicating that the request was not found
     */
    fullById: async (userId, requestId) => {
      const reqResults = await retrieveRequest(requestId, userId)
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
     * Get the current requests for a user (i.e., their complete current journal)
     * @param {string} userId The Id of the user
     * @return The requests that make up the current journal
     */
    journal: async userId => (await pool.query(`${journalSql} ORDER BY "asOf"`, [ userId ])).rows,

    /**
     * Get the notes for a request, most recent first
     * @param {string} userId The Id of the user to whom the request belongs
     * @param {string} requestId The Id of the request whose notes should be retrieved
     * @return The notes for the request
     */
    notesById: async (userId, requestId) => {
      const reqResults = await retrieveRequest(requestId, userId)
      if (0 === reqResults.rowCount) {
        return requestNotFound
      }
      const notes = await pool.query(`
        SELECT "asOf", "notes"
          FROM mpj.note
         WHERE "requestId" = $1
         ORDER BY "asOf" DESC`,
        [ requestId ])
      return notes.rows
    }
    
  }
}
