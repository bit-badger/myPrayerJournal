'use strict'

const chalk = require('chalk')

const env = process.env.NODE_ENV || 'dev'

if ('dev' === env) require('babel-register')

const app = require('./index').default
const db = require('./db').default
const json = require('./json.mjs').default

const fullEnv = ('dev' === env) ? 'Development' : 'Production'

const { port } = json('./appsettings.json')

/**
 * Log a start-up message for the app
 * @param {string} status The status to display
 */
const startupMsg = (status) => {
  console.log(chalk`{reset myPrayerJournal ${status} | Port: {bold ${port}} | Mode: {bold ${fullEnv}}}`)
}

// Ensure the database exists before starting up
db.verify()
.then(() => app.listen(port, () => startupMsg('ready')))
.catch(err => {
  console.log(chalk`\n{reset {bgRed.white.bold ||  Error connecting to PostgreSQL  }}`)
  for (let key of Object.keys(err)) {
    console.log(chalk`${key}: {reset {bold ${err[key]}}}`)
  }
  console.log('')
  startupMsg('failed')
})
