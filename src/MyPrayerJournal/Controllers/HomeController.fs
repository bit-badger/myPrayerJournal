namespace MyPrayerJournal.Controllers

open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open MyPrayerJournal
open RethinkDb.Driver.Net

/// Home controller
[<Authorize>]
[<Route("")>]
type HomeController(data : IConnection) =
  inherit ApplicationController(data)

  [<AllowAnonymous>]
  [<HttpGet("")>]
  member this.Index() =
    async {
      match this.HttpContext.User with
      | :? AppUser as user -> return this.View "Dashboard" :> IActionResult
      | _ -> return upcast this.View () 
    } |> Async.StartAsTask
