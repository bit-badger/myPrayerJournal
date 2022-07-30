/// HTTP handlers for the myPrayerJournal API
[<RequireQualifiedAccess>]
module MyPrayerJournal.Handlers

// fsharplint:disable RecordFieldNames

open Giraffe
open Giraffe.Htmx
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Http
open System
open System.Security.Claims
open NodaTime

/// Helper function to be able to split out log on
[<AutoOpen>]
module private LogOnHelpers =

    /// Log on, optionally specifying a redirected URL once authentication is complete
    let logOn url : HttpHandler = fun next ctx -> backgroundTask {
        match url with
        | Some it ->
            do! ctx.ChallengeAsync ("Auth0", AuthenticationProperties (RedirectUri = it))
            return! next ctx
        | None -> return! challenge "Auth0" next ctx
    }


/// Handlers for error conditions
module Error =

    open Microsoft.Extensions.Logging
    open System.Threading.Tasks

    /// Handle errors
    let error (ex : Exception) (log : ILogger) =
        log.LogError (EventId (), ex, "An unhandled exception has occurred while executing the request.")
        clearResponse
        >=> setStatusCode 500
        >=> setHttpHeader "X-Toast" $"error|||{ex.GetType().Name}: {ex.Message}"
        >=> text ex.Message

    /// Handle unauthorized actions, redirecting to log on for GETs, otherwise returning a 401 Not Authorized response
    let notAuthorized : HttpHandler = fun next ctx ->
        (next, ctx)
        ||> match ctx.Request.Method with
            | "GET" -> logOn None
            | _ -> setStatusCode 401 >=> fun _ _ -> Task.FromResult<HttpContext option> None

    /// Handle 404s from the API, sending known URL paths to the Vue app so that they can be handled there
    let notFound : HttpHandler =
        setStatusCode 404 >=> text "Not found"


/// Handler helpers
[<AutoOpen>]
module private Helpers =
  
    open LiteDB
    open Microsoft.Extensions.Logging
    open Microsoft.Net.Http.Headers

    let debug (ctx : HttpContext) message =
        let fac = ctx.GetService<ILoggerFactory>()
        let log = fac.CreateLogger "Debug"
        log.LogInformation message

    /// Get the LiteDB database
    let db (ctx : HttpContext) = ctx.GetService<LiteDatabase>()

    /// Get the user's "sub" claim
    let user (ctx : HttpContext) =
        ctx.User
        |> Option.ofObj
        |> Option.map (fun user -> user.Claims |> Seq.tryFind (fun u -> u.Type = ClaimTypes.NameIdentifier))
        |> Option.flatten
        |> Option.map (fun claim -> claim.Value)

    /// Get the current user's ID
    //  NOTE: this may raise if you don't run the request through the requiresAuthentication handler first
    let userId ctx =
        (user >> Option.get) ctx |> UserId

    /// Get the system clock
    let clock (ctx : HttpContext) =
        ctx.GetService<IClock> ()
  
    /// Get the current instant
    let now ctx =
        (clock ctx).GetCurrentInstant ()

    /// Return a 201 CREATED response
    let created =
        setStatusCode 201

    /// Return a 201 CREATED response with the location header set for the created resource
    let createdAt url : HttpHandler = fun next ctx ->
        Successful.CREATED
            ($"{ctx.Request.Scheme}://{ctx.Request.Host.Value}{url}" |> setHttpHeader HeaderNames.Location) next ctx
  
    /// Return a 303 SEE OTHER response (forces a GET on the redirected URL)
    let seeOther (url : string) =
        noResponseCaching >=> setStatusCode 303 >=> setHttpHeader "Location" url

    /// Render a component result
    let renderComponent nodes : HttpHandler =
        noResponseCaching
        >=> fun _ ctx -> backgroundTask {
            return! ctx.WriteHtmlStringAsync (ViewEngine.RenderView.AsString.htmlNodes nodes)
        }

    open Views.Layout

    /// Create a page rendering context
    let pageContext (ctx : HttpContext) pageTitle content = backgroundTask {
        let! hasSnoozed = backgroundTask {
          match user ctx with
          | Some _ -> return! Data.hasSnoozed (userId ctx) (now ctx) (db ctx)
          | None   -> return  false
          }
        return {
          isAuthenticated = (user >> Option.isSome) ctx
          hasSnoozed      = hasSnoozed
          currentUrl      = ctx.Request.Path.Value
          pageTitle       = pageTitle
          content         = content
          }
    }

    /// Composable handler to write a view to the output
    let writeView view : HttpHandler = fun _ ctx -> backgroundTask {
        return! ctx.WriteHtmlViewAsync view
    }
    
    
    /// Hold messages across redirects
    module Messages =

        /// The messages being held
        let mutable private messages : Map<string, string * string> = Map.empty

        /// Locked update to prevent updates by multiple threads
        let private upd8 = obj ()

        /// Push a new message into the list
        let push ctx message url = lock upd8 (fun () ->
          messages <- messages.Add (ctx |> (user >> Option.get), (message, url)))

        /// Add a success message header to the response
        let pushSuccess ctx message url =
          push ctx $"success|||{message}" url
        
        /// Pop the messages for the given user
        let pop userId = lock upd8 (fun () ->
          let msg = messages.TryFind userId
          msg |> Option.iter (fun _ -> messages <- messages.Remove userId)
          msg)

    /// Send a partial result if this is not a full page load (does not append no-cache headers)
    let partialStatic (pageTitle : string) content : HttpHandler = fun next ctx -> backgroundTask {
        let  isPartial = ctx.Request.IsHtmx && not ctx.Request.IsHtmxRefresh
        let! pageCtx   = pageContext ctx pageTitle content
        let  view      = (match isPartial with true -> partial | false -> view) pageCtx
        return! 
            (next, ctx)
            ||> match user ctx with
                | Some u ->
                    match Messages.pop u with
                    | Some (msg, url) -> setHttpHeader "X-Toast" msg >=> withHxPush url >=> writeView view
                    | None -> writeView view
                | None -> writeView view
    }
   
    /// Send an explicitly non-cached result, rendering as a partial if this is not a full page load
    let partial pageTitle content =
        noResponseCaching >=> partialStatic pageTitle content

    /// Add a success message header to the response
    let withSuccessMessage : string -> HttpHandler =
        sprintf "success|||%s" >> setHttpHeader "X-Toast"
  
    /// Hide a modal window when the response is sent
    let hideModal (name : string) : HttpHandler =
        setHttpHeader "X-Hide-Modal" name


