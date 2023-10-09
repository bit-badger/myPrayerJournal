module MyPrayerJournal.Api

open Microsoft.AspNetCore.Http

let sameSite (opts : CookieOptions) =
    match opts.SameSite, opts.Secure with
    | SameSiteMode.None, false -> opts.SameSite <- SameSiteMode.Unspecified
    | _, _ -> ()

open Giraffe
open Giraffe.EndpointRouting
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication.OpenIdConnect
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.HttpOverrides
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open MyPrayerJournal.Data
open NodaTime
open System
open System.Text.Json
open System.Threading.Tasks

[<EntryPoint>]
let main args =
    //use host = Configure.webHost [| "wwwroot" |] (Directory.GetCurrentDirectory ())
    //host.Run ()
    let builder = WebApplication.CreateBuilder args
    let _       = builder.Configuration.AddEnvironmentVariables "MPJ_"
    let svc     = builder.Services
    let cfg     = svc.BuildServiceProvider().GetRequiredService<IConfiguration> ()

    let _ = svc.AddRouting ()
    let _ = svc.AddGiraffe ()
    let _ = svc.AddSingleton<IClock> SystemClock.Instance
    let _ = svc.AddSingleton<IDateTimeZoneProvider> DateTimeZoneProviders.Tzdb
    let _ = svc.Configure<ForwardedHeadersOptions>(fun (opts : ForwardedHeadersOptions) ->
                opts.ForwardedHeaders <- ForwardedHeaders.XForwardedFor ||| ForwardedHeaders.XForwardedProto)
    
    let _ =
        svc.Configure<CookiePolicyOptions>(fun (opts : CookiePolicyOptions) ->
            opts.MinimumSameSitePolicy <- SameSiteMode.Unspecified
            opts.OnAppendCookie        <- fun ctx -> sameSite ctx.CookieOptions
            opts.OnDeleteCookie        <- fun ctx -> sameSite ctx.CookieOptions)
    let _ =
        svc.AddAuthentication(fun opts ->
            opts.DefaultAuthenticateScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            opts.DefaultSignInScheme       <- CookieAuthenticationDefaults.AuthenticationScheme
            opts.DefaultChallengeScheme    <- CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie()
            .AddOpenIdConnect("Auth0", fun opts ->
                // Configure OIDC with Auth0 options from configuration
                let auth0 = cfg.GetSection "Auth0"
                opts.Authority    <- $"""https://{auth0["Domain"]}/"""
                opts.ClientId     <- auth0["Id"]
                opts.ClientSecret <- auth0["Secret"]
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
                                    $"{request.Scheme}://{request.Host.Value}{request.PathBase.Value}{redirUri}"
                                | false -> redirUri
                            Uri.EscapeDataString $"&returnTo={finalRedirUri}"
                    ctx.Response.Redirect $"""https://{auth0["Domain"]}/v2/logout?client_id={auth0["Id"]}{returnTo}"""
                    ctx.HandleResponse ()
                    Task.CompletedTask
                opts.Events.OnRedirectToIdentityProvider <- fun ctx ->
                    let uri = UriBuilder ctx.ProtocolMessage.RedirectUri
                    uri.Scheme <- auth0["Scheme"]
                    uri.Port   <- int auth0["Port"]
                    ctx.ProtocolMessage.RedirectUri <- string uri
                    Task.CompletedTask)
    
    let _ = svc.AddSingleton<JsonSerializerOptions> Json.options
    let _ = svc.AddSingleton<Json.ISerializer> (SystemTextJson.Serializer Json.options)
    let _ = Connection.setUp cfg |> Async.AwaitTask |> Async.RunSynchronously
    
    if builder.Environment.IsDevelopment () then builder.Logging.AddFilter (fun l -> l > LogLevel.Information) |> ignore
    let _ = builder.Logging.AddConsole().AddDebug() |> ignore

    use app = builder.Build ()
    let _ = app.UseStaticFiles ()
    let _ = app.UseCookiePolicy ()
    let _ = app.UseRouting ()
    let _ = app.UseAuthentication ()
    let _ = app.UseGiraffeErrorHandler Handlers.Error.error
    let _ = app.UseEndpoints (fun e -> e.MapGiraffeEndpoints Handlers.routes)
    
    app.Run ()

    0
