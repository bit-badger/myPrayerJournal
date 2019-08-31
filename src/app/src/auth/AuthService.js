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
  responseType: 'token id_token',
  scope: 'openid profile email'
})

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

  ACCESS_TOKEN = 'access_token'
  ID_TOKEN = 'id_token'
  EXPIRES_AT = 'expires_at'
  USER_PROFILE = 'user_profile'
  
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
      webAuth.parseHash((err, authResult) => {
        if (err) {
          reject(err)
        } else {
          resolve(authResult)
        }
      })
    })
  }

  handleAuthentication (store) {
    this.parseHash()
      .then(authResult => {
        if (authResult && authResult.accessToken && authResult.idToken) {
          this.setSession(authResult)
          store.commit(mutations.USER_LOGGED_ON, this.profile)
        }
      })
      .catch(err => {
        console.log(err)
        alert(`Error: ${err.error}. Check the console for further details.`)
      })
  }

  setSession (authResult) {
    this.profile = authResult.idTokenPayload
    this.id.token = authResult.idToken
    this.id.expiry = new Date(this.profile.exp * 1000);
    this.access.token = authResult.accessToken
    this.access.expiry = new Date(Date.now() + authResult.expiresIn * 1000)

    localStorage.setItem(this.ACCESS_TOKEN, authResult.accessToken)
    localStorage.setItem(this.ID_TOKEN, authResult.idToken)
    localStorage.setItem(this.EXPIRES_AT, this.id.expiry)
    localStorage.setItem(this.USER_PROFILE, JSON.stringify(this.profile))

    this.emit('loginEvent', {
      loggedIn: true,
      profile: authResult.idTokenPayload,
      state: authResult.appState || {}
    })
  }

  renewTokens () {
    return new Promise((resolve, reject) => {
      if (localStorage.getItem(this.ID_TOKEN) !== null) {
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
    localStorage.removeItem(this.ACCESS_TOKEN)
    localStorage.removeItem(this.ID_TOKEN)
    localStorage.removeItem(this.EXPIRES_AT)
    localStorage.removeItem(this.USER_PROFILE)

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
    return Date.now() < this.id.Expiry && localStorage.getItem(this.ID_TOKEN)
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
