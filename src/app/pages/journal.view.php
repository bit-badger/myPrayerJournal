<article class="container-fluid mt-3">
    <h2 class="pb-3"><?php echo $pageTitle; ?></h2>
    <p class="pb-3 text-center">
        <a <?php $page_link('/request/new/edit'); ?> class="btn btn-primary">
            <span class="material-icons">add_box</span> Add a Prayer Request
        </a>
    </p>
    <p hx-get="/components/journal-items" hx-swap="outerHTML" hx-trigger="load">
        Loading your prayer journal&hellip;
    </p>
    <!--
    div [ _id             "notesModal"
            _class          "modal fade"
            _tabindex       "-1"
            _ariaLabelledBy "nodesModalLabel"
            _ariaHidden     "true" ] [
        div [ _class "modal-dialog modal-dialog-scrollable" ] [
            div [ _class "modal-content" ] [
                div [ _class "modal-header" ] [
                    h5 [ _class "modal-title"; _id "nodesModalLabel" ] [ str "Add Notes to Prayer Request" ]
                    button [ _type "button"; _class "btn-close"; _data "bs-dismiss" "modal"; _ariaLabel "Close" ] []
                ]
                div [ _class "modal-body"; _id "notesBody" ] [ ]
                div [ _class "modal-footer" ] [
                    button [ _type  "button"
                                _id    "notesDismiss"
                                _class "btn btn-secondary"
                                _data  "bs-dismiss" "modal" ] [
                        str "Close"
                    ]
                ]
            ]
        ]
    ]
    div [ _id             "snoozeModal"
            _class          "modal fade"
            _tabindex       "-1"
            _ariaLabelledBy "snoozeModalLabel"
            _ariaHidden     "true" ] [
        div [ _class "modal-dialog modal-sm" ] [
            div [ _class "modal-content" ] [
                div [ _class "modal-header" ] [
                    h5 [ _class "modal-title"; _id "snoozeModalLabel" ] [ str "Snooze Prayer Request" ]
                    button [ _type "button"; _class "btn-close"; _data "bs-dismiss" "modal"; _ariaLabel "Close" ] []
                ]
                div [ _class "modal-body"; _id "snoozeBody" ] [ ]
                div [ _class "modal-footer" ] [
                    button [ _type "button"
                                _id "snoozeDismiss"
                                _class "btn btn-secondary"
                                _data "bs-dismiss" "modal" ] [
                        str "Close"
                    ]
                    ]
            ]
        ]
    ] -->
</article>
