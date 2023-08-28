<?php
declare(strict_types=1);

namespace BitBadger\PgSQL\Documents;

use PDOStatement;

/** Document manipulation functions */
class Document
{
    /** JSON Mapper instance to use for creating a domain type instance from a document */
    private static ?\JsonMapper $mapper = null;

    /** Attribute that prevents PDO from attempting its own PREPARE on a query */
    private const NO_PREPARE = [ \PDO::ATTR_EMULATE_PREPARES => false ];

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
            self::$mapper = new \JsonMapper();
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
     * @param string $docId The ID of the document on which action should be taken
     * @param array|object $document The array or object representing the document
     */
    private static function executeNonQuery(string $query, string $docId, array|object $document)
    {
        $nonQuery = pdo()->prepare($query, self::NO_PREPARE);
        $nonQuery->bindParam(':id', $docId);
        $nonQuery->bindParam(':data', Query::jsonbDocParam($document));
        $nonQuery->execute();
    }

    /**
     * Insert a document
     * 
     * @param string $tableName The name of the table into which a document should be inserted
     * @param string $docId The ID of the document to be inserted
     * @param array|object $document The array or object representing the document
     */
    public static function insert(string $tableName, string $docId, array|object $document)
    {
        self::executeNonQuery(Query::insert($tableName), $docId, $document);
    }

    /**
     * Save (upsert) a document
     * 
     * @param string $tableName The name of the table into which a document should be inserted
     * @param string $docId The ID of the document to be inserted
     * @param array|object $document The array or object representing the document
     */
    public static function save(string $tableName, string $docId, array|object $document)
    {
        self::executeNonQuery(Query::save($tableName), $docId, $document);
    }

    /**
     * Count all documents in a table
     * 
     * @param string $tableName The name of the table in which documents should be counted
     * @return int The number of documents in the table
     */
    public static function countAll(string $tableName): int
    {
        $result = pdo()->query(Query::countAll($tableName))->fetch(\PDO::FETCH_ASSOC);
        return intval($result['it']);
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
        $query = pdo()->prepare(Query::countByContains($tableName), self::NO_PREPARE);
        $query->bindParam(':criteria', Query::jsonbDocParam($criteria));
        $query->execute();
        $result = $query->fetch(\PDO::FETCH_ASSOC);
        return intval($result['it']);
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
        $query = pdo()->prepare(Query::countByContains($tableName), self::NO_PREPARE);
        $query->bindParam(':path', $jsonPath);
        $query->execute();
        $result = $query->fetch(\PDO::FETCH_ASSOC);
        return intval($result['it']);
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
        $query = pdo()->prepare(Query::existsById($tableName), self::NO_PREPARE);
        $query->bindParam(':id', $docId);
        $query->execute();
        $result = $query->fetch(\PDO::FETCH_ASSOC);
        return boolval($result['it']);
    }

    /**
     * Determine if documents exist by JSON containment `@>`
     * 
     * @param string $tableName The name of the table in which existence should be checked
     * @param array|object $criteria The criteria for the JSON containment query
     * @return int True if any documents in the table match the JSON containment query, false if not
     */
    public static function existsByContains(string $tableName, array|object $criteria): bool
    {
        $query = pdo()->prepare(Query::existsByContains($tableName), self::NO_PREPARE);
        $query->bindParam(':criteria', Query::jsonbDocParam($criteria));
        $query->execute();
        $result = $query->fetch(\PDO::FETCH_ASSOC);
        return boolval($result['it']);
    }

    /**
     * Determine if documents exist by JSON Path match `@?`
     * 
     * @param string $tableName The name of the table in which existence should be checked
     * @param string $jsonPath The JSON Path to be matched
     * @return int True if any documents in the table match the JSON Path, false if not
     */
    public static function existsByJsonPath(string $tableName, string $jsonPath): bool
    {
        $query = pdo()->prepare(Query::existsByJsonPath($tableName), self::NO_PREPARE);
        $query->bindParam(':path', $jsonPath);
        $query->execute();
        $result = $query->fetch(\PDO::FETCH_ASSOC);
        return boolval($result['it']);
    }

