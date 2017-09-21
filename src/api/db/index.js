'use strict'

import { Pool } from 'pg'

import appConfig from '../appsettings.json'
import ddl from './ddl'
import request from './request'

/** Pooled PostgreSQL instance */
const pool = new Pool(appConfig.pgPool)

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
