'use strict'

import jwt from 'koa-jwt'
import jwksRsa from 'jwks-rsa-koa'
import Router from 'koa-router'

import appConfig from '../appsettings.json'
import journal from './journal'
import request from './request'

/** Authentication middleware to verify the access token against the Auth0 JSON Web Key Set */
const checkJwt = jwt({
  // Dynamically provide a signing key
  // based on the kid in the header and 
  // the singing keys provided by the JWKS endpoint.
  secret: jwksRsa.koaJwt2Key({
    cache: true,
    rateLimit: true,
    jwksRequestsPerMinute: 5,
    jwksUri: `https://${appConfig.auth0.domain}/.well-known/jwks.json`
  }),

  // Validate the audience and the issuer.
  audience: appConfig.auth0.clientId,
  issuer: `https://${appConfig.auth0.domain}/`,
  algorithms: ['RS256']
})

/** /api/journal routes */
const journalRoutes = journal(checkJwt)
/** /api/request routes */
const requestRoutes = request(checkJwt)

/** Combined router */
const router = new Router({ prefix: '/api' })
router.use('/journal', journalRoutes.routes(), journalRoutes.allowedMethods())
router.use('/request', requestRoutes.routes(), requestRoutes.allowedMethods())

export default router
