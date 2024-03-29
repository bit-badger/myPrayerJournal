/// Layout / home views
module MyPrayerJournal.Views.Layout

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Accessibility

/// The data needed to render a page-level view
type PageRenderContext =
    {   /// Whether the user is authenticated
        IsAuthenticated : bool
        
        /// Whether the user has snoozed requests
        HasSnoozed      : bool
        
        /// The current URL
        CurrentUrl      : string
        
        /// The title for the page to be rendered
        PageTitle       : string
        
        /// The content of the page
        Content         : XmlNode
    }

/// The home page
let home =
    article [ _class "container mt-3" ] [
        p [] [ rawText "&nbsp;" ]
        p [] [
            str "myPrayerJournal is a place where individuals can record their prayer requests, record that they "
            str "prayed for them, update them as God moves in the situation, and record a final answer received on "
            str "that request. It also allows individuals to review their answered prayers."
        ]
        p [] [
            str "This site is open and available to the general public. To get started, simply click the "
            rawText "&ldquo;Log On&rdquo; link above, and log on with either a Microsoft or Google account. You can "
            rawText "also learn more about the site at the &ldquo;Docs&rdquo; link, also above."
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
                    match ctx.CurrentUrl.StartsWith matchUrl with true -> [ _class "is-active-route" ] | false -> []
                    |> pageLink matchUrl
                if ctx.IsAuthenticated then
                    li [ _class "nav-item" ] [ navLink "/journal" [ str "Journal" ] ]
                    li [ _class "nav-item" ] [ navLink "/requests/active" [ str "Active" ] ]
                    if ctx.HasSnoozed then li [ _class "nav-item" ] [ navLink "/requests/snoozed" [ str "Snoozed" ] ]
                    li [ _class "nav-item" ] [ navLink "/requests/answered" [ str "Answered" ] ]
                    li [ _class "nav-item" ] [ a [ _href "/user/log-off" ] [ str "Log Off" ] ]
                else li [ _class "nav-item"] [ a [ _href "/user/log-on" ] [ str "Log On" ] ]
                li [ _class "nav-item" ] [
                    a [ _href "https://docs.prayerjournal.me"; _target "_blank"; _rel "noopener" ] [ str "Docs" ]
                ]
            }
            |> List.ofSeq
            |> ul [ _class "navbar-nav me-auto d-flex flex-row" ]
        ]
    ]

/// The title tag with the application name appended
let titleTag ctx =
    title [] [ str ctx.PageTitle; rawText " &#xab; myPrayerJournal" ]

/// The HTML `head` element
let htmlHead ctx =
    head [ _lang "en" ] [
        meta [ _name "viewport"; _content "width=device-width, initial-scale=1" ]
        meta [ _name "description"; _content "Online prayer journal - free w/Google or Microsoft account" ]
        titleTag ctx
        link [ _href        "https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css"
               _rel         "stylesheet"
               _integrity   "sha384-T3c6CoIi6uLrA9TneNEoa7RxnatzjcDSCmG1MXxSR1GAsXEV/Dwwykc2MPK8M2HN"
               _crossorigin "anonymous" ]
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
            str $"myPrayerJournal {version}"
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
                    a [ _href "https://bitbadger.solutions"; _target "_blank"; _rel "noopener" ] [
                        str "Bit Badger Solutions"
                    ]
                ]
            ]
        ]
        Htmx.Script.minified
        script [] [
            rawText "if (!htmx) document.write('<script src=\"/script/htmx.min.js\"><\/script>')"
        ]
        script [ _async
                 _src         "https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"
                 _integrity   "sha384-C6RzsynM9kWDrMNeT87bh95OGNyZPhcTNXj1NW7RuBCsyN/o0jlpcV8Qyq46cDfL"
                 _crossorigin "anonymous" ] []
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
            section [ _id "top"; _ariaLabel "Top navigation" ] [ navBar ctx; main [ _roleMain ] [ ctx.Content ] ]
            toaster
            htmlFoot
        ]
    ]

/// Create a partial view
let partial ctx =
    html [ _lang "en" ] [
        head [] [ titleTag ctx ]
        body [] [ navBar ctx; main [ _roleMain ] [ ctx.Content ] ]
    ]
