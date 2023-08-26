<?php

require __DIR__ . '/vendor/autoload.php';

use MyPrayerJournal\Data;

Data::configure();

app()->template->config('path', './pages');
app()->template->config('params', [
    'page_link' => function (string $url, bool $checkActive = false) {
       echo 'href="'. $url . '" hx-get="' . $url . '"';
       if ($checkActive && str_starts_with($_SERVER['REQUEST_URI'], $url)) {
           echo ' class="is-active-route"';
       }
       echo 'hx-target="#top" hx-swap="innerHTML" hx-push-url="true"';
    },
    'version' => 'v4',
]);

function renderPage(string $template, array $params, string $pageTitle)
{
    if (is_null($params)) {
        $params = [];
    }
    $params['pageTitle'] = $pageTitle;
    $params['isHtmx'] =
        array_key_exists('HTTP_HX_REQUEST', $_SERVER)
            && (!array_key_exists('HTTP_HX_HISTORY_RESTORE_REQUEST', $_SERVER));
    $params['userId'] = false;
    $params['pageContent'] = app()->template->render($template, $params);
    // TODO: make the htmx distinction here
    response()->markup(app()->template->render('layout/full', $params));
}

app()->get('/', function () {
    renderPage('home', [], 'Welcome');
});

app()->get('/legal/privacy-policy', function () {
    renderPage('legal/privacy-policy', [], 'Privacy Policy');
});

app()->get('/legal/terms-of-service', function () {
    renderPage('legal/terms-of-service', [], 'Terms of Service');
});

app()->run();
