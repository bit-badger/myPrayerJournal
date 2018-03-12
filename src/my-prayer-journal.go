// myPrayerJournal API Server
package main

import (
	"encoding/json"
	"log"
	"net/http"
	"os"

	"github.com/danieljsummers/myPrayerJournal/src/api/data"
	"github.com/danieljsummers/myPrayerJournal/src/api/routes"
)

// Web contains configuration for the web server.
type Web struct {
	Port string `json:"port"`
}

// Settings contains configuration for the myPrayerJournal API.
type Settings struct {
	Data *data.Settings     `json:"data"`
	Web  *Web               `json:"web"`
	Auth *routes.AuthConfig `json:"auth"`
}

// readSettings parses the JSON configuration file into the Settings struct.
func readSettings(f string) *Settings {
	config, err := os.Open(f)
	if err != nil {
		log.Fatal(err)
	}
	defer config.Close()
	parser := json.NewDecoder(config)
	settings := Settings{}
	if err = parser.Decode(&settings); err != nil {
		log.Fatal(err)
	}
	return &settings
}

func main() {
	cfg := readSettings("config.json")
	db, ok := data.Connect(cfg.Data)
	if !ok {
		log.Fatal("Unable to connect to database; exiting")
	}
	log.Printf("myPrayerJournal API listening on %s", cfg.Web.Port)
	log.Fatal(http.ListenAndServe(cfg.Web.Port, routes.NewRouter(db, cfg.Auth)))
}
