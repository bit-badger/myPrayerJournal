// Learn more about F# at http://fsharp.org

open System.IO
open Suave
open Suave.Filters
open Suave.Operators

let app : WebPart =
  choose [
    //GET >=> path "/" >=> Files.file "index.html"
    //GET >=> path "" >=> Files.file "index.html"
    GET >=> Files.browseHome
    GET >=> Files.browseFileHome "index.html"
    RequestErrors.NOT_FOUND "Page not found." 
  ]
[<EntryPoint>]
let main argv = 
  let config =
    { defaultConfig with homeFolder = Some (Path.GetFullPath "./wwwroot/") }

  startWebServer config app
  0 // return an integer exit code
