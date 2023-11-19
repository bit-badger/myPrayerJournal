<?php
declare(strict_types=1);

require_once '../../lib/start.php';

use MyPrayerJournal\Constants;

$auth0->clear();

// Check for return URL; if present, store it in a cookie we'll retrieve when we're logged in
$nonce = '';
if (array_key_exists(Constants::RETURN_URL, $_GET)) {
    $nonce = urlencode(base64_encode(openssl_random_pseudo_bytes(8)));
    setcookie(Constants::COOKIE_REDIRECT, "$nonce|{$_GET[Constants::RETURN_URL]}", [
        'expires'  => -1,
        'secure'   => true,
        'httponly' => true,
        'samesite' => 'Strict'
    ]);
}
$params = $nonce ? [ Constants::LOG_ON_STATE => $nonce ] : [];

header('Location: ' . $auth0->login("{$_ENV[Constants::BASE_URL]}/user/logged-on", $params));
exit;