    /**
     * Map the results of a query to domain type objects
     * 
     * @param \PDOStatement $stmt The statement with the query to be run
     * @param class-string<Type> $className The type of document to be mapped
     * @return array<Type> The documents matching the query
     */
    private static function mapResults(\PDOStatement $stmt, string $className): array
    {
        return array_map(fn ($it) => self::mapFromJson($it, $className), $stmt->fetchAll(\PDO::FETCH_ASSOC));
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
        return self::mapResults(pdo()->query(Query::selectFromTable($tableName)), $className);
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
        $query = pdo()->prepare(Query::findById($tableName), self::NO_PREPARE);
        $query->bindParam(':id', $docId);
        $query->execute();
        $result = $query->fetch(\PDO::FETCH_ASSOC);
        return $result ? self::mapFromJson($result, $className) : null;
    }

    /**
     * Create a JSON containment query
     * 
     * @param string $tableName The table from which documents should be retrieved
     * @param array|object $criteria The criteria for the JSON containment query
     * @return \PDOStatement An executed query ready to be fetched
     */
    private static function queryByContains(string $tableName, array|object $criteria): \PDOStatement
    {
        $query = pdo()->prepare(Query::findByContains($tableName), self::NO_PREPARE);
        $query->bindParam(':criteria', Query::jsonbDocParam($criteria));
        $query->execute();
        return $query;
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
        return self::mapResults(self::queryByContains($tableName, $criteria), $className);
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
        $query = self::queryByContains($tableName, $criteria);
        $result = $query->fetch(\PDO::FETCH_ASSOC);
        return $result ? self::mapFromJson($result, $className) : null;
    }

    /**
     * Retrieve documents in a table via JSON Path match `@?`
     * 
     * @param string $tableName The table from which documents should be retrieved
     * @param string $jsonPath The JSON Path to be matched
     * @return \PDOStatement An executed query ready to be fetched
     */
    private static function queryByJsonPath(string $tableName, string $jsonPath): \PDOStatement
    {
        $query = pdo()->prepare(Query::findByJsonPath($tableName), self::NO_PREPARE);
        $query->bindParam(':path', $jsonPath);
        $query->execute();
        return $query;
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
        return self::mapResults(self::queryByJsonPath($tableName, $jsonPath), $className);
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
        $query = self::queryByJsonPath($tableName, $jsonPath);
        $result = $query->fetch(\PDO::FETCH_ASSOC);
        return $result ? self::mapFromJson($result, $className) : null;
    }

    /**
     * Update a full document
     * 
     * @param string $tableName The table in which the document should be updated
     * @param string $docId The ID of the document to be updated
     * @param array|object $document The document to be updated
     */
    public static function updateFull(string $tableName, string $docId, array|object $document)
    {
        self::executeNonQuery(Query::updateFull($tableName), $docId, $document);
    }

    /**
     * Update a partial document by its ID
     * 
     * @param string $tableName The table in which the document should be updated
     * @param string $docId The ID of the document to be updated
     * @param array|object $document The partial document to be updated
     */
    public static function updatePartialById(string $tableName, string $docId, array|object $document)
    {
        self::executeNonQuery(Query::updatePartialById($tableName), $docId, $document);
    }

    /**
     * Update partial documents by JSON containment `@>`
     * 
     * @param string $tableName The table in which documents should be updated
     * @param array|object $criteria The JSON containment criteria
     * @param array|object $document The document to be updated
     */
    public static function updatePartialByContains(string $tableName, array|object $criteria, array|object $document)
    {
        $query = pdo()->prepare(Query::updatePartialByContains($tableName), self::NO_PREPARE);
        $query->bindParam(':data', Query::jsonbDocParam($document));
        $query->bindParam(':criteria', Query::jsonbDocParam($criteria));
        $query->execute();
    }

