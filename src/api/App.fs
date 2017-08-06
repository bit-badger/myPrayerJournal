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
type Config = {
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

// --- Support ---

/// Configuration instance
let cfg =
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
  with _ -> Config.empty

/// Get the scheme, host, and port of the URL
let schemeHostPort (req : HttpRequest) =
  sprintf "%s://%s" req.url.Scheme (req.headers |> List.filter (fun x -> fst x = "host") |> List.head |> snd)

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
      let payload = Jose.JWT.Decode<JObject>(jwt, cfg.Auth0.ClientSecretJwt)
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
      | Choice1Of2 bearer -> Writers.setUserData "user" ((bearer.Split(' ').[1]) |> getIdFromToken)
      | _ -> Writers.setUserData "user" None)


/// Serialize an object to JSON
let toJson = JsonConvert.SerializeObject

/// Read an item from the user state, downcast to the expected type
let read ctx key : 'value =
  ctx.userState |> Map.tryFind key |> Option.map (fun x -> x :?> 'value) |> Option.get

/// Create a new data context
let dataCtx () =
  new DataContext (((DbContextOptionsBuilder<DataContext>()).UseNpgsql cfg.Conn).Options)

/// Ensure the EF context is created in the right format
let ensureDatabase () =
  async {
    use data = dataCtx ()
    do! data.Database.MigrateAsync ()
    }
  |> Async.RunSynchronously

let suaveCfg =
  { defaultConfig with
      homeFolder = Some (Path.GetFullPath "./wwwroot/")
      serverKey = Text.Encoding.UTF8.GetBytes("12345678901234567890123456789012")
      bindings = [ HttpBinding.createSimple HTTP "127.0.0.1" 8084 ]
    }

// --- Routes ---

/// URL routes for myPrayerJournal
module Route =

  /// /api/journal ~ All active prayer requests for a user
  let journal = "/api/journal"


// --- WebParts ---

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
    >=> Writers.setStatus code
    >=> Response.response code ((toJson >> UTF8.bytes) { error = error })

  /// Journal page
  let viewJournal =
    context (fun ctx ->
      use dataCtx = dataCtx ()
      let reqs = Data.Requests.allForUser (defaultArg (read ctx "user") "") dataCtx
      JSON reqs)

  /// Suave application
  let app =
    Auth.loggedOn
    >=> choose [
          path Route.journal >=> viewJournal
          errorJSON HttpCode.HTTP_404 "Page not found"
          ]

[<EntryPoint>]
let main argv = 
  ensureDatabase ()
  startWebServer suaveCfg WebParts.app
  0 
