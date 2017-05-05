namespace MyPrayerJournal

open Microsoft.EntityFrameworkCore;
open Newtonsoft.Json
open System
open System.Collections.Generic

/// A prayer request
[<AllowNullLiteral>]
type Request() =
  /// The history collection (can be overridden)
  let mutable historyCollection : ICollection<History> = upcast List<History> () 
  
  /// The Id of the prayer request
  member val RequestId = Guid.Empty with get, set
  /// The Id of the user to whom the request belongs
  member val UserId = "" with get, set
  /// The ticks when the request was entered
  member val EnteredOn = 0L with get, set
  
  /// The history for the prayer request
  abstract History : ICollection<History> with get, set
  default this.History
    with get () = historyCollection
    and set v = historyCollection <- v
  
  static member ConfigureEF (mb : ModelBuilder) =
    mb.Entity<Request>().ToTable "Request"
    |> ignore
    mb


/// A historial update to a prayer request
and [<AllowNullLiteral>] History() =
  /// The request to which this entry applies (may be overridden)
  let mutable request = null

  /// The Id of the request to which this update applies
  member val RequestId = Guid.Empty with get, set
  /// The ticks when this entry was made
  member val AsOf = 0L with get, set
  /// The status of the request as of this history entry
  member val Status = "" with get, set
  /// The text of this history entry
  member val Text = "" with get, set

  /// The request to which this entry belongs
  abstract Request : Request with get, set
  default this.Request 
    with get () = request
    and set v = request <- v

  static member ConfigureEF (mb : ModelBuilder) =
    mb.Entity<History>().ToTable("History")
    |> ignore
    mb.Entity<History>().HasKey(fun e -> (e.RequestId, e.AsOf) :> obj)
    |> ignore
    mb

(*
/// A user
type Userr = {
  /// The Id of the user
  [<JsonProperty("id")>]
  Id : string
  /// The user's e-mail address
  Email : string
  /// The user's name
  Name : string
  /// The time zone in which the user resides
  TimeZone : string
  /// The last time the user logged on
  LastSeenOn : int64
}
  with
    /// An empty User
    static member Empty =
      { Id           = ""
        Email        = ""
        Name         = ""
        TimeZone     = ""
        LastSeenOn   = int64 0 }


/// Request history entry
type Historyy = {
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
type Requestt = {
  /// The Id of the request
  [<JsonProperty("id")>]
  Id : string
  /// The Id of the user to whom this request belongs
  UserId : string
  /// The instant this request was entered
  EnteredOn : int64
  /// The history for this request
  History : Historyy list
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
*)