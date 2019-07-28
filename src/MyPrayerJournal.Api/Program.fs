namespace MyPrayerJournal.Api

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting

/// Configuration functions for the application
module Configure =
  
  open Microsoft.Extensions.Configuration
  open Newtonsoft.Json

  /// Set up the configuration for the app
  let configuration (ctx : WebHostBuilderContext) (cfg : IConfigurationBuilder) =
    cfg.SetBasePath(ctx.HostingEnvironment.ContentRootPath)
      .AddJsonFile("appsettings.json", optional = true, reloadOnChange = true)
      .AddJsonFile(sprintf "appsettings.%s.json" ctx.HostingEnvironment.EnvironmentName)
      .AddEnvironmentVariables ()
    |> ignore
    
  open Microsoft.AspNetCore.Server.Kestrel.Core

  /// Configure Kestrel from appsettings.json
  let kestrel (ctx : WebHostBuilderContext) (opts : KestrelServerOptions) =
    (ctx.Configuration.GetSection >> opts.Configure >> ignore) "Kestrel"

  open Giraffe.Serialization
  open Microsoft.FSharpLu.Json

  /// Custom settings for the JSON serializer (uses compact representation for options and DUs)
  let jsonSettings =
    let x = NewtonsoftJsonSerializer.DefaultSettings
    x.Converters.Add (CompactUnionJsonConverter (true))
    x.NullValueHandling     <- NullValueHandling.Ignore
    x.MissingMemberHandling <- MissingMemberHandling.Error
    x.Formatting            <- Formatting.Indented
    x

  open Giraffe
  open Giraffe.TokenRouter
  open Microsoft.AspNetCore.Authentication.JwtBearer
  open Microsoft.Extensions.DependencyInjection
  open MyPrayerJournal
  open Raven.Client.Documents
  open Raven.Client.Documents.Indexes
  open System.Security.Cryptography.X509Certificates

  /// Configure dependency injection
  let services (sc : IServiceCollection) =
    use sp  = sc.BuildServiceProvider ()
    let cfg = sp.GetRequiredService<IConfiguration> ()
    sc.AddGiraffe()
      .AddAuthentication(
        /// Use HTTP "Bearer" authentication with JWTs
        fun opts ->
          opts.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
          opts.DefaultChallengeScheme    <- JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(
        /// Configure JWT options with Auth0 options from configuration
        fun opts ->
          let jwtCfg = cfg.GetSection "Auth0"
          opts.Authority <- sprintf "https://%s/" jwtCfg.["Domain"]
          opts.Audience  <- jwtCfg.["Id"])
    |> ignore
    sc.AddSingleton<IJsonSerializer> (NewtonsoftJsonSerializer jsonSettings)
    |> ignore
    let config = sc.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection "RavenDB"
    let store = new DocumentStore ()
    store.Urls        <- [| config.["URLs"] |]
    store.Database    <- config.["Database"]
    store.Certificate <- new X509Certificate2 (config.["Certificate"], config.["Password"])
    store.Conventions.CustomizeJsonSerializer <- (fun x ->
        x.Converters.Add (RequestIdJsonConverter ())
        x.Converters.Add (TicksJsonConverter ())
        x.Converters.Add (UserIdJsonConverter ())
        x.Converters.Add (CompactUnionJsonConverter true))
    store.Initialize () |> sc.AddSingleton |> ignore
    IndexCreation.CreateIndexes (typeof<Requests_ByUserId>.Assembly, store)

  
  /// Routes for the available URLs within myPrayerJournal
  let webApp =
    router Handlers.Error.notFound [
      route "/" Handlers.Vue.app
      subRoute "/api/" [
        GET [
          route    "journal" Handlers.Journal.journal
          subRoute "request" [
            route  "s/answered" Handlers.Request.answered
            routef "/%s/full"   Handlers.Request.getFull
            routef "/%s/notes"  Handlers.Request.getNotes
            routef "/%s"        Handlers.Request.get
            ]
          ]
        PATCH [
          subRoute "request" [
            routef "/%s/recurrence" Handlers.Request.updateRecurrence
            routef "/%s/show"       Handlers.Request.show
            routef "/%s/snooze"     Handlers.Request.snooze
            ]
          ]
        POST [
          subRoute "request" [
            route  ""            Handlers.Request.add
            routef "/%s/history" Handlers.Request.addHistory
            routef "/%s/note"    Handlers.Request.addNote
            ]
          ]
        ]
      ]

  /// Configure the web application
  let application (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment> ()
    match env.IsDevelopment () with
    | true -> app.UseDeveloperExceptionPage ()
    | false -> app.UseGiraffeErrorHandler Handlers.Error.error
    |> function
    | a ->
        a.UseAuthentication()
          .UseStaticFiles()
          .UseGiraffe webApp
    |> ignore

  open Microsoft.Extensions.Logging

  /// Configure logging
  let logging (log : ILoggingBuilder) =
    let env = log.Services.BuildServiceProvider().GetService<IHostingEnvironment> ()
    match env.IsDevelopment () with
    | true -> log
    | false -> log.AddFilter(fun l -> l > LogLevel.Information)
    |> function l -> l.AddConsole().AddDebug()
    |> ignore


module Program =
  
  open System
  open System.IO

  let exitCode = 0

  let CreateWebHostBuilder _ =
    let contentRoot = Directory.GetCurrentDirectory ()
    WebHostBuilder()
      .UseContentRoot(contentRoot)
      .ConfigureAppConfiguration(Configure.configuration)
      .UseKestrel(Configure.kestrel)
      .UseWebRoot(Path.Combine (contentRoot, "wwwroot"))
      .ConfigureServices(Configure.services)
      .ConfigureLogging(Configure.logging)
      .Configure(Action<IApplicationBuilder> Configure.application)

  [<EntryPoint>]
  let main args =
    CreateWebHostBuilder(args).Build().Run()
    exitCode
