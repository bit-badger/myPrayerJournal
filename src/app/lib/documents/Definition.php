<?php
declare(strict_types=1);

namespace BitBadger\PgDocuments;

use PgSql\Result;

/**
 * Methods to define tables and indexes for document tables
 */
class Definition
{
    /**
     * Create a statement to create a document table
     * 
     * @param string $name The name of the table to create
     * @return string A `CREATE TABLE` statement for the document table
     */
    public static function createTable(string $name): string
    {
        return "CREATE TABLE IF NOT EXISTS $name (data JSONB NOT NULL)";
    }

    /**
     * Create a statement to create a key for a document table
     *
     * @param string $tableName The table (or schema/table) for which a key should be created
     * @return string A `CREATE INDEX` statement for a unique key for the document table
     */
    public static function createKey(string $tableName): string
    {
        return sprintf('CREATE UNIQUE INDEX IF NOT EXISTS idx_%s_key ON %s ((data -> \'%s\'))',
            self::extractTable($tableName), $tableName, Configuration::$keyName);
    }

    /**
     * Create a statement to create an index on a document table
     * 
     * @param string $name The name of the table for which the index should be created
     * @param DocumentIndex $type The type of index to create
     * @return string A `CREATE INDEX` statement for the given table
     */
    public static function createIndex(string $name, DocumentIndex $type): string
    {
        return sprintf('CREATE INDEX IF NOT EXISTS idx_%s ON %s USING GIN (data%s)',
            self::extractTable($name), $name, $type == DocumentIndex::Full ? '' : ' jsonb_path_ops');
    }

    /**
     * Ensure the given document table exists
     * 
     * @param string $tableName The name of the table
     */
    public static function ensureTable(string $tableName): void
    {
        /** @var Result|bool $result */
        $result = pg_query(pg_conn(), self::createTable($tableName));
        if ($result) pg_free_result($result);
        $result = pg_query(pg_conn(), self::createKey($tableName));
        if ($result) pg_free_result($result);
    }

    /**
     * Ensure an index on the given document table exists
     * 
     * @param string $name The name of the table for which the index should be created
     * @param DocumentIndex $type The type of index to create
     */
    public static function ensureIndex(string $name, DocumentIndex $type): void
    {
        /** @var Result|bool $result */
        $result = pg_query(pg_conn(), self::createIndex($name, $type));
        if ($result) pg_free_result($result);
    }

    /**
     * Extract just the table name from a possible `schema.table` name
     *
     * @param string $name The name of the table, possibly including the schema
     * @return string The table name
     */
    private static function extractTable(string $name): string
    {
        $schemaAndTable = explode('.', $name);
        return end($schemaAndTable);
    }
}
