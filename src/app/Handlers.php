<?php
declare(strict_types=1);

namespace MyPrayerJournal;

use MyPrayerJournal\Domain\JournalRequest;

class Handlers
{
    /**
     * Render a BareUI template
     * 
     * @param string $template The template name to render
     * @param string $pageTitle The title for the page
     * @param ?array $params Parameters to use to render the page (optional)
     */
    public static function render(string $template, string $pageTitle, ?array $params = null)
    {
        $params = array_merge($params ?? [], [
            'pageTitle'  => $pageTitle,
            'isHtmx'     =>
                array_key_exists('HTTP_HX_REQUEST', $_SERVER)
                    && (!array_key_exists('HTTP_HX_HISTORY_RESTORE_REQUEST', $_SERVER)),
            'user'       => AppUser::current(),
            'hasSnoozed' => false,
        ]);
        $params['pageContent'] = app()->template->render($template, $params);
        $layout = $params['isHtmx'] ? 'layout/partial' : 'layout/full';
        response()->markup(app()->template->render($layout, $params));
    }

    /**
     * Render a BareUI component template
     * 
     * @param string $template The template name to render
     * @param ?array $params Parameter to use to render the component (optional)
     */
    private static function renderComponent(string $template, ?array $params = null)
    {
        $params = $params ?? [];
        $params['pageContent'] = app()->template->render($template, $params);
        header('Cache-Control: no-cache, max-age=-1');
        response()->markup(app()->template->render('layout/component', $params));
    }

    /**
     * Send a 404 Not Found response
     */
    private static function notFound()
    {
        response()->plain('Not found', 404);
    }

    /** GET: /journal */
    public static function journal()
    {
        AppUser::require();
        
        $user      = AppUser::current()->user;
        $firstName = (array_key_exists('given_name', $user) ? $user['given_name'] : null) ?? 'Your';
        self::render('journal', $firstName . ($firstName == 'Your' ? '' : '&rsquo;s') . ' Prayer Journal');
    }

    /** GET: /components/journal-items */
    public static function journalItems()
    {
        AppUser::require();

        $reqs  = Data::getJournal(AppUser::currentId());
        $utc   = new \DateTimeZone('Etc/UTC');
        $now   = date_create_immutable(timezone: $utc);
        $epoch = date_create_immutable('1970-01-01', $utc);
        array_filter($reqs,
            fn (JournalRequest $req) => $req->snoozedUntil ?? $epoch < $now && $req->showAfter ?? $epoch < $now);
        
        self::renderComponent('components/journal_items', [ 'requests' => $reqs ]);
    }

    /** GET /request/[req-id]/edit */
    public static function requestEdit(string $reqId)
    {
        AppUser::require();

        $returnTo = array_key_exists('HTTP_REFERER', $_SERVER)
            ? match (true) {
                str_ends_with($_SERVER['HTTP_REFERER'], '/active')  => 'active',
                str_ends_with($_SERVER['HTTP_REFERER'], '/snoozed') => 'snoozed',
                default                                             => 'journal'
            }
            : 'journal';
        if ($reqId == 'new') {
            self::render('requests/edit', 'Add Prayer Request', [
                'request'  => new JournalRequest(),
                'isNew'    => true,
                'returnTo' => $returnTo,
            ]);
        } else {
            $req = Data::tryJournalById($reqId, AppUser::currentId());
            if (is_null($req)) {
                self::notFound();
            } else {
                self::render('requests/edit', 'Edit Prayer Request', [
                    'request'  => $req,
                    'isNew'    => false,
                    'returnTo' => $returnTo,
                ]);
            }
        }
    }
}
