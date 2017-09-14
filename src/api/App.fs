/// Main server module for myPrayerJournal
module MyPrayerJournal.App

open Microsoft.EntityFrameworkCore
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open System
open System.IO
open Suave
open Suave.Filters
open Suave.Operators

// --- Types ---

/// Auth0 settings
type Auth0Config = {
  /// The domain used with Auth0
  Domain : string
  /// The client Id
  ClientId : string
  /// The base64-encoded client secret
  ClientSecret : string
  /// The URL-safe base64-encoded client secret
  ClientSecretJwt : string
  }
with
  /// An empty set of Auth0 settings
  static member empty =
    { Domain = ""
      ClientId = ""
      ClientSecret = ""
      ClientSecretJwt = ""
      }

/// Application configuration
type AppConfig = {
  /// PostgreSQL connection string
  Conn : string
  /// Auth0 settings
  Auth0 : Auth0Config
  }
with
  static member empty =
    { Conn = ""
      Auth0 = Auth0Config.empty
      }

/// A JSON response as a data property
type JsonOkResponse<'a> = {
  data : 'a
  }

/// A JSON response indicating an error occurred
type JsonErrorResponse = {
  error : string
}


/// Configuration instances
module Config =
  
  /// Application configuration
  let app =
    try
      use sr = File.OpenText "appsettings.json"
      use tr = new JsonTextReader (sr)
      let settings = JToken.ReadFrom tr
      let secret = settings.["auth0"].["client-secret"].ToObject<string>()
      { Conn = settings.["conn"].ToObject<string>()
        Auth0 =
          { Domain = settings.["auth0"].["domain"].ToObject<string>()
            ClientId = settings.["auth0"].["client-id"].ToObject<string>()
            ClientSecret = secret
            ClientSecretJwt = secret.TrimEnd('=').Replace("-", "+").Replace("_", "/")
            }
        }
    with _ -> AppConfig.empty

  /// Custom Suave configuration
  let suave =
    { defaultConfig with
        homeFolder = Some (Path.GetFullPath "./wwwroot/")
        serverKey = Text.Encoding.UTF8.GetBytes("12345678901234567890123456789012")
        bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" 8084 ]
      }


/// Authorization functions
module Auth =

  /// Shorthand for Console.WriteLine
  let cw (x : string) = Console.WriteLine x

  /// Convert microtime to ticks, add difference from 1/1/1 to 1/1/1970
  let jsDate jsTicks =
    DateTime(jsTicks * 10000000L).AddTicks(DateTime(1970, 1, 1).Ticks)
  
  /// Get the user Id (sub) from a JSON Web Token
  let getIdFromToken jwt =
    try
      let payload = Jose.JWT.Decode<JObject>(jwt, Config.app.Auth0.ClientSecretJwt)
      let tokenExpires = jsDate (payload.["exp"].ToObject<int64>())
      match tokenExpires > DateTime.UtcNow with
      | true -> Some (payload.["sub"].ToObject<string>())
      | _ -> None
    with ex ->
      sprintf "Token Deserialization Exception - %s" (ex.GetType().FullName) |> cw
      sprintf "Message - %s" ex.Message |> cw
      ex.StackTrace |> cw
      None
  
  /// Add the logged on user Id to the context if it exists
  let loggedOn =
    warbler (fun ctx ->
      match ctx.request.header "Authorization" with
      | Choice1Of2 bearer -> Writers.setUserData "user" (getIdFromToken <| bearer.Split(' ').[1])
      | _ -> Writers.setUserData "user" None)


// --- Support ---

/// Get the scheme, host, and port of the URL
let schemeHostPort (req : HttpRequest) =
  sprintf "%s://%s" req.url.Scheme (req.headers |> List.filter (fun x -> fst x = "host") |> List.head |> snd)

/// Serialize an object to JSON
let toJson = JsonConvert.SerializeObject

/// Read an item from the user state, downcast to the expected type
let read ctx key : 'value =
  ctx.userState |> Map.tryFind key |> Option.map (fun x -> x :?> 'value) |> Option.get

/// Create a new data context
let dataCtx () =
  new DataContext (((DbContextOptionsBuilder<DataContext>()).UseNpgsql Config.app.Conn).Options)

/// Ensure the EF context is created in the right format
let ensureDatabase () =
  async {
    use data = dataCtx ()
    do! data.Database.MigrateAsync ()
    }
  |> Async.RunSynchronously


/// URL routes for myPrayerJournal
module Route =

  /// /api/journal ~ All active prayer requests for a user
  let journal = "/api/journal"

/// All WebParts that compose the public API
module WebParts =
  
  let jsonMimeType =
    warbler (fun ctx -> Writers.setMimeType "application/json; charset=utf8")

  /// WebPart to return a JSON response
  let JSON payload =
    jsonMimeType
    >=> Successful.OK (toJson { data = payload })

  /// WebPart to return an JSON error response
  let errorJSON code error =
    jsonMimeType
    >=> Response.response code ((toJson >> Text.Encoding.UTF8.GetBytes) { error = error })

  /// Journal page
  let viewJournal =
    context (fun ctx ->
      use dataCtx = dataCtx ()
      let reqs = Data.Requests.allForUser (defaultArg (read ctx "user") "") dataCtx
      JSON reqs)

  /// API-specific routes
  let apiRoutes =
    choose [
      GET >=> path Route.journal >=> viewJournal
      errorJSON HttpCode.HTTP_404 "Page not found"
      ]

  /// Suave application
  let app =
    Auth.loggedOn
    >=> choose [
          path "/api" >=> apiRoutes
          Files.browseHome
          Files.browseFileHome "index.html"
          ]

[<EntryPoint>]
let main argv = 
  ensureDatabase ()
  startWebServer Config.suave WebParts.app
  0 
