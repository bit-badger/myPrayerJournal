/// Views for request pages and components
module MyPrayerJournal.Views.Request

open Giraffe.Htmx
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open MyPrayerJournal
open NodaTime

/// Create a request within the list
let reqListItem now tz req =
    let isFuture instant = defaultArg (instant |> Option.map (fun it -> it > now)) false
    let reqId      = RequestId.toString req.RequestId
    let isAnswered = req.LastStatus = Answered
    let isSnoozed  = isFuture req.SnoozedUntil
    let isPending  = (not isSnoozed) && isFuture req.ShowAfter
    let btnClass   = _class "btn btn-light mx-2"
    let restoreBtn (link : string) title =
        button [ btnClass; _hxPatch $"/request/{reqId}/{link}"; _title title ] [ icon "restore" ]
    div [ _class "list-group-item px-0 d-flex flex-row align-items-start"
          _hxTarget "this"
          _hxSwap HxSwap.OuterHtml ] [
        pageLink $"/request/{reqId}/full" [ btnClass; _title "View Full Request" ] [ icon "description" ]
        if not isAnswered then pageLink $"/request/{reqId}/edit" [ btnClass; _title "Edit Request" ] [ icon "edit" ]
        if   isSnoozed then restoreBtn "cancel-snooze" "Cancel Snooze"
        elif isPending then restoreBtn "show"          "Show Now"
        p [ _class "request-text mb-0" ] [
            str req.Text
            if isSnoozed || isPending || isAnswered then
                br []
                small [ _class "text-muted" ] [
                    if   isSnoozed then   [ str "Snooze expires ";       relativeDate req.SnoozedUntil.Value now tz ]
                    elif isPending then   [ str "Request appears next "; relativeDate req.ShowAfter.Value    now tz ]
                    else (* isAnswered *) [ str "Answered ";             relativeDate req.AsOf               now tz ]
                    |> em []
                ]
          ]
    ]

/// Create a list of requests
let reqList now tz reqs =
    reqs
    |> List.map (reqListItem now tz)
    |> div [ _class "list-group" ]

/// View for Active Requests page
let active now tz reqs =
    article [ _class "container mt-3" ] [
        h2 [ _class "pb-3" ] [ str "Active Requests" ]
        if List.isEmpty reqs then
            noResults "No Active Requests" "/journal" "Return to your journal"
                [ str "Your prayer journal has no active requests" ]
        else reqList now tz reqs
    ]

/// View for Answered Requests page
let answered now tz reqs =
    article [ _class "container mt-3" ] [
        h2 [ _class "pb-3" ] [ str "Answered Requests" ]
        if List.isEmpty reqs then
            noResults "No Answered Requests" "/journal" "Return to your journal" [
                str "Your prayer journal has no answered requests; once you have marked one as "
                rawText "&ldquo;Answered&rdquo;, it will appear here"
            ]
        else reqList now tz reqs
    ]

/// View for Snoozed Requests page
let snoozed now tz reqs =
    article [ _class "container mt-3" ] [
        h2 [ _class "pb-3" ] [ str "Snoozed Requests" ]
        reqList now tz reqs
    ]

/// View for Full Request page
let full (clock : IClock) tz (req : Request) =
    let now = clock.GetCurrentInstant ()
    let answered =
        req.History
        |> Seq.ofList
        |> Seq.filter History.isAnswered
        |> Seq.tryHead
        |> Option.map (fun x -> x.AsOf)
    let prayed = (req.History |> List.filter History.isPrayed |> List.length).ToString "N0"
    let daysOpen =
        let asOf = defaultArg answered now
        ((asOf - (req.History |> List.filter History.isCreated |> List.head).AsOf).TotalDays |> int).ToString "N0"
    let lastText =
        req.History
        |> Seq.ofList
        |> Seq.filter (fun h -> Option.isSome h.Text)
        |> Seq.sortByDescending (fun h -> h.AsOf)
        |> Seq.map (fun h -> Option.get h.Text)
        |> Seq.head
    // The history log including notes (and excluding the final entry for answered requests)
    let log =
        let toDisp (h : History) = {| asOf = h.AsOf; text = h.Text; status = RequestAction.toString h.Status |}
        let all =
            req.Notes
            |> Seq.ofList
            |> Seq.map (fun n -> {| asOf = n.AsOf; text = Some n.Notes; status = "Notes" |})
            |> Seq.append (req.History |> List.map toDisp)
            |> Seq.sortByDescending (fun it -> it.asOf)
            |> List.ofSeq
        // Skip the first entry for answered requests; that info is already displayed
        match answered with Some _ -> all.Tail | None -> all
    article [ _class "container mt-3" ] [
        div [_class "card" ] [
            h5 [ _class "card-header" ] [ str "Full Prayer Request" ]
            div [ _class "card-body" ] [
                h6 [ _class "card-subtitle text-muted mb-2"] [
                    match answered with
                    | Some date ->
                        str "Answered "
                        date.ToDateTimeOffset().ToString ("D", null) |> str
                        str " ("
                        relativeDate date now tz
                        rawText ") &bull; "
                    | None -> ()
                    rawText $"Prayed %s{prayed} times &bull; Open %s{daysOpen} days"
                ]
                p [ _class "card-text" ] [ str lastText ]
            ]
            log
            |> List.map (fun it ->
                li [ _class "list-group-item" ] [
                    p [ _class "m-0" ] [
                        str it.status
                        rawText "&nbsp; "
                        small [] [ em [] [ it.asOf.ToDateTimeOffset().ToString ("D", null) |> str ] ]
                    ]
                    match it.text with
                    | Some txt -> p [ _class "mt-2 mb-0" ] [ str txt ]
                    | None -> ()
                ])
            |> ul [ _class "list-group list-group-flush" ]
        ]
    ]

