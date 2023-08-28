<?php
declare(strict_types=1);

namespace MyPrayerJournal;

use Auth0\SDK\Auth0;

class AppUser
{
    /** The Auth0 client instance to use for authentication */
    private static ?Auth0 $auth0 = null;

    /**
     * Get the Auth0 instance
     * 
     * @return Auth0 The Auth0 instance, lazily initialized
     */
    private static function auth0Instance(): Auth0
    {
        if (is_null(self::$auth0)) {
            self::$auth0 = new \Auth0\SDK\Auth0([
                'domain'       => $_ENV['AUTH0_DOMAIN'],
                'clientId'     => $_ENV['AUTH0_CLIENT_ID'],
                'clientSecret' => $_ENV['AUTH0_CLIENT_SECRET'],
                'cookieSecret' => $_ENV['AUTH0_COOKIE_SECRET']
            ]);
        }
        return self::$auth0;
    }

    /**
     * Determine the host to use for return URLs
     * 
     * @return string The host for return URLs
     */
    private static function host()
    {
        return 'http' . ($_SERVER['SERVER_PORT'] == 443 ? 's' : '' ) . "://{$_SERVER['HTTP_HOST']}";
    }

    /**
     * Generate the log on callback URL
     * 
     * @return string The log on callback URL
     */
    private static function logOnCallback()
    {
        return self::host() . '/user/log-on/success';
    }

    /**
     * Initiate a redirect to the Auth0 log on page
     * 
     * @param string $nextUrl The URL (other than /journal) to which the user should be redirected
     * @return never This function exits the currently running script
     */
    public static function logOn(?string $nextUrl = null): never
    {
        // TODO: pass the next URL in the Auth0 callback
        self::auth0Instance()->clear();
        header('Location: ' . self::auth0Instance()->login(self::logOnCallback()));
        exit;
    }

    /**
     * Process the log on response from Auth0
     * 
     * @return never This function exits the currently running script
     */
    public static function processLogOn(): never
    {
        self::auth0Instance()->exchange(self::logOnCallback());
        // TODO: check for next URL and redirect if present
        header('Location: /journal');
        exit;
    }

    /**
     * Log off the current user
     * 
     * @return never This function exits the currently running script
     */
    public static function logOff(): never
    {
        header('Location: ' . self::auth0Instance()->logout(self::host() . '/'));
        exit;
    }

    /**
     * Get the current user
     * 
     * @return ?object The current user, or null if one is not signed in
     */
    public static function current(): ?object
    {
        return self::auth0Instance()->getCredentials();
    }

    /**
     * Require that there be a user logged on
     * 
     * @return void This will not return if there is not a user logged on
     */
    public static function require()
    {
        if (is_null(self::current())) {
            // TODO: get the current URL to specify for redirection
            self::logOn();
        }
    }

    /**
     * Get the ID (`sub`) for the current user
     * 
     * @return string The ID of the user (blank string if there is no current user)
     */
    public static function currentId(): string
    {
        return self::auth0Instance()->getCredentials()?->user['sub'] ?? '';
    }
}
