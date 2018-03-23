package routes

import (
	"context"
	"fmt"
	"net/http"

	"github.com/auth0-community/go-auth0"
	"github.com/husobee/vestigo"
	"gopkg.in/square/go-jose.v2"
)

// AuthConfig contains the Auth0 configuration passed from the "auth" JSON object.
type AuthConfig struct {
	Domain       string `json:"domain"`
	ClientID     string `json:"id"`
	ClientSecret string `json:"secret"`
}

// ContextKey is the type of key used in our contexts.
type ContextKey string

// ContextUserKey is the key for the current user in the context.
const ContextUserKey ContextKey = "user"

func withAuth(next http.HandlerFunc, cfg *AuthConfig) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
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
			next(w, r)
		}
	}

}

// NewRouter returns a configured router to handle all incoming requests.
func NewRouter(cfg *AuthConfig) *vestigo.Router {
	router := vestigo.NewRouter()
	for _, route := range routes {
		if route.IsPublic {
			router.Add(route.Method, route.Pattern, route.Func)
		} else {
			router.Add(route.Method, route.Pattern, withAuth(route.Func, cfg))
		}
	}
	router.Get("/*", http.FileServer(http.Dir("/public")).ServeHTTP)
	return router
}
