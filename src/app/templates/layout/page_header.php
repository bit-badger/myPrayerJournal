<?php
use BitBadger\PgDocuments\Document;
use MyPrayerJournal\{ Constants, Data };

$isLoggedOn = array_key_exists('MPJ_USER_ID', $_REQUEST);
$hasSnoozed = false;
if ($isLoggedOn) {
    $hasSnoozed = Document::countByJsonPath(Data::REQ_TABLE, '$.snoozedUntil ? (@ == "0")') > 0;
}

$theTitle = array_key_exists(Constants::PAGE_TITLE, $_REQUEST) ? "{$_REQUEST[Constants::PAGE_TITLE]} &#xab; " : ''; ?>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf8">
    <title><?php echo $theTitle; ?>myPrayerJournal</title><?php
    if (!$_REQUEST[Constants::IS_HTMX]) { ?>
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bulma/0.9.4/css/bulma.min.css"
              integrity="sha512-HqxHUkJM0SYcbvxUw5P60SzdOTy/QVwA1JJrvaXJv4q7lmbDZCmZaqz01UPOaQveoxfYRv1tHozWGPMcuTBuvQ=="
              crossorigin="anonymous" referrerpolicy="no-referrer">
        <link href="https://fonts.googleapis.com/icon?family=Material+Icons" rel="stylesheet">
        <link href="/style/style.css" rel="stylesheet"><?php
    } ?>
</head>
<body>
    <section id="top" aria-label="top navigation">
    <nav class="navbar is-dark has-shadow" role="navigation" aria-label="main navigation">
        <div class="navbar-brand">
            <a <?php page_link('/'); ?> class="navbar-item">
                <span class="m">my</span><span class="p">Prayer</span><span class="j">Journal</span>
            </a>
        </div>
        <div class="navbar-menu">
            <div class="navbar-start"><?php
                if ($isLoggedOn) { ?>
                    <a <?php page_link('/journal', ['navbar-item'], true); ?>>Journal</a>
                    <a <?php page_link('/requests/active', ['navbar-item'], true); ?>>Active</a><?php
                    if ($hasSnoozed) { ?>
                        <a <?php page_link('/requests/snoozed', ['navbar-item'], true); ?>>Snoozed</a><?php
                    } ?>
                    <a <?php page_link('/requests/answered', ['navbar-item'], true); ?>>Answered</a>
                    <a href="/user/log-off" class="navbar-item">Log Off</a><?php
                } else { ?>
                    <a href="/user/log-on" class="navbar-item">Log On</a><?php
                } ?>
                <a href="https://docs.prayerjournal.me" class="navbar-item" target="_blank" rel="noopener">Docs</a>
            </div>
        </div>
    </nav>
