'use strict'

import Router from 'koa-router'
import db from '../db'

const router = new Router()

export default function (checkJwt) {

  router
    // Add a new request
    .post('/', checkJwt, async (ctx, next) => {
      ctx.body = await db.request.addNew(ctx.state.user.sub, ctx.request.body.requestText)
      await next()
    })
    // Add a request history entry (prayed, updated, answered, etc.)
    .post('/:id/history', checkJwt, async (ctx, next) => {
      const body = ctx.request.body
      await db.request.addHistory(ctx.params.id, body.status, body.updateText)
      ctx.response.status = 204
      await next()
    })
    // Get a journal-style request by its Id
    .get('/:id', checkJwt, async (ctx, next) => {
      const req = await db.request.byId(ctx.state.user.sub, ctx.params.id)
      if ('Not Found' === req.text) {
        ctx.response.status = 404
      } else {
        ctx.body = req
      }
      await next()
    })
    // Get a request, along with its full history
    .get('/:id/full', checkJwt, async (ctx, next) => {
      const req = await db.request.fullById(ctx.state.user.sub, ctx.params.id)
      if ('Not Found' === req.text) {
        ctx.response.status = 404
      } else {
        ctx.body = req
      }
      await next()
    })
    // Get the least-recently-updated request (used for the "pray through the journal" feature)
    .get('/:id/oldest', checkJwt, async (ctx, next) => {
      ctx.body = await db.request.oldest(ctx.state.user.sub)
      await next()
    })
    .get('/answered', checkJwt, async (ctx, next) => {
      ctx.body = await db.request.answered(ctx.state.user.sub)
      ctx.response.status = 200
      await next()
    })

  return router
}
