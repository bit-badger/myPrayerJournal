<?php

use BitBadger\PgSQL\Documents\Configuration;

if (!function_exists('pdo')) {
    /**
     * Return the active PostgreSQL PDO object
     *
     * @return \PDO The data connection from the configuration
     */
    function pdo()
    {
        return Configuration::getConn();
    }
}
