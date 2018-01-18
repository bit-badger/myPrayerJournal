// myPrayerJournal API Server
package main

import (
	"log"

	"github.com/danieljsummers/myPrayerJournal/src/api/data"
	"github.com/danieljsummers/myPrayerJournal/src/api/routes"
)

func main() {
	db, ok := data.Connect(&data.Settings{})
	if !ok {
		log.Fatal("Unable to connect to database; exiting")
	}
	router := routes.Routes(db)
	_ = router // TODO: remove
}