/// Strongly-typed models for post requests
module Models =
  
    /// An additional note
    [<CLIMutable; NoComparison; NoEquality>]
    type NoteEntry =
        {   /// The notes being added
            notes : string
        }
    
    /// A prayer request
    [<CLIMutable; NoComparison; NoEquality>]
    type Request =
        {   /// The ID of the request
            requestId     : string
            
            /// Where to redirect after saving
            returnTo      : string
            
            /// The text of the request
            requestText   : string
            
            /// The additional status to record
            status        : string option
            
            /// The recurrence type
            recurType     : string
            
            /// The recurrence count
            recurCount    : int16 option
            
            /// The recurrence interval
            recurInterval : string option
        }
  
    /// The date until which a request should not appear in the journal
    [<CLIMutable; NoComparison; NoEquality>]
    type SnoozeUntil =
        {   /// The date (YYYY-MM-DD) at which the request should reappear
            until : string
        }


open MyPrayerJournal.Data.Extensions
open NodaTime.Text

/// Handlers for less-than-full-page HTML requests
module Components =

    // GET /components/journal-items
    let journalItems : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        let  now   = now ctx
        let! jrnl  = Data.journalByUserId (userId ctx) (db ctx)
        let  shown = jrnl |> List.filter (fun it -> now > it.snoozedUntil && now > it.showAfter)
        return! renderComponent [ Views.Journal.journalItems now shown ] next ctx
    }
  
    // GET /components/request-item/[req-id]
    let requestItem reqId : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        match! Data.tryJournalById (RequestId.ofString reqId) (userId ctx) (db ctx) with
        | Some req -> return! renderComponent [ Views.Request.reqListItem (now ctx) req ] next ctx
        | None     -> return! Error.notFound next ctx
    }

    // GET /components/request/[req-id]/add-notes
    let addNotes requestId : HttpHandler =
        requiresAuthentication Error.notAuthorized
        >=> renderComponent (Views.Journal.notesEdit (RequestId.ofString requestId))

    // GET /components/request/[req-id]/notes
    let notes requestId : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        let! notes = Data.notesById (RequestId.ofString requestId) (userId ctx) (db ctx)
        return! renderComponent (Views.Request.notes (now ctx) notes) next ctx
    }
  
    // GET /components/request/[req-id]/snooze
    let snooze requestId : HttpHandler =
        requiresAuthentication Error.notAuthorized
        >=> renderComponent [ RequestId.ofString requestId |> Views.Journal.snooze ]


/// / URL    
module Home =
  
    // GET /
    let home : HttpHandler =
        partialStatic "Welcome!" Views.Layout.home
  

/// /journal URL
module Journal =
  
    // GET /journal
    let journal : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        let usr =
            ctx.User.Claims
            |> Seq.tryFind (fun c -> c.Type = ClaimTypes.GivenName)
            |> Option.map (fun c -> c.Value)
            |> Option.defaultValue "Your"
        let title = usr |> match usr with "Your" -> sprintf "%s" | _ -> sprintf "%s's"
        return! partial $"{title} Prayer Journal" (Views.Journal.journal usr) next ctx
    }


