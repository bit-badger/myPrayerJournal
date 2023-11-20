<?php
declare(strict_types=1);

require_once '../lib/start.php';

use MyPrayerJournal\Constants;

require_user();

$_REQUEST[Constants::PAGE_TITLE] = "{$session->user[Constants::CLAIM_GIVEN_NAME]}&rsquo;s Prayer Journal";

template('layout/page_header'); ?>
<main class="container">
    <h2 class="title"><?php echo $_REQUEST[Constants::PAGE_TITLE]; ?></h2>
    <p hx-get="/components/journal-items" hx-swap="outerHTML" hx-trigger="load delay:.25s">
        Loading your prayer journal&hellip;
    </p>
</main><?php
template('layout/page_footer');
end_request();
