'use strict'

import { Pool, types } from 'pg'

import appConfig from '../appsettings.json'
import ddl from './ddl'
import request from './request'

/** Pooled PostgreSQL instance */
const pool = new Pool(appConfig.pgPool)

// Return "bigint" (int8) instances as number instead of strings
// ref: https://github.com/brianc/node-pg-types
types.setTypeParser(20, val => parseInt(val))

/**
 * Run a SQL query
 * @param {string} text The SQL command
 * @param {*[]} params The parameters for the query
 */
const query = (text, params) => pool.query(text, params)

export default {
  query: query,
  request: request(pool),
  verify: ddl(query).ensureDatabase
}
