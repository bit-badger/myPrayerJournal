namespace MyPrayerJournal

open Newtonsoft.Json

/// A user
type User = {
  /// The Id of the user
  [<JsonProperty("id")>]
  Id : string
  /// The user's e-mail address
  Email : string
  /// A hash of the user's password
  PasswordHash : string
  /// The user's name
  Name : string
  /// The last time the user logged on
  LastSeenOn : int64
}

/// Request history entry
type History = {
  /// The instant at which the update was made
  AsOf : int64
  /// The action that was taken on the request
  Action : string list
  /// The status of the request (filled if it changed)
  Status : string option
  /// The text of the request (filled if it changed)
  Text : string option
}

/// A prayer request
type Request = {
  /// The Id of the request
  [<JsonProperty("id")>]
  Id : string
  /// The Id of the user to whom this request belongs
  UserId : string
  /// The instant this request was entered
  EnteredOn : int64
  /// The history for this request
  History : History list
}
  with
    /// The current status of the prayer request
    member this.Status =
      this.History
      |> List.sortBy (fun item -> -item.AsOf)
      |> List.map    (fun item -> item.Status)
      |> List.filter Option.isSome
      |> List.map    Option.get
      |> List.head
    /// The current text of the prayer request
    member this.Text =
      this.History
      |> List.sortBy (fun item -> -item.AsOf)
      |> List.map    (fun item -> item.Text)
      |> List.filter Option.isSome
      |> List.map Option.get
      |> List.head
    member this.LastActionOn =
      this.History
      |> List.sortBy (fun item -> -item.AsOf)
      |> List.map    (fun item -> item.AsOf)
      |> List.head
      