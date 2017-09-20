'use strict'

import Router from 'koa-router'
import db from '../db'

const router = new Router()

export default function (checkJwt) {

  router.post('/', checkJwt, async (ctx, next) => {
    const newId = await db.request.addNew(ctx.state.user.sub, ctx.body.requestText)
    ctx.body = { id: newId }
    await next()
  })
  
  return router
}

