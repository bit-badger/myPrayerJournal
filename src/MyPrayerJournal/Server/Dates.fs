/// Date formatting helpers
//  Many thanks to date-fns (https://date-fns.org) for this logic
module MyPrayerJournal.Dates


type internal FormatDistanceToken =
| LessThanXMinutes
| XMinutes
| AboutXHours
| XHours
| XDays
| AboutXWeeks
| XWeeks
| AboutXMonths
| XMonths
| AboutXYears
| XYears
| OverXYears
| AlmostXYears

let internal locales =
  let format = PrintfFormat<int -> string, unit, string, string>
  Map.ofList [
    "en-US", Map.ofList [
      LessThanXMinutes, ("less than a minute", format "less than %i minutes")
      XMinutes,         ("a minute",           format "%i minutes")
      AboutXHours,      ("about an hour",       format "about %i hours")
      XHours,           ("an hour",             format "%i hours")
      XDays,            ("a day",              format "%i days")
      AboutXWeeks,      ("about a week",       format "about %i weeks")
      XWeeks,           ("a week",             format "%i weeks")
      AboutXMonths,     ("about a month",      format "about %i months")
      XMonths,          ("a month",            format "%i months")
      AboutXYears,      ("about a year",       format "about %i years")
      XYears,           ("a year",             format "%i years")
      OverXYears,       ("over a year",        format "over %i years")
      AlmostXYears,     ("almost a year",      format "almost %i years")
    ]
  ]

let aDay          =  1_440.
let almostTwoDays =  2_520.
let aMonth        = 43_200.
let twoMonths     = 86_400.

open System

/// Convert from a JavaScript "ticks" value to a date/time
let fromJs ticks = DateTime.UnixEpoch + TimeSpan.FromTicks (ticks * 10_000L)

let formatDistance (startDate : DateTime) (endDate : DateTime) =
  let format (token, number) locale =
    let labels = locales |> Map.find locale
    match number with 1 -> fst labels.[token] | _ -> sprintf (snd labels.[token]) number
  let round (it : float) = Math.Round it |> int

  let diff        = startDate - endDate
  let formatToken =
    let months = diff.TotalMinutes / aMonth |> round
    let years  = months / 12
    match true with
    | _ when diff.TotalMinutes = 0. -> LessThanXMinutes, 1
    | _ when diff.TotalMinutes < 45. -> XMinutes, round diff.TotalMinutes
    | _ when diff.TotalMinutes < 90. -> AboutXHours, 1
    | _ when diff.TotalMinutes < aDay -> AboutXHours, round (diff.TotalMinutes / 60.)
    | _ when diff.TotalMinutes < almostTwoDays -> XDays, 1
    | _ when diff.TotalMinutes < aMonth -> XDays, round (diff.TotalMinutes / aDay)
    | _ when diff.TotalMinutes < twoMonths -> AboutXMonths, round (diff.TotalMinutes / aMonth)
    | _ when months < 12 -> XMonths, round (diff.TotalMinutes / aMonth)
    | _ when months % 12 < 3 -> AboutXYears, years
    | _ when months % 12 < 9 -> OverXYears, years
    | _ -> AlmostXYears, years + 1
  
  let words = format formatToken "en-US"
  match startDate > endDate with true -> $"{words} ago" | false -> $"in {words}"

