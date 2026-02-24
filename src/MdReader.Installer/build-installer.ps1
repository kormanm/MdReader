# Build and Publish MdReader for Windows x64
# This script prepares the application for installation

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SelfContained = $true
)

Write-Host "Building MdReader..." -ForegroundColor Cyan

# Navigate to project directory
$projectPath = Join-Path $PSScriptRoot "..\MdReader\MdReader.csproj"
$publishPath = Join-Path $PSScriptRoot "..\MdReader\bin\$Configuration\net9.0-windows\$Runtime\publish"

# Clean previous publish
if (Test-Path $publishPath) {
    Write-Host "Cleaning previous publish..." -ForegroundColor Yellow
    Remove-Item -Path $publishPath -Recurse -Force
}

# Build parameters
$publishArgs = @(
    "publish"
    $projectPath
    "-c", $Configuration
    "-r", $Runtime
    "-p:PublishSingleFile=false"
    "-p:SelfContained=$($SelfContained.ToString().ToLower())"
    "-p:PublishReadyToRun=true"
    "-p:IncludeNativeLibrariesForSelfExtract=true"
)

Write-Host "Publishing application..." -ForegroundColor Cyan
Write-Host "Command: dotnet $($publishArgs -join ' ')" -ForegroundColor Gray

& dotnet @publishArgs

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild successful!" -ForegroundColor Green
    Write-Host "Published to: $publishPath" -ForegroundColor Green
    
    # List published files
    Write-Host "`nPublished files:" -ForegroundColor Cyan
    Get-ChildItem $publishPath | ForEach-Object {
        Write-Host "  - $($_.Name)" -ForegroundColor Gray
    }
    
    Write-Host "`nNext steps:" -ForegroundColor Yellow
    Write-Host "1. Install Inno Setup from https://jrsoftware.org/isdl.php" -ForegroundColor White
    Write-Host "2. Open MdReader-Setup.iss in Inno Setup Compiler" -ForegroundColor White
    Write-Host "3. Click 'Build' -> 'Compile'" -ForegroundColor White
    Write-Host "4. Find the installer in ../../installer-output/" -ForegroundColor White
} else {
    Write-Host "`nBuild failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}
