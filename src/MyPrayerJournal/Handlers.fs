/// HTTP handlers for the myPrayerJournal API
[<RequireQualifiedAccess>]
module MyPrayerJournal.Handlers

open Giraffe
open Giraffe.Htmx
open System

/// Helper function to be able to split out log on
[<AutoOpen>]
module private LogOnHelpers =

    open Microsoft.AspNetCore.Authentication
    
    /// Log on, optionally specifying a redirected URL once authentication is complete
    let logOn url : HttpHandler = fun next ctx -> task {
        match url with
        | Some it ->
            do! ctx.ChallengeAsync ("Auth0", AuthenticationProperties (RedirectUri = it))
            return! next ctx
        | None -> return! challenge "Auth0" next ctx
    }


/// Handlers for error conditions
module Error =

    open Microsoft.Extensions.Logging

    /// Handle errors
    let error (ex : Exception) (log : ILogger) =
        log.LogError (EventId (), ex, "An unhandled exception has occurred while executing the request.")
        clearResponse
        >=> setStatusCode 500
        >=> setHttpHeader "X-Toast" $"error|||{ex.GetType().Name}: {ex.Message}"
        >=> text ex.Message

    /// Handle unauthorized actions, redirecting to log on for GETs, otherwise returning a 401 Not Authorized response
    let notAuthorized : HttpHandler = fun next ctx ->
        (if ctx.Request.Method = "GET" then logOn None next else setStatusCode 401 earlyReturn) ctx

    /// Handle 404s from the API, sending known URL paths to the Vue app so that they can be handled there
    let notFound : HttpHandler =
        setStatusCode 404 >=> text "Not found"


open System.Security.Claims
open LiteDB
open Microsoft.AspNetCore.Http
open NodaTime

/// Extensions on the HTTP context
type HttpContext with
    
    /// The LiteDB database
    member this.Db = this.GetService<LiteDatabase> ()
    
    /// The "sub" for the current user (None if no user is authenticated)
    member this.CurrentUser =
        this.User
        |> Option.ofObj
        |> Option.map (fun user -> user.Claims |> Seq.tryFind (fun u -> u.Type = ClaimTypes.NameIdentifier))
        |> Option.flatten
        |> Option.map (fun claim -> claim.Value)
    
    /// The current user's ID
    //  NOTE: this may raise if you don't run the request through the requireUser handler first
    member this.UserId = UserId this.CurrentUser.Value
    
    /// The system clock
    member this.Clock = this.GetService<IClock> ()
    
    /// Get the current instant from the system clock
    member this.Now = this.Clock.GetCurrentInstant
    
    /// Get the time zone from the X-Time-Zone header (default UTC)
    member this.TimeZone =
        match this.TryGetRequestHeader "X-Time-Zone" with
        | Some tz ->
            match this.GetService<IDateTimeZoneProvider>().GetZoneOrNull tz with
            | null -> DateTimeZone.Utc
            | zone -> zone
        | None -> DateTimeZone.Utc


