// Package routes contains endpoint handlers for the myPrayerJournal API.
package routes

// Route is a route served in the application.
type Route struct {
	Name     string
	Method   string
	Pattern  string
	Func     DBHandler
	IsPublic bool
}

// Routes is the collection of all routes served in the application.
type Routes []Route

// routes is the actual list of routes for the application.
var routes = Routes{
	Route{
		"Journal",
		"GET",
		"/journal",
		journal,
		false,
	},
}
