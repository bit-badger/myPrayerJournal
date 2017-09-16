'use strict'

const { Pool } = require('pg')

/** Pooled PostgreSQL instance */
const pool = new Pool(require('../appsettings.json').pgPool)

/**
 * Run a SQL query
 * @param {string} text The SQL command
 * @param {*[]} params The parameters for the query
 */
const query = (text, params) => pool.query(text, params)

module.exports = {
  query: query,
  request: require('./request')(query),
  verify: require('./ddl')(query).ensureDatabase
}
