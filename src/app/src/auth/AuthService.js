'use strict'

import auth0 from 'auth0-js'

import AUTH_CONFIG from './auth0-variables'
import mutations from '@/store/mutation-types'

export default class AuthService {

  constructor () {
    this.login = this.login.bind(this)
    this.setSession = this.setSession.bind(this)
    this.logout = this.logout.bind(this)
    this.isAuthenticated = this.isAuthenticated.bind(this)
  }

  auth0 = new auth0.WebAuth({
    domain: AUTH_CONFIG.domain,
    clientID: AUTH_CONFIG.clientId,
    redirectUri: AUTH_CONFIG.callbackUrl,
    audience: `https://${AUTH_CONFIG.domain}/userinfo`,
    responseType: 'token id_token',
    scope: 'openid profile email'
  })

  login () {
    this.auth0.authorize()
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

  handleAuthentication (store, router) {
    this.parseHash()
      .then(authResult => {
        if (authResult && authResult.accessToken && authResult.idToken) {
          this.setSession(authResult)
          this.userInfo(authResult.accessToken)
            .then(user => {
              store.commit(mutations.USER_LOGGED_ON, user)
              router.replace('/dashboard')
            })
        }
      })
      .catch(err => {
        router.replace('/')
        console.log(err)
        alert(`Error: ${err.error}. Check the console for further details.`)
      })
  }

  setSession (authResult) {
    // Set the time that the access token will expire at
    let expiresAt = JSON.stringify(
      authResult.expiresIn * 1000 + new Date().getTime()
    )
    localStorage.setItem('access_token', authResult.accessToken)
    localStorage.setItem('id_token', authResult.idToken)
    localStorage.setItem('expires_at', expiresAt)
  }

  logout (store, router) {
    // Clear access token and ID token from local storage
    localStorage.removeItem('access_token')
    localStorage.removeItem('id_token')
    localStorage.removeItem('expires_at')
    localStorage.setItem('user_profile', JSON.stringify({}))
    // navigate to the home route
    store.commit(mutations.USER_LOGGED_OFF)
    router.replace('/')
  }

  isAuthenticated () {
    // Check whether the current time is past the access token's expiry time
    let expiresAt = JSON.parse(localStorage.getItem('expires_at'))
    return new Date().getTime() < expiresAt
  }
}
