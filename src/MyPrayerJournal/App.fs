module MyPrayerJournal.App

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Nancy
open Nancy.Authentication.Forms
open Nancy.Bootstrapper
open Nancy.Cryptography
open Nancy.Owin
open Nancy.Security
open Nancy.Session.Persistable
open Nancy.Session.RethinkDB
open Nancy.TinyIoc
open Nancy.ViewEngines.SuperSimpleViewEngine
open NodaTime
open RethinkDb.Driver.Net
open System
open System.Reflection
open System.Security.Claims
open System.Text.RegularExpressions

/// Establish the configuration
let cfg = AppConfig.FromJson (System.IO.File.ReadAllText "config.json")

do
  cfg.DataConfig.Conn.EstablishEnvironment () |> Async.RunSynchronously

/// Support i18n/l10n via the @Translate SSVE alias
type TranslateTokenViewEngineMatcher() =
  static let regex = Regex("@Translate\.(?<TranslationKey>[a-zA-Z0-9-_]+);?", RegexOptions.Compiled)
  interface ISuperSimpleViewEngineMatcher with
    member this.Invoke (content, model, host) =
      let translate (m : Match) = Strings.get m.Groups.["TranslationKey"].Value
      regex.Replace(content, translate)

/// Handle forms authentication
type AppUser(name, claims) =
  inherit ClaimsPrincipal()
  member this.UserName with get() = name
  member this.Claims   with get() = claims
 
type AppUserMapper(container : TinyIoCContainer) =
  
  interface IUserMapper with
    member this.GetUserFromIdentifier (identifier, context) =
      match context.Request.PersistableSession.GetOrDefault(Keys.User, User.Empty) with
      | user when user.Id = string identifier -> upcast AppUser(user.Name, [ "LoggedIn" ])
      | _ -> null


/// Set up the application environment
type AppBootstrapper() =
  inherit DefaultNancyBootstrapper()
  
  override this.ConfigureRequestContainer (container, context) =
    base.ConfigureRequestContainer (container, context)
    /// User mapper for forms authentication
    ignore <| container.Register<IUserMapper, AppUserMapper>()

  override this.ConfigureApplicationContainer (container) =
    base.ConfigureApplicationContainer container
    ignore <| container.Register<AppConfig>(cfg)
    ignore <| container.Register<IConnection>(cfg.DataConfig.Conn)
    // NodaTime
    ignore <| container.Register<IClock>(SystemClock.Instance)
    // I18N in SSVE
    ignore <| container.Register<seq<ISuperSimpleViewEngineMatcher>>
                (fun _ _ -> 
                  Seq.singleton (TranslateTokenViewEngineMatcher() :> ISuperSimpleViewEngineMatcher))
  
  override this.ApplicationStartup (container, pipelines) =
    base.ApplicationStartup (container, pipelines)
    // Forms authentication configuration
    let auth =
      FormsAuthenticationConfiguration(
        CryptographyConfiguration =
          CryptographyConfiguration(
            AesEncryptionProvider(PassphraseKeyGenerator(cfg.AuthEncryptionPassphrase, cfg.AuthSalt)),
            DefaultHmacProvider(PassphraseKeyGenerator(cfg.AuthHmacPassphrase, cfg.AuthSalt))),
        RedirectUrl = "~/user/log-on",
        UserMapper  = container.Resolve<IUserMapper>())
    FormsAuthentication.Enable (pipelines, auth)
    // CSRF
    Csrf.Enable pipelines
    // Sessions
    let sessions = RethinkDBSessionConfiguration(cfg.DataConfig.Conn)
    sessions.Database <- cfg.DataConfig.Database
    PersistableSessions.Enable (pipelines, sessions)
    ()

  override this.Configure (environment) =
    base.Configure environment
    environment.Tracing(true, true)


let version = 
  let v = typeof<AppConfig>.GetType().GetTypeInfo().Assembly.GetName().Version
  match v.Build with
  | 0 -> match v.Minor with 0 -> string v.Major | _ -> sprintf "%d.%d" v.Major v.Minor
  | _ -> sprintf "%d.%d.%d" v.Major v.Minor v.Build
  |> sprintf "v%s"

/// Set up the request environment
type RequestEnvironment() =
  interface IRequestStartup with
    member this.Initialize (pipelines, context) =
      pipelines.BeforeRequest.AddItemToStartOfPipeline
        (fun ctx ->
          ctx.Items.[Keys.RequestStart] <- DateTime.Now.Ticks
          ctx.Items.[Keys.Version]      <- version
          null)

type Startup() =
  member this.Configure (app : IApplicationBuilder) =
    ignore <| app.UseOwin(fun x -> x.UseNancy(fun opt -> opt.Bootstrapper <- new AppBootstrapper()) |> ignore)

[<EntryPoint>]
let main argv = 
//  let app = OwinApp.ofMidFunc "/" (NancyMiddleware.UseNancy(fun opt -> opt.Bootstrapper <- new AppBootstrapper()))
//  startWebServer defaultConfig app
//  0 // return an integer exit code
  WebHostBuilder()
    .UseContentRoot(System.IO.Directory.GetCurrentDirectory())
    .UseKestrel()
    .UseStartup<Startup>()
    .Build()
    .Run()
  0