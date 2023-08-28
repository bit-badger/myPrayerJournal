<?php
declare(strict_types=1);

namespace MyPrayerJournal;

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
        // TODO: make the htmx distinction here
        response()->markup(app()->template->render('layout/full', $params));
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

    /** GET: /journal */
    public static function journal()
    {
        if (!AppUser::current()) AppUser::logOn();
        
        $user      = AppUser::current()->user;
        $firstName = (array_key_exists('given_name', $user) ? $user['given_name'] : null) ?? 'Your';
        self::render('journal', $firstName . ($firstName == 'Your' ? '' : '&rsquo;s') . ' Prayer Journal');
    }

    /** GET: /components/journal-items */
    public static function journalItems()
    {
        if (!AppUser::current()) AppUser::logOn();

        self::renderComponent('components/journal_items', [
            'requests' => Data::getJournal(AppUser::current()->user['sub'])
        ]);
    }
}
