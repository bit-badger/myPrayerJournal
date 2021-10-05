﻿/// HTTP handlers for the myPrayerJournal API
[<RequireQualifiedAccess>]
module MyPrayerJournal.Handlers

// fsharplint:disable RecordFieldNames

open Giraffe
open Giraffe.Htmx
open MyPrayerJournal.Data.Extensions

let writeView view : HttpHandler =
  fun next ctx -> task {
    return! ctx.WriteHtmlViewAsync view
    }

/// Send a partial result if this is not a full page load
let partialIfNotRefresh (pageTitle : string) content : HttpHandler =
  fun next ctx ->
    (next, ctx)
    ||> match ctx.Request.IsHtmx && not ctx.Request.IsHtmxRefresh with
        | true ->
            ctx.Response.Headers.["X-Page-Title"] <- Microsoft.Extensions.Primitives.StringValues pageTitle
            withHxTriggerAfterSettle "menu-refresh" >=> writeView content
        | false -> writeView (Views.Layout.view pageTitle content)

/// Handler to return Vue files
module Vue =
  
  /// The application index page
  let app : HttpHandler = 
    withHxTrigger "menu-refresh"
    >=> partialIfNotRefresh "" (ViewEngine.HtmlElements.str "It works")


open System

/// Handlers for error conditions
module Error =

  open Microsoft.Extensions.Logging

  /// Handle errors
  let error (ex : Exception) (log : ILogger) =
    log.LogError (EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> json ex.Message

  /// Handle 404s from the API, sending known URL paths to the Vue app so that they can be handled there
  let notFound : HttpHandler =
    fun next ctx ->
      [ "/journal"; "/legal"; "/request"; "/user" ]
      |> List.filter ctx.Request.Path.Value.StartsWith
      |> List.length
      |> function
      | 0 -> (setStatusCode 404 >=> json ([ "error", "not found" ] |> dict)) next ctx
      | _ -> Vue.app next ctx

open Cuid
open LiteDB
open System.Security.Claims
open Microsoft.Net.Http.Headers

/// Handler helpers
[<AutoOpen>]
module private Helpers =
  
  open Microsoft.AspNetCore.Http
  open System.Threading.Tasks

  /// Get the LiteDB database
  let db (ctx : HttpContext) = ctx.GetService<LiteDatabase>()

  /// Get the user's "sub" claim
  let user (ctx : HttpContext) =
    ctx.User.Claims |> Seq.tryFind (fun u -> u.Type = ClaimTypes.NameIdentifier)

  /// Get the current user's ID
  //  NOTE: this may raise if you don't run the request through the authorize handler first
  let userId ctx =
    ((user >> Option.get) ctx).Value |> UserId

  /// Create a request ID from a string
  let toReqId x =
    match Cuid.ofString x with
    | Ok cuid -> cuid
    | Error msg -> invalidOp msg
    |> RequestId

  /// Return a 201 CREATED response
  let created =
    setStatusCode 201

  /// Return a 201 CREATED response with the location header set for the created resource
  let createdAt url : HttpHandler =
    fun next ctx ->
      (sprintf "%s://%s%s" ctx.Request.Scheme ctx.Request.Host.Value url |> setHttpHeader HeaderNames.Location
       >=> created) next ctx

  /// The "now" time in JavaScript as Ticks
  let jsNow () =
    DateTime.UtcNow.Subtract(DateTime (1970, 1, 1, 0, 0, 0)).TotalSeconds |> (int64 >> ( * ) 1_000L >> Ticks)
  
  /// Handler to return a 403 Not Authorized reponse
  let notAuthorized : HttpHandler =
    setStatusCode 403 >=> fun _ _ -> Task.FromResult<HttpContext option> None

  /// Handler to require authorization
  let authorize : HttpHandler =
    fun next ctx -> match user ctx with Some _ -> next ctx | None -> notAuthorized next ctx
  
  /// Flip JSON result so we can pipe into it
  let asJson<'T> next ctx (o : 'T) =
    json o next ctx
  
  /// Trigger a menu item refresh
  let withMenuRefresh : HttpHandler =
    withHxTriggerAfterSettle "menu-refresh"

  /// Render a component result
  let renderComponent nodes : HttpHandler =
    fun next ctx -> task {
      return! ctx.WriteHtmlStringAsync (ViewEngine.RenderView.AsString.htmlNodes nodes)
      }


