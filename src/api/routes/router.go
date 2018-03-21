package routes

import (
	"context"
	"database/sql"
	"fmt"
	"net/http"
	"time"

	"github.com/auth0-community/go-auth0"
	"github.com/julienschmidt/httprouter"
	"gopkg.in/square/go-jose.v2"
)

// AuthConfig contains the Auth0 configuration passed from the "auth" JSON object.
type AuthConfig struct {
	Domain       string `json:"domain"`
	ClientID     string `json:"id"`
	ClientSecret string `json:"secret"`
}

// DBHandler extends httprouter's handler with a DB instance.
type DBHandler func(http.ResponseWriter, *http.Request, httprouter.Params, *sql.DB)

//type APIHandler func(http.ResponseWriter, *http.Request, httprouter.Params, *sql.DB, string)

// ContextKey is the type of key used in our contexts.
type ContextKey string

// ContextUserKey is the key for the current user in the context.
const ContextUserKey ContextKey = "user"

func withDB(next DBHandler, db *sql.DB) httprouter.Handle {
	return func(w http.ResponseWriter, r *http.Request, p httprouter.Params) {
		ctx, cancel := context.WithTimeout(context.Background(), time.Duration(60*time.Second))
		defer cancel()
		next(w, r.WithContext(ctx), p, db)
	}
}

func withAuth(next DBHandler, cfg *AuthConfig) DBHandler {
	return func(w http.ResponseWriter, r *http.Request, p httprouter.Params, db *sql.DB) {
		secret := []byte(cfg.ClientSecret)
		secretProvider := auth0.NewKeyProvider(secret)
		audience := []string{fmt.Sprintf("https://%s/userinfo", cfg.Domain)}

		configuration := auth0.NewConfiguration(secretProvider, audience, fmt.Sprintf("https://%s/", cfg.Domain), jose.HS256)
		validator := auth0.NewValidator(configuration, nil)

		token, err := validator.ValidateRequest(r)

		if err != nil {
			fmt.Println(err)
			fmt.Println("Token is not valid:", token)
			w.WriteHeader(http.StatusUnauthorized)
			w.Write([]byte("Unauthorized"))
		} else {
			values := make(map[string]interface{})
			if err := token.Claims(secret, &values); err != nil {
				sendError(w, r, err)
			}
			r = r.WithContext(context.WithValue(r.Context(), ContextUserKey, values["sub"]))
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
	// router.ServeFiles("/*filepath", http.Dir("/public"))
	return router
}
