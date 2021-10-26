/// Layout / home views
module MyPrayerJournal.Views.Layout

// fsharplint:disable RecordFieldNames

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Accessibility

/// The data needed to render a page-level view
type PageRenderContext = {
  /// Whether the user is authenticated
  isAuthenticated : bool
  /// Whether the user has snoozed requests
  hasSnoozed      : bool
  /// The current URL
  currentUrl      : string
  /// The title for the page to be rendered
  pageTitle       : string
  /// The content of the page
  content         : XmlNode
  }

/// The home page
let home = article [ _class "container mt-3" ] [
  p [] [ rawText "&nbsp;" ]
  p [] [
    str "myPrayerJournal is a place where individuals can record their prayer requests, record that they prayed for "
    str "them, update them as God moves in the situation, and record a final answer received on that request. It also "
    str "allows individuals to review their answered prayers."
    ]
  p [] [
    str "This site is open and available to the general public. To get started, simply click the "
    rawText "&ldquo;Log On&rdquo; link above, and log on with either a Microsoft or Google account. You can also "
    rawText "learn more about the site at the &ldquo;Docs&rdquo; link, also above."
    ]
  ]

/// The default navigation bar, which will load the items on page load, and whenever a refresh event occurs
let private navBar ctx =
  nav [ _class "navbar navbar-dark"; _roleNavigation ] [
    div [ _class "container-fluid" ] [
      pageLink "/" [ _class "navbar-brand" ] [
        span [ _class "m" ] [ str "my" ]
        span [ _class "p" ] [ str "Prayer" ]
        span [ _class "j" ] [ str "Journal" ]
      ]
      seq {
        let navLink (matchUrl : string) =
          match ctx.currentUrl.StartsWith matchUrl with true -> [ _class "is-active-route" ] | false -> []
          |> pageLink matchUrl
        match ctx.isAuthenticated with
        | true ->
            li [ _class "nav-item" ] [ navLink "/journal" [ str "Journal" ] ]
            li [ _class "nav-item" ] [ navLink "/requests/active" [ str "Active" ] ]
            if ctx.hasSnoozed then li [ _class "nav-item" ] [ navLink "/requests/snoozed" [ str "Snoozed" ] ]
            li [ _class "nav-item" ] [ navLink "/requests/answered" [ str "Answered" ] ]
            li [ _class "nav-item" ] [ a [ _href "/user/log-off" ] [ str "Log Off" ] ]
        | false -> li [ _class "nav-item"] [ a [ _href "/user/log-on" ] [ str "Log On" ] ]
        li [ _class "nav-item" ] [
          a [ _href "https://docs.prayerjournal.me"; _target "_blank"; _rel "noopener" ] [ str "Docs" ]
          ]
        }
      |> List.ofSeq
      |> ul [ _class "navbar-nav me-auto d-flex flex-row" ]
      ]
    ]

/// The title tag with the application name appended
let titleTag ctx = title [] [ str ctx.pageTitle; rawText " &#xab; myPrayerJournal" ]

/// The HTML `head` element
let htmlHead ctx =
  head [ _lang "en" ] [
    meta [ _name "viewport"; _content "width=device-width, initial-scale=1" ]
    meta [ _name "description"; _content "Online prayer journal - free w/Google or Microsoft account" ]
    titleTag ctx
    link [
      _href        "https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/css/bootstrap.min.css"
      _rel         "stylesheet"
      _integrity   "sha384-EVSTQN3/azprG1Anm3QDgpJLIm9Nao0Yz1ztcQTwFspd3yD65VohhpuuCOmLASjC"
      _crossorigin "anonymous"
      ]
    link [ _href "https://fonts.googleapis.com/icon?family=Material+Icons"; _rel "stylesheet" ]
    link [ _href "/style/style.css"; _rel "stylesheet" ]
    ]

/// Element used to display toasts
let toaster =
  div [ _ariaLive "polite"; _ariaAtomic "true"; _id "toastHost" ] [
    div [ _class "toast-container position-absolute p-3 bottom-0 end-0"; _id "toasts" ] []
  ]

/// The page's `footer` element
let htmlFoot =
  footer [ _class "container-fluid" ] [
    p [ _class "text-muted text-end" ] [
      str "myPrayerJournal v3"
      br []
      em [] [
        small [] [
          pageLink "/legal/privacy-policy" [] [ str "Privacy Policy" ]
          rawText " &bull; "
          pageLink "/legal/terms-of-service" [] [ str "Terms of Service" ]
          rawText " &bull; "
          a [ _href "https://github.com/bit-badger/myprayerjournal"; _target "_blank"; _rel "noopener" ] [
            str "Developed"
            ]
          str " and hosted by "
          a [ _href "https://bitbadger.solutions"; _target "_blank"; _rel "noopener" ] [ str "Bit Badger Solutions" ]
          ]
        ]
      ]
    script [
      _src         "https://unpkg.com/htmx.org@1.5.0"
      _integrity   "sha384-oGA+prIp5Vchu6we2YkI51UtVzN9Jpx2Z7PnR1I78PnZlN8LkrCT4lqqqmDkyrvI"
      _crossorigin "anonymous"
      ] []
    script [] [
      rawText "if (!htmx) document.write('<script src=\"/script/htmx-1.5.0.min.js\"><\/script>')"
      ]
    script [
      _async
      _src         "https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/js/bootstrap.bundle.min.js"
      _integrity   "sha384-MrcW6ZMFYlzcLA8Nl+NtUVF0sA7MsXsP1UyJoMp4YLEuNSfAP+JcXn/tWtIaxVXM"
      _crossorigin "anonymous"
      ] []
    script [] [
      rawText "setTimeout(function () { "
      rawText "if (!bootstrap) document.write('<script src=\"/script/bootstrap.bundle.min.js\"><\/script>') "
      rawText "}, 2000)"
      ]
    script [ _src "/script/mpj.js" ] []
    ]

/// Create the full view of the page
let view ctx =
  html [ _lang "en" ] [
    htmlHead ctx
    body [] [
      section [ _id "top" ] [ navBar ctx; main [ _roleMain ] [ ctx.content ] ]
      toaster
      htmlFoot
      ]
    ]

/// Create a partial view
let partial ctx =
  html [ _lang "en" ] [
    head [] [ titleTag ctx ]
    body [] [ navBar ctx; main [ _roleMain ] [ ctx.content ] ]
    ]
