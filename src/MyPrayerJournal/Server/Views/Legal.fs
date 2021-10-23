/// Views for legal pages
module MyPrayerJournal.Views.Legal

open Giraffe.ViewEngine

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
            rawText "The only identifying data myPrayerJournal stores is the subscriber (&ldquo;sub&rdquo;) field from "
            str "the token we receive from Auth0, once you have signed in through their hosted service. All "
            str "information is associated with you via this field."
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
            str "The data collected and stored is the absolute minimum necessary for the functionality of the service. "
            rawText "There are no plans to &ldquo;monetize&rdquo; this service, and storing the minimum amount of "
            str "information means that the data we have is not interesting to purchasers (or those who may have more "
            str "nefarious purposes)."
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
          str "are responsible to ensure that your use of this site complies with all applicable laws. Your continued "
          str "use of this site implies your acceptance of these terms."
          ]
        ]
      div [ _class "list-group-item" ] [
        h3 [] [ str "2. Description of Service and Registration" ]
        p [ _class "card-text" ] [
          str "myPrayerJournal is a service that allows individuals to enter and amend their prayer requests. It "
          str "requires no registration by itself, but access is granted based on a successful login with an external "
          str "identity provider. See "
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
          str "These terms and conditions may be updated at any time, and this service does not have the capability to "
          str "notify users when these change. The date at the top of the page will be updated when any of the text of "
          str "these terms is updated."
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