/// /legal URLs
module Legal =
  
    // GET /legal/privacy-policy
    let privacyPolicy : HttpHandler =
        partialStatic "Privacy Policy" Views.Legal.privacyPolicy
  
    // GET /legal/terms-of-service
    let termsOfService : HttpHandler =
        partialStatic "Terms of Service" Views.Legal.termsOfService


/// /api/request and /request(s) URLs
module Request =

    // GET /request/[req-id]/edit  
    let edit requestId : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        let returnTo =
            match ctx.Request.Headers.Referer[0] with
            | it when it.EndsWith "/active"  -> "active"
            | it when it.EndsWith "/snoozed" -> "snoozed"
            | _                              -> "journal"
        match requestId with
        | "new" ->
            return! partial "Add Prayer Request"
                        (Views.Request.edit (JournalRequest.ofRequestLite Request.empty) returnTo true) next ctx
        | _     ->
            match! Data.tryJournalById (RequestId.ofString requestId) (userId ctx) (db ctx) with
            | Some req ->
                debug ctx "Found - sending view"
                return! partial "Edit Prayer Request" (Views.Request.edit req returnTo false) next ctx
            | None     ->
                debug ctx "Not found - uh oh..."
                return! Error.notFound next ctx
    }

    // PATCH /request/[req-id]/prayed
    let prayed requestId : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        let db    = db     ctx
        let usrId = userId ctx
        let reqId = RequestId.ofString requestId
        match! Data.tryRequestById reqId usrId db with
        | Some req ->
            let now  = now ctx
            do! Data.addHistory reqId usrId { asOf = now; status = Prayed; text = None } db
            let nextShow =
                match Recurrence.duration req.recurrence with
                | 0L       -> Instant.MinValue
                | duration -> now.Plus (Duration.FromSeconds duration)
            do! Data.updateShowAfter reqId usrId nextShow db
            do! db.saveChanges ()
            return! (withSuccessMessage "Request marked as prayed" >=> Components.journalItems) next ctx
        | None -> return! Error.notFound next ctx
    }
  
    /// POST /request/[req-id]/note
    let addNote requestId : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        let db    = db     ctx
        let usrId = userId ctx
        let reqId = RequestId.ofString requestId
        match! Data.tryRequestById reqId usrId db with
        | Some _ ->
            let! notes = ctx.BindFormAsync<Models.NoteEntry> ()
            do! Data.addNote reqId usrId { asOf = now ctx; notes = notes.notes } db
            do! db.saveChanges ()
            return! (withSuccessMessage "Added Notes" >=> hideModal "notes" >=> created) next ctx
        | None -> return! Error.notFound next ctx
    }
          
    // GET /requests/active
    let active : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        let! reqs = Data.journalByUserId (userId ctx) (db ctx)
        return! partial "Active Requests" (Views.Request.active (now ctx) reqs) next ctx
    }
  
    // GET /requests/snoozed
    let snoozed : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        let! reqs    = Data.journalByUserId (userId ctx) (db ctx)
        let  now     = now ctx
        let  snoozed = reqs |> List.filter (fun it -> it.snoozedUntil > now)
        return! partial "Active Requests" (Views.Request.snoozed now snoozed) next ctx
    }

    // GET /requests/answered
    let answered : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        let! reqs = Data.answeredRequests (userId ctx) (db ctx)
        return! partial "Answered Requests" (Views.Request.answered (now ctx) reqs) next ctx
    }
  
    // GET /request/[req-id]/full
    let getFull requestId : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        match! Data.tryFullRequestById (RequestId.ofString requestId) (userId ctx) (db ctx) with
        | Some req -> return! partial "Prayer Request" (Views.Request.full (clock ctx) req) next ctx
        | None     -> return! Error.notFound next ctx
    }
  
    // PATCH /request/[req-id]/show
    let show requestId : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        let db    = db     ctx
        let usrId = userId ctx
        let reqId = RequestId.ofString requestId
        match! Data.tryRequestById reqId usrId db with
        | Some _ ->
            do! Data.updateShowAfter reqId usrId Instant.MinValue db
            do! db.saveChanges ()
            return! (withSuccessMessage "Request now shown" >=> Components.requestItem requestId) next ctx
        | None -> return! Error.notFound next ctx
    }
  
    // PATCH /request/[req-id]/snooze
    let snooze requestId : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        let db    = db     ctx
        let usrId = userId ctx
        let reqId = RequestId.ofString requestId
        match! Data.tryRequestById reqId usrId db with
        | Some _ ->
            let! until = ctx.BindFormAsync<Models.SnoozeUntil> ()
            let date =
                LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd").Parse(until.until).Value
                    .AtStartOfDayInZone(DateTimeZone.Utc)
                    .ToInstant ()
            do! Data.updateSnoozed reqId usrId date db
            do! db.saveChanges ()
            return!
                (withSuccessMessage $"Request snoozed until {until.until}"
                 >=> hideModal "snooze"
                 >=> Components.journalItems) next ctx
        | None -> return! Error.notFound next ctx
    }
  
    // PATCH /request/[req-id]/cancel-snooze
    let cancelSnooze requestId : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        let db    = db     ctx
        let usrId = userId ctx
        let reqId = RequestId.ofString requestId
        match! Data.tryRequestById reqId usrId db with
        | Some _ ->
            do! Data.updateSnoozed reqId usrId Instant.MinValue db
            do! db.saveChanges ()
            return! (withSuccessMessage "Request unsnoozed" >=> Components.requestItem requestId) next ctx
        | None -> return! Error.notFound next ctx
    }

    /// Derive a recurrence from its representation in the form
    let private parseRecurrence (form : Models.Request) =
        match form.recurInterval with Some x -> $"{defaultArg form.recurCount 0s} {x}" | None -> "Immediate"
        |> Recurrence.ofString

    // POST /request
    let add : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        let! form  = ctx.BindModelAsync<Models.Request> ()
        let  db    = db ctx
        let  usrId = userId ctx
        let  now   = now ctx
        let  req   =
            { Request.empty with
                userId     = usrId
                enteredOn  = now
                showAfter  = Instant.MinValue
                recurrence = parseRecurrence form
                history    = [
                    {   asOf   = now
                        status = Created
                        text   = Some form.requestText
                    }      
                ]
            }
        Data.addRequest req db
        do! db.saveChanges ()
        Messages.pushSuccess ctx "Added prayer request" "/journal"
        return! seeOther "/journal" next ctx
    }
  
    // PATCH /request
    let update : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> backgroundTask {
        let! form  = ctx.BindModelAsync<Models.Request> ()
        let  db    = db ctx
        let  usrId = userId ctx
        match! Data.tryJournalById (RequestId.ofString form.requestId) usrId db with
        | Some req ->
            // update recurrence if changed
            let recur = parseRecurrence form
            match recur = req.recurrence with
            | true  -> ()
            | false ->
                do! Data.updateRecurrence req.requestId usrId recur db
                match recur with
                | Immediate -> do! Data.updateShowAfter req.requestId usrId Instant.MinValue db
                | _         -> ()
            // append history
            let upd8Text = form.requestText.Trim ()
            let text     = match upd8Text = req.text with true -> None | false -> Some upd8Text
            do! Data.addHistory req.requestId usrId
                    { asOf = now ctx; status = (Option.get >> RequestAction.ofString) form.status; text = text } db
            do! db.saveChanges ()
            let nextUrl =
                match form.returnTo with
                | "active"          -> "/requests/active"
                | "snoozed"         -> "/requests/snoozed"
                | _ (* "journal" *) -> "/journal"
            Messages.pushSuccess ctx "Prayer request updated successfully" nextUrl
            return! seeOther nextUrl next ctx
        | None -> return! Error.notFound next ctx
    }


