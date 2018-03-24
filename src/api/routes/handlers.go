package routes

import (
	"encoding/json"
	"log"
	"net/http"
	"strings"

	"github.com/danieljsummers/myPrayerJournal/src/api/data"
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

func journal(w http.ResponseWriter, r *http.Request) {
	user := r.Context().Value(ContextUserKey)
	reqs := data.Journal(user.(string))
	if reqs == nil {
		reqs = []data.JournalRequest{}
	}
	sendJSON(w, r, reqs)
}

func staticFiles(w http.ResponseWriter, r *http.Request) {
	// serve index for known routes handled client-side by the app
	for _, prefix := range ClientPrefixes {
		log.Print("Checking " + r.URL.Path)
		if strings.HasPrefix(r.URL.Path, prefix) {
			w.Header().Add("Content-Type", "text/html")
			http.ServeFile(w, r, "./public/index.html")
			return
		}
	}
	// 404 here is fine; quit hacking, y'all...
	http.ServeFile(w, r, "./public"+r.URL.Path)
}
