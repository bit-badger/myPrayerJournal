// myPrayerJournal API Server
package main

import (
	"fmt"
	"time"
)

func main() {
	fmt.Print(time.Now().UnixNano() / int64(1000000))
}
