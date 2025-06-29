param(
    [string]$Workspace = "$PSScriptRoot"
)

$sourceDir = Join-Path $Workspace "AppPackages"
$destDir = Join-Path $PSScriptRoot "GithubRelease"

if (Test-Path $destDir) { Remove-Item $destDir -Recurse -Force }
New-Item -ItemType Directory -Path $destDir | Out-Null

Get-ChildItem -Path $sourceDir -Recurse -Include *.appinstaller, *.msixbundle, *.cer | ForEach-Object {
    $relativePath = $_.FullName.Substring($sourceDir.Length).TrimStart('\','/')
    $targetPath = Join-Path $destDir $relativePath
    $targetDir = Split-Path $targetPath -Parent
    if (!(Test-Path $targetDir)) { New-Item -ItemType Directory -Path $targetDir -Force | Out-Null }
    Copy-Item $_.FullName -Destination $targetPath
}

& "$PSScriptRoot\flatten-release.ps1" -Folder $destDir