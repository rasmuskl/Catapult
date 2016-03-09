#r "tools/FAKE/tools/FakeLib.dll" // include Fake lib
open Fake 

let artifactsDir = "./artifacts/"
let publishBuildDir = artifactsDir + "publish-build/"
let publishUploadDir = FullName (artifactsDir) + "publish-upload/"

let now = System.DateTime.UtcNow
let buildNumber = sprintf "1.%i.%s.%s" now.Year (now.ToString("MMdd")) (now.ToString("HHmm"))

Target "Clean" (fun _ ->
    CleanDirs [ artifactsDir ]
)

Target "Build" (fun _ ->
    MSBuildReleaseExt null [("ApplicationVersion", buildNumber)] "Build" ["Source/AlphaLaunch.App.sln"]
        |> Log "AppBuild-Output: "
)

Target "Publish" (fun _ ->
    MSBuildReleaseExt publishBuildDir [("ApplicationVersion", buildNumber); ("PublishUrl", "http://alphalaunch.rasmuskl.dk/setup/"); ("PublishDir", publishUploadDir)] "Publish" ["Source/AlphaLaunch.App.sln"]
        |> Log "AppPublish-Output: "
)

let replacePublish = replace (FullName publishUploadDir) ""
let replaceBackslashes = replace "\\" "/"

let curlUpload(path:string) = (execProcess (fun info ->
    info.FileName <- "./tools/curl/curl.exe"
    info.Arguments <- "\"ftp://alphalaunch.rasmuskl.dk/Alphalaunch/setup/" +  (replaceBackslashes (replacePublish path)) + "\" --ssl --insecure --ftp-create-dirs --user " + (environVarOrFail "ALPHALAUNCH_FTP")+ " -T \"" + path + "\""
)(System.TimeSpan.FromMinutes 1.))

Target "Deploy" (fun _ ->
    !! "**\*.*" |> SetBaseDir publishUploadDir |> Seq.map curlUpload |> Seq.toArray |> ignore
)

"Clean"
==> "Build"
==> "Publish"
==> "Deploy"

Run "Deploy"