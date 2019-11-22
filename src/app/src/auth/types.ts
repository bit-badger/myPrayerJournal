/** A token and expiration set */
export class Token {
  /** The actual token */
  token: string = ''

  /** The expiration for the token */
  expiry: number = 0

  /** Whether this token is currently valid */
  isValid (): boolean {
    return this.token !== '' && this.expiry !== 0 && Date.now() < this.expiry
  }
}

/** A user's current session */
export class Session {
  /** The user's profile from Auth0 */
  profile: any = {}

  /** The user's ID token */
  id = new Token()

  /** The user's access token */
  access = new Token()
}
