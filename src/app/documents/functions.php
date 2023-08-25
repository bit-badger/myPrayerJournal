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
if (!function_exists('pgconn')) {
    /**
     * Return the active PostgreSQL connection
     * 
     * @return \PgSql\Connection The open PostgreSQL connection
     */
    function pgconn()
    {
        return Configuration::getRawConn();
    }
}
