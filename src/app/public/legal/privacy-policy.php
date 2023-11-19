<?php
declare(strict_types=1);

require_once '../../lib/start.php';

use MyPrayerJournal\Constants;

$_REQUEST[Constants::PAGE_TITLE] = 'Privacy Policy';

template('layout/page_header'); ?>
<main class="container">
    <h2 class="title">Privacy Policy</h2>
    <h6 class="subtitle">as of May 21<sup>st</sup>, 2018</h6>
    <p class="mb-3">
        The nature of the service is one where privacy is a must. The items below will help you understand the data we
        collect, access, and store on your behalf as you use this service.
    </p>
    <div class="card mb-3">
        <div class="card-header has-background-light">
            <h3 class="card-header-title">Third Party Services</h3>
        </div>
        <p class="card-content">
            myPrayerJournal utilizes a third-party authentication and identity provider. You should familiarize yourself
            with the privacy policy for
            <a href="https://auth0.com/privacy" target="_blank" rel="noopener">Auth0</a>, as well as your chosen
            provider
            (<a href="https://privacy.microsoft.com/en-us/privacystatement" target="_blank"
                rel="noopener">Microsoft</a> or
            <a href="https://policies.google.com/privacy" target="_blank" rel="noopener">Google</a>).
        </p>
    </div>
    <div class="card mb-3">
        <div class="card-header has-background-light">
            <h3 class="card-header-title">What We Collect</h3>
        </div>
        <div class="card-content">
            <h4 class="subtitle mb-3">Identifying Data</h4>
            <ul class="mb-3 mx-5">
                <li>
                    &bull; The only identifying data myPrayerJournal stores is the subscriber (&ldquo;sub&rdquo;) field
                    from the token we receive from Auth0, once you have signed in through their hosted service. All
                    information is associated with you via this field.
                </li>
                <li>
                    &bull; While you are signed in, within your browser, the service has access to your first and last
                    names, along with a URL to the profile picture (provided by your selected identity provider). This
                    information is not transmitted to the server, and is removed when &ldquo;Log Off&rdquo; is clicked.
                </li>
            </ul>
            <h4 class="subtitle mb-3">User Provided Data</h4>
            <ul class="mx-5">
                <li>
                    &bull; myPrayerJournal stores the information you provide, including the text of prayer requests,
                    updates, and notes; and the date/time when certain actions are taken.
                </li>
            </ul>
        </div>
    </div>
    <div class="card mb-3">
        <div class="card-header has-background-light">
            <h3 class="card-header-title">How Your Data Is Accessed / Secured</h3>
        </div>
        <ul class="card-content">
            <li>
                &bull; Your provided data is returned to you, as required, to display your journal or your answered
                requests. On the server, it is stored in a controlled-access database.
            </li>
            <li>
                &bull; Your data is backed up, along with other Bit Badger Solutions hosted systems, in a rolling
                manner; backups are preserved for the prior 7 days, and backups from the 1<sup>st</sup> and
                15<sup>th</sup> are preserved for 3 months. These backups are stored in a private cloud data repository.
            </li>
            <li>
                &bull; The data collected and stored is the absolute minimum necessary for the functionality of the
                service. There are no plans to &ldquo;monetize&rdquo; this service, and storing the minimum amount of
                information means that the data we have is not interesting to purchasers (or those who may have more
                nefarious purposes).
            </li>
            <li>
                &bull; Access to servers and backups is strictly controlled and monitored for unauthorized access
                attempts.
            </li>
        </ul>
    </div>
    <div class="card mb-3">
        <div class="card-header has-background-light">
            <h3 class="card-header-title">Removing Your Data</h3>
        </div>
        <p class="card-content">
            At any time, you may choose to discontinue using this service. Both Microsoft and Google provide ways to
            revoke access from this application. However, if you want your data removed from the database, please
            contact daniel at bitbadger.solutions (via e-mail, replacing at with @) prior to doing so, to ensure we can
            determine which subscriber ID belongs to you.
        </p>
    </div>
</main><?php
template('layout/page_footer');
end_request();
