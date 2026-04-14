# Automated build script for MdReader installer
# Builds the app and creates installer using Inno Setup

param(
    [string]$Configuration = "Release",
    [string]$InnoSetupPath = "",
    [string]$AppVersion = ""
)

# Step 1: Build and publish the application
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 1: Building and Publishing MdReader" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

& "$PSScriptRoot\build-installer.ps1" -Configuration $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed. Aborting installer creation." -ForegroundColor Red
    exit 1
}

# Step 2: Create installer with Inno Setup
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Step 2: Creating Installer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Try common Inno Setup locations
$possiblePaths = @(
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
)

if ([string]::IsNullOrEmpty($InnoSetupPath)) {
    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            $InnoSetupPath = $path
            Write-Host "Found Inno Setup at: $InnoSetupPath" -ForegroundColor Green
            break
        }
    }
    
    # If not found in standard locations, try to find in PATH
    if ([string]::IsNullOrEmpty($InnoSetupPath)) {
        $isccInPath = Get-Command ISCC.exe -ErrorAction SilentlyContinue
        if ($isccInPath) {
            $InnoSetupPath = $isccInPath.Source
            Write-Host "Found Inno Setup in PATH: $InnoSetupPath" -ForegroundColor Green
        }
    }
}

# Check if Inno Setup is installed
if ([string]::IsNullOrEmpty($InnoSetupPath) -or -not (Test-Path $InnoSetupPath)) {
    Write-Host "Inno Setup not found in standard locations" -ForegroundColor Red
    Write-Host "Please install Inno Setup from: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    Write-Host "Or specify the correct path using -InnoSetupPath parameter" -ForegroundColor Yellow
    Write-Host "`nSearched locations:" -ForegroundColor Yellow
    foreach ($path in $possiblePaths) {
        Write-Host "  - $path" -ForegroundColor Gray
    }
    exit 1
}

$scriptPath = Join-Path $PSScriptRoot "MdReader-Setup.iss"
Write-Host "Compiling installer script: $scriptPath" -ForegroundColor Cyan

$compileArgs = @()
if (-not [string]::IsNullOrWhiteSpace($AppVersion)) {
    Write-Host "Using installer version override: $AppVersion" -ForegroundColor Cyan
    $compileArgs += "/DMyAppVersion=$AppVersion"
}
$compileArgs += $scriptPath

& $InnoSetupPath @compileArgs

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "Installer created successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    
    $outputDir = Join-Path $PSScriptRoot "..\..\installer-output"
    Write-Host "`nInstaller location: $outputDir" -ForegroundColor Green
    
    # List installer files
    if (Test-Path $outputDir) {
        Write-Host "`nInstaller files:" -ForegroundColor Cyan
        Get-ChildItem $outputDir -Filter "*.exe" | ForEach-Object {
            Write-Host "  - $($_.Name) ($([math]::Round($_.Length / 1MB, 2)) MB)" -ForegroundColor Gray
        }
    }
} else {
    Write-Host "`nInstaller creation failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}
