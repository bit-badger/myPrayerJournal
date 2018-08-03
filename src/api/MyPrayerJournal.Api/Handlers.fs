/// HTTP handlers for the myPrayerJournal API
[<RequireQualifiedAccess>]
module MyPrayerJournal.Handlers

open Giraffe

/// Handle 404s from the API, sending known URL paths to the Vue app so that they can be handled there
let notFound : HttpHandler =
  fun next ctx ->
    let vueApp () = htmlFile "/index.html" next ctx
    match true with
    | _ when ctx.Request.Path.Value.StartsWith "/answered" -> vueApp ()
    | _ when ctx.Request.Path.Value.StartsWith "/journal" -> vueApp ()
    | _ when ctx.Request.Path.Value.StartsWith "/user" -> vueApp ()
    | _ -> (setStatusCode 404 >=> json ([ "error", "not found" ] |> dict)) next ctx


/// Handler helpers
[<AutoOpen>]
module private Helpers =
  
  open Microsoft.AspNetCore.Http
  open System

  /// Get the database context from DI
  let db (ctx : HttpContext) =
    ctx.GetService<AppDbContext> ()

  /// Get the user's "sub" claim
  let user (ctx : HttpContext) =
    ctx.User.Claims |> Seq.tryFind (fun u -> u.Type = "sub")

  /// Return a 201 CREATED response
  let created next ctx =
    setStatusCode 201 next ctx

  /// The "now" time in JavaScript
  let jsNow () =
    DateTime.Now.Subtract(DateTime (1970, 1, 1)).TotalSeconds |> int64 |> (*) 1000L


/// Strongly-typed models for post requests
module Models =
  
  /// A history entry addition (AKA request update)
  [<CLIMutable>]
  type HistoryEntry =
    { /// The status of the history update
      status     : string
      /// The text of the update
      updateText : string
      }
  
  /// An additional note
  [<CLIMutable>]
  type NoteEntry =
    { /// The notes being added
      notes : string
      }
  
  /// A prayer request
  [<CLIMutable>]
  type Request =
    { /// The text of the request
      requestText : string
      }
  
  /// The time until which a request should not appear in the journal
  [<CLIMutable>]
  type SnoozeUntil =
    { /// The time at which the request should reappear
      until : int64
      }

/// /api/journal URLs
module Journal =
  
  /// GET /api/journal
  let journal : HttpHandler =
    fun next ctx ->
      match user ctx with
      | Some u -> json ((db ctx).JournalByUserId u.Value) next ctx
      | None -> notFound next ctx


/// /api/request URLs
module Request =
  
  open NCuid
  
  /// POST /api/request
  let add : HttpHandler =
    fun next ctx ->
      task {
        match user ctx with
        | Some u ->
            let! r     = ctx.BindJsonAsync<Models.Request> ()
            let  db    = db ctx
            let  reqId = Cuid.Generate ()
            let  now   = jsNow ()
            { Request.empty with
                requestId    = reqId
                userId       = u.Value
                enteredOn    = now
                snoozedUntil = 0L
              }
            |> db.AddEntry
            { History.empty with
                requestId = reqId
                asOf      = now
                status    = "Created"
                text      = Some r.requestText
                }
            |> db.AddEntry
            let! _   = db.SaveChangesAsync ()
            let! req = db.TryJournalById reqId u.Value
            match req with
            | Some rqst -> return! (setStatusCode 201 >=> json rqst) next ctx
            | None -> return! notFound next ctx
        | None -> return! notFound next ctx
        }

  /// POST /api/request/[req-id]/history
  let addHistory reqId : HttpHandler =
    fun next ctx ->
      task {
        match user ctx with
        | Some u ->
            let  db  = db ctx
            let! req = db.TryRequestById reqId u.Value
            match req with
            | Some _ ->
                let! hist = ctx.BindJsonAsync<Models.HistoryEntry> ()
                { History.empty with
                    requestId = reqId
                    asOf      = jsNow ()
                    status    = hist.status
                    text      = match hist.updateText with null | "" -> None | x -> Some x
                  }
                |> db.AddEntry
                let! _ = db.SaveChangesAsync ()
                return! created next ctx
            | None -> return! notFound next ctx
        | None -> return! notFound next ctx
        }
  
  /// POST /api/request/[req-id]/note
  let addNote reqId : HttpHandler =
    fun next ctx ->
      task {
        match user ctx with
        | Some u ->
            let  db = db ctx
            let! req = db.TryRequestById reqId u.Value
            match req with
            | Some _ ->
                let! notes = ctx.BindJsonAsync<Models.NoteEntry> ()
                { Note.empty with
                    requestId = reqId
                    asOf = jsNow ()
                    notes = notes.notes
                  }
                |> db.AddEntry
                let! _ = db.SaveChangesAsync ()
                return! created next ctx
            | None -> return! notFound next ctx
        | None -> return! notFound next ctx
        }
          
  /// GET /api/requests/answered
  let answered : HttpHandler =
    fun next ctx ->
      match user ctx with
      | Some u -> json ((db ctx).AnsweredRequests u.Value) next ctx
      | None -> notFound next ctx
  
  /// GET /api/request/[req-id]
  let get reqId : HttpHandler =
    fun next ctx ->
      task {
        match user ctx with
        | Some u ->
            let! req = (db ctx).TryRequestById reqId u.Value
            match req with
            | Some r -> return! json r next ctx
            | None -> return! notFound next ctx
        | None -> return! notFound next ctx
        }
  
  /// GET /api/request/[req-id]/complete
  let getComplete reqId : HttpHandler =
    fun next ctx ->
      task {
        match user ctx with
        | Some u ->
            let! req = (db ctx).TryCompleteRequestById reqId u.Value
            match req with
            | Some r -> return! json r next ctx
            | None -> return! notFound next ctx
        | None -> return! notFound next ctx
        }
  
  /// GET /api/request/[req-id]/full
  let getFull reqId : HttpHandler =
    fun next ctx ->
      task {
        match user ctx with
        | Some u ->
            let! req = (db ctx).TryFullRequestById reqId u.Value
            match req with
            | Some r -> return! json r next ctx
            | None -> return! notFound next ctx
        | None -> return! notFound next ctx
        }
  
  /// GET /api/request/[req-id]/notes
  let getNotes reqId : HttpHandler =
    fun next ctx ->
      task {
        match user ctx with
        | Some u ->
            let! notes = (db ctx).NotesById reqId u.Value
            return! json notes next ctx
        | None -> return! notFound next ctx
        }
  
  /// POST /api/request/[req-id]/snooze
  let snooze reqId : HttpHandler =
    fun next ctx ->
      task {
        match user ctx with
        | Some u ->
            let  db  = db ctx
            let! req = db.TryRequestById reqId u.Value
            match req with
            | Some r ->
                let! until = ctx.BindJsonAsync<Models.SnoozeUntil> ()
                { r with snoozedUntil = until.until }
                |> db.UpdateEntry
                let! _ = db.SaveChangesAsync ()
                return! setStatusCode 204 next ctx
            | None -> return! notFound next ctx
        | None -> return! notFound next ctx
        }
