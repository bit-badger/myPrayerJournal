<?php
declare(strict_types=1);

namespace MyPrayerJournal\Domain;

use JsonSerializable;

/**
 * An action that was taken on a request
 */
enum RequestAction: string implements JsonSerializable
{
    /** The request was entered */
    case Created = 'Created';

    /** Prayer was recorded for the request */
    case Prayed = 'Prayed';

    /** The request was updated */
    case Updated = 'Updated';

    /** The request was marked as answered */
    case Answered = 'Answered';

    /**
     * Serialize this enum using its name
     */
    public function jsonSerialize(): mixed
    {
        return $this->name;
    }
}
