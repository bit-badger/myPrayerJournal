<?php
declare(strict_types=1);

namespace MyPrayerJournal\Domain;

use JsonSerializable;

/**
 * The unit to use when determining when to show a recurring request
 */
enum RecurrenceType implements JsonSerializable
{
    /** The request should reappear immediately */
    case Immediate;

    /** The request should reappear after the specified number of hours */
    case Hours;

    /** The request should reappear after the specified number of days */
    case Days;

    /** The request should reappear after the specified number of weeks */
    case Weeks;

    /**
     * Serialize this enum using its name
     */
    public function jsonSerialize(): mixed
    {
        return $this->name;
    }
}
