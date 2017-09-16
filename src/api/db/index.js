'use strict'

const { Pool } = require('pg')
const config = require('../appsettings.json')

const pool = new Pool(config.pgPool)

/**
 * Run a SQL query
 * @param {string} text The SQL command
 * @param {*[]} params The parameters for the query
 */
const query = (text, params) => pool.query(text, params)

module.exports = {
  query: query,
  request: require('./request')(query)
}
