<?php
if (count($requests) == 0) {
    echo app()->template->render('components/no_results', [
        'heading'    => 'No Active Requests',
        'link'       => '/request/new/edit',
        'buttonText' => 'Add a Request',
        'text'       => 'You have no requests to be shown; see the &ldquo;Active&rdquo; link above for snoozed or '
                            . 'deferred requests, and the &ldquo;Answered&rdquo; link for answered requests'
    ]);
} else { ?>
    <section id="journalItems" class="row row-cols-1 row-cols-md-2 row-cols-lg-3 row-cols-xl-4 g-3" hx-target="this"
             hx-swap="outerHTML" aria-label="Prayer Requests"><?php
        foreach ($requests as $request) {
            echo app()->template->render('components/journal_card', [ 'request' => $request ]);
        } ?>
    </section><?php
}
