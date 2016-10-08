module MyPrayerJournal.App

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Localization
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options
open RethinkDB.DistributedCache
open System
open System.IO

/// Startup class for myPrayerJournal
type Startup(env : IHostingEnvironment) =
  
  /// Configuration for this application
  member this.Configuration =
    let builder =
      ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional = true, reloadOnChange = true)
        .AddJsonFile(sprintf "appsettings.%s.json" env.EnvironmentName, optional = true)
    // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
    match env.IsDevelopment () with true -> ignore <| builder.AddUserSecrets () | _ -> ()
    ignore <| builder.AddEnvironmentVariables ()
    builder.Build ()

  // This method gets called by the runtime. Use this method to add services to the container.
  member this.ConfigureServices (services : IServiceCollection) =
    services.AddOptions () |> ignore
    services.Configure<AppConfig> (this.Configuration.GetSection "MyPrayerJournal") |> ignore
    services.AddLocalization (fun opt -> opt.ResourcesPath <- "Resources") |> ignore
    services.AddMvc () |> ignore
    //ignore <| services.AddDistributedMemoryCache ()
    // RethinkDB connection
    async {
      let cfg = services.BuildServiceProvider().GetService<IOptions<AppConfig>>().Value
      let! conn = DataConfig.Connect cfg.DataConfig
      do! conn.EstablishEnvironment cfg
      services.AddSingleton conn |> ignore
      services.AddDistributedRethinkDBCache (fun options ->
        options.Database  <- match cfg.DataConfig.Database with null -> "" | db -> db
        options.TableName <- "Session") |> ignore
      services.AddSession () |> ignore
    } |> Async.RunSynchronously

  // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
  member this.Configure (app : IApplicationBuilder, env : IHostingEnvironment, loggerFactory : ILoggerFactory) =
    loggerFactory.AddConsole(this.Configuration.GetSection "Logging") |> ignore
    loggerFactory.AddDebug () |> ignore

    match env.IsDevelopment () with
    | true -> app.UseDeveloperExceptionPage () |> ignore
              app.UseBrowserLink () |> ignore
    | _ -> app.UseExceptionHandler "/error" |> ignore

    app.UseStaticFiles () |> ignore

    app.UseCookieAuthentication(
      CookieAuthenticationOptions(
        AuthenticationScheme  = Keys.Authentication,
        LoginPath             = PathString "/user/log-on",
        AutomaticAuthenticate = true,
        AutomaticChallenge    = true,
        ExpireTimeSpan        = TimeSpan (2, 0, 0),
        SlidingExpiration     = true)) |> ignore
    app.UseMvc(fun routes ->
      routes.MapRoute(name = "default", template = "{controller=Home}/{action=Index}/{id?}") |> ignore) |> ignore

/// Default to Development environment
let defaults = seq { yield WebHostDefaults.EnvironmentKey, "Development" }
               |> dict

[<EntryPoint>]
let main argv =
  let cfg =
    ConfigurationBuilder()
      .AddInMemoryCollection(defaults)
      .AddEnvironmentVariables("ASPNETCORE_")
      .AddCommandLine(argv)
      .Build()
  use host =
     WebHostBuilder()
      .UseConfiguration(cfg)
      .UseKestrel()
      .UseContentRoot(Directory.GetCurrentDirectory())
      .UseStartup<Startup>()
      .Build()
  host.Run()
  0