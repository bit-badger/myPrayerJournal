namespace MyPrayerJournal.Api

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting


/// Configuration functions for the application
module Configure =
  
  open Microsoft.AspNetCore.Authentication.JwtBearer
  open Microsoft.AspNetCore.Server.Kestrel.Core
  open Microsoft.Extensions.Configuration
  open Microsoft.Extensions.DependencyInjection
  open Microsoft.Extensions.Logging
  open Giraffe
  open Giraffe.TokenRouter

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
    sc.AddGiraffe () |> ignore
    // mad props to Andrea Chiarelli @ https://auth0.com/blog/securing-asp-dot-net-core-2-applications-with-jwts/
    use sp  = sc.BuildServiceProvider()
    let cfg = sp.GetRequiredService<IConfiguration>().GetSection "Auth0"
    sc.AddAuthentication(
      fun opts ->
        opts.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
        opts.DefaultChallengeScheme    <- JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer (
        fun opts ->
          opts.Authority <- sprintf "https://%s/" cfg.["Domain"]
          opts.Audience  <- cfg.["Audience"]
          opts.TokenValidationParameters.ValidateAudience <- false)
    |> ignore
    sc.AddAuthorization (fun opts -> opts.AddPolicy ("LoggedOn", fun p -> p.RequireClaim "sub" |> ignore))
    |> ignore
  
  /// Routes for the available URLs within myPrayerJournal
  let webApp =
    router Handlers.Error.notFound [
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
  
  open System
  open System.IO

  let exitCode = 0

  let CreateWebHostBuilder args =
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
