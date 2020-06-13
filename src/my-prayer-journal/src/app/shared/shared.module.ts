import { CommonModule } from '@angular/common'
import { NgModule } from '@angular/core'
import { MatToolbarModule } from '@angular/material/toolbar'

import { NavigationComponent } from './ui/navigation/navigation.component'
import { TopHeaderComponent } from './ui/top-header/top-header.component'

/**
 * myPrayerJournal Shared Module.
 * 
 * This module contains UI components designed to be used throughout the application.
 * 
 * @author Daniel J. Summers <daniel@bitbadger.solutions>
 * @version 3
 */
@NgModule({
  declarations: [
    NavigationComponent,
    TopHeaderComponent
  ],
  imports: [
    CommonModule,
    MatToolbarModule
  ],
  exports: [
    TopHeaderComponent
  ]
})
export class SharedModule { }
