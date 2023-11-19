<?php
use MyPrayerJournal\Constants; ?>
    </section><?php
if (!$_REQUEST[Constants::IS_HTMX]) { ?>
    <footer class="container-fluid mx-1">
        <p class="text-muted has-text-right">
            myPrayerJournal <?php echo $_REQUEST[Constants::VERSION]; ?><br>
            <em><small>
                <a <?php page_link('/legal/privacy-policy'); ?>>Privacy Policy</a> &bull;
                <a <?php page_link('/legal/terms-of-service'); ?>>Terms of Service</a> &bull;
                <a href="https://github.com/bit-badger/myprayerjournal" target="_blank" rel="noopener">Developed</a>
                and hosted by
                <a href="https://bitbadger.solutions" target="_blank" rel="noopener">Bit Badger Solutions</a>
            </small></em>
        </p>
    </footer>
    <script src="https://unpkg.com/htmx.org@1.9.4"
            integrity="sha384-zUfuhFKKZCbHTY6aRR46gxiqszMk5tcHjsVFxnUo8VMus4kHGVdIYVbOYYNlKmHV"
            crossorigin="anonymous"></script>
    <script src="/script/mpj.js"></script><?php
}
