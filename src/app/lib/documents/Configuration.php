<?php
declare(strict_types=1);

namespace BitBadger\PgDocuments;

use PgSql\Connection;

/**
 * Document table configuration
 */
class Configuration
{
    /** @var string $connectionString The connection string to use when establishing a database connection */
    public static string $connectionString = "";

    /** @var Connection $pgConn The active connection */
    private static ?Connection $pgConn = null;

    /** @var ?string $startUp The name of a function to run on first connection to the database */
    public static ?string $startUp = null;

    /**
     * Ensure that the connection string is set, either explicity, by environment variables, or with defaults
     */
    private static function ensureConnectionString()
    {
        if (self::$connectionString == "") {
            $host = $_ENV['PGDOC_HOST'] ?? 'localhost';
            $port = $_ENV['PGDOC_PORT'] ?? 5432;
            $db   = $_ENV['PGDOC_DB']   ?? 'postgres';
            $user = $_ENV['PGDOC_USER'] ?? 'postgres';
            $pass = $_ENV['PGDOC_PASS'] ?? 'postgres';
            $opts = $_ENV['PGDOC_OPTS'] ?? '';
            self::$connectionString = "host=$host port=$port dbname=$db user=$user password=$pass"
                . ($opts ? " options='$opts'" : '');
        }
    }

    /**
     * A no-op function to force this file to be autoloaded if no explicit configuration is required
     */
    public static function init() { }

    /**
     * Get the PostgreSQL connection, connecting on first request
     * 
     * @return Connection The open PostgreSQL connection
     */
    public static function getPgConn(): Connection
    {
        if (is_null(self::$pgConn)) {
            self::ensureConnectionString();
            self::$pgConn = pg_connect(self::$connectionString);
        }
        return self::$pgConn;
    }

    /**
     * Close the PostgreSQL connection if it is open
     */
    public static function closeConn()
    {
        if (!is_null(self::$pgConn)) {
            pg_close(self::$pgConn);
            self::$pgConn = null;
        }
    }
}

require('functions.php');
