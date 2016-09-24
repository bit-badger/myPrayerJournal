namespace MyPrayerJournal

open Nancy

type UserModule() as this =
  inherit NancyModule("user")

  do
    this.Get ("/log-on", fun _ -> this.ShowLogOn ())
    this.Post("/log-on", fun parms -> this.DoLogOn (downcast parms))

  member this.ShowLogOn () : obj =
    let model = MyPrayerJournalModel(this.Context)
    model.PageTitle <- Strings.get "LogOn"
    upcast this.View.["user/log-on", model]
  
  member this.DoLogOn (parms : DynamicDictionary) : obj =
    upcast "X"