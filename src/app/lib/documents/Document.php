<?php
declare(strict_types=1);

namespace BitBadger\PgDocuments;

use Exception;
use JsonMapper;
use PgSql\Result;

/** Document manipulation functions */
class Document
{
    /** JSON Mapper instance to use for creating a domain type instance from a document */
    private static ?JsonMapper $mapper = null;

    /**
     * Map a domain type from the JSON document retrieved
     * 
     * @param string $columnName The name of the column from the database
     * @param array $result An associative array with a single result to be mapped
     * @param class-string<Type> $className The name of the class onto which the JSON will be mapped
     * @return Type The domain type
     */
    public static function mapDocFromJson(string $columnName, array $result, string $className): mixed
    {
        if (is_null(self::$mapper)) {
            self::$mapper = new JsonMapper();
        }

        $mapped = new $className();
        self::$mapper->map(json_decode($result[$columnName]), $mapped);
        return $mapped;
    }

    /**
     * Map a domain type from the JSON document retrieved
     * 
     * @param array $result An associative array with a single result to be mapped
     * @param class-string<Type> $className The name of the class onto which the JSON will be mapped
     * @return Type The domain type
     */
    public static function mapFromJson(array $result, string $className): mixed
    {
        return self::mapDocFromJson('data', $result, $className);
    }

    /**
     * Execute a document-focused statement that does not return results
     *
     * @param string $query The query to be executed
     * @param array|object $document The array or object representing the document
     * @throws Exception If the document's ID is null
     */
    private static function executeNonQuery(string $query, array|object $document): void
    {
        $docId = is_array($document)
            ? $document[Configuration::$keyName]
            : get_object_vars($document)[Configuration::$keyName];
        if (is_null($docId)) throw new Exception('PgDocument: ID cannot be NULL');
        /** @var Result|bool $result */
        $result = pg_query_params(pg_conn(), $query, [ $docId, Query::jsonbDocParam($document) ]);
        if ($result) pg_free_result($result);
    }

    /**
     * Insert a document
     * 
     * @param string $tableName The name of the table into which a document should be inserted
     * @param array|object $document The array or object representing the document
     */
    public static function insert(string $tableName, array|object $document): void
    {
        self::executeNonQuery(Query::insert($tableName), $document);
    }

    /**
     * Save (upsert) a document
     * 
     * @param string $tableName The name of the table into which a document should be inserted
     * @param array|object $document The array or object representing the document
     */
    public static function save(string $tableName, array|object $document): void
    {
        self::executeNonQuery(Query::save($tableName), $document);
    }

    /**
     * Run a count query, returning the `it` parameter of that query as an integer
     * 
     * @param string $sql The SQL query that will return a count
     * @param array $params Parameters needed for that query
     * @return int The count of matching rows for the query
     */
    private static function runCount(string $sql, array $params): int
    {
        /** @var Result|bool $result */
        $result = pg_query_params(pg_conn(), $sql, $params);
        if (!$result) return -1;
        $count = intval(pg_fetch_assoc($result)['it']);
        pg_free_result($result);
        return $count;
    }

    /**
     * Count all documents in a table
     * 
     * @param string $tableName The name of the table in which documents should be counted
     * @return int The number of documents in the table
     */
    public static function countAll(string $tableName): int
    {
        return self::runCount(Query::countAll($tableName), []);
    }

    /**
     * Count documents in a table by JSON containment `@>`
     * 
     * @param string $tableName The name of the table in which documents should be counted
     * @param array|object $criteria The criteria for the JSON containment query
     * @return int The number of documents in the table matching the JSON containment query
     */
    public static function countByContains(string $tableName, array|object $criteria): int
    {
        return self::runCount(Query::countByContains($tableName), [ Query::jsonbDocParam($criteria) ]);
    }

    /**
     * Count documents in a table by JSON Path match `@?`
     * 
     * @param string $tableName The name of the table in which documents should be counted
     * @param string $jsonPath The JSON Path to be matched
     * @return int The number of documents in the table matching the JSON Path
     */
    public static function countByJsonPath(string $tableName, string $jsonPath): int
    {
        return self::runCount(Query::countByJsonPath($tableName), [ $jsonPath ]);
    }

    /**
     * Run an existence query (returning the `it` parameter of that query)
     * 
     * @param string $sql The SQL query that will return existence
     * @param array $params Parameters needed for that query
     * @return bool The result of the existence query
     */
    private static function runExists(string $sql, array $params): bool
    {
        /** @var Result|bool $result */
        $result = pg_query_params(pg_conn(), $sql, $params);
        if (!$result) return false;
        $exists = boolval(pg_fetch_assoc($result)['it']);
        pg_free_result($result);
        return $exists;
    }

