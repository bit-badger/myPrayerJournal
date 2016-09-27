namespace MyPrayerJournal.ViewModels

open System.ComponentModel.DataAnnotations

[<AllowNullLiteral>]
type LogOnViewModel() =
  inherit AppViewModel()

  [<Required>]
  [<DataType(DataType.EmailAddress)>]
  [<Display(Name = "E-mail Address")>]
  member val Email = "" with get, set

  [<Required>]
  [<DataType(DataType.Password)>]
  [<Display(Name = "Password")>]
  member val Password = "" with get, set
