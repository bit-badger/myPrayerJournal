<?php
declare(strict_types=1);

namespace BitBadger\PgSQL\Documents;

use \PgSql\Connection;

/**
 * Document table configuration
 */
class Configuration
{
    /** @var string $connectionString The connection string to use when establishing a database connection */
    public static string $connectionString = "";

    /** @var ?\PDO $conn The active connection */
    private static ?\PDO $conn = null;

    /** @var ?Connection $rawConn An active non-PDO PostgreSQL connection */
    private static ?Connection $rawConn = null;

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
            self::$connectionString = "pgsql:host=$host;port=$port;dbname=$db;user=$user;pass=$pass";
        }
    }

    /**
     * Get the database connection, connecting on first request
     * 
     * @return PDO The PDO object representing the connection
     */
    public static function getConn(): \PDO
    {
        if (is_null(self::$conn)) {
            self::ensureConnectionString();
            self::$conn = new \PDO(self::$connectionString);

            if (!is_null(self::$startUp)) {
                call_user_func(self::$startUp);
            }
        }
        return self::$conn;
    }

    /**
     * 
     */
    public static function getRawConn(): Connection
    {
        if (is_null(self::$rawConn)) {
            self::ensureConnectionString();
            self::$rawConn = pg_connect(str_replace(';', ' ', self::$connectionString));

            if (!is_null(self::$startUp)) {
                call_user_func(self::$startUp);
            }
        }
        return self::$rawConn;
    }
}

require('functions.php');
