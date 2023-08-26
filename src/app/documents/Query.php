<?php
declare(strict_types=1);

namespace BitBadger\PgSQL\Documents;

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
     * Create a `WHERE` clause fragment to implement a @> (JSON contains) condition
     * 
     * @param string $paramName The name of the parameter for the contains clause
     * @return string A `WHERE` clause fragment with the named parameter
     */
    public static function whereDataContains(string $paramName): string
    {
        return "data @> $paramName::jsonb";
    }
    
    /**
     * Create a `WHERE` clause fragment to implement a @? (JSON Path match) condition
     * 
     * @param string $paramName THe name of the parameter for the JSON Path match
     * @return string A `WHERE` clause fragment with the named parameter
     */
    public static function whereJsonPathMatches(string $paramName): string
    {
        return "data @?? {$paramName}::jsonpath";
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

    /// Create ID and data parameters for a query
    /* let docParameters<'T> docId (doc : 'T) =
        [ "@id", Sql.string docId; "@data", jsonbDocParam doc ]
    */
    /**
     * Query to insert a document
     * 
     * @param string $tableName The name of the table into which the document will be inserted
     * @return string The `INSERT` statement (with `@id` and `@data` parameters defined)
     */
    public static function insert(string $tableName): string
    {
        return "INSERT INTO $tableName (id, data) VALUES (:id, :data)";
    }

    /**
     * Query to save a document, inserting it if it does not exist and updating it if it does (AKA "upsert")
     * 
     * @param string $tableName The name of the table into which the document will be saved
     * @return string The `INSERT`/`ON CONFLICT DO UPDATE` statement (with `@id` and `@data` parameters defined)
     */
    public static function save(string $tableName): string
    {
        return "INSERT INTO $tableName (id, data) VALUES (:id, :data)
                  ON CONFLICT (id) DO UPDATE SET data = EXCLUDED.data";
    }
    
    /**
     * Query to count all documents in a table
     * 
     * @param string $tableName The name of the table whose rows will be counted
     * @return string A `SELECT` statement to obtain the count of all documents in the given table
     */
    public static function countAll(string $tableName): string
    {
        return "SELECT COUNT(*) AS it FROM $tableName";
    }
    
    /**
     * Query to count matching documents using a JSON containment query `@>`
     * 
     * @param string $tableName The name of the table from which the count should be obtained
     * @return string A `SELECT` statement to obtain the count of documents via JSON containment
     */
    public static function countByContains(string $tableName): string
    {
        return sprintf("SELECT COUNT(*) AS it FROM $tableName WHERE %s", self::whereDataContains(':criteria'));
    }
    
    /**
     * Query to count matching documents using a JSON Path match `@?`
     * 
     * @param string $tableName The name of the table from which the count should be obtained
     * @return string A `SELECT` statement to obtain the count of documents via JSON Path match
     */
    public static function countByJsonPath(string $tableName): string
    {
        return sprintf("SELECT COUNT(*) AS it FROM $tableName WHERE %s", self::whereJsonPathMatches(':path'));
    }
    
    /**
     * Query to determine if a document exists for the given ID
     * 
     * @param string $tableName The name of the table in which existence should be checked
     * @return string A `SELECT` statement to check existence of a document by its ID
     */
    public static function existsById(string $tableName): string
    {
        return "SELECT EXISTS (SELECT 1 FROM $tableName WHERE id = :id) AS it";
    }

    /**
     * Query to determine if documents exist using a JSON containment query `@>`
     * 
     * @param string $tableName The name of the table in which existence should be checked
     * @return string A `SELECT` statement to check existence of a document by JSON containment
     */
    public static function existsByContains(string $tableName): string
    {
        return sprintf("SELECT EXISTS (SELECT 1 FROM $tableName WHERE %s AS it", self::whereDataContains(':criteria'));
    }
    
    /**
     * Query to determine if documents exist using a JSON Path match `@?`
     * 
     * @param string $tableName The name of the table in which existence should be checked
     * @return string A `SELECT` statement to check existence of a document by JSON Path match
     */
    public static function existsByJsonPath(string $tableName): string
    {
        return sprintf("SELECT EXISTS (SELECT 1 FROM $tableName WHERE %s AS it", self::whereJsonPathMatches(':path'));
    }
    
    /**
     * Query to retrieve a document by its ID
     * 
     * @param string $tableName The name of the table from which a document should be retrieved
     * @return string A `SELECT` statement to retrieve a document by its ID
     */
    public static function findById(string $tableName): string
    {
        return sprintf('%s WHERE id = :id', self::selectFromTable($tableName));
    }
    
    /**
     * Query to retrieve documents using a JSON containment query `@>`
     * 
     * @param string $tableName The name of the table from which a document should be retrieved
     * @return string A `SELECT` statement to retrieve documents by JSON containment
     */
    public static function findByContains(string $tableName): string
    {
        return sprintf('%s WHERE %s', self::selectFromTable($tableName), self::whereDataContains(':criteria'));
    }
    
    /**
     * Query to retrieve documents using a JSON Path match `@?`
     * 
     * @param string $tableName The name of the table from which a document should be retrieved
     * @return string A `SELECT` statement to retrieve a documents by JSON Path match
     */
    public static function findByJsonPath(string $tableName): string
    {
        return sprintf('%s WHERE %s', self::selectFromTable($tableName), self::whereJsonPathMatches(':path'));
    }
    
    /**
     * Query to update a document, replacing the existing document
     * 
     * @param string $tableName The name of the table in which a document should be updated
     * @return string An `UPDATE` statement to update a document by its ID
     */
    public static function updateFull(string $tableName): string
    {
        return "UPDATE $tableName SET data = :data WHERE id = :id";
    }

    /**
     * Query to update a document, merging the existing document with the one provided
     * 
     * @param string $tableName The name of the table in which a document should be updated
     * @return string An `UPDATE` statement to update a document by its ID
     */
    public static function updatePartialById(string $tableName): string
    {
        return "UPDATE $tableName SET data = data || :data WHERE id = :id";
    }
    
    /**
     * Query to update partial documents matching a JSON containment query `@>`
     * 
     * @param string $tableName The name of the table in which documents should be updated
     * @return string An `UPDATE` statement to update documents by JSON containment
     */
    public static function updatePartialByContains(string $tableName): string
    {
        return sprintf("UPDATE $tableName SET data = data || :data WHERE %s", self::whereDataContains(':criteria'));
    }

    /**
     * Query to update partial documents matching a JSON containment query `@>`
     * 
     * @param string $tableName The name of the table in which documents should be updated
     * @return string An `UPDATE` statement to update  documents by JSON Path match
     */
    public static function updatePartialByJsonPath(string $tableName): string
    {
        return sprintf("UPDATE $tableName SET data = data || :data WHERE %s", self::whereJsonPathMatches(':path'));
    }

    /**
     * Query to delete a document by its ID
     * 
     * @param string $tableName The name of the table from which a document should be deleted
     * @return string A `DELETE` statement to delete a document by its ID
     */
    public static function deleteById(string $tableName): string
    {
        return "DELETE FROM $tableName WHERE id = :id";
    }

    /**
     * Query to delete documents using a JSON containment query `@>`
     * 
     * @param string $tableName The name of the table from which documents should be deleted
     * @return string A `DELETE` statement to delete documents by JSON containment
     */
    public static function deleteByContains(string $tableName): string
    {
        return sprintf("DELETE FROM $tableName WHERE %s", self::whereDataContains(':criteria'));
    }

    /**
     * Query to delete documents using a JSON Path match `@?`
     * 
     * @param string $tableName The name of the table from which documents should be deleted
     * @return string A `DELETE` statement to delete documents by JSON Path match
     */
    public static function deleteByJsonPath(string $tableName): string
    {
        return sprintf("DELETE FROM $tableName WHERE %s", self::whereJsonPathMatches(':path'));
    }
}
