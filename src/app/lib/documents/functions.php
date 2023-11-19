<?php

use BitBadger\PgDocuments\Configuration;
use PgSql\Connection;

if (!function_exists('pg_conn')) {
    /**
     * Return the active PostgreSQL connection
     * 
     * @return Connection The data connection from the configuration
     */
    function pg_conn(): Connection
    {
        return Configuration::getPgConn();
    }
}
