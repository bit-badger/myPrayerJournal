module App

open MyPrayerJournal
open Nancy
open Nancy.Owin
open Suave.Web
open Suave.Owin
open System

/// Establish the configuration
let cfg = AppConfig.FromJson (System.IO.File.ReadAllText "config.json")

do
  cfg.DataConfig.Conn.EstablishEnvironment () |> Async.RunSynchronously

[<EntryPoint>]
let main argv = 
  let app = OwinApp.ofMidFunc "/" (NancyMiddleware.UseNancy(NancyOptions()))
  startWebServer defaultConfig app
  0 // return an integer exit code
