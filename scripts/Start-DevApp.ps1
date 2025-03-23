# Script for building and launching CoreView.App during development
$ErrorActionPreference = 'Stop'

Write-Host "Building CoreView.App..."

# Ensure build directory exists and is empty
$buildPath = Join-Path $PSScriptRoot "build"
if (Test-Path $buildPath) {
    Remove-Item -Path $buildPath -Recurse -Force
}
New-Item -ItemType Directory -Path $buildPath | Out-Null

try {
    # Publish the application
    $publishResult = dotnet publish (Join-Path $PSScriptRoot "CoreView.App") `
        --configuration Debug `
        --runtime win-x64 `
        --output $buildPath `
        --no-self-contained `
        --verbosity minimal

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }

    # Launch the application
    $exePath = Join-Path $buildPath "CoreView.App.exe"
    if (Test-Path $exePath) {
        Write-Host "Launching CoreView.App..."
        Start-Process -FilePath $exePath
    } else {
        Write-Error "Could not find CoreView.App.exe in build output"
        exit 1
    }
} catch {
    Write-Error "An error occurred: $_"
    exit 1
}