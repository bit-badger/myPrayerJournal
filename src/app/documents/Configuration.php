<?php
declare(strict_types=1);

namespace BitBadger\PgSQL\Documents;

/**
 * Document table configuration
 */
class Configuration
{
    /** @var string $connectionString The connection string to use when establishing a database connection */
    public static string $connectionString = "";

    /** @var ?\PDO $conn The active connection */
    private static ?\PDO $conn = null;

    /**
     * Get the database connection, connecting on first request
     * 
     * @return PDO The PDO object representing the connection
     */
    public static function getConn(): \PDO
    {
        if (is_null(self::$conn)) {
            self::$conn = new \PDO(self::$connectionString);
        }
        return self::$conn;
    }
}

require('functions.php');
