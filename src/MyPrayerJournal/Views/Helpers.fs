/// Internal partial views
[<AutoOpen>]
module private MyPrayerJournal.Views.Helpers
  
open Giraffe.Htmx
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open MyPrayerJournal
open NodaTime

/// Create a link that targets the `#top` element and pushes a URL to history
let pageLink href attrs =
    attrs
    |> List.append [ _href href; _hxBoost; _hxTarget "#top"; _hxSwap HxSwap.InnerHtml; _hxPushUrl "true" ]
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

/// Create a date with a span tag, displaying the relative date with the full date/time in the tooltip
let relativeDate (date : Instant) now (tz : DateTimeZone) =
    span [ _title (date.InZone(tz).ToDateTimeOffset().ToString ("f", null)) ] [ Dates.formatDistance now date |> str ]

/// The version of myPrayerJournal
let version =
    let v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
    seq {
        string v.Major
        if v.Minor > 0 then
            $".{v.Minor}"
            if v.Revision > 0 then $".{v.Revision}"
    } |> Seq.reduce (+)
