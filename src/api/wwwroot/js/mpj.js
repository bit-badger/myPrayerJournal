/**
 * myPrayerJournal script file
 */
var mpj = {
  lock: new Auth0Lock('Of2s0RQCQ3mt3dwIkOBY5h85J9sXbF2n', 'djs-consulting.auth0.com', {
    auth: {
      redirectUrl: 'http://localhost:8080/user/log-on',
      allowSignUp: false
    }
  }),

  signIn: function() {
    this.lock.show()
  }
}