module MyPrayerJournal.Views

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open System

[<AutoOpen>]
module Helpers =
  
  /// Create a link that targets the `main` element and pushes a URL to history
  let pageLink href attrs = a (attrs |> List.append [ _href href; _hxBoost; _hxTarget "main"; _hxPushUrl ])

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
    span [ _title (date.ToString "f") ] [ Dates.formatDistance DateTime.Now date |> str ]


/// Views for home and log on pages
module Home =
  
  /// The home page
  let home = article [ _class "container mt-3" ] [
    p [] [ rawText "&nbsp;" ]
    p [] [
      str "myPrayerJournal is a place where individuals can record their prayer requests, record that they prayed for "
      str "them, update them as God moves in the situation, and record a final answer received on that request. It "
      str "also allows individuals to review their answered prayers."
      ]
    p [] [
      str "This site is open and available to the general public. To get started, simply click the "
      rawText "&ldquo;Log On&rdquo; link above, and log on with either a Microsoft or Google account. You can also "
      rawText "learn more about the site at the &ldquo;Docs&rdquo; link, also above."
      ]
    ]
  
  /// The log on page
  let logOn = article [ _class "container mt-3" ] [
    p [] [
      em [] [ str "Verifying..." ]
      ]
    ]


/// Views for legal pages
module Legal =
  
  /// View for the "Privacy Policy" page
  let privacyPolicy = article [ _class "container mt-3" ] [
    h2 [ _class "mb-2" ] [ str "Privacy Policy" ]
    h6 [ _class "text-muted pb-3" ] [ str "as of May 21"; sup [] [ str "st"]; str ", 2018" ]
    p [] [
      str "The nature of the service is one where privacy is a must. The items below will help you understand the data "
      str "we collect, access, and store on your behalf as you use this service."
      ]
    div [ _class "card" ] [
      div [ _class "list-group list-group-flush" ] [
        div [ _class "list-group-item"] [
          h3 [] [ str "Third Party Services" ]
          p [ _class "card-text" ] [
            str "myPrayerJournal utilizes a third-party authentication and identity provider. You should familiarize "
            str "yourself with the privacy policy for "
            a [ _href "https://auth0.com/privacy"; _target "_blank" ] [ str "Auth0" ]
            str ", as well as your chosen provider ("
            a [ _href "https://privacy.microsoft.com/en-us/privacystatement"; _target "_blank" ] [ str "Microsoft"]
            str " or "
            a [ _href "https://policies.google.com/privacy"; _target "_blank" ] [ str "Google" ]
            str ")."
            ]
          ]
        div [ _class "list-group-item" ] [
          h3 [] [ str "What We Collect" ]
          h4 [] [ str "Identifying Data" ]
          ul [] [
            li [] [
              rawText "The only identifying data myPrayerJournal stores is the subscriber (&ldquo;sub&rdquo;) field "
              str "from the token we receive from Auth0, once you have signed in through their hosted service. "
              str "All information is associated with you via this field."
              ]
            li [] [
              str "While you are signed in, within your browser, the service has access to your first and last names, "
              str "along with a URL to the profile picture (provided by your selected identity provider). This "
              rawText "information is not transmitted to the server, and is removed when &ldquo;Log Off&rdquo; is "
              str "clicked."
              ]
            ]
          h4 [] [ str "User Provided Data" ]
          ul [ _class "mb-0" ] [
            li [] [
              str "myPrayerJournal stores the information you provide, including the text of prayer requests, updates, "
              str "and notes; and the date/time when certain actions are taken."
              ]
            ]
          ]
        div [ _class "list-group-item" ] [
          h3 [] [ str "How Your Data Is Accessed / Secured" ]
          ul [ _class "mb-0" ] [
            li [] [
              str "Your provided data is returned to you, as required, to display your journal or your answered "
              str "requests. On the server, it is stored in a controlled-access database."
              ]
            li [] [
              str "Your data is backed up, along with other Bit Badger Solutions hosted systems, in a rolling manner; "
              str "backups are preserved for the prior 7 days, and backups from the 1"
              sup [] [ str "st" ]
              str " and 15"
              sup [] [ str "th" ]
              str " are preserved for 3 months. These backups are stored in a private cloud data repository."
              ]
            li [] [
              str "The data collected and stored is the absolute minimum necessary for the functionality of the "
              rawText "service. There are no plans to &ldquo;monetize&rdquo; this service, and storing the minimum "
              str "amount of information means that the data we have is not interesting to purchasers (or those who "
              str "may have more nefarious purposes)."
              ]
            li [] [
              str "Access to servers and backups is strictly controlled and monitored for unauthorized access attempts."
              ]
            ]
          ]
        div [ _class "list-group-item" ] [
          h3 [] [ str "Removing Your Data" ]
          p [ _class "card-text" ] [
            str "At any time, you may choose to discontinue using this service. Both Microsoft and Google provide ways "
            str "to revoke access from this application. However, if you want your data removed from the database, "
            str "please contact daniel at bitbadger.solutions (via e-mail, replacing at with @) prior to doing so, to "
            str "ensure we can determine which subscriber ID belongs to you."
            ]
          ]
        ]
      ]
    ]
  
  /// View for the "Terms of Service" page
  let termsOfService = article [ _class "container mt-3" ] [
    h2 [ _class "mb-2" ] [ str "Terms of Service" ]
    h6 [ _class "text-muted pb-3"] [ str "as of May 21"; sup [] [ str "st" ]; str ", 2018" ]
    div [ _class "card" ] [
      div [ _class "list-group list-group-flush" ] [
        div [ _class "list-group-item" ] [
          h3 [] [ str "1. Acceptance of Terms" ]
          p [ _class "card-text" ] [
            str "By accessing this web site, you are agreeing to be bound by these Terms and Conditions, and that you "
            str "are responsible to ensure that your use of this site complies with all applicable laws. Your "
            str "continued use of this site implies your acceptance of these terms."
            ]
          ]
        div [ _class "list-group-item" ] [
          h3 [] [ str "2. Description of Service and Registration" ]
          p [ _class "card-text" ] [
            str "myPrayerJournal is a service that allows individuals to enter and amend their prayer requests. It "
            str "requires no registration by itself, but access is granted based on a successful login with an "
            str "external identity provider. See "
            pageLink "/legal/privacy-policy" [] [ str "our privacy policy" ]
            str " for details on how that information is accessed and stored."
            ]
          ]
        div [ _class "list-group-item" ] [
          h3 [] [ str "3. Third Party Services" ]
          p [ _class "card-text" ] [
            str "This service utilizes a third-party service provider for identity management. Review the terms of "
            str "service for "
            a [ _href "https://auth0.com/terms"; _target "_blank" ] [ str "Auth0"]
            str ", as well as those for the selected authorization provider ("
            a [ _href "https://www.microsoft.com/en-us/servicesagreement"; _target "_blank" ] [ str "Microsoft"]
            str " or "
            a [ _href "https://policies.google.com/terms"; _target "_blank" ] [ str "Google" ]
            str ")."
            ]
          ]
        div [ _class "list-group-item" ] [
          h3 [] [ str "4. Liability" ]
          p [ _class "card-text" ] [
            rawText "This service is provided &ldquo;as is&rdquo;, and no warranty (express or implied) exists. The "
            str "service and its developers may not be held liable for any damages that may arise through the use of "
            str "this service."
            ]
          ]
        div [ _class "list-group-item" ] [
          h3 [] [ str "5. Updates to Terms" ]
          p [ _class "card-text" ] [
            str "These terms and conditions may be updated at any time, and this service does not have the capability "
            str "to notify users when these change. The date at the top of the page will be updated when any of the "
            str "text of these terms is updated."
            ]
          ]
        ]
      ]
    p [ _class "pt-3" ] [
      str "You may also wish to review our "
      pageLink "/legal/privacy-policy" [] [ str "privacy policy" ]
      str " to learn how we handle your data."
      ]
    ]


