namespace MyPrayerJournal

open Nancy

type HomeModule() as this =
  inherit NancyModule()

  do
    this.Get ("/", fun _ -> this.Home ())

  member this.Home () : obj =
    let model = MyPrayerJournalModel(this.Context)
    model.PageTitle <- Strings.get "Welcome"
    upcast this.View.["home/index", model]
  