/// Handler helpers
[<AutoOpen>]
module private Helpers =
  
    open Microsoft.Extensions.Logging
    open Microsoft.Net.Http.Headers

    /// Require a user to be logged on
    let requireUser : HttpHandler =
        requiresAuthentication Error.notAuthorized
    
    /// Debug logger
    let debug (ctx : HttpContext) message =
        let fac = ctx.GetService<ILoggerFactory> ()
        let log = fac.CreateLogger "Debug"
        log.LogInformation message

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
    open System.Threading.Tasks
    
    /// Create a page rendering context
    let pageContext (ctx : HttpContext) pageTitle content = backgroundTask {
        let! hasSnoozed =
            match ctx.CurrentUser with
            | Some _ -> Data.hasSnoozed ctx.UserId (ctx.Now ()) ctx.Db
            | None   -> Task.FromResult false
        return
            {   IsAuthenticated = Option.isSome ctx.CurrentUser
                HasSnoozed      = hasSnoozed
                CurrentUrl      = ctx.Request.Path.Value
                PageTitle       = pageTitle
                Content         = content
            }
    }

    /// Composable handler to write a view to the output
    let writeView view : HttpHandler = fun _ ctx -> backgroundTask {
        return! ctx.WriteHtmlViewAsync view
    }
    
    
    /// Hold messages across redirects
    module Messages =

        /// The messages being held
        let mutable private messages : Map<UserId, string * string> = Map.empty

        /// Locked update to prevent updates by multiple threads
        let private upd8 = obj ()

        /// Push a new message into the list
        let push (ctx : HttpContext) message url = lock upd8 (fun () ->
          messages <- messages.Add (ctx.UserId, (message, url)))

        /// Add a success message header to the response
        let pushSuccess ctx message url =
          push ctx $"success|||%s{message}" url
        
        /// Pop the messages for the given user
        let pop userId = lock upd8 (fun () ->
          let msg = messages.TryFind userId
          msg |> Option.iter (fun _ -> messages <- messages.Remove userId)
          msg)

    /// Send a partial result if this is not a full page load (does not append no-cache headers)
    let partialStatic (pageTitle : string) content : HttpHandler = fun next ctx -> task {
        let  isPartial = ctx.Request.IsHtmx && not ctx.Request.IsHtmxRefresh
        let! pageCtx   = pageContext ctx pageTitle content
        let  view      = (match isPartial with true -> partial | false -> view) pageCtx
        return! 
            (next, ctx)
            ||> match ctx.CurrentUser with
                | Some _ ->
                    match Messages.pop ctx.UserId with
                    | Some (msg, url) -> setHttpHeader "X-Toast" msg >=> withHxPushUrl url >=> writeView view
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
    let journalItems : HttpHandler = requireUser >=> fun next ctx -> task {
        let now = ctx.Now ()
        let shouldBeShown (req : JournalRequest) =
            match req.SnoozedUntil, req.ShowAfter with
            | None, None -> true
            | Some snooze, Some hide when snooze < now && hide < now -> true
            | Some snooze, _ when snooze < now -> true
            | _, Some hide when hide < now -> true
            | _, _ -> false
        let! journal = Data.journalByUserId ctx.UserId ctx.Db
        let  shown   = journal |> List.filter shouldBeShown
        return! renderComponent [ Views.Journal.journalItems now ctx.TimeZone shown ] next ctx
    }
  
    // GET /components/request-item/[req-id]
    let requestItem reqId : HttpHandler = requireUser >=> fun next ctx -> task {
        match! Data.tryJournalById (RequestId.ofString reqId) ctx.UserId ctx.Db with
        | Some req -> return! renderComponent [ Views.Request.reqListItem (ctx.Now ()) ctx.TimeZone req ] next ctx
        | None     -> return! Error.notFound next ctx
    }

    // GET /components/request/[req-id]/add-notes
    let addNotes requestId : HttpHandler =
        requireUser >=> renderComponent (Views.Journal.notesEdit (RequestId.ofString requestId))

    // GET /components/request/[req-id]/notes
    let notes requestId : HttpHandler = requireUser >=> fun next ctx -> task {
        let! notes = Data.notesById (RequestId.ofString requestId) ctx.UserId ctx.Db
        return! renderComponent (Views.Request.notes (ctx.Now ()) ctx.TimeZone (List.ofArray notes)) next ctx
    }
  
    // GET /components/request/[req-id]/snooze
    let snooze requestId : HttpHandler =
        requireUser >=> renderComponent [ RequestId.ofString requestId |> Views.Journal.snooze ]


/// / URL    
module Home =
  
    // GET /
    let home : HttpHandler =
        partialStatic "Welcome!" Views.Layout.home
  

/// /journal URL
module Journal =
  
    // GET /journal
    let journal : HttpHandler = requireUser >=> fun next ctx -> task {
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
    let edit requestId : HttpHandler = requireUser >=> fun next ctx -> task {
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
            match! Data.tryJournalById (RequestId.ofString requestId) ctx.UserId ctx.Db with
            | Some req ->
                debug ctx "Found - sending view"
                return! partial "Edit Prayer Request" (Views.Request.edit req returnTo false) next ctx
            | None     ->
                debug ctx "Not found - uh oh..."
                return! Error.notFound next ctx
    }

    // PATCH /request/[req-id]/prayed
    let prayed requestId : HttpHandler = requireUser >=> fun next ctx -> task {
        let db     = ctx.Db
        let userId = ctx.UserId
        let reqId  = RequestId.ofString requestId
        match! Data.tryRequestById reqId userId db with
        | Some req ->
            let now  = ctx.Now ()
            do! Data.addHistory reqId userId { AsOf = now; Status = Prayed; Text = None } db
            let nextShow =
                match Recurrence.duration req.Recurrence with
                | 0L       -> None
                | duration -> Some <| now.Plus (Duration.FromSeconds duration)
            do! Data.updateShowAfter reqId userId nextShow db
            do! db.SaveChanges ()
            return! (withSuccessMessage "Request marked as prayed" >=> Components.journalItems) next ctx
        | None -> return! Error.notFound next ctx
    }
  
    /// POST /request/[req-id]/note
    let addNote requestId : HttpHandler = requireUser >=> fun next ctx -> task {
        let db     = ctx.Db
        let userId = ctx.UserId
        let reqId  = RequestId.ofString requestId
        match! Data.tryRequestById reqId userId db with
        | Some _ ->
            let! notes = ctx.BindFormAsync<Models.NoteEntry> ()
            do! Data.addNote reqId userId { AsOf = ctx.Now (); Notes = notes.notes } db
            do! db.SaveChanges ()
            return! (withSuccessMessage "Added Notes" >=> hideModal "notes" >=> created) next ctx
        | None -> return! Error.notFound next ctx
    }
          
    // GET /requests/active
    let active : HttpHandler = requireUser >=> fun next ctx -> task {
        let! reqs = Data.journalByUserId ctx.UserId ctx.Db
        return! partial "Active Requests" (Views.Request.active (ctx.Now ()) ctx.TimeZone reqs) next ctx
    }
  
    // GET /requests/snoozed
    let snoozed : HttpHandler = requireUser >=> fun next ctx -> task {
        let! reqs    = Data.journalByUserId ctx.UserId ctx.Db
        let  now     = ctx.Now ()
        let  snoozed = reqs
                       |> List.filter (fun it -> defaultArg (it.SnoozedUntil |> Option.map (fun it -> it > now)) false)
        return! partial "Snoozed Requests" (Views.Request.snoozed now ctx.TimeZone snoozed) next ctx
    }

    // GET /requests/answered
    let answered : HttpHandler = requireUser >=> fun next ctx -> task {
        let! reqs = Data.answeredRequests ctx.UserId ctx.Db
        return! partial "Answered Requests" (Views.Request.answered (ctx.Now ()) ctx.TimeZone reqs) next ctx
    }
  
    // GET /request/[req-id]/full
    let getFull requestId : HttpHandler = requireUser >=> fun next ctx -> task {
        match! Data.tryFullRequestById (RequestId.ofString requestId) ctx.UserId ctx.Db with
        | Some req -> return! partial "Prayer Request" (Views.Request.full ctx.Clock ctx.TimeZone req) next ctx
        | None     -> return! Error.notFound next ctx
    }
  
    // PATCH /request/[req-id]/show
    let show requestId : HttpHandler = requireUser >=> fun next ctx -> task {
        let db     = ctx.Db
        let userId = ctx.UserId
        let reqId  = RequestId.ofString requestId
        match! Data.tryRequestById reqId userId db with
        | Some _ ->
            do! Data.updateShowAfter reqId userId None db
            do! db.SaveChanges ()
            return! (withSuccessMessage "Request now shown" >=> Components.requestItem requestId) next ctx
        | None -> return! Error.notFound next ctx
    }
  
    // PATCH /request/[req-id]/snooze
    let snooze requestId : HttpHandler = requireUser >=> fun next ctx -> task {
        let db     = ctx.Db
        let userId = ctx.UserId
        let reqId  = RequestId.ofString requestId
        match! Data.tryRequestById reqId userId db with
        | Some _ ->
            let! until = ctx.BindFormAsync<Models.SnoozeUntil> ()
            let date =
                LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd").Parse(until.until).Value
                    .AtStartOfDayInZone(DateTimeZone.Utc)
                    .ToInstant ()
            do! Data.updateSnoozed reqId userId (Some date) db
            do! db.SaveChanges ()
            return!
                (withSuccessMessage $"Request snoozed until {until.until}"
                 >=> hideModal "snooze"
                 >=> Components.journalItems) next ctx
        | None -> return! Error.notFound next ctx
    }
  
    // PATCH /request/[req-id]/cancel-snooze
    let cancelSnooze requestId : HttpHandler = requireUser >=> fun next ctx -> task {
        let db     = ctx.Db
        let userId = ctx.UserId
        let reqId  = RequestId.ofString requestId
        match! Data.tryRequestById reqId userId db with
        | Some _ ->
            do! Data.updateSnoozed reqId userId None db
            do! db.SaveChanges ()
            return! (withSuccessMessage "Request unsnoozed" >=> Components.requestItem requestId) next ctx
        | None -> return! Error.notFound next ctx
    }

    /// Derive a recurrence from its representation in the form
    let private parseRecurrence (form : Models.Request) =
        match form.recurInterval with Some x -> $"{defaultArg form.recurCount 0s} {x}" | None -> "Immediate"
        |> Recurrence.ofString

    // POST /request
    let add : HttpHandler = requireUser >=> fun next ctx -> task {
        let! form   = ctx.BindModelAsync<Models.Request> ()
        let  db     = ctx.Db
        let  userId = ctx.UserId
        let  now    = ctx.Now ()
        let  req    =
            { Request.empty with
                UserId     = userId
                EnteredOn  = now
                ShowAfter  = None
                Recurrence = parseRecurrence form
                History    = [|
                    {   AsOf   = now
                        Status = Created
                        Text   = Some form.requestText
                    }      
                |]
            }
        Data.addRequest req db
        do! db.SaveChanges ()
        Messages.pushSuccess ctx "Added prayer request" "/journal"
        return! seeOther "/journal" next ctx
    }
  
    // PATCH /request
    let update : HttpHandler = requireUser >=> fun next ctx -> task {
        let! form   = ctx.BindModelAsync<Models.Request> ()
        let  db     = ctx.Db
        let  userId = ctx.UserId
        match! Data.tryJournalById (RequestId.ofString form.requestId) userId db with
        | Some req ->
            // update recurrence if changed
            let recur = parseRecurrence form
            match recur = req.Recurrence with
            | true  -> ()
            | false ->
                do! Data.updateRecurrence req.RequestId userId recur db
                match recur with
                | Immediate -> do! Data.updateShowAfter req.RequestId userId None db
                | _         -> ()
            // append history
            let upd8Text = form.requestText.Trim ()
            let text     = if upd8Text = req.Text then None else Some upd8Text
            do! Data.addHistory req.RequestId userId
                    { AsOf = ctx.Now (); Status = (Option.get >> RequestAction.ofString) form.status; Text = text } db
            do! db.SaveChanges ()
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

    open Microsoft.AspNetCore.Authentication
    open Microsoft.AspNetCore.Authentication.Cookies

    // GET /user/log-on
    let logOn : HttpHandler =
        logOn (Some "/journal")
  
    // GET /user/log-off
    let logOff : HttpHandler = requireUser >=> fun next ctx -> task {
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
