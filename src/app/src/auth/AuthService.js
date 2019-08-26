'use strict'
/* eslint-disable */
import auth0        from 'auth0-js'
import EventEmitter from 'events'

import AUTH_CONFIG from './auth0-variables'
import mutations   from '@/store/mutation-types'
/* es-lint-enable*/

const webAuth = new auth0.WebAuth({
  domain: AUTH_CONFIG.domain,
  clientID: AUTH_CONFIG.clientId,
  redirectUri: AUTH_CONFIG.appDomain + AUTH_CONFIG.callbackUrl,
  audience: `https://${AUTH_CONFIG.domain}/userinfo`,
  responseType: 'token id_token',
  scope: 'openid profile email'
})

const ACCESS_TOKEN = 'access_token'
const ID_TOKEN = 'id_token'
const EXPIRES_AT = 'expires_at'

class AuthService extends EventEmitter {
  
  id = {
    token: null,
    expiry: null
  }
  access = {
    token: null,
    expiry: null
  }
  profile = null

  auth0 = new auth0.WebAuth({
    domain: AUTH_CONFIG.domain,
    clientID: AUTH_CONFIG.clientId,
    redirectUri: AUTH_CONFIG.appDomain + AUTH_CONFIG.callbackUrl,
    audience: `https://${AUTH_CONFIG.domain}/userinfo`,
    responseType: 'token id_token',
    scope: 'openid profile email'
  })

  /**
   * Starts the user log in flow
   */
  login (customState) {
    webAuth.authorize({
      appState: customState
    })
  }

  /**
   * Promisified parseHash function
   */
  parseHash () {
    return new Promise((resolve, reject) => {
      this.auth0.parseHash((err, authResult) => {
        if (err) {
          reject(err)
        } else {
          resolve(authResult)
        }
      })
    })
  }

  /**
   * Promisified userInfo function
   *
   * @param token The auth token from the login result
   */
  userInfo (token) {
    return new Promise((resolve, reject) => {
      this.auth0.client.userInfo(token, (err, user) => {
        if (err) {
          reject(err)
        } else {
          resolve(user)
        }
      })
    })
  }

  handleAuthentication (store) {
    this.parseHash()
      .then(authResult => {
        if (authResult && authResult.accessToken && authResult.idToken) {
          this.setSession(authResult)
          this.userInfo(authResult.accessToken)
            .then(user => {
              store.commit(mutations.USER_LOGGED_ON, user)
            })
        }
      })
      .catch(err => {
        console.log(err)
        alert(`Error: ${err.error}. Check the console for further details.`)
      })
  }

  setSession (authResult) {
    this.id.token = authResult.idToken
    this.id.expiry = new Date(this.profile.exp * 1000);
    this.profile = authResult.idTokenPayload
    this.access.token = authResult.accessToken
    this.access.expiry = new Date(Date.now() + authResult.expiresIn * 1000)

    localStorage.setItem(ACCESS_TOKEN, authResult.accessToken)
    localStorage.setItem(ID_TOKEN, authResult.idToken)
    localStorage.setItem(EXPIRES_AT, this.id.expiry)

    this.emit('loginEvent', {
      loggedIn: true,
      profile: authResult.idTokenPayload,
      state: authResult.appState || {}
    })
  }

  renewTokens () {
    return new Promise((resolve, reject) => {
      if (localStorage.getItem(ID_TOKEN)) {
        webAuth.checkSession({}, (err, authResult) => {
          if (err) {
            reject(err)
          } else {
            this.setSession(authResult)
            resolve(authResult)
          }
        })
      } else {
        reject('Not logged in')
      }
    })
  }

  logout (store, router) {
    // Clear access token and ID token from local storage
    localStorage.removeItem(ACCESS_TOKEN)
    localStorage.removeItem(ID_TOKEN)
    localStorage.removeItem(EXPIRES_AT)

    this.idToken = null
    this.idTokenExpiry = null
    this.profile = null

    store.commit(mutations.USER_LOGGED_OFF)

    webAuth.logout({
      // navigate to the home route
      returnTo: '/'
    })
    this.emit('loginEvent', { loggedIn: false })
  }

  isAuthenticated () {
    return Date().now() < this.id.Expiry && localStorage.getItem(ID_TOKEN)
  }

  isAccessTokenValid () {
    return this.access.token && this.access.expiry && Date.now() < this.access.expiry
  }

  getAccessToken () {
    return new Promise((resolve, reject) => {
      if (this.isAccessTokenValid()) {
        resolve(this.access.token)
      } else {
        this.renewTokens()
          .then(authResult => {
            resolve(authResult.accessToken)
          }, reject)
      }
    })
  }
}

export default new AuthService()
