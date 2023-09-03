<?php

require __DIR__ . '/vendor/autoload.php';
(Dotenv\Dotenv::createImmutable(__DIR__))->load();

use MyPrayerJournal\{ AppUser, Data, Handlers };

Data::configure();

app()->template->config('path', './pages');
app()->template->config('params', [
    'page_link'  => function (string $url, bool $checkActive = false) {
       echo 'href="'. $url . '" hx-get="' . $url . '"';
       if ($checkActive && str_starts_with($_SERVER['REQUEST_URI'], $url)) {
           echo ' class="is-active-route"';
       }
       echo 'hx-target="#top" hx-swap="innerHTML" hx-push-url="true"';
    },
    'version' => 'v4',
]);

app()->get('/', fn () => Handlers::render('home', 'Welcome'));

app()->group('/components', function () {
    app()->get('/journal-items', Handlers::journalItems(...));
});
app()->get('/journal', Handlers::journal(...));
app()->group('/legal', function () {
    app()->get('/privacy-policy',   fn () => Handlers::render('legal/privacy-policy',   'Privacy Policy'));
    app()->get('/terms-of-service', fn () => Handlers::render('legal/terms-of-service', 'Terms of Service'));
});
app()->group('/request', function () {
    app()->get('/{reqId}/edit', Handlers::requestEdit(...));
});
app()->group('/user', function () {
    app()->get('/log-on',         AppUser::logOn(...));
    app()->get('/log-on/success', AppUser::processLogOn(...));
    app()->get('/log-off',        AppUser::logOff(...));
});

// Extract the user's time zone from the request, if present
app()->use(new class extends \Leaf\Middleware {
    public function call()
    {
        $_REQUEST['USER_TIME_ZONE'] = new \DateTimeZone(
            array_key_exists('HTTP_X_TIME_ZONE', $_SERVER) ? $_SERVER['HTTP_X_TIME_ZONE'] : 'Etc/UTC');
        $this->next();
    }
});

// TODO: remove before go-live
$stdOut = fopen('php://stdout', 'w');
function stdout(string $msg)
{
    global $stdOut;
    fwrite($stdOut, $msg . "\n");
}

app()->run();
