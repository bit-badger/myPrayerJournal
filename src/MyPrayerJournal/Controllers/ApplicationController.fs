namespace MyPrayerJournal.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.Filters
open Microsoft.Extensions.Localization
open MyPrayerJournal
open RethinkDb.Driver.Net
open System
open System.Reflection

/// Base controller for all myPrayerJournal controllers
type ApplicationController(data : IConnection) =
  inherit Controller()

  let version = 
    let v = typeof<ApplicationController>.GetType().GetTypeInfo().Assembly.GetName().Version
    match v.Build with
    | 0 -> match v.Minor with 0 -> string v.Major | _ -> sprintf "%d.%d" v.Major v.Minor
    | _ -> sprintf "%d.%d.%d" v.Major v.Minor v.Build
    |> sprintf "v%s"

  /// Fill common items for every request
  override this.OnActionExecuting (context : ActionExecutingContext) =
    let sw = System.Diagnostics.Stopwatch()
    sw.Start()
    base.OnActionExecuting context
    this.ViewData.[Keys.CurrentUser]  <- Option<User>.None
    this.ViewData.[Keys.Generator]    <- sprintf "myPrayerJournal %s" version
    this.ViewData.[Keys.RequestTimer] <- sw
     