    /**
     * Determine if a document exists for the given ID
     * 
     * @param string $tableName The name of the table in which existence should be checked
     * @param string $docId The ID of the document whose existence should be checked
     * @return bool True if the document exists, false if not
     */
    public static function existsById(string $tableName, string $docId): bool
    {
        return self::runExists(Query::existsById($tableName), [ $docId ]);
    }

    /**
     * Determine if documents exist by JSON containment `@>`
     * 
     * @param string $tableName The name of the table in which existence should be checked
     * @param array|object $criteria The criteria for the JSON containment query
     * @return bool True if any documents in the table match the JSON containment query, false if not
     */
    public static function existsByContains(string $tableName, array|object $criteria): bool
    {
        return self::runExists(Query::existsByContains($tableName), [ Query::jsonbDocParam($criteria) ]);
    }

    /**
     * Determine if documents exist by JSON Path match `@?`
     * 
     * @param string $tableName The name of the table in which existence should be checked
     * @param string $jsonPath The JSON Path to be matched
     * @return bool True if any documents in the table match the JSON Path, false if not
     */
    public static function existsByJsonPath(string $tableName, string $jsonPath): bool
    {
        return self::runExists(Query::existsByJsonPath($tableName), [ $jsonPath ]);
    }

    /**
     * Run a query, mapping the results to an array of domain type objects
     * 
     * @param string $sql The query to be run
     * @param array $params The parameters for the query
     * @param class-string<Type> $className The type of document to be mapped
     * @return array<Type> The documents matching the query
     */
    private static function runListQuery(string $sql, array $params, string $className): array
    {
        /** @var Result|bool $result */
        $result = pg_query_params(pg_conn(), $sql, $params);
        try {
            if (!$result || pg_result_status($result) == PGSQL_EMPTY_QUERY) return [];
            return array_map(fn ($it) => self::mapFromJson($it, $className), pg_fetch_all($result));
        } finally {
            if ($result) pg_free_result($result);
        }
    }

    /**
     * Retrieve all documents in a table
     * 
     * @param string $tableName The table from which all documents should be retrieved
     * @param class-string<Type> $className The type of document to be retrieved
     * @return array<Type> An array of documents
     */
    public static function findAll(string $tableName, string $className): array
    {
        return self::runListQuery(Query::selectFromTable($tableName), [], $className);
    }

    /**
     * Retrieve a document by its ID
     * 
     * @param string $tableName The table from which a document should be retrieved
     * @param string $docId The ID of the document to retrieve
     * @param class-string<Type> $className The type of document to retrieve
     * @return Type|null The document, or null if it is not found
     */
    public static function findById(string $tableName, string $docId, string $className): mixed
    {
        $results = self::runListQuery(Query::findById($tableName), [ $docId ], $className);
        return $results ? $results[0] : null;
    }

    /**
     * Retrieve documents in a table via JSON containment `@>`
     * 
     * @param string $tableName The table from which documents should be retrieved
     * @param array|object $criteria The criteria for the JSON containment query
     * @param class-string<Type> $className The type of document to be retrieved
     * @return array<Type> Documents matching the JSON containment query
     */
    public static function findByContains(string $tableName, array|object $criteria, string $className): array
    {
        return self::runListQuery(Query::findByContains($tableName), [ Query::jsonbDocParam($criteria) ], $className);
    }

    /**
     * Retrieve the first matching document via JSON containment `@>`
     * 
     * @param string $tableName The table from which documents should be retrieved
     * @param array|object $criteria The criteria for the JSON containment query
     * @param class-string<Type> $className The type of document to be retrieved
     * @return Type|null The document, or null if none match
     */
    public static function findFirstByContains(string $tableName, array|object $criteria, string $className): mixed
    {
        $results = self::runListQuery(Query::findByContains($tableName) . ' LIMIT 1',
            [ Query::jsonbDocParam($criteria) ], $className);
        return $results ? $results[0] : null;
    }

    /**
     * Retrieve documents in a table via JSON Path match `@?`
     * 
     * @param string $tableName The table from which documents should be retrieved
     * @param string $jsonPath The JSON Path to be matched
     * @param class-string<Type> $className The type of document to be retrieved
     * @return array<Type> Documents matching the JSON Path
     */
    public static function findByJsonPath(string $tableName, string $jsonPath, string $className): array
    {
        return self::runListQuery(Query::findByJsonPath($tableName), [ $jsonPath ], $className);
    }

    /**
     * Retrieve the first matching document via JSON Path match `@?`
     * 
     * @param string $tableName The table from which documents should be retrieved
     * @param string $jsonPath The JSON Path to be matched
     * @param class-string<Type> $className The type of document to be retrieved
     * @return Type|null The document, or null if none match
     */
    public static function findFirstByJsonPath(string $tableName, string $jsonPath, string $className): mixed
    {
        $results = self::runListQuery(Query::findByJsonPath($tableName) . ' LIMIT 1', [ $jsonPath ], $className);
        return $results ? $results[0] : null;
    }

