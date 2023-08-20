<?php
declare(strict_types=1);

namespace MyPrayerJournal;

use BitBadger\PgSQL\Documents\{ Configuration, Definition, Document, DocumentIndex, Query };
use MyPrayerJournal\Domain\{ History, JournalRequest, Note, Request, RequestAction };

class Data
{
    /** The prayer request table */
    const REQ_TABLE = 'prayer_request';

    /**
     * Ensure the table and index exist
     */
    public static function startUp()
    {
        Configuration::$connectionString = "pgsql:host=localhost;port=5432;dbname=leafjson;user=leaf;password=leaf";
        Definition::ensureTable(Data::REQ_TABLE);
        Definition::ensureIndex(Data::REQ_TABLE, DocumentIndex::Optimized);
    }

    /**
     * Find a full prayer request by its ID
     * 
     * @param string $reqId The request ID
     * @param string $userId The ID of the currently logged-on user
     * @return ?Request The request, or null if it is not found
     */
    public static function findFullRequestById(string $reqId, string $userId): ?Request
    {
        $req = Document::findById(Data::REQ_TABLE, $reqId, Request::class);
        return is_null($req) || $req->userId != $userId ? null : $req;
    }

    /**
     * Add a history entry to the specified request
     * 
     * @param string $reqId The request ID
     * @param string $userId The ID of the currently logged-on user
     * @param History $history The history entry to be added
     */
    public static function addHistory(string $reqId, string $userId, History $history)
    {
        $req = Data::findFullRequestById($reqId, $userId);
        if (is_null($req)) throw new \InvalidArgumentException("$reqId not found");
        array_unshift($req->history, $history);
        Document::updateFull(Data::REQ_TABLE, $reqId, $req);
    }

    /**
     * Add a note to the specified request
     * 
     * @param string $reqId The request ID
     * @param string $userId The ID of the currently logged-on user
     * @param Note $note The note to be added
     */
    public static function addNote(string $reqId, string $userId, Note $note)
    {
        $req = Data::findFullRequestById($reqId, $userId);
        if (is_null($req)) throw new \InvalidArgumentException("$reqId not found");
        array_unshift($req->notes, $note);
        Document::updateFull(Data::REQ_TABLE, $reqId, $req);
    }

    /**
     * Add a new request
     * 
     * @param Request $req The request to be added
     */
    public static function addRequest(Request $req)
    {
        Document::insert(Data::REQ_TABLE, $req->id, $req);
    }

    /**
     * Map an array of `Request`s to an array of `JournalRequest`s
     * 
     * @param Request[] $reqs The requests to map
     * @param bool $full Whether to include history and notes (true) or not (false)
     * @return JournalRequest[] The journal request objects
     */
    private static function mapToJournalRequest(array $reqs, bool $full): array
    {
        return array_map(fn (Request $req) => new JournalRequest($req, $full), $reqs);
    }

    /**
     * Get journal requests for the given user by "answered" status
     * 
     * @param string $userId The ID of the user for whom requests should be retrieved
     * @param string $op The JSON Path operator to use for comparison (`==` or `<>`)
     * @return JournalRequest[] The journal request objects
     */
    private static function getJournalByAnswered(string $userId, string $op): array
    {
        $sql = Query::selectFromTable(Data::REQ_TABLE)
            . ' WHERE ' . Query::whereDataContains(':criteria') . ' AND ' . Query::whereJsonPathMatches(':path');
        $params = [
            ':criteria' => Query::jsonbDocParam([ 'userId' => $userId ]),
            ':path'     => '$.history[*].action (@ ' . $op . ' "' . RequestAction::Answered->name . '")'
        ];
        return Data::mapToJournalRequest(
            Document::customList($sql, $params, Request::class, Document::mapFromJson(...)), true);
    }
    
    /**
     * Retrieve all answered requests for this user
     * 
     * @param string $userId The ID of the user for whom answered requests should be retrieved
     * @return JournalRequest[] The answered requests
     */
    public static function getAnsweredRequests(string $userId): array
    {
        $answered = Data::getJournalByAnswered($userId, '==');
        usort($answered,
            fn (JournalRequest $a, JournalRequest $b) => $a->asOf == $b->asOf ? 0 : ($a->asOf > $b->asOf ? -1 : 1));
        return $answered;
    }

    /**
     * Get the user's current prayer request journal
     * 
     * @param string $userId The ID of the user whose journal should be retrieved
     * @return JournalRequest[] The journal request objects
     */
    public static function getJournal(string $userId): array
    {
        $reqs = data::getJournalByAnswered($userId, '<>');
        usort($reqs,
            fn (JournalRequest $a, JournalRequest $b) => $a->asOf == $b->asOf ? 0 : ($a->asOf < $b->asOf ? -1 : 1));
        return $reqs;
    }
}
