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
