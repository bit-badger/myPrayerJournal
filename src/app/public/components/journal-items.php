<?php
declare(strict_types=1);

require_once '../../lib/start.php';

use MyPrayerJournal\{ Constants, Data, Dates };
use MyPrayerJournal\Domain\JournalRequest;

require_user(true);

$requests = Data::getJournal($_REQUEST[Constants::USER_ID]);

bare_header();
if ($requests) { ?>
    <section id="journalItems" class="row row-cols-1 row-cols-md-2 row-cols-lg-3 row-cols-xl-4 g-3" hx-target="this"
             hx-swap="outerHTML" aria-label="Prayer Requests">
        <p class="mb-3 has-text-centered">
            <a <?php page_link('/request/edit?new'); ?> class="button is-light">
                <span class="material-icons">add_box</span> Add a Prayer Request
            </a>
        </p><?php
        array_walk($requests, journal_card(...)); ?>
    </section><?php
} else {
    $_REQUEST['EMPTY_HEADING'] = 'No Active Requests';
    $_REQUEST['EMPTY_LINK']    = '/request/edit?new';
    $_REQUEST['EMPTY_BTN_TXT'] = 'Add a Request';
    $_REQUEST['EMPTY_TEXT']    = 'You have no requests to be shown; see the &ldquo;Active&rdquo; link above for '
        . 'snoozed or deferred requests, and the &ldquo;Answered&rdquo; link for answered requests';
    template('no_content');
}
end_request();

/**
 * Format the activity and relative time
 * 
 * @param string $activity The activity performed (activity or prayed)
 * @param DateTimeImmutable $asOf The date/time the activity was performed
 */
function format_activity(string $activity, DateTimeImmutable $asOf): void
{
    echo sprintf('last %s <span title="%s">%s</span>', $activity,
        $asOf->setTimezone($_REQUEST[Constants::TIME_ZONE])->format('l, F jS, Y/g:ia T'),
        Dates::formatDistance(Dates::now(), $asOf));
} 

/**
 * Create a card for a prayer request
 * 
 * @param JournalRequest $req The request for which a card should be generated
 */
function journal_card(JournalRequest $req): void
{
    $spacer = '<span>&nbsp;</span>'; ?>
    <div class="col">
        <div class="card h-100">
            <div class="card-header p-0 d-flex" role="toolbar">
                <a <?php page_link("/request/edit?{$req->id}"); ?> class="button btn-secondary" title="Edit Request">
                    <span class="material-icons">edit</span>
                </a><?php echo $spacer; ?>
                <button type="button" class="btn btn-secondary" title="Add Notes" data-bs-toggle="modal"
                        data-bs-target="#notesModal" hx-get="/components/request/<?php echo $req->id; ?>/add-notes"
                        hx-target="#notesBody" hx-swap="innerHTML">
                    <span class="material-icons">comment</span>
                </button><?php echo $spacer; ?>
                <button type="button" class="btn btn-secondary" title="Snooze Request" data-bs-toggle="modal"
                        data-bs-target="#snoozeModal" hx-get="/components/request/<?php echo $req->id; ?>/snooze"
                        hx-target="#snoozeBody" hx-swap="innerHTML">
                    <span class="material-icons">schedule</span>
                </button>
                <div class="flex-grow-1"></div>
                <a href="/request/prayed?<?php echo $req->id; ?>" class="button btn-success w-25"
                   hx-patch="/request/prayed?<?php echo $req->id; ?>" title="Mark as Prayed">
                    <span class="material-icons">done</span>
                </a>
            </div>
            <div class="card-body">
                <p class="request-text"><?php echo htmlentities($req->text); ?></p>
            </div>
            <div class="card-footer text-end text-muted px-1 py-0">
                <em><?php
                    [ $activity, $asOf ] = is_null($req->lastPrayed)
                        ? [ 'activity', $req->asOf ]
                        : [ 'prayed',   $req->lastPrayed ];
                    format_activity($activity, $asOf); ?>
                </em>
            </div>
        </div>
    </div><?php
}