    /**
     * Update partial documents by JSON Path match `@?`
     * 
     * @param string $tableName The table in which documents should be updated
     * @param string $jsonPath The JSON Path to be matched
     * @param array|object $document The document to be updated
     */
    public static function updatePartialByJsonPath(string $tableName, string $jsonPath, array|object $document)
    {
        $query = pdo()->prepare(Query::updatePartialByContains($tableName), self::NO_PREPARE);
        $query->bindParam(':data', Query::jsonbDocParam($document));
        $query->bindParam(':path', $jsonPath);
        $query->execute();
    }

    /**
     * Delete a document by its ID
     * 
     * @param string $tableName The table from which a document should be deleted
     * @param string $docId The ID of the document to be deleted
     */
    public static function deleteById(string $tableName, string $docId)
    {
        self::executeNonQuery(Query::deleteById($tableName), $docId, []);
    }

    /**
     * Delete documents by JSON containment `@>`
     * 
     * @param string $tableName The table from which documents should be deleted
     * @param array|object $criteria The criteria for the JSON containment query
     */
    public static function deleteByContains(string $tableName, array|object $criteria)
    {
        $query = pdo()->prepare(Query::deleteByContains($tableName), self::NO_PREPARE);
        $query->bindParam(':criteria', Query::jsonbDocParam($criteria));
        $query->execute();
    }

    /**
     * Delete documents by JSON Path match `@?`
     * 
     * @param string $tableName The table from which documents should be deleted
     * @param string $jsonPath The JSON Path expression to be matched
     */
    public static function deleteByJsonPath(string $tableName, string $jsonPath)
    {
        $query = pdo()->prepare(Query::deleteByJsonPath($tableName), self::NO_PREPARE);
        $query->bindParam(':path', $jsonPath);
        $query->execute();
    }

    // TODO: custom

    /**
     * Create and execute a custom query
     * 
     * @param string $sql The SQL query to execute
     * @param array $params An associative array of parameters for the SQL query
     * @return PDOStatement The query, executed and ready to be fetched
     */
    private static function createCustomQuery(string $sql, array $params): PDOStatement
    {
        $query = pdo()->prepare($sql, [ \PDO::ATTR_EMULATE_PREPARES => false ]);
        array_walk($params, fn ($value, $name) => $query->bindParam($name, $value));
        $query->execute();
        return $query;
    }

    /**
     * Retrieve documents via a custom query and mapping
     * 
     * @param string $sql The SQL query to execute
     * @param array $params An associative array of parameters for the SQL query
     * @param callable $mapFunc A function that expects an associative array and returns a value of the desired type
     * @param class-string<Type> $className The type of document to be mapped
     * @return array<Type> The documents matching the query
     */
    public static function customList(string $sql, array $params, string $className, callable $mapFunc): array
    {
        return array_map(
            fn ($it) => $mapFunc($it, $className),
            Document::createCustomQuery($sql, $params)->fetchAll(\PDO::FETCH_ASSOC));
    }

    /**
     * Retrieve a document via a custom query and mapping
     * 
     * @param string $sql The SQL query to execute
     * @param array $params An associative array of parameters for the SQL query
     * @param callable $mapFunc A function that expects an associative array and returns a value of the desired type
     * @param class-string<Type> $className The type of document to be mapped
     * @return ?Type The document matching the query, or null if none is found
     */
    public static function customSingle(string $sql, array $params, string $className, callable $mapFunc): mixed
    {
        $result = self::createCustomQuery($sql, $params)->fetch(\PDO::FETCH_ASSOC);
        return $result ? $mapFunc($result, $className) : null;
    }

    /**
     * Execute a custom query that does not return a result
     * 
     * @param string $sql The SQL query to execute
     * @param array $params An associative array of parameters for the SQL query
     */
    public static function customNonQuery(string $sql, array $params)
    {
        self::createCustomQuery($sql, $params);
    }
}
