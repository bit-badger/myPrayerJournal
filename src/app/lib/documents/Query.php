<?php
declare(strict_types=1);

namespace BitBadger\PgDocuments;

/** Query construction functions */
class Query
{
    /**
     * Create a `SELECT` clause to retrieve the document data from the given table
     * 
     * @param string $tableName The name of the table from which documents should be selected
     * @return string A `SELECT` clause for the given table
     */
    public static function selectFromTable(string $tableName): string
    {
        return "SELECT data FROM $tableName";
    }

    /**
     * Create a `WHERE` clause fragment to implement a key check condition
     *
     * @param string $paramName The name of the parameter to be replaced when the query is executed
     * @return string A `WHERE` clause fragment with the named key and parameter
     */
    public static function whereById(string $paramName): string
    {
        return sprintf("data -> '%s' = %s", Configuration::$keyName, $paramName);
    }

    /**
     * Create a `WHERE` clause fragment to implement a @> (JSON contains) condition
     * 
     * @param string $paramName The name of the parameter for the contains clause
     * @return string A `WHERE` clause fragment with the named parameter
     */
    public static function whereDataContains(string $paramName): string
    {
        return "data @> $paramName";
    }
    
    /**
     * Create a `WHERE` clause fragment to implement a @? (JSON Path match) condition
     * 
     * @param string $paramName THe name of the parameter for the JSON Path match
     * @return string A `WHERE` clause fragment with the named parameter
     */
    public static function whereJsonPathMatches(string $paramName): string
    {
        return "data @? $paramName::jsonpath";
    }
    
    /**
     * Create a JSONB document parameter
     * 
     * @param array|object $it The array or object to become a JSONB parameter
     * @return string The encoded JSON
     */
    public static function jsonbDocParam(array|object $it): string
    {
        return json_encode($it);
    }

    /**
     * Query to insert a document
     * 
     * @param string $tableName The name of the table into which the document will be inserted
     * @return string The `INSERT` statement (with `$1` parameter defined for the document)
     */
    public static function insert(string $tableName): string
    {
        return sprintf('INSERT INTO %s (data) VALUES ($1)', $tableName);
    }

    /**
     * Query to save a document, inserting it if it does not exist and updating it if it does (AKA "upsert")
     * 
     * @param string $tableName The name of the table into which the document will be saved
     * @return string The `INSERT`/`ON CONFLICT DO UPDATE` statement (with `$1` parameter defined for the document)
     */
    public static function save(string $tableName): string
    {
        return sprintf('INSERT INTO %s (data) VALUES ($1) ON CONFLICT (data) DO UPDATE SET data = EXCLUDED.data',
            $tableName);
    }

    /**
     * Query to count documents in a table
     *
     * @param string $tableName The name of the table for which documents should be counted
     * @param string $where The condition for which documents should be counted
     * @return string A `SELECT` statement to obtain the count of documents for the given table
     */
    private static function countQuery(string $tableName, string $where): string
    {
        return "SELECT COUNT(*) AS it FROM $tableName WHERE $where";
    }

    /**
     * Query to count all documents in a table
     * 
     * @param string $tableName The name of the table whose rows will be counted
     * @return string A `SELECT` statement to obtain the count of all documents in the given table
     */
    public static function countAll(string $tableName): string
    {
        return self::countQuery($tableName, '1 = 1');
    }
    
    /**
     * Query to count matching documents using a JSON containment query `@>`
     * 
     * @param string $tableName The name of the table from which the count should be obtained
     * @return string A `SELECT` statement to obtain the count of documents via JSON containment
     */
    public static function countByContains(string $tableName): string
    {
        return self::countQuery($tableName, self::whereDataContains('$1'));
    }
    
    /**
     * Query to count matching documents using a JSON Path match `@?`
     * 
     * @param string $tableName The name of the table from which the count should be obtained
     * @return string A `SELECT` statement to obtain the count of documents via JSON Path match
     */
    public static function countByJsonPath(string $tableName): string
    {
        return self::countQuery($tableName, self::whereJsonPathMatches('$1'));
    }

    /**
     * Query to check document existence
     *
     * @param string $tableName The name of the table in which document existence should be checked
     * @param string $where The criteria for which document existence should be checked
     * @return string A `SELECT` statement to check document existence for the given criteria
     */
    private static function existsQuery(string $tableName, string $where): string
    {
        return "SELECT EXISTS (SELECT 1 FROM $tableName WHERE $where) AS it";
    }
    /**
     * Query to determine if a document exists for the given ID
     * 
     * @param string $tableName The name of the table in which existence should be checked
     * @return string A `SELECT` statement to check existence of a document by its ID
     */
    public static function existsById(string $tableName): string
    {
        return self::existsQuery($tableName, self::whereById('$1'));
    }

