import {Router, RouterConfiguration} from "aurelia-router"
import {EventAggregator} from "aurelia-event-aggregator"
import {inject} from "aurelia-framework"
import {PageTitle} from "./messages"
import {WebAPI} from "./web-api"

@inject(WebAPI, EventAggregator)
export class App {
  router: Router;
  pageTitle: string;

  constructor(public api: WebAPI, private ea: EventAggregator) {
    this.ea.subscribe(PageTitle, msg => this.pageTitle = msg.title)
  }

  configureRouter(config: RouterConfiguration, router: Router){
    config.title = "myPrayerJournal"
    config.options.pushState = true
    config.options.root = "/"
    config.map([
      { route: "",              moduleId: "home",           name: "home", title: "Welcome" },
      { route: 'contacts/:id',  moduleId: 'contact-detail', name:'contacts' }
    ])

    this.router = router
  }
}