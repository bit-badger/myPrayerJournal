'use strict'

const express = require('express')

/** Configuration for the application */
const config = require('./appsettings.json')

/** Express app */
const app = express()

const jwt = require('express-jwt')
const jwksRsa = require('jwks-rsa')

// Authentication middleware. When used, the
// access token must exist and be verified against
// the Auth0 JSON Web Key Set
const checkJwt = jwt({
  // Dynamically provide a signing key
  // based on the kid in the header and 
  // the singing keys provided by the JWKS endpoint.
  secret: jwksRsa.expressJwtSecret({
    cache: true,
    rateLimit: true,
    jwksRequestsPerMinute: 5,
    jwksUri: `https://${config.auth0.domain}/.well-known/jwks.json`
  }),

  // Validate the audience and the issuer.
  audience: config.auth0.clientId,
  issuer: `https://${config.auth0.domain}/`,
  algorithms: ['RS256']
})

// Serve the Vue files from /public
app.use(express.static('public'))

// Logging FTW!
app.use(require('morgan')('dev'))

// Tie in all the API routes
require('./routes').mount(app, checkJwt)

// Send the index.html file for what would normally get a 404
app.use(async (req, res, next) => {
  const options = {
    root: __dirname + '/public/',
    dotfiles: 'deny'
  }
  try {
    await res.sendFile('index.html', options)
  }
  catch (err) {
    return next(err)
  }
})

// Start it up!
app.listen(config.port, () => {
  console.log(`Listening on port ${config.port}`)
})
