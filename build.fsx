#r "tools/FAKE/tools/FakeLib.dll"
open Fake 
open System
open Fake.AssemblyInfoFile

let artifactsDir = "./artifacts/"
let publishBuildDir = artifactsDir + "publish-build/"

let buildNumber = environVarOrDefault "BUILD_BUILDNUMBER" "0.2.13"

let storageUrl = "https://github.com/rasmuskl/Catapult"

setBuildParam "MSBuild" "C:\\Program Files\\Microsoft Visual Studio\\2022\\Enterprise\\MSBuild\\Current\\Bin"

Target "Clean" (fun _ ->
    CleanDirs [ artifactsDir ]
)

Target "Version" (fun _ ->
    CreateCSharpAssemblyInfo "./Source/Catapult.App/Properties/AssemblyInfo.cs"
         [Attribute.Title "Catapult"
          Attribute.Description "Catapult Launcher"
          Attribute.Guid "1c2e3e46-7e39-4c03-8816-ea2791fa184d"
          Attribute.Product "Catapult"
          Attribute.Version buildNumber
          Attribute.FileVersion buildNumber
          Attribute.Metadata("SquirrelAwareVersion", "1")]
    CreateCSharpAssemblyInfo "./Source/Catapult.Core/Properties/AssemblyInfo.cs"
         [Attribute.Title "Catapult.Core"
          Attribute.Guid "a33d57d2-0a7f-4901-beab-a92382a033eb"
          Attribute.Product "Catapult"
          Attribute.Version buildNumber
          Attribute.FileVersion buildNumber]
)

Target "Build" (fun _ ->
    MSBuildReleaseExt publishBuildDir List.empty "Build" ["Source/Catapult.sln"]
        |> Log "AppBuild-Output: "
)

Target "Package" (fun _ ->
    CreateDir (artifactsDir + "package/")
    NuGetPackDirectly (fun p -> 
        {p with 
            Version = buildNumber
            OutputPath = artifactsDir + "package/"
            WorkingDir = artifactsDir + "package/"}) 
        "./Catapult.nuspec"
)

Target "SyncReleases" (fun _ ->
    let result = ExecProcess (fun x -> 
        x.FileName <- "tools/squirrel.windows/tools/SyncReleases.exe"
        x.Arguments <- "-u " + storageUrl + " -r " + "./artifacts/releases") (TimeSpan.FromMinutes 5.0)
    if result <> 0 then failwith "Failed result from SyncReleases"
)

Target "Releasify" (fun _ ->
    let result = ExecProcess (fun x -> 
        x.FileName <- "tools/squirrel.windows/tools/squirrel.com"
        x.Arguments <- "--releasify=" + "./artifacts/package/Catapult."+buildNumber+".nupkg" + " -releaseDir=" + "./artifacts/releases") (TimeSpan.FromMinutes 5.0)
    if result <> 0 then failwith "Failed result from squirrel --releasify"
)


"Clean"
==> "Version"
==> "Build"
==> "Package"
==> "SyncReleases"
==> "Releasify"

Run "Releasify"