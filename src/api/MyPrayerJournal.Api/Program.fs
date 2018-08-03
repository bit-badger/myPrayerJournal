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

  /// Configure dependency injection
  let services (sc : IServiceCollection) =
    sc.AddAuthentication()
      .AddJwtBearer ("Auth0",
        fun opt ->
          opt.Audience <- "")
    |> ignore
    ()
  
  /// Response that will load the Vue application to handle the given URL
  let vueApp = fun next ctx -> htmlFile "/index.html" next ctx

  /// Routes for the available URLs within myPrayerJournal
  let webApp =
    router Handlers.notFound [
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
    let log = app.ApplicationServices.GetService<ILoggerFactory> ()
    match env.IsDevelopment () with
    | true ->
        app.UseDeveloperExceptionPage () |> ignore
    | false ->
        ()

    app.UseAuthentication()
      .UseStaticFiles()
      .UseGiraffe webApp
    |> ignore


module Program =

  let exitCode = 0

  let CreateWebHostBuilder args =
    WebHost
      .CreateDefaultBuilder(args)
      .ConfigureServices(Configure.services)
      .Configure(Action<IApplicationBuilder> Configure.application)

  [<EntryPoint>]
  let main args =
    CreateWebHostBuilder(args).Build().Run()

    exitCode
