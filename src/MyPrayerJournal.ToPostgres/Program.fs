open LiteDB
open MyPrayerJournal.Domain
open MyPrayerJournal.LiteData


let lite = new LiteDatabase "Filename=./mpj.db"
Startup.ensureDb lite


