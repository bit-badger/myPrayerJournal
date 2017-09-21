'use strict'

import Router from 'koa-router'
import db from '../db'

const router = new Router()

export default function (checkJwt) {

  router.post('/', checkJwt, async (ctx, next) => {
    ctx.body = await db.request.addNew(ctx.state.user.sub, ctx.request.body.requestText)
    await next()
  })
  
  return router
}

