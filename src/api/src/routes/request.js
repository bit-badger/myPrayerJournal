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
      ctx.response.status = await db.request.addHistory(ctx.state.user.sub, ctx.params.id, body.status, body.updateText)
      await next()
    })
    // Add a note to a request
    .post('/:id/note', checkJwt, async (ctx, next) => {
      const body = ctx.request.body
      ctx.response.status = await db.request.addNote(ctx.state.user.sub, ctx.params.id, body.notes)
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
    // Get the notes for a request
    .get('/:id/notes', checkJwt, async (ctx, next) => {
      const notes = await db.request.notesById(ctx.state.user.sub, ctx.params.id)
      if (notes.text && 'Not Found' === notes.text) {
        ctx.response.status = 404
      } else {
        ctx.body = notes
        ctx.response.status = 200
      }
      await next()
    })
    // Get a complete request; equivalent to full + notes
    .get('/:id/complete', checkJwt, async (ctx, next) => {
      const req = await db.request.fullById(ctx.state.user.sub, ctx.params.id)
      if ('Not Found' === req.text) {
        ctx.response.status = 404
      } else {
        ctx.response.status = 200
        req.notes = await db.request.notesById(ctx.state.user.sub, ctx.params.id)
        ctx.body = req
      }
      await next()
    })
    // Get all answered requests
    .get('/answered', checkJwt, async (ctx, next) => {
      ctx.body = await db.request.answered(ctx.state.user.sub)
      ctx.response.status = 200
      await next()
    })

  return router
}
