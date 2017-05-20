#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open System

let buildDir = "./build/"

/// Path to the Aurelia app
let appPath = "src" @@ "app"

/// Path to the Suave API
let apiPath = "src" @@ "api"

// --- Targets ---

Target "Clean" (fun _ ->
  CleanDir buildDir
)

Target "BuildApp" (fun _ ->
  let result =
    ExecProcessAndReturnMessages (fun info ->
      info.UseShellExecute <- false
      info.FileName <- "." @@ "build-au.bat") (TimeSpan.FromMinutes 2.)
  match result.ExitCode with
  | 0 -> Log "AppBuild-Output: " result.Messages
  | _ -> failwith "Aurelia build failed"
)

Target "CopyApp" (fun _ ->
  let apiWebPath = apiPath @@ "wwwroot"
  [ "scripts" @@ "app-bundle.js"
    "scripts" @@ "vendor-bundle.js"
    "index.html"
    ]
  |> List.iter (fun file ->
      IO.File.Copy (appPath @@ file,  apiWebPath @@ file, true)
      Log "CopyApp--Output: " (Seq.singleton file))
)

Target "BuildApi" (fun _ ->
  !! "src/api/*.fsproj"
  |> MSBuildRelease buildDir "Build"
  |> Log "ApiBuild-Output: "
)

Target "Run" (fun _ ->
  ExecProcess (fun info ->
    info.FileName <- "dotnet"
    info.Arguments <- "myPrayerJournal.dll"
    info.WorkingDirectory <- "build") TimeSpan.MaxValue
  |> ignore
)

Target "Default" (fun _ ->
  Log "" Seq.empty
)

// --- Dependencies ---

"Clean"
  ==> "BuildApp"
  ==> "CopyApp"
  ==> "BuildApi"
  ==> "Default"

"BuildApi"
  ==> "Run"

RunTargetOrDefault "Default"