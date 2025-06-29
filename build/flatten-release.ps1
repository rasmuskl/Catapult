param(
    [Parameter(Mandatory=$true)]
    [string]$FolderPath
)

# Ensure folder path exists
if (-not (Test-Path $FolderPath)) {
    Write-Error "Folder path '$FolderPath' does not exist."
    exit 1
}

Write-Host "Flattening folder: $FolderPath"

# Get all files from subdirectories (excluding .appinstaller files)
$filesToMove = Get-ChildItem -Path $FolderPath -Recurse -File | Where-Object { 
    $_.Extension -ne '.appinstaller' -and $_.DirectoryName -ne $FolderPath 
}

# Move files to root directory
foreach ($file in $filesToMove) {
    $destinationPath = Join-Path $FolderPath $file.Name
    
    # Handle file name conflicts by adding a number suffix
    $counter = 1
    while (Test-Path $destinationPath) {
        $baseName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
        $extension = $file.Extension
        $destinationPath = Join-Path $FolderPath "$baseName($counter)$extension"
        $counter++
    }
    
    Write-Host "Moving: $($file.FullName) -> $destinationPath"
    Move-Item -Path $file.FullName -Destination $destinationPath
}

# Remove empty subdirectories
$emptyDirs = Get-ChildItem -Path $FolderPath -Directory -Recurse | Where-Object { 
    (Get-ChildItem -Path $_.FullName -Force | Measure-Object).Count -eq 0 
} | Sort-Object FullName -Descending

foreach ($dir in $emptyDirs) {
    Write-Host "Removing empty directory: $($dir.FullName)"
    Remove-Item -Path $dir.FullName -Force
}

# Update .appinstaller file to remove subfolder paths
$appInstallerPath = Get-ChildItem -Path $FolderPath -Filter "*.appinstaller" | Select-Object -First 1

if ($appInstallerPath) {
    Write-Host "Updating .appinstaller file: $($appInstallerPath.FullName)"
    
    $content = Get-Content -Path $appInstallerPath.FullName -Raw
    
    # Remove subfolder paths from URLs (e.g., "Catapult.Package_1.0.8.0_Test/" becomes "")
    $content = $content -replace '/[^/]+/([^/"]+\.(msixbundle|msix|cer))', '/$1'
    
    Set-Content -Path $appInstallerPath.FullName -Value $content -Encoding UTF8
    Write-Host "Updated .appinstaller file URLs"
}

Write-Host "Folder flattening complete!"