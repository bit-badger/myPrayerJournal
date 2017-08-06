#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open System

let buildDir = "./build/"

/// Path to the Vue app
let appPath = "src" @@ "app"

/// Path to the Suave API
let apiPath = "src" @@ "api"

// --- Targets ---

Target "Clean" (fun _ ->
  CleanDir buildDir
  CleanDir (apiPath @@ "wwwroot")
)

Target "BuildApp" (fun _ ->
  let result =
    ExecProcessAndReturnMessages (fun info ->
      info.UseShellExecute <- false
      info.FileName <- "build-vue.bat") (TimeSpan.FromMinutes 2.)
  match result.ExitCode with 0 -> Log "AppBuild-Output: " result.Messages | _ -> failwith "Vue build failed"
)

Target "BuildApi" (fun _ ->
  let result =
    ExecProcessAndReturnMessages (fun info ->
      info.UseShellExecute <- false
      info.FileName <- "dotnet"
      info.Arguments <- "build"
      info.WorkingDirectory <- apiPath) (TimeSpan.FromMinutes 2.)
  Log "AppBuild-Output: " result.Messages
  match result.ExitCode with 0 -> () | _ -> failwith "API build failed"
)

Target "Publish" (fun _ ->
  ExecProcess (fun info ->
    info.FileName <- "dotnet"
    info.Arguments <- """publish -o ..\..\build"""
    info.WorkingDirectory <- apiPath) TimeSpan.MaxValue
  |> ignore
)

Target "Run" (fun _ ->
  ExecProcess (fun info ->
    info.FileName <- "dotnet"
    info.Arguments <- "myPrayerJournal.dll"
    info.WorkingDirectory <- buildDir) TimeSpan.MaxValue
  |> ignore
)

Target "Default" (fun _ ->
  Log "" Seq.empty
)

// --- Dependencies ---

"Clean"
  ==> "BuildApp"

"BuildApp"
  ==> "BuildApi"

"BuildApi"
  ==> "Publish"

"Publish"
  ==> "Run"

"BuildApi" 
  ==> "Default"


RunTargetOrDefault "Default"