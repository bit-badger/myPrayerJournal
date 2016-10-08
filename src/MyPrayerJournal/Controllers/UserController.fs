namespace MyPrayerJournal.Controllers

open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Options
open MyPrayerJournal
open MyPrayerJournal.ViewModels
open RethinkDb.Driver.Net

/// Controller for all /user URLs
[<Authorize>]
[<Route("user")>]
type UserController(data : IConnection, cfg : IOptions<AppConfig>) =
  inherit ApplicationController(data)

  [<AllowAnonymous>]
  [<HttpGet("log-on")>]
  member this.ShowLogOn () =
    this.View(LogOnViewModel())

  [<AllowAnonymous>]
  [<HttpPost("log-on")>]
  [<ValidateAntiForgeryToken>]
  member this.DoLogOn (form : LogOnViewModel) =
    async {
      let! user = data.LogOnUser form.Email (User.HashPassword form.Password cfg.Value.PasswordSaltBytes)
      match user with
      | Some usr -> do! this.HttpContext.Authentication.SignInAsync (Keys.Authentication, AppUser user)
                    // TODO: welcome message
                    (* this.Session.[Keys.User] <- usr
                    { UserMessage.Empty with Level   = Level.Info
                                             Message = Strings.get "LogOnSuccess" }
                    |> model.AddMessage *)
                    return this.Redirect "/" :> IActionResult
      | _ -> (*{ UserMessage.Empty with Level   = Level.Error
                                      Message = Strings.get "LogOnFailure" }
             |> model.AddMessage
             return this.Redirect "/user/log-on" model *)
             return upcast this.RedirectToAction "ShowLogOn"
      }
    |> Async.StartAsTask

  [<HttpGet("log-off")>]
  member this.LogOff () =
    async {
      do! this.HttpContext.Authentication.SignOutAsync Keys.Authentication
      // TODO: goodbye message
      return this.LocalRedirect "/"
    } |> Async.StartAsTask