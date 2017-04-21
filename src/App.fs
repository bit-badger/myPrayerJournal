/// Main server module for myPrayerJournal
module MyPrayerJournal.App

open Auth0.AuthenticationApi
open Auth0.AuthenticationApi.Models
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open Reader
open System
open System.IO
open Suave
open Suave.Filters
open Suave.Operators
open Suave.RequestErrors
open Suave.State.CookieStateStore
open Suave.Successful

type Auth0Config = {
  Domain : string
  ClientId : string
  ClientSecret : string
}
with
  static member empty =
    { Domain = ""
      ClientId = ""
      ClientSecret = ""
      }

let auth0 =
  try
    use sr = File.OpenText "appsettings.json"
    let settings = JToken.ReadFrom(new JsonTextReader(sr)) :?> JObject
    { Domain = settings.["auth0"].["domain"].ToObject<string>()
      ClientId = settings.["auth0"].["client-id"].ToObject<string>()
      ClientSecret = settings.["auth0"].["client-secret"].ToObject<string>()
      }
  with _ -> Auth0Config.empty

/// Data Configuration singleton 
//let lazyCfg = lazy (DataConfig.FromJson <| try File.ReadAllText "data-config.json" with _ -> "{}")
/// RethinkDB connection singleton
//let lazyConn = lazy lazyCfg.Force().CreateConnection ()
/// Application dependencies
//let deps = {
//  new IDependencies with
//    member __.Conn with get () = lazyConn.Force ()
//  }

let auth code = context (fun ctx ->
    async {
      let client = AuthenticationApiClient(Uri(sprintf "https://%s" auth0.Domain))
      let! req =
        client.ExchangeCodeForAccessTokenAsync
          (ExchangeCodeRequest
            (AuthorizationCode = code,
            ClientId = auth0.ClientId,
            ClientSecret = auth0.ClientSecret,
            RedirectUri = "http://localhost:8080/user/log-on"))
      let! user = client.GetUserInfoAsync((req : AccessToken).AccessToken)
      return
        ctx
        |> HttpContext.state
        |> function
        | None -> FORBIDDEN "Cannot sign in without state"
        | Some state ->
            state.set "auth-token" req.IdToken
            >=> Writers.setUserData "user" user
      }
    |> Async.RunSynchronously
  )

let viewHome =
  Suave.Writers.setUserData "test" "howdy"
  >=> fun x -> OK (Views.page Views.home (string x.userState.["test"])) x

let handleSignIn =
  context (fun ctx ->
    GET
    >=> match ctx.request.queryParam "code" with
        | Choice1Of2 authCode ->
          auth authCode >=> OK (Views.page Views.home (Newtonsoft.Json.JsonConvert.SerializeObject(ctx.userState.["user"])))
        | Choice2Of2 msg -> BAD_REQUEST msg
  )

let session = statefulForSession

/// Suave application
let app =
  session
  >=> choose [
        path Route.home >=> viewHome
        path Route.User.logOn >=> handleSignIn
        Files.browseHome
        NOT_FOUND "Page not found." 
        ]

let suaveCfg = { defaultConfig with homeFolder = Some (Path.GetFullPath "./wwwroot/") }
  
[<EntryPoint>]
let main argv = 
  // Establish the data environment
  //liftDep getConn (Data.establishEnvironment >> Async.RunSynchronously)
  //|> run deps

  startWebServer suaveCfg app
  0 
