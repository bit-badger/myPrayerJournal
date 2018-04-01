package routes

import (
	"encoding/json"
	"errors"
	"log"
	"net/http"
	"strings"

	"github.com/danieljsummers/myPrayerJournal/src/api/data"
	jwt "github.com/dgrijalva/jwt-go"
	"github.com/husobee/vestigo"
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
	if err := json.NewEncoder(w).Encode(result); err != nil {
		sendError(w, r, err)
	}
}

// userID is a convenience function to extract the subscriber ID from the user's JWT.
// NOTE: Do not call this from public routes; there are a lot of type assertions that won't be true if the request
//       hasn't gone through the authorization process.
func userID(r *http.Request) string {
	return r.Context().Value("user").(*jwt.Token).Claims.(jwt.MapClaims)["sub"].(string)
}

/* Handlers */

// GET: /api/journal/
func journal(w http.ResponseWriter, r *http.Request) {
	reqs := data.Journal(userID(r))
	if reqs == nil {
		reqs = []data.JournalRequest{}
	}
	sendJSON(w, r, reqs)
}

// POST: /api/request/
func requestAdd(w http.ResponseWriter, r *http.Request) {
	if err := r.ParseForm(); err != nil {
		sendError(w, r, err)
	}
	result, ok := data.AddNew(userID(r), r.FormValue("requestText"))
	if !ok {
		sendError(w, r, errors.New("error adding request"))
	}
	sendJSON(w, r, result)
}

// GET: /api/request/:id
func requestGet(w http.ResponseWriter, r *http.Request) {
	request, ok := data.ByID(userID(r), vestigo.Param(r, "id"))
	if !ok {
		sendError(w, r, errors.New("error retrieving request"))
	}
	sendJSON(w, r, request)
}

// GET: /api/request/:id/complete
func requestGetComplete(w http.ResponseWriter, r *http.Request) {
	request, ok := data.FullByID(userID(r), vestigo.Param(r, "id"))
	if !ok {
		sendError(w, r, errors.New("error retrieving request"))
	}
	request.Notes = data.NotesByID(userID(r), vestigo.Param(r, "id"))
	sendJSON(w, r, request)
}

// GET: /api/request/:id/full
func requestGetFull(w http.ResponseWriter, r *http.Request) {
	request, ok := data.FullByID(userID(r), vestigo.Param(r, "id"))
	if !ok {
		sendError(w, r, errors.New("error retrieving request"))
	}
	sendJSON(w, r, request)
}

// POST: /api/request/:id/history
func requestAddHistory(w http.ResponseWriter, r *http.Request) {
	if err := r.ParseForm(); err != nil {
		sendError(w, r, err)
	}
	w.WriteHeader(data.AddHistory(userID(r), vestigo.Param(r, "id"), r.FormValue("status"), r.FormValue("updateText")))
}

// POST: /api/request/:id/note
func requestAddNote(w http.ResponseWriter, r *http.Request) {
	if err := r.ParseForm(); err != nil {
		sendError(w, r, err)
	}
	w.WriteHeader(data.AddNote(userID(r), vestigo.Param(r, "id"), r.FormValue("notes")))
}

// GET: /api/request/:id/notes
func requestGetNotes(w http.ResponseWriter, r *http.Request) {
	notes := data.NotesByID(userID(r), vestigo.Param(r, "id"))
	if notes == nil {
		w.WriteHeader(http.StatusNotFound)
		return
	}
	sendJSON(w, r, notes)
}

// GET: /api/request/answered
func requestsAnswered(w http.ResponseWriter, r *http.Request) {
	reqs := data.Answered(userID(r))
	if reqs == nil {
		reqs = []data.JournalRequest{}
	}
	sendJSON(w, r, reqs)
}

// GET: /*
func staticFiles(w http.ResponseWriter, r *http.Request) {
	// serve index for known routes handled client-side by the app
	for _, prefix := range ClientPrefixes {
		if strings.HasPrefix(r.URL.Path, prefix) {
			w.Header().Add("Content-Type", "text/html")
			http.ServeFile(w, r, "./public/index.html")
			return
		}
	}
	// 404 here is fine; quit hacking, y'all...
	http.ServeFile(w, r, "./public"+r.URL.Path)
}
