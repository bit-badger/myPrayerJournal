<?php
declare(strict_types=1);

require_once '../../lib/start.php';

use MyPrayerJournal\Constants;

$auth0->exchange("{$_ENV[Constants::BASE_URL]}/user/logged-on");

$nextUrl = '/journal';
if (array_key_exists(Constants::LOG_ON_STATE, $_GET)) {
    $nonce = base64_decode(urldecode($_GET[Constants::LOG_ON_STATE]));
    [$verify, $newNext] = explode('|', $_COOKIE[Constants::COOKIE_REDIRECT]);
    if ($verify == $nonce && $newNext && str_starts_with($newNext, '/') && !str_starts_with($newNext, '//')) {
        $nextUrl = $newNext;
    }
}

setcookie(Constants::COOKIE_REDIRECT, '', [
    'expires'  => -1,
    'secure'   => true,
    'httponly' => true,
    'samesite' => 'Strict'
]);
header("Location: $nextUrl");
exit;