/// Handlers for /user URLs
module User =

    open Microsoft.AspNetCore.Authentication.Cookies

    // GET /user/log-on
    let logOn : HttpHandler =
        logOn (Some "/journal")
  
    // GET /user/log-off
    let logOff : HttpHandler = requiresAuthentication Error.notAuthorized >=> fun next ctx -> task {
        do! ctx.SignOutAsync ("Auth0", AuthenticationProperties (RedirectUri = "/"))
        do! ctx.SignOutAsync CookieAuthenticationDefaults.AuthenticationScheme
        return! next ctx
    }


open Giraffe.EndpointRouting

/// The routes for myPrayerJournal
let routes = [
    GET_HEAD [ route "/" Home.home ]
    subRoute "/components/" [
        GET_HEAD [
            route  "journal-items"        Components.journalItems
            routef "request/%s/add-notes" Components.addNotes
            routef "request/%s/item"      Components.requestItem
            routef "request/%s/notes"     Components.notes
            routef "request/%s/snooze"    Components.snooze
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
            routef "/%s/edit"   Request.edit
            routef "/%s/full"   Request.getFull
            route  "s/active"   Request.active
            route  "s/answered" Request.answered
            route  "s/snoozed"  Request.snoozed
        ]
        PATCH [
            route  ""                  Request.update
            routef "/%s/cancel-snooze" Request.cancelSnooze
            routef "/%s/prayed"        Request.prayed
            routef "/%s/show"          Request.show
            routef "/%s/snooze"        Request.snooze
        ]
        POST [
            route  ""         Request.add
            routef "/%s/note" Request.addNote
        ]
    ]
    subRoute "/user/" [
        GET_HEAD [
            route "log-off" User.logOff
            route "log-on"  User.logOn
        ]
    ]
]
