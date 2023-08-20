<?php
if (!$isHtmx) { ?>
    <footer class="container-fluid">
        <p class="text-muted text-end">
            myPrayerJournal <?= $version ?><br>
            <em><small>
                <a <?php $page_link('/legal/privacy-policy'); ?>>Privacy Policy</a> &bull;
                <a <?php $page_link('/legal/terms-of-service'); ?>>Terms of Service</a> &bull;
                <a href="https://github.com/bit-badger/myprayerjournal" target="_blank" rel="noopener">Developed</a>
                and hosted by
                <a href="https://bitbadger.solutions" target="_blank" rel="noopener">Bit Badger Solutions</a>
            </small></em>
        </p>
        <script src="https://unpkg.com/htmx.org@1.9.4"
                integrity="sha384-zUfuhFKKZCbHTY6aRR46gxiqszMk5tcHjsVFxnUo8VMus4kHGVdIYVbOYYNlKmHV"
                crossorigin="anonymous"></script>
        <!-- script [] [
            rawText "if (!htmx) document.write('<script src=\"/script/htmx.min.js\"><\/script>')"
        ] -->
        <script async src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/js/bootstrap.bundle.min.js"
                integrity="sha384-A3rJD856KowSb7dwlZdYEkO39Gagi7vIsF0jrRAoQmDKKtQBHUuLZ9AsSv4jD4Xa"
                crossorigin="anonymous"></script>
        <!-- script [] [
            rawText "setTimeout(function () { "
            rawText "if (!bootstrap) document.write('<script src=\"/script/bootstrap.bundle.min.js\"><\/script>') "
            rawText "}, 2000)"
        ] -->
        <script src="/script/mpj.js"></script>
    </footer><?php
}
