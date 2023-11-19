<?php
declare(strict_types=1);

namespace MyPrayerJournal;

/**
 * Constants for use throughout the application
 */
class Constants
{
    /** @var string The `$_ENV` key for the Auth0 domain configured for myPrayerJournal */
    const AUTH0_DOMAIN = 'AUTH0_DOMAIN';

    /** @var string The `$_ENV` key for the Auth0 client ID for myPrayerJournal */
    const AUTH0_CLIENT_ID = 'AUTH0_CLIENT_ID';

    /** @var string The `$_ENV` key for the Auth0 client secret */
    const AUTH0_CLIENT_SECRET = 'AUTH0_CLIENT_SECRET';

    /** @var string The `$_ENV` key for the Auth0 cookie secret */
    const AUTH0_COOKIE_SECRET = 'AUTH0_COOKIE_SECRET';

    /** @var string The `$_ENV` key for the base URL for this instance of myPrayerJournal */
    const BASE_URL = 'AUTH0_BASE_URL';

    /** @var string The Auth0 given name (first name) claim */
    const CLAIM_GIVEN_NAME = 'given_name';

    /** @var string The Auth0 subscriber (sub) claim */
    const CLAIM_SUB = 'sub';

    /** @var string The name of the cookie used to persist redirection after Auth0 authentication */
    const COOKIE_REDIRECT = 'mpjredirect';

    /** @var string the `$_SERVER` key for the HX-Request header */
    const HEADER_HX_REQUEST = 'HTTP_HX_REQUEST';

    /** @var string The `$_SERVER` key for the HX-History-Restore-Request header */
    const HEADER_HX_HIST_REQ = 'HTTP_HX_HISTORY_RESTORE_REQUEST';

    /** @var string The `$_SERVER` key for the X-Time-Zone header */
    const HEADER_USER_TZ = 'HTTP_X_TIME_ZONE';

    /** @var string The `$_REQUEST` key for whether the request was initiated by htmx */
    const IS_HTMX = 'MPJ_IS_HTMX';

    /** @var string The `$_GET` key for state passed to Auth0 if redirection is required once authenticated */
    const LOG_ON_STATE = 'state';

    /** @var string The `$_REQUEST` key for the page title for this request */
    const PAGE_TITLE = 'MPJ_PAGE_TITLE';

    /** @var string The `$_SERVER` key for the current page's relative URI */
    const REQUEST_URI = 'REQUEST_URI';

    /** @var string The `$_GET` key sent to the log on page if redirection is required once authenticated */
    const RETURN_URL = 'return_url';

    /** @var string The `$_REQUEST` key for the timezone reference to use for this request */
    const TIME_ZONE = 'MPJ_TIME_ZONE';

    /** @var string The `$_REQUEST` key for the current user's ID */
    const USER_ID = 'MPJ_USER_ID';

    /** @var string The `$_REQUEST` key for the current version of myPrayerJournal */
    const VERSION = 'MPJ_VERSION';
}
