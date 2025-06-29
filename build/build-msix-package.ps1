param(
    [string]$Workspace = "$PSScriptRoot"
)

dotnet restore ../src/Catapult.sln --runtime win-x64

msbuild ../src/Catapult/Catapult.wapproj `
    -p:Configuration=Release `
    -p:Platform=x64 `
    -p:AppxBundle=Always `
    -p:AppxBundlePlatforms="x64" `
    -p:AppxPackageDir="$Workspace/AppPackages/" `
    -p:GenerateAppInstallerFile=true `
    -p:AppInstallerUri="https://github.com/rasmuskl/Catapult/releases/latest/download/" `
    -p:AppInstallerCheckForUpdateFrequency=OnApplicationRun `
    -p:AppInstallerUpdateFrequency=1 `
    -p:GenerateTestArtifacts=false