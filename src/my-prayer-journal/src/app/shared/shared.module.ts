import { CommonModule } from '@angular/common'
import { NgModule } from '@angular/core'
import { MatToolbarModule } from '@angular/material/toolbar'

import { TopHeaderComponent } from './ui/top-header/top-header.component'



@NgModule({
  declarations: [TopHeaderComponent],
  imports: [
    CommonModule,
    MatToolbarModule
  ],
  exports: [
    TopHeaderComponent
  ]
})
export class SharedModule { }
