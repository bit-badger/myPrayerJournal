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
  open Microsoft.AspNetCore.Authentication.Cookies
  open Microsoft.AspNetCore.Authentication.OpenIdConnect
  open Microsoft.AspNetCore.Http
  open Microsoft.Extensions.DependencyInjection
  open Microsoft.IdentityModel.Protocols.OpenIdConnect
  open System
  open System.Text.Json
  open System.Text.Json.Serialization
  open System.Threading.Tasks

  /// Configure dependency injection
  let services (bldr : WebApplicationBuilder) =
    let sameSite (opts : CookieOptions) =
      match opts.SameSite, opts.Secure with
      | SameSiteMode.None, false -> opts.SameSite <- SameSiteMode.Unspecified
      | _, _ -> ()

    bldr.Services
      .AddRouting()
      .AddGiraffe()
      .Configure<CookiePolicyOptions>(
        fun (opts : CookiePolicyOptions) ->
          opts.MinimumSameSitePolicy <- SameSiteMode.Unspecified
          opts.OnAppendCookie        <- fun ctx -> sameSite ctx.CookieOptions
          opts.OnDeleteCookie        <- fun ctx -> sameSite ctx.CookieOptions)
      .AddAuthentication(
        /// Use HTTP "Bearer" authentication with JWTs
        fun opts ->
          opts.DefaultAuthenticateScheme <- CookieAuthenticationDefaults.AuthenticationScheme
          opts.DefaultSignInScheme       <- CookieAuthenticationDefaults.AuthenticationScheme
          opts.DefaultChallengeScheme    <- CookieAuthenticationDefaults.AuthenticationScheme)
      .AddCookie()
      .AddOpenIdConnect("Auth0",
        /// Configure OIDC with Auth0 options from configuration
        fun opts ->
          let cfg = bldr.Configuration.GetSection "Auth0"
          opts.Authority    <- sprintf "https://%s/" cfg.["Domain"]
          opts.ClientId     <- cfg.["Id"]
          opts.ClientSecret <- cfg.["Secret"]
          opts.ResponseType <- OpenIdConnectResponseType.Code
          
          opts.Scope.Clear ()
          opts.Scope.Add "openid"
          opts.Scope.Add "profile"

          opts.CallbackPath <- PathString "/user/log-on/success"
          opts.ClaimsIssuer <- "Auth0"
          opts.SaveTokens   <- true

          opts.Events <- OpenIdConnectEvents ()
          opts.Events.OnRedirectToIdentityProviderForSignOut <- fun ctx ->
            let returnTo =
              match ctx.Properties.RedirectUri with
              | it when isNull it || it = "" -> ""
              | redirUri ->
                  let finalRedirUri =
                    match redirUri.StartsWith "/" with
                    | true ->
                        // transform to absolute
                        let request = ctx.Request
                        sprintf "%s://%s%s%s" request.Scheme request.Host.Value request.PathBase.Value redirUri
                    | false -> redirUri
                  Uri.EscapeDataString finalRedirUri |> sprintf "&returnTo=%s"
            sprintf "https://%s/v2/logout?client_id=%s%s" cfg.["Domain"] cfg.["Id"] returnTo
            |> ctx.Response.Redirect
            ctx.HandleResponse ()

            Task.CompletedTask
          )
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
    // match app.Environment.IsDevelopment () with
    // | true -> app.UseDeveloperExceptionPage ()
    // | false -> app.UseGiraffeErrorHandler Handlers.Error.error
    // |> ignore
    app
      .UseStaticFiles()
      .UseCookiePolicy()
      .UseRouting()
      .UseAuthentication()
      .UseGiraffeErrorHandler(Handlers.Error.error)
      // .UseAuthorization()
      .UseEndpoints (fun e ->
          e.MapGiraffeEndpoints Handlers.routes
          // TODO: fallback to 404
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
