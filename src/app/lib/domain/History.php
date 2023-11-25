<?php
declare(strict_types=1);

namespace MyPrayerJournal\Domain;

/**
 * A record of action taken on a prayer request, including updates to its text
 */
class History extends AsOf
{
    /** The action taken that generated this history entry */
    public RequestAction $status = RequestAction::Created;

    /** The text of the update, if applicable */
    public ?string $text = null;

    public function __construct()
    {
        $this->asOf = unix_epoch();
    }

    public function isCreated(): bool
    {
        return $this->status == RequestAction::Created;
    }

    public function isPrayed(): bool
    {
        return $this->status == RequestAction::Prayed;
    }

    public function isAnswered(): bool
    {
        return $this->status == RequestAction::Answered;
    }
}
