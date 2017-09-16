'use strict'

const express = require('express')

/** Configuration for the application */
const appConfig = require('./appsettings.json')

/** Express app */
const app = express()

// Serve the Vue files from /public
app.use(express.static('public'))

// Logging FTW!
app.use(require('morgan')('dev'))

// Tie in all the API routes
require('./routes').mount(app)

// Send the index.html file for what would normally get a 404
app.use(async (req, res, next) => {
  try {
    await res.sendFile('index.html', { root: __dirname + '/public/', dotfiles: 'deny' })
  }
  catch (err) {
    return next(err)
  }
})

// Ensure the database exists...
require('./db').verify().then(() => 
  // ...and start it up!
  app.listen(appConfig.port, () => {
    console.log(`Listening on port ${appConfig.port}`)
  }))