/// Strongly-typed models for post requests
module Models =
  
  /// A history entry addition (AKA request update)
  [<CLIMutable>]
  type HistoryEntry = {
    /// The status of the history update
    status     : string
    /// The text of the update
    updateText : string
    }
  
  /// An additional note
  [<CLIMutable>]
  type NoteEntry = {
    /// The notes being added
    notes : string
    }
  
  /// Recurrence update
  [<CLIMutable>]
  type Recurrence = {
    /// The recurrence type
    recurType  : string
    /// The recurrence cound
    recurCount : int16
    }

  /// A prayer request
  [<CLIMutable>]
  type Request = {
    /// The ID of the request
    id          : string
    /// The text of the request
    requestText : string
    /// The additional status to record
    status      : string option
    /// The recurrence type
    recurType   : string
    /// The recurrence count
    recurCount  : int16 option
    /// The recurrence interval
    recurInterval : string option
    }
  
  /// The time until which a request should not appear in the journal
  [<CLIMutable>]
  type SnoozeUntil = {
    /// The time at which the request should reappear
    until : int64
    }


/// Handlers for less-than-full-page HTML requests
module Components =

  // GET /components/nav-items
  let navItems : HttpHandler =
    fun next ctx -> task {
      let url          = ctx.Request.Headers.HxCurrentUrl
      let isAuthorized = ctx |> (user >> Option.isSome)
      return! renderComponent (Views.Navigation.currentNav isAuthorized false url) next ctx
      }
  
  // GET /components/journal-items
  let journalItems : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let! jrnl = Data.journalByUserId (userId ctx) (db ctx)
      return! renderComponent [ Views.Journal.journalItems jrnl ] next ctx
      }


/// / URL    
module Home =
  
  // GET /
  let home : HttpHandler =
    withMenuRefresh >=> partialIfNotRefresh "Welcome!" Views.Home.home
  
  // GET /user/log-on
  let logOn : HttpHandler =
    partialIfNotRefresh "Logging on..." Views.Home.logOn


/// /journal URL
module Journal =
  
  // GET /journal
  let journal : HttpHandler =
    authorize
    >=> withMenuRefresh
    >=> fun next ctx -> task {
      let usr = ctx.Request.Headers.["X-Given-Name"].[0]
      return! partialIfNotRefresh "Your Prayer Journal" (Views.Journal.journal usr) next ctx
    }


/// /legal URLs
module Legal =
  
  // GET /legal/privacy-policy
  let privacyPolicy : HttpHandler =
    withMenuRefresh >=> partialIfNotRefresh "Privacy Policy" Views.Legal.privacyPolicy
  
  // GET /legal/terms-of-service
  let termsOfService : HttpHandler =
    withMenuRefresh >=> partialIfNotRefresh "Terms of Service" Views.Legal.termsOfService


/// Alias for the Ply task module (The F# "task" CE can't handle differing types well within the same CE)
module Ply = FSharp.Control.Tasks.Affine


