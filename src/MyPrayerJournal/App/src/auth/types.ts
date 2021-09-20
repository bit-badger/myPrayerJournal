/** A token and expiration set */
export class Token {
  /**
   * Create a new token
   * @param token The actual token
   * @param expiry The expiration for the token
   */
  constructor (public token: string, public expiry: number) { } // eslint-disable-line no-useless-constructor
}

/** A user's current session */
export class Session {
  /** The user's profile from Auth0 */
  profile: any = {}
  /** The user's ID token */
  id = new Token("", 0)
  /** The user's access token */
  access = new Token("", 0)
}
