namespace MyPrayerJournal.Controllers

open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open MyPrayerJournal
open RethinkDb.Driver.Net

/// Home controller
[<Authorize>]
[<Route("")>]
type HomeController(data : IConnection, logger : ILogger<HomeController>) =
  inherit ApplicationController(data)

  [<AllowAnonymous>]
  [<HttpGet("")>]
  member this.Index() =
    logger.LogDebug(Newtonsoft.Json.JsonConvert.SerializeObject this.HttpContext.User)
    async {
      match this.HttpContext.User with
      | :? AppUser as user -> return this.View "Dashboard" :> IActionResult
      | _ -> return upcast this.View () 
      }
    |> Async.StartAsTask
