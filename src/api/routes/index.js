'use strict'

module.exports = {
  mount: (app, checkJwt) => {
    app.use('/api/journal', require('./journal')(checkJwt))
  }
}
