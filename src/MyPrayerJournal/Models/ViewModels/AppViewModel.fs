namespace MyPrayerJournal.ViewModels

//open MyPrayerJournal

/// Parent view model for all myPrayerJournal view models
type AppViewModel() =
  member this.Q = "X"
  (*
  /// User messages
  member val Messages = getMessages () with get, set
  /// The currently logged in user
  member val User = Option<User>.None with get, set
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
  /// The page title with the application name appended
  member this.DisplayPageTitle =
    match this.PageTitle with
    | "" -> Strings.get "myPrayerJournal"
    | pt -> sprintf "%s | %s" pt (Strings.get "myPrayerJournal")

  /// An image with the version and load time in the tool tip
  member this.FooterLogo =
    seq {
      yield "<span title=\""
      yield sprintf "%s %s &bull; " (Strings.get "PoweredBy") this.Generator
      yield Strings.get "LoadedIn"
      yield " "
      yield TimeSpan(System.DateTime.Now.Ticks - this.RequestStart).TotalSeconds.ToString "f3"
      yield " "
      yield (Strings.get "Seconds").ToLower ()
      yield "\"><span style=\"font-weight:100;\">my</span><span style=\"font-weight:600;\">Prayer</span><span style=\"font-weight:700;\">Journal</span></span>"
      }
    |> Seq.reduce (+)
  *)