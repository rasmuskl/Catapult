param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    [Parameter(Mandatory=$false)]
    [string]$AppInstallerPath = "GithubRelease\Catapult.appinstaller"
)

# Ensure the .appinstaller file exists
if (-not (Test-Path $AppInstallerPath)) {
    Write-Error "AppInstaller file not found at: $AppInstallerPath"
    exit 1
}

Write-Host "Updating .appinstaller file: $AppInstallerPath"
Write-Host "Setting version to: $Version"

# Read the current content
$content = Get-Content -Path $AppInstallerPath -Raw

# Replace "latest" with the specified version only in MainBundle Uri
$content = $content -replace '(<MainBundle[^>]*Uri="[^"]*)/latest/download/([^"]*")', "`$1/download/v$Version/`$2"

# Write the updated content back
Set-Content -Path $AppInstallerPath -Value $content -Encoding UTF8

Write-Host "Successfully updated MainBundle Uri to use version: $Version"