import { Component } from '@angular/core'
import { AuthService } from 'src/app/auth.service'

/**
 * myPrayerJournal Navigation.
 * 
 * @author Daniel J. Summers <daniel@bitbadger.solutions>
 * @version 3
 */
@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.sass']
})
export class NavigationComponent {

  // TODO: get this from the store's state
  hasSnoozed = false

  constructor(public auth: AuthService) { }

  /**
   * Start the log on process.
   * 
   * @param e The click event (used to stop the default action)
   */
  logOn(e: Event) {
    e.preventDefault()
    this.auth.login()
  }

  /**
   * Log the user off.
   * 
   * @param e The click event (used to stop the default action)
   */
  logOff(e: Event) {
    e.preventDefault()
    this.auth.logout()
  }
}
