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
        if (is_null(Configuration::$conn)) {
            Configuration::$conn = new \PDO(Configuration::$connectionString);
        }
        return Configuration::$conn;
    }
}

require('functions.php');
