# Script for building and launching CoreView.App during development
$ErrorActionPreference = 'Stop'

Write-Host "Building CoreView.App..."

# Get the repository root directory (parent of scripts folder)
$repoRoot = Split-Path $PSScriptRoot -Parent
$buildPath = Join-Path $repoRoot "build"

# Stop any existing CoreView processes first
$existingProcess = Get-Process -Name "CoreView.App" -ErrorAction SilentlyContinue
if ($existingProcess) {
    Write-Host "Stopping existing CoreView.App process..."
    $existingProcess | Stop-Process -Force
    Start-Sleep -Seconds 1  # Give the process time to fully terminate
}

# Ensure build directory exists and is empty
if (Test-Path $buildPath) {
    Remove-Item -Path $buildPath -Recurse -Force
}
New-Item -ItemType Directory -Path $buildPath | Out-Null

try {
    # Publish the application from repository root
    Write-Host "Publishing CoreView.App..."
    & dotnet publish (Join-Path $repoRoot "CoreView.App") `
        --configuration Debug `
        --runtime win-x64 `
        --output $buildPath `
        --no-self-contained

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }

    # Launch the application
    $exePath = Join-Path $buildPath "CoreView.App.exe"
    if (Test-Path $exePath) {
        Write-Host "Launching CoreView.App..."
        Start-Process -FilePath $exePath
    }
    else {
        Write-Error "Could not find CoreView.App.exe in build output"
        exit 1
    }
}
catch {
    Write-Error "An error occurred: $_"
    exit 1
}