    /**
     * Query to determine if documents exist using a JSON containment query `@>`
     * 
     * @param string $tableName The name of the table in which existence should be checked
     * @return string A `SELECT` statement to check existence of a document by JSON containment
     */
    public static function existsByContains(string $tableName): string
    {
        return self::existsQuery($tableName, self::whereDataContains('$1'));
    }
    
    /**
     * Query to determine if documents exist using a JSON Path match `@?`
     * 
     * @param string $tableName The name of the table in which existence should be checked
     * @return string A `SELECT` statement to check existence of a document by JSON Path match
     */
    public static function existsByJsonPath(string $tableName): string
    {
        return self::existsQuery($tableName, self::whereJsonPathMatches('$1'));
    }
    
    /**
     * Query to retrieve a document by its ID
     * 
     * @param string $tableName The name of the table from which a document should be retrieved
     * @return string A `SELECT` statement to retrieve a document by its ID
     */
    public static function findById(string $tableName): string
    {
        return sprintf('%s WHERE %s', self::selectFromTable($tableName), self::whereById('$1'));
    }
    
    /**
     * Query to retrieve documents using a JSON containment query `@>`
     * 
     * @param string $tableName The name of the table from which a document should be retrieved
     * @return string A `SELECT` statement to retrieve documents by JSON containment
     */
    public static function findByContains(string $tableName): string
    {
        return sprintf('%s WHERE %s', self::selectFromTable($tableName), self::whereDataContains('$1'));
    }
    
    /**
     * Query to retrieve documents using a JSON Path match `@?`
     * 
     * @param string $tableName The name of the table from which a document should be retrieved
     * @return string A `SELECT` statement to retrieve a documents by JSON Path match
     */
    public static function findByJsonPath(string $tableName): string
    {
        return sprintf('%s WHERE %s', self::selectFromTable($tableName), self::whereJsonPathMatches('$1'));
    }
    
    /**
     * Query to update a document, replacing the existing document
     * 
     * @param string $tableName The name of the table in which a document should be updated
     * @return string An `UPDATE` statement to update a document by its ID
     */
    public static function updateFull(string $tableName): string
    {
        return sprintf('UPDATE %s SET data = $2 WHERE %s', $tableName, self::whereById('$1'));
    }

    /**
     * Query to apply a partial update to a document
     *
     * @param string $tableName The name of the table in which documents should be updated
     * @param string $where The `WHERE` clause specifying which documents should be updated
     * @return string An `UPDATE` statement to update a partial document ($1 is ID, $2 is document)
     */
    private static function updatePartial(string $tableName, string $where): string
    {
        return sprintf('UPDATE %s SET data = data || $2 WHERE %s', $tableName, $where);
    }

    /**
     * Query to update a document, merging the existing document with the one provided
     * 
     * @param string $tableName The name of the table in which a document should be updated
     * @return string An `UPDATE` statement to update a document by its ID
     */
    public static function updatePartialById(string $tableName): string
    {
        return self::updatePartial($tableName, self::whereById('$1'));
    }
    
    /**
     * Query to update partial documents matching a JSON containment query `@>`
     * 
     * @param string $tableName The name of the table in which documents should be updated
     * @return string An `UPDATE` statement to update documents by JSON containment
     */
    public static function updatePartialByContains(string $tableName): string
    {
        return self::updatePartial($tableName, self::whereDataContains('$1'));
    }

    /**
     * Query to update partial documents matching a JSON containment query `@>`
     * 
     * @param string $tableName The name of the table in which documents should be updated
     * @return string An `UPDATE` statement to update  documents by JSON Path match
     */
    public static function updatePartialByJsonPath(string $tableName): string
    {
        return self::updatePartial($tableName, self::whereJsonPathMatches('$1'));
    }

    /**
     * Query to delete documents
     *
     * @param string $tableName The name of the table from which documents should be deleted
     * @param string $where The criteria by which documents should be deleted
     * @return string A `DELETE` statement to delete documents in the specified table
     */
    private static function deleteQuery(string $tableName, string $where): string
    {
        return "DELETE FROM $tableName WHERE $where";
    }

    /**
     * Query to delete a document by its ID
     * 
     * @param string $tableName The name of the table from which a document should be deleted
     * @return string A `DELETE` statement to delete a document by its ID
     */
    public static function deleteById(string $tableName): string
    {
        return self::deleteQuery($tableName, self::whereById('$1'));
    }

    /**
     * Query to delete documents using a JSON containment query `@>`
     * 
     * @param string $tableName The name of the table from which documents should be deleted
     * @return string A `DELETE` statement to delete documents by JSON containment
     */
    public static function deleteByContains(string $tableName): string
    {
        return self::deleteQuery($tableName, self::whereDataContains('$1'));
    }

    /**
     * Query to delete documents using a JSON Path match `@?`
     * 
     * @param string $tableName The name of the table from which documents should be deleted
     * @return string A `DELETE` statement to delete documents by JSON Path match
     */
    public static function deleteByJsonPath(string $tableName): string
    {
        return self::deleteQuery($tableName, self::whereJsonPathMatches('$1'));
    }
}
