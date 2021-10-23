/// Views for journal pages and components
module MyPrayerJournal.Views.Journal

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Accessibility
open Giraffe.ViewEngine.Htmx
open MyPrayerJournal

/// Display a card for this prayer request
let journalCard req =
  let reqId = RequestId.toString req.requestId
  let spacer = span [] [ rawText "&nbsp;" ]
  div [ _class "col" ] [
    div [ _class "card h-100" ] [
      div [ _class "card-header p-0 d-flex"; _roleToolBar ] [
        pageLink $"/request/{reqId}/edit" [ _class  "btn btn-secondary"; _title "Edit Request" ] [ icon "edit" ]
        spacer
        button [
          _type     "button"
          _class    "btn btn-secondary"
          _title    "Add Notes"
          _data     "bs-toggle" "modal"
          _data     "bs-target" "#notesModal"
          _hxGet    $"/components/request/{reqId}/add-notes"
          _hxTarget "#notesBody"
          _hxSwap   HxSwap.InnerHtml
          ] [ icon "comment" ]
        spacer
        //   md-button(@click.stop='snooze()').md-icon-button.md-raised
        //     md-icon schedule
        //     md-tooltip(md-direction='top'
        //                md-delay=1000) Snooze Request
        div [ _class "flex-grow-1" ] []
        button [
          _type    "button"
          _class   "btn btn-success w-25"
          _hxPatch $"/request/{reqId}/prayed"
          _title   "Mark as Prayed"
          ] [ icon "done" ]
        ]
      div [ _class "card-body" ] [
        p [ _class "request-text" ] [ str req.text ]
        ]
      div [ _class "card-footer text-end text-muted px-1 py-0" ] [
        em [] [ str "last activity "; relativeDate req.asOf ]
        ]
      ]
    ]

/// The journal loading page
let journal user = article [ _class "container-fluid mt-3" ] [
  h2 [ _class "pb-3" ] [
    str user
    match user with "Your" -> () | _ -> rawText "&rsquo;s"
    str " Prayer Journal"
    ]
  p [ _class "pb-3 text-center" ] [
    pageLink "/request/new/edit" [ _class "btn btn-primary "] [ icon "add_box"; str " Add a Prayer Request" ]
    ]
  p [ _hxGet "/components/journal-items"; _hxSwap HxSwap.OuterHtml; _hxTrigger HxTrigger.Load ] [
    rawText "Loading your prayer journal&hellip;"
    ]
  div [
    _id             "notesModal"
    _class          "modal fade"
    _tabindex       "-1"
    _ariaLabelledBy "nodesModalLabel"
    _ariaHidden     "true"
    ] [
    div [ _class "modal-dialog modal-dialog-scrollable" ] [
      div [ _class "modal-content" ] [
        div [ _class "modal-header" ] [
          h5 [ _class "modal-title"; _id "nodesModalLabel" ] [ str "Add Notes to Prayer Request" ]
          button [ _type "button"; _class "btn-close"; _data "bs-dismiss" "modal"; _ariaLabel "Close" ] []
          ]
        div [ _class "modal-body"; _id "notesBody" ] [ ]
        div [ _class "modal-footer" ] [
          button [ _type "button"; _class "btn btn-secondary"; _data "bs-dismiss" "modal" ] [ str "Close" ]
          ]
        ]
      ]
    ]
  ]

/// The journal items
let journalItems items =
  match items |> List.isEmpty with
  | true ->
      noResults "No Active Requests" "/request/new/edit" "Add a Request" [
        rawText "You have no requests to be shown; see the &ldquo;Active&rdquo; link above for snoozed or deferred "
        rawText "requests, and the &ldquo;Answered&rdquo; link for answered requests"
        ]
  | false ->
      items
      |> List.map journalCard
      |> section [
          _class    "row row-cols-1 row-cols-md-2 row-cols-lg-3 row-cols-xl-4 g-3"
          _hxTarget "this"
          _hxSwap   HxSwap.OuterHtml
          ]

/// The notes edit modal body
let notesEdit requestId =
  let reqId = RequestId.toString requestId
  [ form [ _hxPost $"/request/{reqId}/note"; _hxTarget "#top" ] [
      str "TODO"
      button [ _type "submit"; _class "btn btn-primary" ] [ str "Add Notes" ]
      ]
    div [ _id "priorNotes" ] [
      p [ _class "text-center pt-5" ] [
        button [
          _type     "button"
          _class    "btn btn-secondary"
          _hxGet    $"/components/request/{reqId}/notes"
          _hxSwap   HxSwap.OuterHtml
          _hxTarget "#priorNotes"
          ] [str "Load Prior Notes" ]
        ]
      ]
    ]
