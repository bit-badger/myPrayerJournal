<nav class="navbar navbar-dark" role="navigation">
    <div class="container-fluid">
        <a <?php $page_link('/'); ?> class="navbar-brand">
            <span class="m">my</span><span class="p">Prayer</span><span class="j">Journal</span>
        </a>
        <ul class="navbar-nav me-auto d-flex flex-row"><?php
            if ($user) { ?>
                <li class="nav-item"><a <?php $page_link('/journal', true); ?>>Journal</a></li>
                <li class="nav-item"><a <?php $page_link('/requests/active', true); ?>>Active</a></li><?php
                if ($hasSnoozed) { ?>
                    <li class="nav-item"><a <?php $page_link('/requests/snoozed', true); ?>>Snoozed</a></li><?php
                } ?>
                <li class="nav-item"><a <?php $page_link('/requests/answered', true); ?>>Answered</a></li>
                <li class="nav-item"><a href="/user/log-off">Log Off</a></li><?php
            } else { ?>
                <li class="nav-item"><a href="/user/log-on">Log On</a></li><?php
            } ?>
            <li class="nav-item"><a href="https://docs.prayerjournal.me" target="_blank" rel="noopener">Docs</a></li>
        </ul>
    </div>
</nav>
