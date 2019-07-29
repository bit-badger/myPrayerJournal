namespace MyPrayerJournal.Api

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open System.IO

/// Configuration functions for the application
module Configure =
  
  /// Configure the content root
  let contentRoot root (bldr : IWebHostBuilder) =
    bldr.UseContentRoot root

  open Microsoft.Extensions.Configuration

  /// Configure the application configuration
  let appConfiguration (bldr : IWebHostBuilder) =
    let configuration (ctx : WebHostBuilderContext) (cfg : IConfigurationBuilder) =
      cfg.SetBasePath(ctx.HostingEnvironment.ContentRootPath)
        .AddJsonFile("appsettings.json", optional = true, reloadOnChange = true)
        .AddJsonFile(sprintf "appsettings.%s.json" ctx.HostingEnvironment.EnvironmentName)
        .AddEnvironmentVariables ()
      |> ignore
    bldr.ConfigureAppConfiguration configuration
    
  open Microsoft.AspNetCore.Server.Kestrel.Core

  /// Configure Kestrel from appsettings.json
  let kestrel (bldr : IWebHostBuilder) =
    let kestrel (ctx : WebHostBuilderContext) (opts : KestrelServerOptions) =
      (ctx.Configuration.GetSection >> opts.Configure >> ignore) "Kestrel"
    bldr.ConfigureKestrel kestrel

  /// Configure the web root directory
  let webRoot pathSegments (bldr : IWebHostBuilder) =
    (Path.Combine >> bldr.UseWebRoot) pathSegments

  open Giraffe
  open Giraffe.Serialization
  open Giraffe.TokenRouter
  open Microsoft.AspNetCore.Authentication.JwtBearer
  open Microsoft.Extensions.DependencyInjection
  open Microsoft.FSharpLu.Json
  open MyPrayerJournal
  open MyPrayerJournal.Indexes
  open Newtonsoft.Json
  open Raven.Client.Documents
  open Raven.Client.Documents.Indexes
  open System.Security.Cryptography.X509Certificates

  /// Configure dependency injection
  let services (bldr : IWebHostBuilder) =
    let svcs (sc : IServiceCollection) =
      /// A set of JSON converters used for both Giraffe's request serialization and RavenDB's storage
      let jsonConverters : JsonConverter seq =
        seq {
          yield! Converters.all
          yield CompactUnionJsonConverter true
        }
      /// Custom settings for the JSON serializer (uses compact representation for options and DUs)
      let jsonSettings =
        let x = NewtonsoftJsonSerializer.DefaultSettings
        jsonConverters |> List.ofSeq |> List.iter x.Converters.Add
        x.NullValueHandling     <- NullValueHandling.Ignore
        x.MissingMemberHandling <- MissingMemberHandling.Error
        x.Formatting            <- Formatting.Indented
        x

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
      store.Urls        <- [| config.["URL"] |]
      store.Database    <- config.["Database"]
      // store.Certificate <- new X509Certificate2 (config.["Certificate"], config.["Password"])
      store.Conventions.CustomizeJsonSerializer <- fun x -> jsonConverters |> List.ofSeq |> List.iter x.Converters.Add
      store.Initialize () |> (sc.AddSingleton >> ignore)
      IndexCreation.CreateIndexes (typeof<Requests_ByUserId>.Assembly, store)
    bldr.ConfigureServices svcs
  
  open Microsoft.Extensions.Logging

  /// Configure logging
  let logging (bldr : IWebHostBuilder) =
    let logz (log : ILoggingBuilder) =
      let env = log.Services.BuildServiceProvider().GetService<IHostingEnvironment> ()
      match env.IsDevelopment () with
      | true -> log
      | false -> log.AddFilter(fun l -> l > LogLevel.Information)
      |> function l -> l.AddConsole().AddDebug()
      |> ignore
    bldr.ConfigureLogging logz

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

  open System

  /// Configure the web application
  let application (bldr : IWebHostBuilder) =
    let appConfig =
      Action<IApplicationBuilder> (
        fun (app : IApplicationBuilder) ->
            let env = app.ApplicationServices.GetService<IHostingEnvironment> ()
            match env.IsDevelopment () with
            | true -> app.UseDeveloperExceptionPage ()
            | false -> app.UseGiraffeErrorHandler Handlers.Error.error
            |> function
            | a ->
                a.UseAuthentication()
                  .UseStaticFiles()
                  .UseGiraffe webApp
            |> ignore)
    bldr.Configure appConfig

  /// Compose all the configurations into one
  let webHost appRoot pathSegments =
    contentRoot appRoot
    >> appConfiguration
    >> kestrel
    >> webRoot (Array.concat [ [| appRoot |]; pathSegments ])
    >> services
    >> logging
    >> application

  /// Build the web host from the given configuration
  let buildHost (bldr : IWebHostBuilder) = bldr.Build ()

module Program =
  
  let exitCode = 0

  [<EntryPoint>]
  let main _ =
    let appRoot = Directory.GetCurrentDirectory ()
    use host = WebHostBuilder () |> (Configure.webHost appRoot [| "wwwroot" |] >> Configure.buildHost)
    host.Run ()
    exitCode
