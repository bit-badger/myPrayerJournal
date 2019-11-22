import { Store } from 'vuex'
import auth0, { Auth0DecodedHash } from 'auth0-js'
import { EventEmitter } from 'events'

import { Mutations, AppState } from '@/store/types'

import Auth0Config from './auth0-variables'
import { Session, Token } from './types'

// Auth0 web authentication instance to use for our calls
const webAuth = new auth0.WebAuth({
  domain: Auth0Config.domain,
  clientID: Auth0Config.clientId,
  redirectUri: Auth0Config.appDomain + Auth0Config.callbackUrl,
  audience: `https://${Auth0Config.domain}/userinfo`,
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
  session = new Session()

  constructor () {
    super()
    this.refreshSession()
  }

  /**
   * Starts the user log in flow
   */
  login (customState: any) {
    webAuth.authorize({
      appState: customState
    })
  }

  /**
   * Promisified parseHash function
   */
  parseHash (): Promise<Auth0DecodedHash> {
    return new Promise((resolve, reject) => {
      webAuth.parseHash((err, authResult) => {
        if (err || authResult === null) {
          reject(err)
        } else {
          resolve(authResult)
        }
      })
    })
  }

  /**
   * Handle authentication replies from Auth0
   * @param store The Vuex store
   */
  async handleAuthentication (store: Store<AppState>) {
    try {
      const authResult = await this.parseHash()
      if (authResult && authResult.accessToken && authResult.idToken) {
        this.setSession(authResult)
        store.commit(Mutations.UserLoggedOn, this.session.profile)
      }
    } catch (err) {
      console.error(err) // eslint-disable-line no-console
      alert(`Error: ${err.error}. Check the console for further details.`)
    }
  }

  /**
   * Set up the session and commit it to local storage
   * @param authResult The authorization result
   */
  setSession (authResult: Auth0DecodedHash) {
    this.session.profile = authResult.idTokenPayload
    this.session.id = new Token(authResult.idToken!, this.session.profile.exp * 1000)
    this.session.access = new Token(authResult.accessToken!, authResult.expiresIn! * 1000 + Date.now())

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
        : new Session()
  }

  /**
   * Renew authorzation tokens with Auth0
   */
  renewTokens (): Promise<Auth0DecodedHash> {
    return new Promise((resolve, reject) => {
      this.refreshSession()
      if (this.session.id.token !== null) {
        webAuth.checkSession({}, (err, authResult) => {
          if (err) {
            reject(err)
          } else {
            const result = authResult as Auth0DecodedHash
            this.setSession(result)
            resolve(result)
          }
        })
      } else {
        reject(new Error('Not logged in'))
      }
    })
  }

  /**
   * Log out of myPrayerJournal
   * @param store The Vuex store
   */
  logout (store: Store<AppState>) {
    // Clear access token and ID token from local storage
    localStorage.removeItem(this.AUTH_SESSION)
    this.refreshSession()

    store.commit(Mutations.UserLoggedOff)

    webAuth.logout({
      returnTo: `${Auth0Config.appDomain}/`,
      clientID: Auth0Config.clientId
    })
    this.emit('loginEvent', { loggedIn: false })
  }

  /**
   * Is there a user authenticated?
   */
  isAuthenticated () {
    return this.session && this.session.id && this.session.id.isValid()
  }

  /**
   * Is the current access token valid?
   */
  isAccessTokenValid () {
    return this.session && this.session.access && this.session.access.isValid()
  }

  /**
   * Get the user's access token, renewing it if required
   */
  async getAccessToken (): Promise<string> {
    if (this.isAccessTokenValid()) {
      return this.session.access.token
    } else {
      const authResult = await this.renewTokens()
      return authResult.accessToken!
    }
  }
}

export default new AuthService()
