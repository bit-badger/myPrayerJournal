'use strict'
/* eslint-disable */
import auth0            from 'auth0-js'
import { EventEmitter } from 'events'

import AUTH_CONFIG from './auth0-variables'
import mutations   from '@/store/mutation-types'
/* es-lint-enable*/

// Auth0 web authentication instance to use for our calls
const webAuth = new auth0.WebAuth({
  domain: AUTH_CONFIG.domain,
  clientID: AUTH_CONFIG.clientId,
  redirectUri: AUTH_CONFIG.appDomain + AUTH_CONFIG.callbackUrl,
  audience: `https://${AUTH_CONFIG.domain}/userinfo`,
  responseType: 'token id_token',
  scope: 'openid profile email'
})

/**
 * A class to handle all authentication calls and determinations
 */
class AuthService extends EventEmitter {
  
  // Local storage key for our session data
  AUTH_SESSION = 'auth-session'

  // Received and calculated values for our ssesion (initially loaded from local storage if present)
  session: any = {}

  constructor() {
    super()
    this.refreshSession()
  }

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

  /**
   * Handle authentication replies from Auth0
   * 
   * @param store The Vuex store
   */
  async handleAuthentication (store) {
    try {
      const authResult: any = await this.parseHash()
      if (authResult && authResult.accessToken && authResult.idToken) {
        this.setSession(authResult)
        store.commit(mutations.USER_LOGGED_ON, this.session.profile)
      }
    } catch(err) {
      console.error(err)
      alert(`Error: ${err.error}. Check the console for further details.`)
    }
  }

  /**
   * Set up the session and commit it to local storage
   * 
   * @param authResult The authorization result
   */
  setSession (authResult) {
    this.session.profile = authResult.idTokenPayload
    this.session.id.token = authResult.idToken
    this.session.id.expiry = this.session.profile.exp * 1000
    this.session.access.token = authResult.accessToken
    this.session.access.expiry = authResult.expiresIn * 1000 + Date.now()

    localStorage.setItem(this.AUTH_SESSION, JSON.stringify(this.session))

    this.emit('loginEvent', {
      loggedIn: true,
      profile: authResult.idTokenPayload,
      state: authResult.appState || {}
    })
  }

  /**
   * Refresh this instance's session from the one in local storage
   */
  refreshSession () {
    this.session = 
      localStorage.getItem(this.AUTH_SESSION)
      ? JSON.parse(localStorage.getItem(this.AUTH_SESSION) || '{}')
      : { profile: {},
          id: {
            token: null,
            expiry: null
          },
          access: {
            token: null,
            expiry: null
          }
        }
  }

  /**
   * Renew authorzation tokens with Auth0
   */
  renewTokens () {
    return new Promise((resolve, reject) => {
      this.refreshSession()
      if (this.session.id.token !== null) {
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

  /**
   * Log out of myPrayerJournal
   * 
   * @param store The Vuex store
   */
  logout (store) {
    // Clear access token and ID token from local storage
    localStorage.removeItem(this.AUTH_SESSION)
    this.refreshSession()

    store.commit(mutations.USER_LOGGED_OFF)

    webAuth.logout({
      returnTo: `${AUTH_CONFIG.appDomain}/`,
      clientID: AUTH_CONFIG.clientId
    })
    this.emit('loginEvent', { loggedIn: false })
  }

  /**
   * Check expiration for a token (the way it's stored in the session)
   */
  checkExpiry = (it) => it.token && it.expiry && Date.now() < it.expiry
  
  /**
   * Is there a user authenticated?
   */
  isAuthenticated () {
    return this.checkExpiry(this.session.id)
  }

  /**
   * Is the current access token valid?
   */
  isAccessTokenValid () {
    return this.checkExpiry(this.session.access)
  }

  /**
   * Get the user's access token, renewing it if required
   */
  async getAccessToken () {
    if (this.isAccessTokenValid()) {
      return this.session.access.token
    } else {
      try {
        const authResult: any = await this.renewTokens()
        return authResult.accessToken
      } catch (reject) {
        throw reject
      }
    }
  }
}

export default new AuthService()
