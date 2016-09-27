namespace MyPrayerJournal.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Options
open MyPrayerJournal
open MyPrayerJournal.ViewModels
open RethinkDb.Driver.Net

/// Controller for all /user URLs
[<Route("user")>]
type UserController(data : IConnection, cfg : IOptions<AppConfig>) =
  inherit ApplicationController(data)

  [<HttpGet("log-on")>]
  member this.ShowLogOn () =
    this.View(LogOnViewModel())

  
  [<HttpPost("log-on")>]
  [<ValidateAntiForgeryToken>]
  member this.DoLogOn (form : LogOnViewModel) =
    async {
      let! user = data.LogOnUser form.Email (User.HashPassword form.Password cfg.Value.PasswordSaltBytes)
      match user with
      | Some usr -> (* this.Session.[Keys.User] <- usr
                    { UserMessage.Empty with Level   = Level.Info
                                             Message = Strings.get "LogOnSuccess" }
                    |> model.AddMessage
                    this.Redirect "" model |> ignore // Save the messages in the session before the Nancy redirect
                    // TODO: investigate if addMessage should update the session when it's called
                    return this.LoginAndRedirect (System.Guid.Parse usr.Id, fallbackRedirectUrl = "/") :> obj
                    *)
                    return this.Redirect "/" :> IActionResult
      | _ -> (*{ UserMessage.Empty with Level   = Level.Error
                                      Message = Strings.get "LogOnFailure" }
             |> model.AddMessage
             return this.Redirect "/user/log-on" model *)
             return upcast this.RedirectToAction("ShowLogOn")
      //return this.View()
    } |> Async.StartAsTask