/// Views for navigation support
module Navigation =
  
  /// The default navigation bar, which will load the items on page load, and whenever a refresh event occurs
  let navBar =
    nav [ _class "navbar navbar-dark" ] [
      div [ _class "container-fluid" ] [
        pageLink "/" [ _class "navbar-brand" ] [
          span [ _class "m" ] [ str "my" ]
          span [ _class "p" ] [ str "Prayer" ]
          span [ _class "j" ] [ str "Journal" ]
        ]
        ul [
          _class     "navbar-nav me-auto d-flex flex-row"
          _hxGet     "/components/nav-items"
          _hxTarget  ".navbar-nav"
          _hxTrigger (sprintf "%s, menu-refresh from:body" HxTrigger.Load)
          ] [ ]
        ]
      ]

  /// Generate the navigation items based on the current state
  let currentNav isAuthenticated hasSnoozed (url : Uri option) =
    seq {
      match isAuthenticated with
      | true ->
          let currUrl = match url with Some u -> (u.PathAndQuery.Split '?').[0] | None -> ""
          let navLink (matchUrl : string) =
            match currUrl.StartsWith matchUrl with true -> [ _class "is-active-route" ] | false -> []
            |> pageLink matchUrl
          li [ _class "nav-item" ] [ navLink "/journal" [ str "Journal" ] ]
          li [ _class "nav-item" ] [ navLink "/requests/active" [ str "Active" ] ]
          if hasSnoozed then li [ _class "nav-item" ] [ navLink "/requests/snoozed" [ str "Snoozed" ] ]
          li [ _class "nav-item" ] [ navLink "/requests/answered" [ str "Answered" ] ]
          li [ _class "nav-item" ] [ a [ _href "/user/log-off"; _onclick "mpj.logOff(event)" ] [ str "Log Off" ] ]
      | false -> li [ _class "nav-item"] [ a [ _href "/user/log-on"; _onclick "mpj.logOn(event)"] [ str "Log On" ] ]
      li [ _class "nav-item" ] [ a [ _href "https://docs.prayerjournal.me"; _target "_blank" ] [ str "Docs" ] ]
      }
    |> List.ofSeq


