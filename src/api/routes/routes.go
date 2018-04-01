// Package routes contains endpoint handlers for the myPrayerJournal API.
package routes

import (
	"net/http"
)

// Route is a route served in the application.
type Route struct {
	Name     string
	Method   string
	Pattern  string
	Func     http.HandlerFunc
	IsPublic bool
}

// Routes is the collection of all routes served in the application.
type Routes []Route

// routes is the actual list of routes for the application.
var routes = Routes{
	Route{
		"Journal",
		http.MethodGet,
		"/api/journal/",
		journal,
		false,
	},
	Route{
		"AddNewRequest",
		http.MethodPost,
		"/api/request/",
		requestAdd,
		false,
	},
	Route{
		"GetRequestByID",
		http.MethodGet,
		"/api/request/:id",
		requestGet,
		false,
	},
	Route{
		"GetCompleteRequestByID",
		http.MethodGet,
		"/api/request/:id/complete",
		requestGetComplete,
		false,
	},
	Route{
		"GetFullRequestByID",
		http.MethodGet,
		"/api/request/:id/full",
		requestGetFull,
		false,
	},
	Route{
		"AddNewHistoryEntry",
		http.MethodPost,
		"/api/request/:id/history",
		requestAddHistory,
		false,
	},
	Route{
		"AddNewNote",
		http.MethodPost,
		"/api/request/:id/note",
		requestAddNote,
		false,
	},
	Route{
		"GetNotesForRequest",
		http.MethodGet,
		"/api/request/:id/notes",
		requestGetNotes,
		false,
	},
	Route{
		"GetAnsweredRequests",
		http.MethodGet,
		"/api/request/answered",
		requestsAnswered,
		false,
	},
	// keep this route last
	Route{
		"StaticFiles",
		http.MethodGet,
		"/*",
		staticFiles,
		true,
	},
}

// ClientPrefixes is a list of known route prefixes handled by the Vue app.
var ClientPrefixes = []string{"/answered", "/journal", "/user"}
