'use strict'

const Router = require('express-promise-router')
const db = require('../db')

module.exports = checkJwt => {
  let router = new Router()

  router.get('/', checkJwt, async (req, res) => {
    const reqs = await db.request.journal(req.user.sub)
    res.json(reqs)
  })
  return router
}

