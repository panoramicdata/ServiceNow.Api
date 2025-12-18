<#
.SYNOPSIS
    Publishes the ServiceNow.Api package to NuGet.org

.DESCRIPTION
    This script performs the following steps:
    1. Checks for clean git working directory (porcelain)
    2. Determines the Nerdbank GitVersion
    3. Checks that nuget-key.txt exists, has content and is gitignored
    4. Runs unit tests (unless -SkipTests is specified)
    5. Publishes to nuget.org

.PARAMETER SkipTests
    Skip running unit tests

.EXAMPLE
    .\Publish.ps1
    .\Publish.ps1 -SkipTests
#>

param(
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'

# Step 1: Check for clean git working directory
Write-Host "Checking git status..." -ForegroundColor Cyan
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Error "Git working directory is not clean. Please commit or stash changes before publishing."
    exit 1
}
Write-Host "Git working directory is clean." -ForegroundColor Green

# Step 2: Determine the Nerdbank GitVersion
Write-Host "Determining version from Nerdbank.GitVersioning..." -ForegroundColor Cyan
$versionOutput = nbgv get-version --format json 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to get version from Nerdbank.GitVersioning. Ensure nbgv tool is installed: dotnet tool install -g nbgv"
    exit 1
}
$versionInfo = $versionOutput | ConvertFrom-Json
$version = $versionInfo.NuGetPackageVersion
if (-not $version) {
    Write-Error "Failed to determine NuGet package version."
    exit 1
}
Write-Host "Version: $version" -ForegroundColor Green

# Step 3: Check that nuget-key.txt exists, has content and is gitignored
Write-Host "Checking nuget-key.txt..." -ForegroundColor Cyan
$nugetKeyFile = "nuget-key.txt"
if (-not (Test-Path $nugetKeyFile)) {
    Write-Error "File '$nugetKeyFile' does not exist. Create this file with your NuGet API key."
    exit 1
}

$nugetApiKey = (Get-Content $nugetKeyFile -Raw).Trim()
if ([string]::IsNullOrWhiteSpace($nugetApiKey)) {
    Write-Error "File '$nugetKeyFile' is empty. Add your NuGet API key to this file."
    exit 1
}

# Check if the file is gitignored
$gitCheckIgnore = git check-ignore $nugetKeyFile 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "File '$nugetKeyFile' is not in .gitignore. This file should be ignored to prevent accidental commits of API keys."
    exit 1
}
Write-Host "nuget-key.txt exists, has content, and is gitignored." -ForegroundColor Green

# Step 4: Run unit tests (unless -SkipTests is specified)
if (-not $SkipTests) {
    Write-Host "Running unit tests..." -ForegroundColor Cyan
    dotnet test --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Unit tests failed."
        exit 1
    }
    Write-Host "Unit tests passed." -ForegroundColor Green
} else {
    Write-Host "Skipping unit tests." -ForegroundColor Yellow
}

# Step 5: Build and pack the project
Write-Host "Building and packing..." -ForegroundColor Cyan
dotnet build ServiceNow.Api\ServiceNow.Api.csproj --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed."
    exit 1
}
Write-Host "Build succeeded." -ForegroundColor Green

# Step 6: Publish to NuGet.org
Write-Host "Publishing to NuGet.org..." -ForegroundColor Cyan
$nupkgPath = "ServiceNow.Api\bin\Release\ServiceNow.Api.$version.nupkg"
if (-not (Test-Path $nupkgPath)) {
    Write-Error "NuGet package not found at: $nupkgPath"
    exit 1
}

dotnet nuget push $nupkgPath --api-key $nugetApiKey --source https://api.nuget.org/v3/index.json --skip-duplicate
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to publish to NuGet.org."
    exit 1
}

Write-Host "Successfully published ServiceNow.Api v$version to NuGet.org!" -ForegroundColor Green
exit 0