/// Views for journal pages and components
module Journal =

  /// The journal loading page
  let journal user = article [ _class "container-fluid mt-3" ] [
    h2 [ _class "pb-3" ] [ str user; rawText "&rsquo;s Prayer Journal" ]
    p [
      _hxGet     "/components/journal-items"
      _hxSwap    HxSwap.OuterHtml
      _hxTrigger HxTrigger.Load
      ] [ rawText "Loading your prayer journal&hellip;" ]
    ]

  /// The journal items
  let journalItems items =
    match items |> List.isEmpty with
    | true ->
        noResults "No Active Requests" "/request/new/edit" "Add a Request" [
          rawText "You have no requests to be shown; see the &ldquo;Active&rdquo; link above for snoozed or "
          rawText "deferred requests, and the &ldquo;Answered&rdquo; link for answered requests"
          ]
    | false -> p [] [ str "There are requests" ]


/// Views for request pages and components
module Request =

  /// Create a request within the list
  let reqListItem req =
    let jsNow      = int64 (DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds
    let reqId      = RequestId.toString req.requestId
    let isAnswered = req.lastStatus = Answered
    let isSnoozed  = Ticks.toLong req.snoozedUntil > jsNow
    let isPending  = (not isSnoozed) && Ticks.toLong req.showAfter > jsNow
    let btnClass   = _class "btn btn-light mx-2"
    div [
      _class    "list-group-item px-0 d-flex flex-row align-items-start"
      _hxTarget "this"
      _hxSwap   HxSwap.OuterHtml
      ] [
      pageLink $"/request/{reqId}/full" [ btnClass; _title "View Full Request" ] [ icon "description" ]
      if not isAnswered then
        button [ btnClass; _hxGet $"/components/request/{reqId}/edit"; _title "Edit Request" ] [ icon "edit" ]
      // TODO: these next two should use hx-patch, targeting replacement of this tr when complete
      if isSnoozed then
        pageLink $"/request/{reqId}/cancel-snooze" [ btnClass; _title "Cancel Snooze" ] [ icon "restore" ]
      if isPending then
        pageLink $"/request/{reqId}/show-now" [ btnClass; _title "Show Now" ] [ icon "restore" ]
      p [ _class "mpj-request-text mb-0" ] [
        str req.text
        if isSnoozed || isPending || isAnswered then
          br []
          small [ _class "text-muted" ] [
            em [] [
              if isSnoozed then
                str "Snooze expires "
                relativeDate req.snoozedUntil
              if isPending then
                str "Request appears next "
                relativeDate req.showAfter
              if isAnswered then
                str "Answered "
                relativeDate req.asOf
              ]
            ]
        ]
      ]
  
  /// Create a list of requests
  let reqList reqs =
    reqs
    |> List.map reqListItem
    |> div [ _class "list-group" ]
  
  /// View for Active Requests page
  let active reqs = article [ _class "container mt-3" ] [
    h2 [ _class "pb-3" ] [ str "Active Requests" ]
    match reqs |> List.isEmpty with
    | true ->
        noResults "No Active Requests" "/journal" "Return to your journal"
          [ str "Your prayer journal has no active requests" ]
    | false -> reqList reqs
    ]

  /// View for Answered Requests page
  let answered reqs = article [ _class "container mt-3" ] [
    h2 [ _class "pb-3" ] [ str "Answered Requests" ]
    match reqs |> List.isEmpty with
    | true ->
        noResults "No Active Requests" "/journal" "Return to your journal" [
          rawText "Your prayer journal has no answered requests; once you have marked one as &ldquo;Answered&rdquo;, "
          str "it will appear here"
          ]
    | false -> reqList reqs
    ]

  /// View for Snoozed Requests page
  let snoozed reqs = article [ _class "container mt-3" ] [
    h2 [ _class "pb-3" ] [ str "Snoozed Requests" ]
    reqList reqs
    ]

  /// View for Full Request page
  let full (req : Request) =
    let answered =
      req.history
      |> List.filter RequestAction.isAnswered
      |> List.tryHead
      |> Option.map (fun x -> x.asOf)
    let prayed = req.history |> List.filter RequestAction.isPrayed |> List.length
    let daysOpen =
      let asOf = answered |> Option.map fromJs |> Option.defaultValue DateTime.Now
      (asOf - fromJs (req.history |> List.filter RequestAction.isCreated |> List.head).asOf).TotalDays |> int
    let lastText =
      req.history
      |> List.filter (fun h -> Option.isSome h.text)
      |> List.sortByDescending (fun h -> Ticks.toLong h.asOf)
      |> List.map (fun h -> Option.get h.text)
      |> List.head
    // The history log including notes (and excluding the final entry for answered requests)
    let log =
      let toDisp (h : History) = {| asOf = fromJs h.asOf; text = h.text; status = RequestAction.toString h.status |}
      let all =
        req.notes
        |> List.map (fun n -> {| asOf = fromJs n.asOf; text = Some n.notes; status = "Notes" |})
        |> List.append (req.history |> List.map toDisp)
        |> List.sortByDescending (fun it -> it.asOf)
      // Skip the first entry for answered requests; that info is already displayed
      match answered with Some _ -> all |> List.skip 1 | None -> all
    article [ _class "container mt-3" ] [
      div [_class "card" ] [
        h5 [ _class "card-header" ] [ str "Full Prayer Request" ]
        div [ _class "card-body" ] [
          h6 [ _class "card-subtitle text-muted mb-2"] [
            match answered with
            | Some ticks ->
                str "Answered "
                (fromJs ticks).ToString "D" |> str
                str " ("
                relativeDate ticks
                rawText ") &bull; "
            | None -> ()
            sprintf "Prayed %i times &bull; Open %i days" prayed daysOpen |> rawText
            ]
          p [ _class "card-text" ] [ str lastText ]
          ]
        log
        |> List.map (fun it -> li [ _class "list-group-item" ] [
          p [ _class "m-0" ] [
            str it.status
            rawText "&nbsp; "
            small [] [ em [] [ it.asOf.ToString "D" |> str ] ]
            ]
          match it.text with
          | Some txt -> p [ _class "mt-2 mb-0" ] [ str txt ]
          | None -> ()
        ])
        |> ul [ _class "list-group list-group-flush" ]
        ]
      ]

  /// View for the edit request component
  let edit (req : JournalRequest) isNew =
    let cancelUrl = req.requestId |> (RequestId.toString >> sprintf "/components/request/%s/item")
    section [ _class "container list-group-item"; _hxTarget "this"; _hxSwap HxSwap.OuterHtml ] [
      h5 [ _class "pb-3" ] [ (match isNew with true -> "Add" | false -> "Edit") |> strf "%s Prayer Request" ]
      form [ "/request" |> match isNew with true -> _hxPost | false -> _hxPatch ] [
        input [
          _type  "hidden"
          _name  "requestId"
          _value (match isNew with true -> "new" | false -> RequestId.toString req.requestId)
          ]
        div [ _class "form-floating pb-3" ] [
          textarea [
            _id          "requestText"
            _name        "requestText"
            _class       "form-control"
            _style       "min-height: 8rem;"
            _placeholder "Enter the text of the request"
            _autofocus;  _required
            ] [ str req.text ]
          label [ _for "requestText" ] [ str "Prayer Request" ]
          ]
        br []
        match isNew with
        | true -> ()
        | false ->
            div [ _class "pb-3" ] [
              label [] [ str "Also Mark As" ]
              br []
              div [ _class "form-check form-check-inline" ] [
                input [ _type "radio"; _class "form-check-input"; _id "sU"; _name "status"; _value "Updated"; _checked ]
                label [ _for "sU" ] [ str "Updated" ]
                ]
              div [ _class "form-check form-check-inline" ] [
                input [ _type "radio"; _class "form-check-input"; _id "sP"; _name "status"; _value "Prayed" ]
                label [ _for "sP" ] [ str "Prayed" ]
                ]
              div [ _class "form-check form-check-inline" ] [
                input [ _type "radio"; _class "form-check-input"; _id "sA"; _name "status"; _value "Answered" ]
                label [ _for "sA" ] [ str "Answered" ]
                ]
              ]
        div [ _class "row" ] [
          div [ _class "col-12 offset-md-2 col-md-8 offset-lg-3 col-lg-6" ] [
            p [] [
              strong [] [ rawText "Recurrence &nbsp; " ]
              em [ _class "text-muted" ] [ rawText "After prayer, request reappears&hellip;" ]
              ]
            div [ _class "d-flex flex-row flex-wrap justify-content-center align-items-center" ] [
              div [ _class "form-check mx-2" ] [
                input [
                  _type    "radio"
                  _class   "form-check-input"
                  _id      "rI"
                  _name    "recurType"
                  _value   "Immediate"
                  _onclick "mpj.edit.toggleRecurrence(event)"
                  match req.recurType with Immediate -> _checked | _ -> ()
                  ]
                label [ _for "rI" ] [ str "Immediately" ]
                ]
              div [ _class "form-check mx-2"] [
                input [
                  _type    "radio"
                  _class   "form-check-input"
                  _id      "rO"
                  _name    "recurType"
                  _value   "Other"
                  _onclick "mpj.edit.toggleRecurrence(event)"
                  match req.recurType with Immediate -> () | _ -> _checked
                  ]
                label [ _for "rO" ] [ rawText "Every&hellip;" ]
                ]
              div [ _class "form-floating mx-2"] [
                input [
                  _type        "number"
                  _class       "form-control"
                  _id          "recurCount"
                  _name        "recurCount"
                  _placeholder "0"
                  _value       (string req.recurCount)
                  _style       "width:6rem;"
                  _required
                  match req.recurType with Immediate -> _disabled | _ -> ()
                  ]
                label [ _for "recurCount" ] [ str "Count" ]
                ]
              div [ _class "form-floating mx-2" ] [
                select [
                  _class    "form-control"
                  _id       "recurInterval"
                  _name     "recurInterval"
                  _style    "width:6rem;"
                  _required
                  match req.recurType with Immediate -> _disabled | _ -> ()
                  ] [
                  option [ _value "Hours"; match req.recurType with Hours -> _selected | _ -> () ] [ str "hours" ]
                  option [ _value "Days";  match req.recurType with Days  -> _selected | _ -> () ] [ str "days" ]
                  option [ _value "Weeks"; match req.recurType with Weeks -> _selected | _ -> () ] [ str "weeks" ]
                  ]
                label [ _form "recurInterval" ] [ str "Interval" ]
                ]
              ]
            ]
          ]
        div [ _class "text-end pt-3" ] [
          button [ _class "btn btn-primary me-2"; _type "submit" ] [ icon "save"; str " Save" ]
          a [ _class "btn btn-secondary ms-2"; _href cancelUrl; _hxGet cancelUrl ] [icon "arrow_back"; str " Cancel"]
          ]
        ]
      ]

/// Layout views
module Layout =

  open Giraffe.ViewEngine.Accessibility

  /// The HTML `head` element
  let htmlHead pageTitle =
    head [] [
      title [] [ str pageTitle; rawText " &#xab; myPrayerJournal" ]
      link [
        _href        "https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/css/bootstrap.min.css"
        _rel         "stylesheet"
        _integrity   "sha384-EVSTQN3/azprG1Anm3QDgpJLIm9Nao0Yz1ztcQTwFspd3yD65VohhpuuCOmLASjC"
        _crossorigin "anonymous"
        ]
      link [ _href "https://fonts.googleapis.com/icon?family=Material+Icons"; _rel "stylesheet" ]
      link [ _href "/style/style.css"; _rel "stylesheet" ]
      script [
        _src         "https://unpkg.com/htmx.org@1.5.0"
        _integrity   "sha384-oGA+prIp5Vchu6we2YkI51UtVzN9Jpx2Z7PnR1I78PnZlN8LkrCT4lqqqmDkyrvI"
        _crossorigin "anonymous"
        ] []
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
            a [ _href "https://github.com/bit-badger/myprayerjournal"; _target "_blank" ] [ str "Developed" ]
            str " and hosted by "
            a [ _href "https://bitbadger.solutions"; _target "_blank" ] [ str "Bit Badger Solutions" ]
            ]
          ]
        ]
      script [
        _async
        _src         "https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/js/bootstrap.bundle.min.js"
        _integrity   "sha384-MrcW6ZMFYlzcLA8Nl+NtUVF0sA7MsXsP1UyJoMp4YLEuNSfAP+JcXn/tWtIaxVXM"
        _crossorigin "anonymous"
        ] []
      script [ _src "https://cdn.auth0.com/js/auth0-spa-js/1.13/auth0-spa-js.production.js" ] []
      script [ _src "/script/mpj.js" ] []
      ]

  /// Create the full view of the page
  let view pageTitle content =
    html [ _lang "en" ] [
      htmlHead pageTitle
      body [ _hxHeaders "" ] [
        Navigation.navBar
        main [] [ content ]
        toaster
        htmlFoot
      ]
    ]

