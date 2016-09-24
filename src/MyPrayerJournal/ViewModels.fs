namespace MyPrayerJournal

open Nancy
open Nancy.Session.Persistable
open Newtonsoft.Json
open NodaTime
open NodaTime.Text
open System

/// Levels for a user message
[<RequireQualifiedAccess>]
module Level =
  /// An informational message
  let Info = "Info"
  /// A message regarding a non-fatal but non-optimal condition
  let Warning = "WARNING"
  /// A message regarding a failure of the expected result
  let Error = "ERROR"


/// A message for the user
type UserMessage = 
  { /// The level of the message (use Level module constants)
    Level : string
    /// The text of the message
    Message : string
    /// Further details regarding the message
    Details : string option }
with
  /// An empty message
  static member Empty =
    { Level   = Level.Info
      Message = ""
      Details = None }

  /// Display version
  [<JsonIgnore>]
  member this.ToDisplay =
    let classAndLabel =
      dict [
        Level.Error,   ("danger",  Strings.get "Error")
        Level.Warning, ("warning", Strings.get "Warning")
        Level.Info,    ("info",    "")
        ]
    seq {
      yield "<div class=\"alert alert-dismissable alert-"
      yield fst classAndLabel.[this.Level]
      yield "\" role=\"alert\"><button type=\"button\" class=\"close\" data-dismiss=\"alert\" aria-label=\""
      yield Strings.get "Close"
      yield "\">&times;</button><strong>"
      match snd classAndLabel.[this.Level] with
      | ""  -> ()
      | lbl -> yield lbl.ToUpper ()
               yield " &#xbb; "
      yield this.Message
      yield "</strong>"
      match this.Details with
      | Some d -> yield "<br />"
                  yield d
      | None -> ()
      yield "</div>"
      }
    |> Seq.reduce (+)


/// Helpers to format local date/time using NodaTime
module FormatDateTime =
  
  /// Convert ticks to a zoned date/time
  let zonedTime timeZone ticks = Instant.FromUnixTimeTicks(ticks).InZone(DateTimeZoneProviders.Tzdb.[timeZone])

  /// Display a long date
  let longDate timeZone ticks =
    zonedTime timeZone ticks
    |> ZonedDateTimePattern.CreateWithCurrentCulture("MMMM d',' yyyy", DateTimeZoneProviders.Tzdb).Format
  
  /// Display a short date
  let shortDate timeZone ticks =
    zonedTime timeZone ticks
    |> ZonedDateTimePattern.CreateWithCurrentCulture("MMM d',' yyyy", DateTimeZoneProviders.Tzdb).Format
  
  /// Display the time
  let time timeZone ticks =
    (zonedTime timeZone ticks
     |> ZonedDateTimePattern.CreateWithCurrentCulture("h':'mmtt", DateTimeZoneProviders.Tzdb).Format).ToLower()
  

/// Parent view model for all myPrayerJournal view models
type MyPrayerJournalModel(ctx : NancyContext) =
  
  /// Get the messages from the session
  let getMessages () =
    let msg = ctx.Request.PersistableSession.GetOrDefault<UserMessage list>(Keys.Messages, [])
    match List.length msg with
    | 0 -> ()
    | _ -> ctx.Request.Session.Delete Keys.Messages
    msg

  /// User messages
  member val Messages = getMessages () with get, set
  /// The currently logged in user
  member this.User = ctx.Request.PersistableSession.GetOrDefault<User>(Keys.User, User.Empty)
  /// The title of the page
  member val PageTitle = "" with get, set
  /// The name and version of the application
  member this.Generator = sprintf "myPrayerJournal %s" (ctx.Items.[Keys.Version].ToString ())
  /// The request start time
  member this.RequestStart = ctx.Items.[Keys.RequestStart] :?> int64
  /// Is a user authenticated for this request?
  member this.IsAuthenticated = "" <> this.User.Id
  /// Add a message to the output
  member this.AddMessage message = this.Messages <- message :: this.Messages

  /// Display a long date
  member this.DisplayLongDate ticks = FormatDateTime.longDate this.User.TimeZone ticks
  /// Display a short date
  member this.DisplayShortDate ticks = FormatDateTime.shortDate this.User.TimeZone ticks
  /// Display the time
  member this.DisplayTime ticks = FormatDateTime.time this.User.TimeZone ticks
  /// The page title with the web log name appended
  member this.DisplayPageTitle = this.PageTitle (*
    match this.PageTitle with
    | "" -> match this.WebLog.Subtitle with
            | Some st -> sprintf "%s | %s" this.WebLog.Name st
            | None -> this.WebLog.Name
    | pt -> sprintf "%s | %s" pt this.WebLog.Name *)

  /// An image with the version and load time in the tool tip
  member this.FooterLogo =
    seq {
      yield "<img src=\"/default/footer-logo.png\" alt=\"myWebLog\" title=\""
      yield sprintf "%s %s &bull; " (Strings.get "PoweredBy") this.Generator
      yield Strings.get "LoadedIn"
      yield " "
      yield TimeSpan(System.DateTime.Now.Ticks - this.RequestStart).TotalSeconds.ToString "f3"
      yield " "
      yield (Strings.get "Seconds").ToLower ()
      yield "\" />"
      }
    |> Seq.reduce (+)
