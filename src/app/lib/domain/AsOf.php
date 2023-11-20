<?php
declare(strict_types=1);

namespace MyPrayerJournal\Domain;

use DateTimeImmutable;

class AsOf
{
    /** The "as of" date/time */
    public DateTimeImmutable $asOf;

    /**
     * Sort an as-of item from oldest to newest
     * 
     * @param AsOf $a The first item to compare
     * @param AsOf $b The second item to compare
     * @return int 0 if they are equal, -1 if A is earlier than B, or 1 if B is earlier than A
     */
    public static function oldestToNewest(AsOf $a, AsOf $b): int
    {
        return $a->asOf == $b->asOf ? 0 : ($a->asOf < $b->asOf ? -1 : 1);
    }
    
    /**
     * Sort an as-of item from newest to oldest
     * 
     * @param AsOf $a The first item to compare
     * @param AsOf $b The second item to compare
     * @return int 0 if they are equal, -1 if B is earlier than A, or 1 if A is earlier than B
     */
    public static function newestToOldest(AsOf $a, AsOf $b): int
    {
        return $a->asOf == $b->asOf ? 0 : ($a->asOf > $b->asOf ? -1 : 1);
    }
}
