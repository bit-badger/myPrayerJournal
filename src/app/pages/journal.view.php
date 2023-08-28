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
    <div id="notesModal" class="modal fade" tabindex="-1" aria-labelled-by="nodesModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="nodesModalLabel">Add Notes to Prayer Request</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" />
                </div>
                <div class="modal-body" id="notesBody"></div>
                <div class="modal-footer">
                    <button type="button" id="notesDismiss" class="btn btn-secondary" data-bs-dismiss="modal">
                        Close
                    </button>
                </div>
            </div>
        </div>
    </div>
    <div id="snoozeModal" class="modal fade" tabindex="-1" aria-labelled-by="snoozeModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-sm">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="snoozeModalLabel">Snooze Prayer Request</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" />
                </div>
                <div class="modal-body" id="snoozeBody"></div>
                <div class="modal-footer">
                    <button type="button" id="snoozeDismiss" class="btn btn-secondary" data-bs-dismiss="modal">
                        Close
                    </button>
                </div>
            </div>
        </div>
    </div>
</article>
