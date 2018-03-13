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
	w.Header().Set("Content-Type", "application/json; encoding=UTF-8")
	w.WriteHeader(http.StatusInternalServerError)
	if err := json.NewEncoder(w).Encode(map[string]string{"error": err.Error()}); err != nil {
		log.Print("Error creating error JSON: " + err.Error())
	}
}

// Set the content type and return the JSON to the user.
func sendJSON(w http.ResponseWriter, r *http.Request, result interface{}) {
	w.Header().Set("Content-Type", "application/json; encoding=UTF-8")
	w.WriteHeader(http.StatusOK)
	if err := json.NewEncoder(w).Encode(map[string]interface{}{"data": result}); err != nil {
		sendError(w, r, err)
	}
}

/* Handlers */

func journal(w http.ResponseWriter, r *http.Request, _ httprouter.Params, db *sql.DB) {
	user := r.Context().Value(ContextUserKey)
	reqs := data.Journal(db, user.(string))
	if reqs == nil {
		reqs = []data.JournalRequest{}
	}
	sendJSON(w, r, reqs)
}
