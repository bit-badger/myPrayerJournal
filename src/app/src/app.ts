import {Router, RouterConfiguration} from "aurelia-router"
import {EventAggregator} from "aurelia-event-aggregator"
import {inject} from "aurelia-framework"
import {HttpClient} from "aurelia-fetch-client"
import {PageTitle} from "./messages"
import {Auth0Lock} from "auth0-lock"

@inject(EventAggregator, HttpClient)
export class App {
  router: Router;
  pageTitle: string;
  
  lock = new Auth0Lock('Of2s0RQCQ3mt3dwIkOBY5h85J9sXbF2n', 'djs-consulting.auth0.com', {
      oidcConformant: true,
      autoclose: true,
      auth: {
        redirectUrl: "http://localhost:8080/user/log-on",
        responseType: 'token id_token',
        audience: `https://djs-consulting.auth0.com/userinfo`,
        params: {
          scope: 'openid'
        }
      }
    })

  private setSession(authResult): void {
    // Set the time that the access token will expire at
    const expiresAt = JSON.stringify((authResult.expiresIn * 1000) + new Date().getTime())
    localStorage.setItem('access_token', authResult.accessToken)
    localStorage.setItem('id_token', authResult.idToken)
    localStorage.setItem('expires_at', expiresAt)
  }

  public logoff(): void {
    // Remove tokens and expiry time from localStorage
    localStorage.removeItem('access_token')
    localStorage.removeItem('id_token')
    localStorage.removeItem('expires_at')
    // Go back to the home route
    this.router.navigateToRoute("")
  }

  public isAuthenticated(): boolean {
    // Check whether the current time is past the
    // access token's expiry time
    const expiresAt = JSON.parse(localStorage.getItem('expires_at'))
    return new Date().getTime() < expiresAt
  }

  constructor(private ea: EventAggregator, private http: HttpClient) {
    this.ea.subscribe(PageTitle, msg => this.pageTitle = msg.title)
    var self = this
    this.lock.on('authenticated', (authResult) => {
          if (authResult && authResult.accessToken && authResult.idToken) {
            this.setSession(authResult)
            this.router.navigateToRoute("")
          }
        });
        this.lock.on('authorization_error', (err) => {
          this.router.navigateToRoute("")
          console.log(err)
          alert(`Error: ${err.error}. Check the console for further details.`)
    });
  }

  configureRouter(config: RouterConfiguration, router: Router){
    config.title = "myPrayerJournal"
    config.options.pushState = true
    config.options.root = "/"
    config.map([
      { route: "", moduleId: "home", name: "home", title: "Welcome" }
    ])

    this.router = router
  }

  public logon() {
    this.lock.show()
  }
}