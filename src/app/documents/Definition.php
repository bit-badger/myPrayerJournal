<?php
declare(strict_types=1);

namespace BitBadger\PgSQL\Documents;

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
        return "CREATE TABLE IF NOT EXISTS $name (id TEXT NOT NULL PRIMARY KEY, data JSONB NOT NULL)";
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
        $extraOps       = $type == DocumentIndex::Full ? '' : ' jsonb_path_ops';
        $schemaAndTable = explode('.', $name);
        $tableName      = end($schemaAndTable);
        return "CREATE INDEX IF NOT EXISTS idx_$tableName ON $name USING GIN (data$extraOps)";
    }

    /**
     * Ensure the given document table exists
     * 
     * @param string $name The name of the table
     */
    public static function ensureTable(string $name)
    {
        pdo()->query(self::createTable($name))->execute();
    }

    /**
     * Ensure an index on the given document table exists
     * 
     * @param string $name The name of the table for which the index should be created
     * @param DocumentIndex $type The type of index to create
     */
    public static function ensureIndex(string $name, DocumentIndex $type)
    {
        pdo()->query(self::createIndex($name, $type))->execute();
    }
}
