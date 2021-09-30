module MyPrayerJournal.Api

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open System.IO

/// Configuration functions for the application
module Configure =
  
  /// Configure the content root
  let contentRoot root =
    WebApplicationOptions (ContentRootPath = root) |> WebApplication.CreateBuilder


  open Microsoft.Extensions.Configuration

  /// Configure the application configuration
  let appConfiguration (bldr : WebApplicationBuilder) =
    bldr.Configuration
      .SetBasePath(bldr.Environment.ContentRootPath)
      .AddJsonFile("appsettings.json", optional = false, reloadOnChange = true)
      .AddJsonFile($"appsettings.{bldr.Environment.EnvironmentName}.json", optional = true, reloadOnChange = true)
      .AddEnvironmentVariables ()
    |> ignore
    bldr


  open Microsoft.AspNetCore.Server.Kestrel.Core

  /// Configure Kestrel from appsettings.json
  let kestrel (bldr : WebApplicationBuilder) =
    let kestrelOpts (ctx : WebHostBuilderContext) (opts : KestrelServerOptions) =
      (ctx.Configuration.GetSection >> opts.Configure >> ignore) "Kestrel"
    bldr.WebHost.UseKestrel().ConfigureKestrel kestrelOpts |> ignore
    bldr


  /// Configure the web root directory
  let webRoot pathSegments (bldr : WebApplicationBuilder) =
    Array.concat [ [| bldr.Environment.ContentRootPath |]; pathSegments ]
    |> (Path.Combine >> bldr.WebHost.UseWebRoot >> ignore)
    bldr


  open Microsoft.Extensions.Logging
  open Microsoft.Extensions.Hosting

  /// Configure logging
  let logging (bldr : WebApplicationBuilder) =
    match bldr.Environment.IsDevelopment () with
    | true -> ()
    | false -> bldr.Logging.AddFilter (fun l -> l > LogLevel.Information) |> ignore
    bldr.Logging.AddConsole().AddDebug() |> ignore
    bldr


  open Giraffe
  open LiteDB
  open Microsoft.AspNetCore.Authentication.JwtBearer
  open Microsoft.Extensions.DependencyInjection
  open System.Text.Json
  open System.Text.Json.Serialization

  /// Configure dependency injection
  let services (bldr : WebApplicationBuilder) =
    bldr.Services
      .AddRouting()
      .AddGiraffe()
      .AddAuthentication(
        /// Use HTTP "Bearer" authentication with JWTs
        fun opts ->
          opts.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
          opts.DefaultChallengeScheme    <- JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(
        /// Configure JWT options with Auth0 options from configuration
        fun opts ->
          let jwtCfg = bldr.Configuration.GetSection "Auth0"
          opts.Authority <- sprintf "https://%s/" jwtCfg.["Domain"]
          opts.Audience  <- jwtCfg.["Id"])
    |> ignore
    let jsonOptions = JsonSerializerOptions ()
    jsonOptions.Converters.Add (JsonFSharpConverter ())
    let db = new LiteDatabase (bldr.Configuration.GetConnectionString "db")
    Data.Startup.ensureDb db
    bldr.Services.AddSingleton(jsonOptions)
      .AddSingleton<Json.ISerializer, SystemTextJson.Serializer>()
      .AddSingleton<LiteDatabase>(db)
    |> ignore
    bldr.Build ()
  

  open Giraffe.EndpointRouting

  /// Configure the web application
  let application (app : WebApplication) =
    match app.Environment.IsDevelopment () with
    | true -> app.UseDeveloperExceptionPage ()
    | false -> app.UseGiraffeErrorHandler Handlers.Error.error
    |> ignore
    app.UseAuthentication()
      .UseStaticFiles()
      .UseRouting()
      .UseEndpoints (fun e ->
          e.MapGiraffeEndpoints Handlers.routes
          e.MapFallbackToFile "index.html" |> ignore)
    |> ignore
    app

  /// Compose all the configurations into one
  let webHost pathSegments =
    contentRoot
    >> appConfiguration
    >> kestrel
    >> webRoot pathSegments
    >> logging
    >> services
    >> application


[<EntryPoint>]
let main _ =
  use host = Configure.webHost [| "wwwroot" |] (Directory.GetCurrentDirectory ())
  host.Run ()
  0
