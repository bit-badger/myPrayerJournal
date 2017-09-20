'use strict'

import Koa from 'koa'
import morgan from 'koa-morgan'
import send from 'koa-send'
import serveFrom from 'koa-static'

import appConfig from './appsettings.json'
import router from './routes'

/** Koa app */
const app = new Koa()

export default app
  // Logging FTW!
  .use(morgan('dev'))
  // Serve the Vue files from /public
  .use(serveFrom('public'))
  // Tie in all the routes
  .use(router.routes())
  .use(router.allowedMethods())
  // Send the index.html file for what would normally get a 404
  .use(async (ctx, next) => {
    if (ctx.url.indexOf('/api') === -1) {
      try {
        await send(ctx, 'index.html', { root: __dirname + '/public/' })
      }
      catch (err) {
        return await next(err)
      }
    }
    return await next()
  })
