/// Internal partial views
[<AutoOpen>]
module private MyPrayerJournal.Views.Helpers
  
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open MyPrayerJournal
open System

/// Create a link that targets the `#top` element and pushes a URL to history
let pageLink href attrs =
  attrs
  |> List.append [ _href href; _hxBoost; _hxTarget "#top"; _hxSwap HxSwap.InnerHtml; _hxPushUrl ]
  |> a

/// Create a Material icon
let icon name = span [ _class "material-icons" ] [ str name ]

/// Create a card when there are no results found
let noResults heading link buttonText text =
  div [ _class "card" ] [
    h5 [ _class "card-header"] [ str heading ]
    div [ _class "card-body text-center" ] [
      p [ _class "card-text" ] text
      pageLink link [ _class "btn btn-primary" ] [ str buttonText ]
      ]
    ]

/// Convert `Ticks` to `DateTime`
let fromJs = Ticks.toLong >> Dates.fromJs

/// Create a date with a span tag, displaying the relative date with the full date/time in the tooltip
let relativeDate jsDate =
  let date = fromJs jsDate
  span [ _title (date.ToString "f") ] [ Dates.formatDistance DateTime.UtcNow date |> str ]
