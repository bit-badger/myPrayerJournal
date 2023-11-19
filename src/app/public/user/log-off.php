<?php
declare(strict_types=1);

require_once '../../lib/start.php';

use MyPrayerJournal\Constants;

header("Location: {$auth0->logout($_ENV[Constants::BASE_URL])}");
exit;
