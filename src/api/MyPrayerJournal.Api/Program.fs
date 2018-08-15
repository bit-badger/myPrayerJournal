namespace MyPrayerJournal.Api

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open System


/// Configuration functions for the application
module Configure =
  
  open Giraffe
  open Giraffe.TokenRouter
  open Microsoft.AspNetCore.Authentication.JwtBearer
  open Microsoft.AspNetCore.Server.Kestrel.Core
  open Microsoft.EntityFrameworkCore
  open Microsoft.Extensions.Configuration
  open Microsoft.Extensions.DependencyInjection
  open Microsoft.Extensions.Logging
  open MyPrayerJournal

  /// Set up the configuration for the app
  let configuration (ctx : WebHostBuilderContext) (cfg : IConfigurationBuilder) =
    cfg.SetBasePath(ctx.HostingEnvironment.ContentRootPath)
      .AddJsonFile("appsettings.json", optional = true, reloadOnChange = true)
      .AddJsonFile(sprintf "appsettings.%s.json" ctx.HostingEnvironment.EnvironmentName)
      .AddEnvironmentVariables()
    |> ignore
    
  /// Configure Kestrel from appsettings.json
  let kestrel (ctx : WebHostBuilderContext) (opts : KestrelServerOptions) =
    (ctx.Configuration.GetSection >> opts.Configure >> ignore) "Kestrel"

  /// Configure dependency injection
  let services (sc : IServiceCollection) =
    use sp  = sc.BuildServiceProvider()
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
    sc.AddDbContext<AppDbContext>(fun opts -> opts.UseNpgsql(cfg.GetConnectionString "mpj") |> ignore)
    |> ignore
  
  /// Routes for the available URLs within myPrayerJournal
  let webApp =
    router Handlers.Error.notFound [
      route "/" Handlers.Vue.app
      subRoute "/api/" [
        GET [
          route    "journal" Handlers.Journal.journal
          subRoute "request" [
            route  "s/answered"   Handlers.Request.answered
            routef "/%s/complete" Handlers.Request.getComplete
            routef "/%s/full"     Handlers.Request.getFull
            routef "/%s/notes"    Handlers.Request.getNotes
            routef "/%s"          Handlers.Request.get
            ]
          ]
        POST [
          subRoute "request" [
            route  ""            Handlers.Request.add
            routef "/%s/history" Handlers.Request.addHistory
            routef "/%s/note"    Handlers.Request.addNote
            routef "/%s/snooze"  Handlers.Request.snooze
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

  /// Configure logging
  let logging (log : ILoggingBuilder) =
    let env = log.Services.BuildServiceProvider().GetService<IHostingEnvironment> ()
    match env.IsDevelopment () with
    | true -> log
    | false -> log.AddFilter(fun l -> l > LogLevel.Information)
    |> function l -> l.AddConsole().AddDebug()
    |> ignore


module Program =
  
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
