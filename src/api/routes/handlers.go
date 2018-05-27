package routes

import (
	"encoding/json"
	"errors"
	"log"
	"net/http"
	"strings"

	"github.com/danieljsummers/myPrayerJournal/src/api/data"
	jwt "github.com/dgrijalva/jwt-go"
	routing "github.com/go-ozzo/ozzo-routing"
)

/* Support */

// Set the content type, the HTTP error code, and return the error message.
func sendError(c *routing.Context, err error) error {
	w := c.Response
	w.Header().Set("Content-Type", "application/json; encoding=UTF-8")
	w.WriteHeader(http.StatusInternalServerError)
	if err := json.NewEncoder(w).Encode(map[string]string{"error": err.Error()}); err != nil {
		log.Print("Error creating error JSON: " + err.Error())
	}
	return err
}

// Set the content type and return the JSON to the user.
func sendJSON(c *routing.Context, result interface{}) error {
	w := c.Response
	w.Header().Set("Content-Type", "application/json; encoding=UTF-8")
	w.WriteHeader(http.StatusOK)
	if err := json.NewEncoder(w).Encode(result); err != nil {
		return sendError(c, err)
	}
	return nil
}

// Parse the request body as JSON.
func parseJSON(c *routing.Context) (map[string]interface{}, error) {
	payload := make(map[string]interface{})
	if err := json.NewDecoder(c.Request.Body).Decode(&payload); err != nil {
		log.Println("Error decoding JSON:", err)
		return payload, err
	}
	return payload, nil
}

// userID is a convenience function to extract the subscriber ID from the user's JWT.
// NOTE: Do not call this from public routes; there are a lot of type assertions that won't be true if the request
//       hasn't gone through the authorization process.
func userID(c *routing.Context) string {
	return c.Request.Context().Value("user").(*jwt.Token).Claims.(jwt.MapClaims)["sub"].(string)
}

/* Handlers */

// GET: /api/journal/
func journal(c *routing.Context) error {
	reqs := data.Journal(userID(c))
	if reqs == nil {
		reqs = []data.JournalRequest{}
	}
	return sendJSON(c, reqs)
}

// POST: /api/request/
func requestAdd(c *routing.Context) error {
	payload, err := parseJSON(c)
	if err != nil {
		return sendError(c, err)
	}
	result, ok := data.AddNew(userID(c), payload["requestText"].(string))
	if !ok {
		return sendError(c, errors.New("error adding request"))
	}
	return sendJSON(c, result)
}

// GET: /api/request/<id>
func requestGet(c *routing.Context) error {
	request, ok := data.ByID(userID(c), c.Param("id"))
	if !ok {
		return sendError(c, errors.New("error retrieving request"))
	}
	return sendJSON(c, request)
}

// GET: /api/request/<id>/complete
func requestGetComplete(c *routing.Context) error {
	request, ok := data.FullByID(userID(c), c.Param("id"))
	if !ok {
		return sendError(c, errors.New("error retrieving request"))
	}
	request.Notes = data.NotesByID(userID(c), c.Param("id"))
	return sendJSON(c, request)
}

// GET: /api/request/<id>/full
func requestGetFull(c *routing.Context) error {
	request, ok := data.FullByID(userID(c), c.Param("id"))
	if !ok {
		return sendError(c, errors.New("error retrieving request"))
	}
	return sendJSON(c, request)
}

// POST: /api/request/<id>/history
func requestAddHistory(c *routing.Context) error {
	payload, err := parseJSON(c)
	if err != nil {
		return sendError(c, err)
	}
	c.Response.WriteHeader(
		data.AddHistory(userID(c), c.Param("id"), payload["status"].(string), payload["updateText"].(string)))
	return nil
}

// POST: /api/request/<id>/note
func requestAddNote(c *routing.Context) error {
	payload, err := parseJSON(c)
	if err != nil {
		return sendError(c, err)
	}
	c.Response.WriteHeader(data.AddNote(userID(c), c.Param("id"), payload["notes"].(string)))
	return nil
}

// GET: /api/request/<id>/notes
func requestGetNotes(c *routing.Context) error {
	notes := data.NotesByID(userID(c), c.Param("id"))
	if notes == nil {
		c.Response.WriteHeader(http.StatusNotFound)
		return errors.New("Not Found")
	}
	return sendJSON(c, notes)
}

// GET: /api/request/answered
func requestsAnswered(c *routing.Context) error {
	reqs := data.Answered(userID(c))
	if reqs == nil {
		reqs = []data.JournalRequest{}
	}
	return sendJSON(c, reqs)
}

// GET: /*
func staticFiles(c *routing.Context) error {
	// serve index for known routes handled client-side by the app
	r := c.Request
	w := c.Response
	for _, prefix := range ClientPrefixes {
		if strings.HasPrefix(r.URL.Path, prefix) {
			w.Header().Add("Content-Type", "text/html")
			http.ServeFile(w, r, "./public/index.html")
			return nil
		}
	}
	// 404 here is fine; quit hacking, y'all...
	http.ServeFile(w, r, "./public"+r.URL.Path)
	return nil
}
