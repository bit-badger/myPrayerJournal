'use strict'

const { Pool } = require('pg')

/**
 * SQL to check the existence of a table in the mpj schema
 * @param {string} table The name of the table whose existence should be checked
 */
const tableSql = table => `SELECT 1 FROM pg_tables WHERE schemaname='mpj' AND tablename='${table}'`

/**
 * SQL to determine if an index exists
 * @param {string} table The name of the table which the given index indexes
 * @param {string} index The name of the index
 */
const indexSql = (table, index) =>
`SELECT 1 FROM pg_indexes WHERE schemaname='mpj' AND tablename='${table}' AND indexname='${index}'`

const ddl = [
  {
    name: 'myPrayerJournal Schema',
    check: `SELECT 1 FROM pg_namespace WHERE nspname='mpj'`,
    fix: `
      CREATE SCHEMA mpj;
      COMMENT ON SCHEMA mpj IS 'myPrayerJournal data'`
  },
  {
    name: 'request Table',
    check: tableSql('request'),
    fix: `
      CREATE TABLE mpj.request (
        "requestId" varchar(25) PRIMARY KEY,
        "enteredOn" bigint NOT NULL,
        "userId" varchar(100) NOT NULL);
      COMMENT ON TABLE mpj.request IS 'Requests'`
  },
  {
    name: 'history Table',
    check: tableSql('history'),
    fix: `
      CREATE TABLE mpj.history (
        "requestId" varchar(25) NOT NULL REFERENCES mpj.request,
        "asOf" bigint NOT NULL,
        "status" varchar(25),
        "text" text,
        PRIMARY KEY ("requestId", "asOf"));
      COMMENT ON TABLE mpj.history IS 'Request update history'`
  },
  {
    name: 'request.userId Index',
    check: indexSql('request', 'idx_request_userId'),
    fix: `
      CREATE INDEX "idx_request_userId" ON mpj.request ("userId");
      COMMENT ON INDEX "idx_request_userId" IS 'Requests are retrieved by user'`
  }
]

module.exports = query => {
  return {
    /**
     * Ensure that the database schema, tables, and indexes exist
     */
    ensureDatabase: async () => {
      for (let item of ddl) {
        const result = await query(item.check, [])
        if (1 > result.rowCount) await query(item.fix, [])
      }
    }
  }
}
