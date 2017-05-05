module MyPrayerJournal.Views

//open Suave.Html
open Suave.Xml

type UserContext = { Id: string option }

[<AutoOpen>]
module Tags =
  /// Generate a meta tag
  let meta attr = tag "meta" attr empty

  /// Generate a link to a stylesheet
  let stylesheet url = linkAttr [ "rel", "stylesheet"; "href", url ]

  let aAttr attr x = tag "a" attr (flatten x)
  let a = aAttr []
  let buttonAttr attr x = tag "button" attr (flatten x)
  let button = buttonAttr []

  let footerAttr attr x = tag "footer" attr (flatten x)
  let footer = footerAttr []
  let ulAttr attr x = tag "ul" attr (flatten x)
  let ul = ulAttr []

  /// Used to prevent a self-closing tag where we need no text
  let noText = text ""
  let navLinkAttr attr url linkText = aAttr (("href", url) :: attr) [ text linkText ]

  let navLink = navLinkAttr []

  let jsLink func linkText = navLinkAttr [ "onclick", func ] "javascript:void(0)" linkText 

  /// Create a link to a JavaScript file
  let js src = scriptAttr [ "src", src ] [ noText ]

[<AutoOpen>]
module PageComponents =
  let prependDoctype document = sprintf "<!DOCTYPE html>\n%s" document
  let render = xmlToString >> prependDoctype

  let navigation userCtx =
    [ 
      match userCtx.Id with
      | Some _ ->
          yield navLink Route.journal "Journal"
          yield navLink Route.User.logOff "Log Off"
      | _ -> yield jsLink "mpj.signIn()" "Log On"
      
      ]
    |> List.map (fun x -> tag "li" [] x)
  let pageHeader userCtx =
    divAttr [ "class", "navbar navbar-inverse navbar-fixed-top" ] [
      divAttr [ "class", "container" ] [
        divAttr [ "class", "navbar-header" ] [
          buttonAttr [ "class", "navbar-toggle"; "data-toggle", "collapse"; "data-target", ".navbar-collapse" ] [
            spanAttr [ "class", "sr-only" ] (text "Toggle navigation")
            spanAttr [ "class", "icon-bar" ] noText
            spanAttr [ "class", "icon-bar" ] noText
            spanAttr [ "class", "icon-bar" ] noText
            ]
          navLinkAttr [ "class", "navbar-brand" ] "/" "myPrayerJournal"
          ]
        divAttr [ "class", "navbar-collapse collapse" ] [
          ulAttr [ "class", "nav navbar-nav navbar-right" ] (navigation userCtx)
          ]
        ]
      ]
  let pageFooter =
    footerAttr [ "class", "mpj-footer" ] [
      pAttr [ "class", "text-right" ] [
        text "myPrayerJournal v0.8.1"
        ]
      ]
  let row = divAttr [ "class", "row" ]

  let fullRow xml =
    row [ divAttr [ "class", "col-xs-12" ] xml ]

/// Display a page
let page userCtx content =
  html [
    head [
      meta [ "charset", "UTF-8" ]
      meta [ "name", "viewport"; "content", "width=device-width, initial-scale=1" ]
      title "myPrayerJournal"
      stylesheet "https://ajax.aspnetcdn.com/ajax/bootstrap/3.3.6/css/bootstrap.min.css"
      stylesheet "/content/styles.css"
      stylesheet "https://fonts.googleapis.com/icon?family=Material+Icons"
    ]
    body [
      pageHeader userCtx
      divAttr [ "class", "container body-content" ] [
        content
        pageFooter
        ]
      js "https://cdn.auth0.com/js/lock/10.14/lock.min.js"
      js "/js/mpj.js"
      ]
    ]
  |> render

let home =
  fullRow [
    p [ text "&nbsp;"]
    p [ text "myPrayerJournal is a place where individuals can record their prayer requests, record that they prayed for them, update them as God moves in the situation, and record a final answer received on that request.  It will also allow individuals to review their answered prayers." ]
    p [ text "This site is currently in very limited alpha, as it is being developed with a core group of test users.  If this is something you are interested in using, check back around mid-February 2017 to check on the development progress." ]
    ]

let journal (reqs : Request list) =
  fullRow [
    p [ text "journal goes here" ]
  ]
