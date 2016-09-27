namespace MyPrayerJournal.Controllers

open Microsoft.AspNetCore.Mvc
open RethinkDb.Driver.Net

/// Home controller
[<Route("")>]
type HomeController(data : IConnection) =
  inherit ApplicationController(data)

  [<HttpGet("")>]
  member this.Index() = this.View()