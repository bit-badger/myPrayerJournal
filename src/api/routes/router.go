package routes

import (
	"database/sql"
	"fmt"
	"net/http"

	auth0 "github.com/auth0-community/go-auth0"
	"github.com/julienschmidt/httprouter"
	jose "gopkg.in/square/go-jose.v2"
)

// AuthConfig contains the Auth0 configuration passed from the "auth" JSON object.
type AuthConfig struct {
	Domain       string `json:"domain"`
	ClientID     string `json:"id"`
	ClientSecret string `json:"secret"`
}

// DBHandler extends httprouter's handler with a DB instance
type DBHandler func(http.ResponseWriter, *http.Request, httprouter.Params, *sql.DB)

//type APIHandler func(http.ResponseWriter, *http.Request, httprouter.Params, *sql.DB, string)

func withDB(next DBHandler, db *sql.DB) httprouter.Handle {
	return func(w http.ResponseWriter, r *http.Request, p httprouter.Params) {
		next(w, r, p, db)
	}
}

func withAuth(next DBHandler, cfg *AuthConfig) DBHandler {
	return func(w http.ResponseWriter, r *http.Request, p httprouter.Params, db *sql.DB) {
		secret := []byte(cfg.ClientSecret)
		secretProvider := auth0.NewKeyProvider(secret)
		audience := []string{"{YOUR-AUTH0-API-AUDIENCE}"}

		configuration := auth0.NewConfiguration(secretProvider, audience, fmt.Sprintf("https://%s.auth0.com/", cfg.Domain), jose.HS256)
		validator := auth0.NewValidator(configuration)

		token, err := validator.ValidateRequest(r)

		if err != nil {
			fmt.Println(err)
			fmt.Println("Token is not valid:", token)
			w.WriteHeader(http.StatusUnauthorized)
			w.Write([]byte("Unauthorized"))
		} else {
			// TODO pass the user ID (sub) along; this -> doesn't work | r.Header.Add("user-id", token.Claims("sub"))
			next(w, r, p, db)
		}
	}

}

// NewRouter returns a configured router to handle all incoming requests.
func NewRouter(db *sql.DB, cfg *AuthConfig) *httprouter.Router {
	router := httprouter.New()
	for _, route := range routes {
		if route.IsPublic {
			router.Handle(route.Method, route.Pattern, withDB(route.Func, db))
		} else {
			router.Handle(route.Method, route.Pattern, withDB(withAuth(route.Func, cfg), db))
		}
	}
	return router
}
