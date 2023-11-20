<?php
declare(strict_types=1);

namespace MyPrayerJournal\Domain;

use DateTimeImmutable, DateTimeZone;

/**
 * A prayer request, along with calculated fields, for use in displaying journal lists
 */
class JournalRequest extends AsOf
{
    /** The ID of the prayer request */
    public string $id = '';

    /** The ID of the user to whom the prayer request belongs */
    public string $userId = '';

    /** The current text of the request */
    public string $text = '';

    /** The date/time this request was last marked as prayed */
    public ?DateTimeImmutable $lastPrayed = null;

    /** The last action taken on this request */
    public RequestAction $lastAction = RequestAction::Created;

    /** When this request will be shown again after having been snoozed */
    public ?DateTimeImmutable $snoozedUntil = null;

    /** When this request will be show agains after a non-immediate recurrence */
    public ?DateTimeImmutable $showAfter = null;

    /** The type of recurrence for this request */
    public RecurrenceType $recurrenceType = RecurrenceType::Immediate;

    /** The units for non-immediate recurrence */
    public ?int $recurrence = null;

    /**
     * The history for this request
     * @var History[] $history
     */
    public array $history = [];
    
    /**
     * The notes for this request
     * @var Note[] $notes
     */
    public array $notes = [];

    /**
     * Constructor
     * 
     * @param ?Request $req The request off which this journal request should be populated
     * @param bool $full Whether to include history and notes (true) or exclude them (false)
     */
    public function __construct(?Request $req = null, bool $full = false)
    {
        if (is_null($req)) {
            $this->asOf       = unix_epoch();
            $this->lastPrayed = null;
        } else {
            $this->id             = $req->id;
            $this->userId         = $req->userId;
            $this->snoozedUntil   = $req->snoozedUntil;
            $this->showAfter      = $req->showAfter;
            $this->recurrenceType = $req->recurrenceType;
            $this->recurrence     = $req->recurrence;

            usort($req->history, AsOf::newestToOldest(...));
            $this->asOf = $req->history[array_key_first($req->history)]->asOf;
            $lastText = array_filter($req->history, fn (History $it) => !is_null($it->text));
            $this->text = $lastText[array_key_first($lastText)]->text;
            $lastPrayed = array_filter($req->history, fn (History $it) => $it->isPrayed());
            if ($lastPrayed) $this->lastPrayed = $lastPrayed[array_key_first($lastPrayed)]->asOf;
            
            if ($full) {
                usort($req->notes, AsOf::newestToOldest(...));
                $this->history = $req->history;
                $this->notes   = $req->notes;
            }
        }
    }
}