/// /api/request and /request(s) URLs
module Request =

  // GET /request/[req-id]/edit  
  let edit requestId : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      match requestId with
      | "new" ->
          return! partialIfNotRefresh "Add Prayer Request"
                    (Views.Request.edit (JournalRequest.ofRequestLite Request.empty) false) next ctx
      | _ ->
          match! Data.tryJournalById (RequestId.ofString requestId) (userId ctx) (db ctx) with
          | Some req -> return! partialIfNotRefresh "Edit Prayer Request" (Views.Request.edit req false) next ctx
          | None -> return! Error.notFound next ctx
      }

  /// Add a new prayer request
  let private addRequest (form : Models.Request) : HttpHandler =
    fun next ctx -> task {
      let  db    = db ctx
      let  usrId = userId ctx
      let  now   = jsNow ()
      let  req   =
        { Request.empty with
            userId     = usrId
            enteredOn  = now
            showAfter  = Ticks 0L
            recurType  = Recurrence.fromString (match form.recurInterval with Some x -> x | _ -> "Immediate")
            recurCount = defaultArg form.recurCount (int16 0)
            history    = [
              { asOf   = now
                status = Created
                text   = Some form.requestText
                }      
              ]
          }
      Data.addRequest req db
      do! db.saveChanges ()
      return! (withHxRedirect "/journal" >=> createdAt (RequestId.toString req.id |> sprintf "/request/%s")) next ctx
      }
  
  /// Update a prayer request
  let private updateRequest (form : Models.Request) : HttpHandler =
    fun next ctx -> Ply.task {
      let  db    = db ctx
      let  usrId = userId ctx
      match! Data.tryJournalById (RequestId.ofString form.id) usrId db with
      | Some req ->
          // TODO: Update recurrence if changed
          let text =
            match form.requestText.Trim () = req.text with
            | true -> None
            | false -> form.requestText.Trim () |> Some
          do! Data.addHistory req.requestId usrId
                { asOf = jsNow (); status = (Option.get >> RequestAction.fromString) form.status; text = text } db
          return! setStatusCode 200 next ctx
      | None -> return! Error.notFound next ctx
      }

  /// POST /request
  let save : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let! form = ctx.BindModelAsync<Models.Request> ()
      let  func = match form.id with "new" -> addRequest | _ -> updateRequest
      return! func form next ctx
      }

  /// POST /api/request/[req-id]/history
  let addHistory requestId : HttpHandler =
    authorize
    >=> fun next ctx -> FSharp.Control.Tasks.Affine.task {
      let db    = db     ctx
      let usrId = userId ctx
      let reqId = toReqId requestId
      match! Data.tryRequestById reqId usrId db with
      | Some req ->
          let! hist = ctx.BindJsonAsync<Models.HistoryEntry> ()
          let  now  = jsNow ()
          let  act  = RequestAction.fromString hist.status
          do! Data.addHistory reqId usrId
                { asOf   = now
                  status = act
                  text   = match hist.updateText with null | "" -> None | x -> Some x
                  } db
          match act with
          | Prayed ->
              let nextShow =
                match Recurrence.duration req.recurType with
                | 0L -> 0L
                | duration -> (Ticks.toLong now) + (duration * int64 req.recurCount)
              do! Data.updateShowAfter reqId usrId (Ticks nextShow) db
          | _ -> ()
          do! db.saveChanges ()
          return! created next ctx
      | None -> return! Error.notFound next ctx
      }
  
  /// POST /api/request/[req-id]/note
  let addNote requestId : HttpHandler =
    authorize
    // >=> allowSyncIO
    >=> fun next ctx -> task {
      let db    = db     ctx
      let usrId = userId ctx
      let reqId = toReqId requestId
      match! Data.tryRequestById reqId usrId db with
      | Some _ ->
          let! notes = ctx.BindJsonAsync<Models.NoteEntry> ()
          do! Data.addNote reqId usrId { asOf = jsNow (); notes = notes.notes } db
          do! db.saveChanges ()
          return! created next ctx
      | None -> return! Error.notFound next ctx
      }
          
  /// GET /requests/active
  let active : HttpHandler =
    authorize
    >=> withMenuRefresh
    >=> fun next ctx -> task {
      let! reqs = Data.journalByUserId (userId ctx) (db ctx)
      return! partialIfNotRefresh "Active Requests" (Views.Request.active reqs) next ctx
      }
  
  /// GET /requests/answered
  let answered : HttpHandler =
    authorize
    >=> withMenuRefresh
    >=> fun next ctx -> task {
      let! reqs = Data.answeredRequests (userId ctx) (db ctx)
      return! partialIfNotRefresh "Answered Requests" (Views.Request.answered reqs) next ctx
      }
  
  /// GET /api/request/[req-id]
  let get requestId : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      match! Data.tryJournalById (toReqId requestId) (userId ctx) (db ctx) with
      | Some req -> return! json req next ctx
      | None -> return! Error.notFound next ctx
      }
  
  /// GET /request/[req-id]/full
  let getFull requestId : HttpHandler =
    authorize
    >=> withMenuRefresh
    >=> fun next ctx -> task {
      match! Data.tryFullRequestById (toReqId requestId) (userId ctx) (db ctx) with
      | Some req -> return! partialIfNotRefresh "Full Prayer Request" (Views.Request.full req) next ctx
      | None -> return! Error.notFound next ctx
      }
  
  /// GET /api/request/[req-id]/notes
  let getNotes requestId : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let! notes = Data.notesById (toReqId requestId) (userId ctx) (db ctx)
      return! json notes next ctx
      }
  
  /// PATCH /api/request/[req-id]/show
  let show requestId : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let db    = db     ctx
      let usrId = userId ctx
      let reqId = toReqId requestId
      match! Data.tryRequestById reqId usrId db with
      | Some _ ->
          do! Data.updateShowAfter reqId usrId (Ticks 0L) db
          do! db.saveChanges ()
          return! setStatusCode 204 next ctx
      | None -> return! Error.notFound next ctx
      }
  
  /// PATCH /api/request/[req-id]/snooze
  let snooze requestId : HttpHandler =
    authorize
    >=> fun next ctx -> task {
      let db    = db     ctx
      let usrId = userId ctx
      let reqId = toReqId requestId
      match! Data.tryRequestById reqId usrId db with
      | Some _ ->
          let! until = ctx.BindJsonAsync<Models.SnoozeUntil> ()
          do! Data.updateSnoozed reqId usrId (Ticks until.until) db
          do! db.saveChanges ()
          return! setStatusCode 204 next ctx
      | None -> return! Error.notFound next ctx
      }

  /// PATCH /api/request/[req-id]/recurrence
  let updateRecurrence requestId : HttpHandler =
    authorize
    >=> fun next ctx -> FSharp.Control.Tasks.Affine.task {
      let db    = db     ctx
      let usrId = userId ctx
      let reqId = toReqId requestId
      match! Data.tryRequestById reqId usrId db with
      | Some _ ->
          let! recur      = ctx.BindJsonAsync<Models.Recurrence> ()
          let  recurrence = Recurrence.fromString recur.recurType
          do! Data.updateRecurrence reqId usrId recurrence recur.recurCount db
          match recurrence with
          | Immediate -> do! Data.updateShowAfter reqId usrId (Ticks 0L) db
          | _ -> ()
          do! db.saveChanges ()
          return! setStatusCode 204 next ctx
      | None -> return! Error.notFound next ctx
      }


