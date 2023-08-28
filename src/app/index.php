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

app()->get('/components/journal-items', Handlers::journalItems(...));

app()->get('/journal', Handlers::journal(...));

app()->get('/legal/privacy-policy',   fn () => Handlers::render('legal/privacy-policy',   'Privacy Policy'));
app()->get('/legal/terms-of-service', fn () => Handlers::render('legal/terms-of-service', 'Terms of Service'));

app()->get('/user/log-on',         AppUser::logOn(...));
app()->get('/user/log-on/success', AppUser::processLogOn(...));
app()->get('/user/log-off',        AppUser::logOff(...));

// TODO: remove before go-live
$stdOut = fopen('php://stdout', 'w');
function stdout(string $msg)
{
    global $stdOut;
    fwrite($stdOut, $msg . "\n");
}

app()->run();
