<?php
declare(strict_types=1);

namespace MyPrayerJournal\Domain;

use DateTimeImmutable, DateTimeZone;

/**
 * A note entered on a prayer request
 */
class Note extends AsOf
{
    /** The note */
    public string $notes = '';

    public function __construct()
    {
        $this->asOf = unix_epoch();
    }
}
