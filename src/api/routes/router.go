package routes

import (
	"encoding/json"
	"errors"
	"fmt"
	"io/ioutil"
	"log"
	"net/http"

	"github.com/auth0/go-jwt-middleware"
	jwt "github.com/dgrijalva/jwt-go"
	"github.com/go-ozzo/ozzo-routing"
	"github.com/go-ozzo/ozzo-routing/access"
	"github.com/go-ozzo/ozzo-routing/fault"
)

// AuthConfig contains the Auth0 configuration passed from the "auth" JSON object.
type AuthConfig struct {
	Domain       string `json:"domain"`
	ClientID     string `json:"id"`
	ClientSecret string `json:"secret"`
}

// JWKS is a structure into which the JSON Web Key Set is unmarshaled.
type JWKS struct {
	Keys []JWK `json:"keys"`
}

// JWK is a structure into which a single JSON Web Key is unmarshaled.
type JWK struct {
	Kty string   `json:"kty"`
	Kid string   `json:"kid"`
	Use string   `json:"use"`
	N   string   `json:"n"`
	E   string   `json:"e"`
	X5c []string `json:"x5c"`
}

// authCfg is the Auth0 configuration provided at application startup.
var authCfg *AuthConfig

// jwksBytes is a cache of the JSON Web Key Set for this domain.
var jwksBytes = make([]byte, 0)

// getPEMCert is a function to get the applicable certificate for a JSON Web Token.
func getPEMCert(token *jwt.Token) (string, error) {
	cert := ""

	if len(jwksBytes) == 0 {
		resp, err := http.Get(fmt.Sprintf("https://%s/.well-known/jwks.json", authCfg.Domain))
		if err != nil {
			return cert, err
		}
		defer resp.Body.Close()

		if jwksBytes, err = ioutil.ReadAll(resp.Body); err != nil {
			return cert, err
		}
	}

	jwks := JWKS{}
	if err := json.Unmarshal(jwksBytes, &jwks); err != nil {
		return cert, err
	}
	for k, v := range jwks.Keys[0].X5c {
		if token.Header["kid"] == jwks.Keys[k].Kid {
			cert = fmt.Sprintf("-----BEGIN CERTIFICATE-----\n%s\n-----END CERTIFICATE-----", v)
		}
	}
	if cert == "" {
		err := errors.New("unable to find appropriate key")
		return cert, err
	}

	return cert, nil
}

// authZero is an instance of Auth0's JWT middlware. Since it doesn't support the http.HandlerFunc sig, it is wrapped
// below; it's defined outside that function, though, so it does not get recreated every time.
var authZero = jwtmiddleware.New(jwtmiddleware.Options{
	ValidationKeyGetter: func(token *jwt.Token) (interface{}, error) {
		if checkAud := token.Claims.(jwt.MapClaims).VerifyAudience(authCfg.ClientID, false); !checkAud {
			return token, errors.New("invalid audience")
		}
		iss := fmt.Sprintf("https://%s/", authCfg.Domain)
		if checkIss := token.Claims.(jwt.MapClaims).VerifyIssuer(iss, false); !checkIss {
			return token, errors.New("invalid issuer")
		}

		cert, err := getPEMCert(token)
		if err != nil {
			panic(err.Error())
		}

		result, _ := jwt.ParseRSAPublicKeyFromPEM([]byte(cert))
		return result, nil
	},
	SigningMethod: jwt.SigningMethodRS256,
})

// authMiddleware is a wrapper for the Auth0 middleware above with a signature ozzo-routing recognizes.
func authMiddleware(c *routing.Context) error {
	return authZero.CheckJWT(c.Response, c.Request)
}

// NewRouter returns a configured router to handle all incoming requests.
func NewRouter(cfg *AuthConfig) *routing.Router {
	authCfg = cfg
	router := routing.New()
	router.Use(
		access.Logger(log.Printf), // TODO: remove before go-live
		fault.Recovery(log.Printf),
	)
	for _, route := range routes {
		if route.IsPublic {
			router.To(route.Method, route.Pattern, route.Func)
		} else {
			router.To(route.Method, route.Pattern, authMiddleware, route.Func)
		}
	}
	return router
}
