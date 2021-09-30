module MyPrayerJournal.Views

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open System

let toMain = _hxTarget "main"

/// Views for legal pages
module Legal =
  
  /// View for the "Privacy Policy" page
  let privacyPolicy = article [] [
    div [ _class "card" ] [
      h5 [ _class "card-header" ] [ str "Privacy Policy" ]
      div [ _class "card-body" ] [
        h6 [ _class "card-subtitle text-muted" ] [ str "as of May 21"; sup [] [ str "st"]; str ", 2018" ]
        p [ _class "card-text" ] [
          str "The nature of the service is one where privacy is a must. The items below will help you understand "
          str "the data we collect, access, and store on your behalf as you use this service."
          ]
        hr []
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
        hr []
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
        ul [] [
          li [] [
            str "myPrayerJournal stores the information you provide, including the text of prayer requests, updates, "
            str "and notes; and the date/time when certain actions are taken."
            ]
          ]
        hr []
        h3 [] [ str "How Your Data Is Accessed / Secured" ]
        ul [] [
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
        hr []
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
  
  /// View for the "Terms of Service" page
  let termsOfService = article [ _class "container" ] [
    div [ _class "card" ] [
      h5 [ _class "card-header" ] [ str "Terms of Service" ]
      div [ _class "card-body" ] [
        h6 [ _class "card-subtitle text-muted"] [ str "as of May 21"; sup [] [ str "st" ]; str ", 2018" ]
        h3 [] [ str "1. Acceptance of Terms" ]
        p [ _class "card-text" ] [
          str "By accessing this web site, you are agreeing to be bound by these Terms and Conditions, and that you "
          str "are responsible to ensure that your use of this site complies with all applicable laws. Your continued "
          str "use of this site implies your acceptance of these terms."
          ]
        h3 [] [ str "2. Description of Service and Registration" ]
        p [ _class "card-text" ] [
          str "myPrayerJournal is a service that allows individuals to enter and amend their prayer requests. It "
          str "requires no registration by itself, but access is granted based on a successful login with an external "
          str "identity provider. See "
          a [ _href "/legal/privacy-policy"; _hxBoost; toMain ] [ str "our privacy policy" ]
          str " for details on how that information is accessed and stored."
          ]
        h3 [] [ str "3. Third Party Services" ]
        p [ _class "card-text" ] [
          str "This service utilizes a third-party service provider for identity management. Review the terms of "
          str "service for"
          a [ _href "https://auth0.com/terms"; _target "_blank" ] [ str "Auth0"]
          str ", as well as those for the selected authorization provider ("
          a [ _href "https://www.microsoft.com/en-us/servicesagreement"; _target "_blank" ] [ str "Microsoft"]
          str " or "
          a [ _href "https://policies.google.com/terms"; _target "_blank" ] [ str "Google" ]
          str ")."
          ]
        h3 [] [ str "4. Liability" ]
        p [ _class "card-text" ] [
          rawText "This service is provided &ldquo;as is&rdquo;, and no warranty (express or implied) exists. The "
          str "service and its developers may not be held liable for any damages that may arise through the use of "
          str "this service."
          ]
        h3 [] [ str "5. Updates to Terms" ]
        p [ _class "card-text" ] [
          str "These terms and conditions may be updated at any time, and this service does not have the capability to "
          str "notify users when these change. The date at the top of the page will be updated when any of the text of "
          str "these terms is updated."
          ]
        hr []
        p [ _class "card-text" ] [
          str "You may also wish to review our "
          a [ _href "/legal/privacy-policy"; _hxBoost; toMain ] [ str "privacy policy" ]
          str " to learn how we handle your data."
          ]
        ]
      ]
    ]

/// Views for navigation support
module Navigation =
  
  /// The default navigation bar, which will load the items on page load, and whenever a refresh event occurs
  let navBar =
    nav [ _class "navbar navbar-dark"; _hxBoost; toMain ] [
      div [ _class "container-fluid" ] [
        a [ _href "/"; _class "navbar-brand" ] [
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
          let deriveClass (matchUrl : string) css =
            match currUrl.StartsWith matchUrl with
            | true -> sprintf "%s is-active-route" css
            | false -> css
            |> _class
          li [ deriveClass "/journal" "nav-item" ] [ a [ _href "/journal" ] [ str "Journal" ] ]
          li [ deriveClass "/requests/active" "nav-item" ] [ a [ _href "/requests/active" ] [ str "Active" ] ]
          if hasSnoozed then
            li [ deriveClass "/requests/snoozed" "nav-item" ] [ a [ _href "/requests/snoozed" ] [ str "Snoozed" ] ]
          li [ deriveClass "/requests/answered" "nav-item" ] [ a [ _href "/requests/answered" ] [ str "Answered" ] ]
          li [ _class "nav-item" ] [ a [ _href "/user/log-off"; _onclick "logOff()" ] [ str "Log Off" ] ]
      | false -> li [ _class "nav-item"] [ a [ _href "/user/log-on"; _onclick "logOn()"] [ str "Log On" ] ]
      li [ _class "nav-item" ] [ a [ _href "https://docs.prayerjournal.me"; _target "_blank" ] [ str "Docs" ] ]
      }
    |> List.ofSeq


/// Layout views
module Layout =
  
  let htmlHead =
    head [] [
      link [
        _href        "https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/css/bootstrap.min.css"
        _rel         "stylesheet"
        _integrity   "sha384-EVSTQN3/azprG1Anm3QDgpJLIm9Nao0Yz1ztcQTwFspd3yD65VohhpuuCOmLASjC"
        _crossorigin "anonymous"
        ]
      link [ _href "/css/style.css"; _rel  "stylesheet" ]
      script [
        _src         "https://unpkg.com/htmx.org@1.5.0"
        _integrity   "sha384-oGA+prIp5Vchu6we2YkI51UtVzN9Jpx2Z7PnR1I78PnZlN8LkrCT4lqqqmDkyrvI"
        _crossorigin "anonymous"
        ] []
      ]

  let htmlFoot =
    footer [ _class "container-fluid"; _hxBoost; toMain ] [
      p [ _class "text-muted text-end" ] [
        str "myPrayerJournal v3"
        br []
        em [] [
          small [] [
            a [ _href "/legal/privacy-policy" ] [ str "Privacy Policy" ]
            rawText " &bull; "
            a [ _href "/legal/terms-of-service" ] [ str "Terms of Service" ]
            rawText " &bull; "
            a [ _href "https://github.com/bit-badger/myprayerjournal"; _target "_blank" ] [ str "Developed" ]
            str " and hosted by "
            a [ _href "https://bitbadger.solutions"; _target "_blank" ] [ str "Bit Badger Solutions" ]
            ]
          ]
        ]
      script [
        _src         "https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/js/bootstrap.bundle.min.js"
        _integrity   "sha384-MrcW6ZMFYlzcLA8Nl+NtUVF0sA7MsXsP1UyJoMp4YLEuNSfAP+JcXn/tWtIaxVXM"
        _crossorigin "anonymous"
        ] []
      ]

  let full content =
    html [ _lang "en" ] [
      htmlHead
      body [] [
        Navigation.navBar
        content
        htmlFoot
      ]
    ]
  
  let standard content =
    main [ _class "container" ] [ content ] |> full

  let wide content =
    main [ _class "container-fluid" ] [ content ] |> full

