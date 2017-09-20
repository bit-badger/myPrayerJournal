'use strict'

const chalk = require('chalk')

const env = process.env.NODE_ENV || 'dev'

if ('dev' === env) require('babel-register')

const fullEnv = ('dev' === env) ? 'Development' : 'Production'

/** Configuration for the application */
const appConfig = require('./appsettings.json')

/** Express app */
const app = require('./index').default

// Ensure the database exists...
require('./db').default.verify().then(() => 
  // ...and start it up!
  app.listen(appConfig.port, () => {
    console.log(chalk`{reset myPrayerJournal | Port: {bold ${appConfig.port}} | Mode: {bold ${fullEnv}}}`)
  }))
