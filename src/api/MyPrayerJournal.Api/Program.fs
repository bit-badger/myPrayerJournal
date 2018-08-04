namespace MyPrayerJournal.Api

open System
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging

/// Configuration functions for the application
module Configure =
  
  open Microsoft.Extensions.DependencyInjection
  open Giraffe
  open Giraffe.TokenRouter
  open MyPrayerJournal

  /// Set up the configuration for the app
  let configuration (ctx : WebHostBuilderContext) (cfg : IConfigurationBuilder) =
    cfg.SetBasePath(ctx.HostingEnvironment.ContentRootPath)
      .AddJsonFile("appsettings.json", optional = true, reloadOnChange = true)
      .AddJsonFile(sprintf "appsettings.%s.json" ctx.HostingEnvironment.EnvironmentName, optional = true)
      .AddEnvironmentVariables()
    |> ignore
    
  /// Configure dependency injection
  let services (sc : IServiceCollection) =
    sc.AddAuthentication()
      .AddJwtBearer("Auth0",
        fun opt ->
          opt.Audience <- "")
    |> ignore
    ()
  
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
  
  open System.IO

  let exitCode = 0

  let CreateWebHostBuilder args =
    let contentRoot = Directory.GetCurrentDirectory ()
    WebHostBuilder()
      .UseKestrel()
      .UseContentRoot(contentRoot)
      .UseWebRoot(Path.Combine (contentRoot, "wwwroot"))
      .ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> Configure.configuration)
      .ConfigureServices(Configure.services)
      .ConfigureLogging(Configure.logging)
      .Configure(Action<IApplicationBuilder> Configure.application)

  [<EntryPoint>]
  let main args =
    CreateWebHostBuilder(args).Build().Run()
    exitCode
