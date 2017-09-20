'use strict'

import Router from 'koa-router'
import db from '../db'

const router = new Router()

export default function (checkJwt) {

  router.get('/', checkJwt, async (ctx, next) => {
    const reqs = await db.request.journal(ctx.state.user.sub)
    ctx.body = reqs
    return await next()
  })
  return router
}
