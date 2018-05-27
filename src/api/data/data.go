// Package data contains data access functions for myPrayerJournal.
package data

import (
	"database/sql"
	"fmt"
	"log"
	"time"

	// Register the PostgreSQL driver.
	_ "github.com/lib/pq"
	"github.com/lucsky/cuid"
)

const (
	currentRequestSQL = `
		SELECT "requestId", "text", "asOf", "lastStatus"
		  FROM mpj.journal`
	journalSQL = `
		SELECT "requestId", "text", "asOf", "lastStatus"
		  FROM mpj.journal
		 WHERE "userId" = $1
		   AND "lastStatus" <> 'Answered'`
)

// db is a connection to the database for the entire application.
var db *sql.DB

// Settings holds the PostgreSQL configuration for myPrayerJournal.
type Settings struct {
	Host     string `json:"host"`
	Port     int    `json:"port"`
	User     string `json:"user"`
	Password string `json:"password"`
	DbName   string `json:"dbname"`
}

/* Data Access */

// Retrieve a basic request
func retrieveRequest(reqID, userID string) (*Request, bool) {
	req := Request{}
	err := db.QueryRow(`
		SELECT "requestId", "enteredOn"
		  FROM mpj.request
		 WHERE "requestId" = $1
		   AND "userId" = $2`, reqID, userID).Scan(
		&req.ID, &req.EnteredOn,
	)
	if err != nil {
		if err != sql.ErrNoRows {
			log.Print(err)
		}
		return nil, false
	}
	req.UserID = userID
	return &req, true
}

// Unix time in JavaScript Date.now() precision.
func jsNow() int64 {
	return time.Now().UnixNano() / int64(1000000)
}

// Loop through rows and create journal requests from them.
func makeJournal(rows *sql.Rows, userID string) []JournalRequest {
	var out []JournalRequest
	for rows.Next() {
		req := JournalRequest{}
		err := rows.Scan(&req.RequestID, &req.Text, &req.AsOf, &req.LastStatus)
		if err != nil {
			log.Print(err)
			continue
		}
		out = append(out, req)
	}
	if rows.Err() != nil {
		log.Print(rows.Err())
		return nil
	}
	return out
}

// AddHistory creates a history entry for a prayer request, given the status and updated text.
func AddHistory(userID, reqID, status, text string) int {
	if _, ok := retrieveRequest(reqID, userID); !ok {
		return 404
	}
	_, err := db.Exec(`
        INSERT INTO mpj.history
        	("requestId", "asOf", "status", "text")
        VALUES
			($1, $2, $3, NULLIF($4, ''))`,
		reqID, jsNow(), status, text)
	if err != nil {
		log.Print(err)
		return 500
	}
	return 204
}

// AddNew stores a new prayer request and its initial history record.
func AddNew(userID, text string) (*JournalRequest, bool) {
	id := cuid.New()
	now := jsNow()
	tx, err := db.Begin()
	if err != nil {
		log.Print(err)
		return nil, false
	}
	defer func() {
		if err != nil {
			log.Print(err)
			tx.Rollback()
		} else {
			tx.Commit()
		}
	}()
	_, err = tx.Exec(
		`INSERT INTO mpj.request ("requestId", "enteredOn", "userId") VALUES ($1, $2, $3)`,
		id, now, userID)
	if err != nil {
		return nil, false
	}
	_, err = tx.Exec(
		`INSERT INTO mpj.history ("requestId", "asOf", "status", "text") VALUES ($1, $2, 'Created', $3)`,
		id, now, text)
	if err != nil {
		return nil, false
	}
	return &JournalRequest{RequestID: id, Text: text, AsOf: now, LastStatus: `Created`}, true
}

// AddNote adds a note to a prayer request.
func AddNote(userID, reqID, note string) int {
	if _, ok := retrieveRequest(reqID, userID); !ok {
		return 404
	}
	_, err := db.Exec(`
		INSERT INTO mpj.note
			("requestId", "asOf", "notes")
		VALUES
			($1, $2, $3)`,
		reqID, jsNow(), note)
	if err != nil {
		log.Print(err)
		return 500
	}
	return 204
}

// Answered retrieves all answered requests for the given user.
func Answered(userID string) []JournalRequest {
	rows, err := db.Query(currentRequestSQL+
		` WHERE "userId" = $1
		   AND "lastStatus" = 'Answered'
		 ORDER BY "asOf" DESC`,
		userID)
	if err != nil {
		log.Print(err)
		return nil
	}
	defer rows.Close()
	return makeJournal(rows, userID)
}

// ByID retrieves a journal request by its ID.
func ByID(userID, reqID string) (*JournalRequest, bool) {
	req := JournalRequest{}
	err := db.QueryRow(currentRequestSQL+
		` WHERE "requestId" = $1
		   AND "userId" = $2`,
		reqID, userID).Scan(
		&req.RequestID, &req.Text, &req.AsOf, &req.LastStatus,
	)
	if err != nil {
		if err == sql.ErrNoRows {
			return nil, true
		}
		log.Print(err)
		return nil, false
	}
	return &req, true
}

// Connect establishes a connection to the database.
func Connect(s *Settings) bool {
	connStr := fmt.Sprintf("host=%s port=%d user=%s password=%s dbname=%s sslmode=disable",
		s.Host, s.Port, s.User, s.Password, s.DbName)
	var err error
	db, err = sql.Open("postgres", connStr)
	if err != nil {
		log.Print(err)
		return false
	}
	err = db.Ping()
	if err != nil {
		log.Print(err)
		return false
	}
	log.Printf("Connected to postgres://%s@%s:%d/%s\n", s.User, s.Host, s.Port, s.DbName)
	return true
}