/// View for the edit request component
let edit (req : JournalRequest) returnTo isNew =
    let cancelLink =
        match returnTo with
        | "active"          -> "/requests/active"
        | "snoozed"         -> "/requests/snoozed"
        | _ (* "journal" *) -> "/journal"
    let recurCount =
        match req.Recurrence with
        | Immediate -> None
        | Hours   h -> Some h
        | Days    d -> Some d
        | Weeks   w -> Some w
        |> Option.map string
        |> Option.defaultValue ""
    article [ _class "container" ] [
        h2 [ _class "pb-3" ] [ (match isNew with true -> "Add" | false -> "Edit") |> strf "%s Prayer Request" ]
        form [ _hxBoost
               _hxTarget  "#top"
               _hxPushUrl "true"
               "/request" |> match isNew with true -> _hxPost | false -> _hxPatch ] [
            input [ _type  "hidden"
                    _name  "requestId"
                    _value (match isNew with true -> "new" | false -> RequestId.toString req.RequestId) ]
            input [ _type "hidden"; _name "returnTo"; _value returnTo ]
            div [ _class "form-floating pb-3" ] [
                textarea [ _id          "requestText"
                           _name        "requestText"
                           _class       "form-control"
                           _style       "min-height: 8rem;"
                           _placeholder "Enter the text of the request"
                           _autofocus;  _required ] [ str req.Text ]
                label [ _for "requestText" ] [ str "Prayer Request" ]
            ]
            br []
            if not isNew then
                div [ _class "pb-3" ] [
                    label [] [ str "Also Mark As" ]
                    br []
                    div [ _class "form-check form-check-inline" ] [
                        input [ _type  "radio"
                                _class "form-check-input"
                                _id    "sU"
                                _name  "status"
                                _value "Updated"
                                _checked ]
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
                            input [ _type    "radio"
                                    _class   "form-check-input"
                                    _id      "rI"
                                    _name    "recurType"
                                    _value   "Immediate"
                                    _onclick "mpj.edit.toggleRecurrence(event)"
                                    match req.Recurrence with Immediate -> _checked | _ -> () ]
                            label [ _for "rI" ] [ str "Immediately" ]
                        ]
                        div [ _class "form-check mx-2"] [
                            input [ _type    "radio"
                                    _class   "form-check-input"
                                    _id      "rO"
                                    _name    "recurType"
                                    _value   "Other"
                                    _onclick "mpj.edit.toggleRecurrence(event)"
                                    match req.Recurrence with Immediate -> () | _ -> _checked ]
                            label [ _for "rO" ] [ rawText "Every&hellip;" ]
                        ]
                        div [ _class "form-floating mx-2"] [
                            input [ _type        "number"
                                    _class       "form-control"
                                    _id          "recurCount"
                                    _name        "recurCount"
                                    _placeholder "0"
                                    _value       recurCount
                                    _style       "width:6rem;"
                                    _required
                                    match req.Recurrence with Immediate -> _disabled | _ -> () ]
                            label [ _for "recurCount" ] [ str "Count" ]
                        ]
                        div [ _class "form-floating mx-2" ] [
                            select [ _class    "form-control"
                                     _id       "recurInterval"
                                     _name     "recurInterval"
                                     _style    "width:6rem;"
                                     _required
                                     match req.Recurrence with Immediate -> _disabled | _ -> () ] [
                                option [ _value "Hours"; match req.Recurrence with Hours _ -> _selected | _ -> () ] [
                                    str "hours"
                                ]
                                option [ _value "Days";  match req.Recurrence with Days  _ -> _selected | _ -> () ] [
                                    str "days"
                                ]
                                option [ _value "Weeks"; match req.Recurrence with Weeks _ -> _selected | _ -> () ] [
                                    str "weeks"
                                ]
                            ]
                            label [ _form "recurInterval" ] [ str "Interval" ]
                        ]
                    ]
                ]
            ]
            div [ _class "text-end pt-3" ] [
                button [ _class "btn btn-primary me-2"; _type "submit" ] [ icon "save"; str " Save" ]
                pageLink cancelLink [ _class "btn btn-secondary ms-2" ] [ icon "arrow_back"; str " Cancel" ]
            ]
        ]
    ]

/// Display a list of notes for a request
let notes now tz notes =
    let toItem (note : Note) =
        p [] [ small [ _class "text-muted" ] [ relativeDate note.AsOf now tz ]; br []; str note.Notes ]
    [   p [ _class "text-center" ] [ strong [] [ str "Prior Notes for This Request" ] ]
        match notes with
        | [] -> p [ _class "text-center text-muted" ] [ str "There are no prior notes for this request" ]
        | _  -> yield! notes |> List.map toItem
    ]