open Giraffe.EndpointRouting

/// The routes for myPrayerJournal
let routes =
  [ GET_HEAD [ route "/" Home.home ]
    subRoute "/components/" [
      GET_HEAD [
        route "journal-items" Components.journalItems
        route "nav-items"     Components.navItems
        ]
      ]
    GET_HEAD [ route "/journal" Journal.journal ]
    subRoute "/legal/" [
      GET_HEAD [
        route "privacy-policy"   Legal.privacyPolicy
        route "terms-of-service" Legal.termsOfService
        ]
      ]
    subRoute "/request" [
      GET_HEAD [
        route  "s/active"   Request.active
        route  "s/answered" Request.answered
        routef "/%s/full"   Request.getFull
        routef "/%s/edit"   Request.edit
        ]
      POST [
        route  "/request" Request.save
        ]
      ]
    GET_HEAD [ route "/user/log-on" Home.logOn ]
    subRoute "/api/" [
      GET [
        subRoute "request" [
          routef "/%s/notes"  Request.getNotes
          routef "/%s"        Request.get
          ]
        ]
      PATCH [
        subRoute "request" [
          routef "/%s/recurrence" Request.updateRecurrence
          routef "/%s/show"       Request.show
          routef "/%s/snooze"     Request.snooze
          ]
        ]
      POST [
        subRoute "request" [
          routef "/%s/history" Request.addHistory
          routef "/%s/note"    Request.addNote
          ]
        ]
      ]
    ]
