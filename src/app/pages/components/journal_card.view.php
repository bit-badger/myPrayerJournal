<?php
$spacer = '<span>&nbsp;</span>'; ?>
<div class="col">
    <div class="card h-100">
        <div class="card-header p-0 d-flex" role="tool-bar">
            <a <?php $page_link("/request/{$request->id}/edit"); ?> class="btn btn-secondary" title="Edit Request">
                <span class="material-icons">edit</span>
            </a><?php echo $spacer; ?>
            <button type="button" class="btn btn-secondary" title="Add Notes" data-bs-toggle="modal"
                    data-bs-target="#notesModal" hx-get="/components/request/<?php echo $request->id; ?>/add-notes"
                    hx-target="#notesBody" hx-swap="innerHTML">
                <span class="material-icons">comment</span>
            </button><?php echo $spacer; ?>
            <button type="button" class="btn btn-secondary" title="Snooze Request" data-bs-toggle="modal"
                    data-bs-target="#snoozeModal" hx-get="/components/request/<?php echo $request->id; ?>/snooze"
                    hx-target="#snoozeBody" hx-swap="innerHTML">
                <span class="material-icons">schedule</span>
            </button>
            <div class="flex-grow-1"></div>
            <button type="button" class="btn btn-success w-25" hx-patch="/request/<?php echo $request->id; ?>/prayed"
                    title="Mark as Prayed">
                <span class="material-icons">done</span>
            </button>
        </div>
        <div class="card-body">
            <p class="request-text"><?php echo htmlentities($request->text); ?></p>
        </div>
        <div class="card-footer text-end text-muted px-1 py-0">
            <em><?php
                // TODO: relative time, time zone handling, etc.
                if (is_null($request->lastPrayed)) {
                    echo "last activity {$request->asOf}";

                } else {
                    echo "last prayed {$request->lastPrayed}";
                } ?>
            </em>
        </div>
    </div>
</div>
