<?php
declare(strict_types=1);

require_once '../lib/start.php';

use MyPrayerJournal\Constants;

$_REQUEST[Constants::PAGE_TITLE] = 'Welcome';

template('layout/page_header'); ?>
<main class="container">
    <p class="block">&nbsp;</p>
    <p class="block">
        myPrayerJournal is a place where individuals can record their prayer requests, record that they prayed for them,
        update them as God moves in the situation, and record a final answer received on that request. It also allows
        individuals to review their answered prayers.
    </p>
    <p class="block">
        This site is open and available to the general public. To get started, simply click the &ldquo;Log On&rdquo;
        link above, and log on with either a Microsoft or Google account. You can also learn more about the site at the
        &ldquo;Docs&rdquo; link, also above.
    </p>
</main><?php
template('layout/page_footer');
end_request();
