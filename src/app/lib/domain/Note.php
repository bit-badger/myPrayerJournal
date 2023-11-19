<?php
declare(strict_types=1);

namespace MyPrayerJournal\Domain;

use DateTimeImmutable, DateTimeZone;

/**
 * A note entered on a prayer request
 */
class Note
{
    use AsOf;

    /** The note */
    public string $notes = '';

    public function __construct()
    {
        $this->asOf = new DateTimeImmutable('1/1/1970', new DateTimeZone('Etc/UTC'));
    }
}
