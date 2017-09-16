'use strict'

const jwt = require('express-jwt')
const jwksRsa = require('jwks-rsa')
const config = require('../appsettings.json')

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

module.exports = {
  mount: app => {
    app.use('/api/journal', require('./journal')(checkJwt))
  }
}
