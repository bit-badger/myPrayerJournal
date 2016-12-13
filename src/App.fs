/// Main server module for myPrayerJournal
module MyPrayerJournal.App

open Reader
open System.IO
open Suave
open Suave.Filters
open Suave.Operators

/// Data Configuration singleton 
let lazyCfg = lazy (DataConfig.FromJson <| try File.ReadAllText "data-config.json" with _ -> "{}")
/// RethinkDB connection singleton
let lazyConn = lazy lazyCfg.Force().CreateConnection ()
/// Application dependencies
let deps = {
  new IDependencies with
    member __.Conn with get () = lazyConn.Force ()
  }

/// Suave application
let app : WebPart =
  choose [
    GET >=> Files.browseHome
    GET >=> Files.browseFileHome "index.html"
    RequestErrors.NOT_FOUND "Page not found." 
  ]
let suaveCfg = { defaultConfig with homeFolder = Some (Path.GetFullPath "./wwwroot/") }
  
[<EntryPoint>]
let main argv = 
  // Establish the data environment
  liftDep getConn (Data.establishEnvironment >> Async.RunSynchronously)
  |> run deps

  startWebServer suaveCfg app
  0 
