<?php
declare(strict_types=1);

namespace MyPrayerJournal\Domain;

/**
 * An action that was taken on a request
 */
enum RequestAction implements \JsonSerializable
{
    /** The request was entered */
    case Created;

    /** Prayer was recorded for the request */
    case Prayed;

    /** The request was updated */
    case Updated;

    /** The request was marked as answered */
    case Answered;

    /**
     * Serialize this enum using its name
     */
    public function jsonSerialize(): mixed
    {
        return $this->name;
    }
}
