// Package routes contains endpoint handlers for the myPrayerJournal API.
package routes

import (
	"database/sql"
	"encoding/json"
	"log"
	"net/http"

	"github.com/danieljsummers/myPrayerJournal/src/api/data"
	"github.com/julienschmidt/httprouter"
)

/* Support */

// Set the content type, the HTTP error code, and return the error message.
func sendError(w http.ResponseWriter, r *http.Request, err error) {
	m := map[string]string{"error": err.Error()}
	j, jErr := json.Marshal(m)
	if jErr != nil {
		log.Print("Error creating error JSON: " + jErr.Error())
	}
	w.WriteHeader(500)
	w.Header().Set("Content-Type", "application/json")
	w.Write(j)
}

// Set the content type and return the JSON to the user.
func sendJSON(w http.ResponseWriter, r *http.Request, result interface{}) {
	payload, err := json.Marshal(result)
	if err != nil {
		sendError(w, r, err)
		return
	}
	w.Header().Set("Content-Type", "application/json")
	w.Write([]byte("{ data: "))
	w.Write(payload)
	w.Write([]byte(" }"))
}

/* Handlers */

func journal(w http.ResponseWriter, r *http.Request, _ httprouter.Params, db *sql.DB) {
	reqs := data.Journal(db, "TODO: get user ID")
	if reqs == nil {
		reqs = []data.JournalRequest{}
	}
	sendJSON(w, r, reqs)
}

/* Wrappers */

func withDB(fn func(w http.ResponseWriter, r *http.Request, p httprouter.Params, db *sql.DB), db *sql.DB) httprouter.Handle {
	return func(w http.ResponseWriter, r *http.Request, p httprouter.Params) {
		fn(w, r, p, db)
	}
}

// Routes returns a configured router to handle all incoming requests.
func Routes(db *sql.DB) *httprouter.Router {
	router := httprouter.New()
	router.GET("/journal", withDB(journal, db))
	return router
}
