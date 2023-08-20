<?php
declare(strict_types=1);

namespace MyPrayerJournal\Domain;

use DateTimeImmutable;

/**
 * A record of action taken on a prayer request, including updates to its text
 */
class History
{
    /** The date/time this action was taken */
    public DateTimeImmutable $asOf;

    /** The action taken that generated this history entry */
    public RequestAction $action = RequestAction::Created;

    /** The text of the update, if applicable */
    public ?string $text = null;

    public function __construct()
    {
        $this->asOf = new DateTimeImmutable('1/1/1970', new \DateTimeZone('Etc/UTC'));
    }

    public function isCreated(): bool
    {
        return $this->action == RequestAction::Created;
    }

    public function isPrayed(): bool
    {
        return $this->action == RequestAction::Prayed;
    }

    public function isAnswered(): bool
    {
        return $this->action == RequestAction::Answered;
    }
}