// FullByID retrieves a journal request, including its full history and notes.
func FullByID(userID, reqID string) (*JournalRequest, bool) {
	req, ok := ByID(userID, reqID)
	if !ok {
		return nil, false
	}
	hRows, err := db.Query(`
        SELECT "asOf", "status", COALESCE("text", '') AS "text"
          FROM mpj.history
         WHERE "requestId" = $1
		 ORDER BY "asOf"`,
		reqID)
	if err != nil {
		log.Print(err)
		return nil, false
	}
	defer hRows.Close()
	for hRows.Next() {
		hist := History{}
		err = hRows.Scan(&hist.AsOf, &hist.Status, &hist.Text)
		if err != nil {
			log.Print(err)
			continue
		}
		req.History = append(req.History, hist)
	}
	if hRows.Err() != nil {
		log.Print(hRows.Err())
		return nil, false
	}
	req.Notes = NotesByID(userID, reqID)
	return req, true
}

// Journal retrieves the current user's active prayer journal.
func Journal(userID string) []JournalRequest {
	rows, err := db.Query(journalSQL+` ORDER BY "asOf"`, userID)
	if err != nil {
		log.Print(err)
		return nil
	}
	defer rows.Close()
	return makeJournal(rows, userID)
}

// NotesByID retrieves the notes for a given prayer request
func NotesByID(userID, reqID string) []Note {
	if _, ok := retrieveRequest(reqID, userID); !ok {
		return nil
	}
	rows, err := db.Query(`
        SELECT "asOf", "notes"
          FROM mpj.note
         WHERE "requestId" = $1
		 ORDER BY "asOf" DESC`,
		reqID)
	if err != nil {
		log.Print(err)
		return nil
	}
	defer rows.Close()
	var notes []Note
	for rows.Next() {
		note := Note{}
		err = rows.Scan(&note.AsOf, &note.Notes)
		if err != nil {
			log.Print(err)
			continue
		}
		notes = append(notes, note)
	}
	if rows.Err() != nil {
		log.Print(rows.Err())
		return nil
	}
	return notes
}

/* DDL */

// EnsureDB makes sure we have a known state of data structures.
func EnsureDB() {
	tableSQL := func(table string) string {
		return fmt.Sprintf(`SELECT 1 FROM pg_tables WHERE schemaname='mpj' AND tablename='%s'`, table)
	}
	indexSQL := func(table, index string) string {
		return fmt.Sprintf(`SELECT 1 FROM pg_indexes WHERE schemaname='mpj' AND tablename='%s' AND indexname='%s'`,
			table, index)
	}
	check := func(name, test, fix string) {
		count := 0
		err := db.QueryRow(test).Scan(&count)
		if err != nil {
			if err == sql.ErrNoRows {
				log.Printf("Fixing up %s...\n", name)
				_, err = db.Exec(fix)
				if err != nil {
					log.Fatal(err)
				}
			} else {
				log.Fatal(err)
			}
		}
	}
	check(`myPrayerJournal Schema`, `SELECT 1 FROM pg_namespace WHERE nspname='mpj'`,
		`CREATE SCHEMA mpj;
		COMMENT ON SCHEMA mpj IS 'myPrayerJournal data'`)
	if _, err := db.Exec(`SET search_path TO mpj`); err != nil {
		log.Fatal(err)
	}
	check(`request Table`, tableSQL(`request`),
		`CREATE TABLE mpj.request (
			"requestId" varchar(25) PRIMARY KEY,
			"enteredOn" bigint NOT NULL,
			"userId" varchar(100) NOT NULL);
		COMMENT ON TABLE mpj.request IS 'Requests'`)
	check(`history Table`, tableSQL(`history`),
		`CREATE TABLE mpj.history (
			"requestId" varchar(25) NOT NULL REFERENCES mpj.request,
			"asOf" bigint NOT NULL,
			"status" varchar(25),
			"text" text,
			PRIMARY KEY ("requestId", "asOf"));
		COMMENT ON TABLE mpj.history IS 'Request update history'`)
	check(`note Table`, tableSQL(`note`),
		`CREATE TABLE mpj.note (
			"requestId" varchar(25) NOT NULL REFERENCES mpj.request,
			"asOf" bigint NOT NULL,
			"notes" text NOT NULL,
			PRIMARY KEY ("requestId", "asOf"));
		COMMENT ON TABLE mpj.note IS 'Notes regarding a request'`)
	check(`request.userId Index`, indexSQL(`request`, `idx_request_userId`),
		`CREATE INDEX "idx_request_userId" ON mpj.request ("userId");
		COMMENT ON INDEX "idx_request_userId" IS 'Requests are retrieved by user'`)
	check(`journal View`, `SELECT 1 FROM pg_views WHERE schemaname='mpj' AND viewname='journal'`,
		`CREATE VIEW mpj.journal AS
			SELECT
				request."requestId",
				request."userId",
				(SELECT "text"
				   FROM mpj.history
				  WHERE history."requestId" = request."requestId"
					AND "text" IS NOT NULL
				  ORDER BY "asOf" DESC
				  LIMIT 1) AS "text",
				(SELECT "asOf"
				   FROM mpj.history
				  WHERE history."requestId" = request."requestId"
				  ORDER BY "asOf" DESC
				  LIMIT 1) AS "asOf",
				(SELECT "status"
				   FROM mpj.history
				  WHERE history."requestId" = request."requestId"
				  ORDER BY "asOf" DESC
				  LIMIT 1) AS "lastStatus"
			  FROM mpj.request;
		COMMENT ON VIEW mpj.journal IS 'Requests with latest text'`)
}
