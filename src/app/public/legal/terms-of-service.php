<?php
declare(strict_types=1);

require_once '../../lib/start.php';

use MyPrayerJournal\Constants;

$_REQUEST[Constants::PAGE_TITLE] = 'Terms of Service';

template('layout/page_header'); ?>
<main class="container">
    <h2 class="title">Terms of Service</h2>
    <h6 class="subtitle">as of May 21<sup>st</sup>, 2018</h6>
    <div class="card mb-3">
        <div class="card-header has-background-light">
            <h3 class="card-header-title">1. Acceptance of Terms</h3>
        </div>
        <p class="card-content">
            By accessing this web site, you are agreeing to be bound by these Terms and Conditions, and that you are
            responsible to ensure that your use of this site complies with all applicable laws. Your continued use of
            this site implies your acceptance of these terms.
        </p>
    </div>
    <div class="card mb-3">
        <div class="card-header has-background-light">
            <h3 class="card-header-title">2. Description of Service and Registration</h3>
        </div>
        <p class="card-content">
            myPrayerJournal is a service that allows individuals to enter and amend their prayer requests. It requires
            no registration by itself, but access is granted based on a successful login with an external identity
            provider. See <a <?php page_link('/legal/privacy-policy'); ?>>our privacy policy</a> for details on how that
            information is accessed and stored.
        </p>
    </div>
    <div class="card mb-3">
        <div class="card-header has-background-light">
            <h3 class="card-header-title">3. Third Party Services</h3>
        </div>
        <p class="card-content">
            This service utilizes a third-party service provider for identity management. Review the terms of service
            for <a href="https://auth0.com/terms" target="_blank" rel="noopener">Auth0</a>, as well as those for the
            selected authorization provider
            (<a href="https://www.microsoft.com/en-us/servicesagreement" target="_blank"
                rel="noopener">Microsoft</a> or
            <a href="https://policies.google.com/terms" target="_blank" rel="noopener">Google</a>).
        </p>
    </div>
    <div class="card mb-3">
        <div class="card-header has-background-light">
            <h3 class="card-header-title">4. Liability</h3>
        </div>
        <p class="card-content">
            This service is provided &ldquo;as is&rdquo;, and no warranty (express or implied) exists. The service and
            its developers may not be held liable for any damages that may arise through the use of this service.
        </p>
    </div>
    <div class="card mb-3">
        <div class="card-header has-background-light">
            <h3 class="card-header-title">5. Updates to Terms</h3>
        </div>
        <p class="card-content">
            These terms and conditions may be updated at any time, and this service does not have the capability to
            notify users when these change. The date at the top of the page will be updated when any of the text of
            these terms is updated.
        </p>
    </div>
    <p>
        You may also wish to review our <a <?php page_link('/legal/privacy-policy'); ?>>privacy policy</a> to learn how
        we handle your data.
    </p>
</main><?php
template('layout/page_footer');
end_request();
