<?php
declare(strict_types=1);

require __DIR__ . '/../vendor/autoload.php';

error_reporting(E_ALL);
ini_set('display_errors', 'On');

use Auth0\SDK\Auth0;
use BitBadger\PgDocuments\Configuration;
use DateTimeZone;
use Dotenv\Dotenv;
use MyPrayerJournal\Constants;

Dotenv::createImmutable(__DIR__)->load();

/** @var Auth0 The Auth0 instance to use for the request */
$auth0 = new Auth0([
    'domain'       => $_ENV[Constants::AUTH0_DOMAIN],
    'clientId'     => $_ENV[Constants::AUTH0_CLIENT_ID],
    'clientSecret' => $_ENV[Constants::AUTH0_CLIENT_SECRET],
    'cookieSecret' => $_ENV[Constants::AUTH0_COOKIE_SECRET]
]);

/** @var ?object The Auth0 session for the current user */
$session = $auth0->getCredentials();
if (!is_null($session)) $_REQUEST[Constants::USER_ID] = $session->user[Constants::CLAIM_SUB];

$_REQUEST[Constants::IS_HTMX] = array_key_exists(Constants::HEADER_HX_REQUEST, $_SERVER)
    && (!array_key_exists(Constants::HEADER_HX_HIST_REQ, $_SERVER));

$_REQUEST[Constants::TIME_ZONE] = new DateTimeZone(
    array_key_exists(Constants::HEADER_USER_TZ, $_SERVER) ? $_SERVER[Constants::HEADER_USER_TZ] : 'Etc/UTC');

$_REQUEST[Constants::VERSION] = 4;

Configuration::$startUp = '\MyPrayerJournal\Data::startUp';

/**
 * Bring in a template
 */
function template(string $name): void
{
    require_once __DIR__ . "/../templates/$name.php";
}

/**
 * If a user is not found, either redirect them or fail the request
 * 
 * @param bool $fail Whether to fail the request (true) or redirect to log on (false - optional, default)
 */
function require_user(bool $fail = false): void
{
    if (!array_key_exists(Constants::USER_ID, $_REQUEST)) {
        if ($fail) {
            http_response_code(403);
        } else {
            header(sprintf('Location: /user/log-on?%s=%s', Constants::RETURN_URL, $_SERVER[Constants::REQUEST_URI]));
        }
        exit;
    }
}

/**
 * Write a bare header for a component result
 */
function bare_header(): void
{
    echo '<!DOCTYPE html><html lang="en"><head><meta charset="utf8"><title></title></head><body>';
}

/**
 * Create a traditional and htmx link, and apply an active class if the link is active
 * 
 * @param string $url The URL of the page to be linked
 * @param array $classNames CSS class names to be applied to the link (optional, default none)
 * @param bool $checkActive Whether to apply an active class if the route matches (optional, default false)
 */
function page_link(string $url, array $classNames = [], bool $checkActive = false): void
{
    echo 'href="'. $url . '" hx-get="' . $url . '"';
    if ($checkActive && str_starts_with($_SERVER[Constants::REQUEST_URI], $url)) {
        $classNames[] = 'is-active-route';
    }
    if (!empty($classNames)) {
        echo sprintf(' class="%s"', implode(' ', $classNames));
    }
    echo ' hx-target="#top" hx-swap="innerHTML" hx-push-url="true"';
}

/**
 * Close any open database connection; close the `body` and `html` tags
 */
function end_request(): void
{
    Configuration::closeConn();
    echo '</body></html>';
}

/**
 * Create a new instance of the Unix epoch
 *
 * @return DateTimeImmutable An immutable date/time as of the Unix epoch
 */
function unix_epoch(): DateTimeImmutable
{
    return new DateTimeImmutable('1/1/1970', new DateTimeZone('Etc/UTC'));
}
