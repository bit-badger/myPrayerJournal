<?php
declare(strict_types=1);

namespace MyPrayerJournal\Domain;

use DateTimeImmutable;
use Visus\Cuid2\Cuid2;

/**
 * A prayer request
 */
class Request
{
    /** The ID for the request */
    public string $id;

    /** The date/time the request was originally entered */
    public DateTimeImmutable $enteredOn;

    /** The ID of the user to whom this request belongs */
    public string $userId = '';

    /** The date/time the snooze expires for this request */
    public ?DateTimeImmutable $snoozedUntil = null;

    /** The date/time this request should once again show as defined by recurrence */
    public ?DateTimeImmutable $showAfter = null;

    /** The type of recurrence for this request */
    public RecurrenceType $recurrenceType = RecurrenceType::Immediate;

    /** The units which apply to recurrences other than Immediate */
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

    public function __construct()
    {
        $this->id = (new Cuid2())->toString();
        $this->enteredOn = unix_epoch();
    }
}
