import {inject} from 'aurelia-framework';
import {EventAggregator} from "aurelia-event-aggregator"
import {PageTitle} from "./messages"

@inject(EventAggregator)
export class Home {
  constructor(private ea: EventAggregator) {
    this.ea.publish(new PageTitle("Welcome to myPrayerJournal"));
  }
}