    /**
     * Update a full document
     * 
     * @param string $tableName The table in which the document should be updated
     * @param array|object $document The document to be updated
     */
    public static function updateFull(string $tableName, array|object $document): void
    {
        self::executeNonQuery(Query::updateFull($tableName), $document);
    }

    /**
     * Update a partial document by its ID
     * 
     * @param string $tableName The table in which the document should be updated
     * @param array|object $document The partial document to be updated
     */
    public static function updatePartialById(string $tableName, array|object $document): void
    {
        self::executeNonQuery(Query::updatePartialById($tableName), $document);
    }

    /**
     * Update partial documents by JSON containment `@>`
     * 
     * @param string $tableName The table in which documents should be updated
     * @param array|object $criteria The JSON containment criteria
     * @param array|object $document The document to be updated
     */
    public static function updatePartialByContains(string $tableName, array|object $criteria,
                                                   array|object $document): void
    {
        /** @var Result|bool $result */
        $result = pg_query_params(pg_conn(), Query::updatePartialByContains($tableName),
            [ Query::jsonbDocParam($criteria), Query::jsonbDocParam($document) ]);
        if ($result) pg_free_result($result);
    }

    /**
     * Update partial documents by JSON Path match `@?`
     * 
     * @param string $tableName The table in which documents should be updated
     * @param string $jsonPath The JSON Path to be matched
     * @param array|object $document The document to be updated
     */
    public static function updatePartialByJsonPath(string $tableName, string $jsonPath, array|object $document): void
    {
        /** @var Result|bool $result */
        $result = pg_query_params(pg_conn(), Query::updatePartialByJsonPath($tableName),
            [ $jsonPath, Query::jsonbDocParam($document) ]);
        if ($result) pg_free_result($result);
    }

    /**
     * Delete a document by its ID
     * 
     * @param string $tableName The table from which a document should be deleted
     * @param string $docId The ID of the document to be deleted
     */
    public static function deleteById(string $tableName, string $docId): void
    {
        self::executeNonQuery(Query::deleteById($tableName), [ Configuration::$keyName => $docId ]);
    }

    /**
     * Delete documents by JSON containment `@>`
     * 
     * @param string $tableName The table from which documents should be deleted
     * @param array|object $criteria The criteria for the JSON containment query
     */
    public static function deleteByContains(string $tableName, array|object $criteria): void
    {
        /** @var Result|bool $result */
        $result = pg_query_params(pg_conn(), Query::deleteByContains($tableName), [ Query::jsonbDocParam($criteria) ]);
        if ($result) pg_free_result($result);
    }

    /**
     * Delete documents by JSON Path match `@?`
     * 
     * @param string $tableName The table from which documents should be deleted
     * @param string $jsonPath The JSON Path expression to be matched
     */
    public static function deleteByJsonPath(string $tableName, string $jsonPath): void
    {
        /** @var Result|bool $result */
        $result = pg_query_params(pg_conn(), Query::deleteByJsonPath($tableName), [ $jsonPath ]);
        if ($result) pg_free_result($result);
    }

    /**
     * Retrieve documents via a custom query and mapping
     * 
     * @param string $sql The SQL query to execute
     * @param array $params A positional array of parameters for the SQL query
     * @param callable $mapFunc A function that expects an associative array and returns a value of the desired type
     * @param class-string<Type> $className The type of document to be mapped
     * @return array<Type> The documents matching the query
     */
    public static function customList(string $sql, array $params, string $className, callable $mapFunc): array
    {
        /** @var Result|bool $result */
        $result = pg_query_params(pg_conn(), $sql, $params);
        try {
            if (!$result || pg_result_status($result) == PGSQL_EMPTY_QUERY) return [];
            return array_map(fn ($it) => $mapFunc($it, $className), pg_fetch_all($result));
        } finally {
            if ($result) pg_free_result($result);
        }
    }

    /**
     * Retrieve a document via a custom query and mapping
     * 
     * @param string $sql The SQL query to execute ("LIMIT 1" will be appended)
     * @param array $params A positional array of parameters for the SQL query
     * @param callable $mapFunc A function that expects an associative array and returns a value of the desired type
     * @param class-string<Type> $className The type of document to be mapped
     * @return ?Type The document matching the query, or null if none is found
     */
    public static function customSingle(string $sql, array $params, string $className, callable $mapFunc): mixed
    {
        $results = self::customList("$sql LIMIT 1", $params, $className, $mapFunc);
        return $results ? $results[0] : null;
    }

    /**
     * Execute a custom query that does not return a result
     * 
     * @param string $sql The SQL query to execute
     * @param array $params A positional array of parameters for the SQL query
     */
    public static function customNonQuery(string $sql, array $params): void
    {
        /** @var Result|bool $result */
        $result = pg_query_params(pg_conn(), $sql, $params);
        if ($result) pg_free_result($result);
    }
}
