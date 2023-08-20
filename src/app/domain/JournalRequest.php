<?php
declare(strict_types=1);

namespace MyPrayerJournal\Domain;

/**
 * A prayer request, along with calculated fields, for use in displaying journal lists
 */
class JournalRequest
{
    /** The ID of the prayer request */
    public string $id = '';

    /** The ID of the user to whom the prayer request belongs */
    public string $userId = '';

    /** The current text of the request */
    public string $text = '';

    /** The date/time this request was last updated */
    public \DateTimeImmutable $asOf;

    /** The date/time this request was last marked as prayed */
    public \DateTimeImmutable $lastPrayed;

    /** The last action taken on this request */
    public RequestAction $lastAction = RequestAction::Created;

    /** When this request will be shown again after having been snoozed */
    public ?\DateTimeImmutable $snoozedUntil = null;

    /** When this request will be show agains after a non-immediate recurrence */
    public ?\DateTimeImmutable $showAfter = null;

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
            $this->asOf       = new \DateTimeImmutable('1/1/1970', new \DateTimeZone('Etc/UTC'));
            $this->lastPrayed = new \DateTimeImmutable('1/1/1970', new \DateTimeZone('Etc/UTC'));
        } else {
            $this->id             = $req->id;
            $this->userId         = $req->userId;
            $this->snoozedUntil   = $req->snoozedUntil;
            $this->showAfter      = $req->showAfter;
            $this->recurrenceType = $req->recurrenceType;
            $this->recurrence     = $req->recurrence;

            usort($req->history,
                fn (History $a, History $b) => $a->asOf == $b->asOf ? 0 : ($a->asOf > $b->asOf ? -1 : 1));
            $this->asOf = $req->history[0]->asOf;
            $this->lastPrayed =
                array_values(array_filter($req->history, fn (History $it) => $it->action == RequestAction::Prayed))[0]
                ?->asOf;
            
            if ($full) {
                usort($req->notes,
                    fn (Note $a, Note $b) => $a->asOf == $b->asOf ? 0 : ($a->asOf > $b->asOf ? -1 : 1));
                $this->history = $req->history;
                $this->notes = $req->notes;
            }
        }
    }
}
