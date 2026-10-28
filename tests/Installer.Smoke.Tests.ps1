param([Parameter(Mandatory)][string]$Version)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath("$PSScriptRoot/..")
$logs = Join-Path $root 'installer-output/logs'
New-Item -ItemType Directory -Force $logs | Out-Null
$target = Join-Path $env:RUNNER_TEMP 'MdReader-smoke'
$installer = Join-Path $root "installer-output/MdReaderSetup-$Version.exe"
$iscc = (Get-Command ISCC.exe -ErrorAction SilentlyContinue).Source
if (-not $iscc) { $iscc = "${env:ProgramFiles(x86)}/Inno Setup 6/ISCC.exe" }

# Compile a test-only bootstrap variant. Production never exposes a command-line
# option to skip detection. Both variants must perform the same post-install check.
& $iscc "/DMyAppVersion=$Version" '/DCIForceRuntimeBootstrap' `
    '/Fbootstrap-test' "$root/src/MdReader.Installer/MdReader-Setup.iss"
if ($LASTEXITCODE -ne 0) { throw 'Bootstrap test installer compilation failed' }

function Invoke-Setup($Path, $LogName, $Arguments) {
    $log = Join-Path $logs $LogName
    $process = Start-Process -FilePath $Path -Wait -PassThru -ArgumentList (@(
        '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/RESTARTEXITCODE=3010', "/LOG=`"$log`""
    ) + $Arguments)
    if ($process.ExitCode -notin @(0, 3010)) {
        Get-Content $log -ErrorAction SilentlyContinue | ForEach-Object { Write-Host $_ }
        Get-Content "$log.runtime.log" -ErrorAction SilentlyContinue | ForEach-Object { Write-Host $_ }
        throw "Setup failed: $LogName, exit code $($process.ExitCode)"
    }
    return Get-Content $log -Raw
}

& $iscc "$PSScriptRoot/RuntimeDetection.Tests.iss"
if ($LASTEXITCODE -ne 0) { throw 'Runtime detection fixture compilation failed' }
$detectionLog = Invoke-Setup "$root/installer-output/runtime-detection-tests.exe" 'detection.log' @()
if ($detectionLog -notmatch 'All runtime detection fixture checks passed') {
    throw 'Runtime detection fixture tests did not execute'
}

$bootstrapLog = Invoke-Setup "$root/installer-output/bootstrap-test.exe" 'bootstrap.log' @("/DIR=`"$target`"")
if ($bootstrapLog -notmatch 'Bootstrapping x64') { throw 'Runtime bootstrap did not execute' }
if (Test-Path "$target/coreclr.dll") { throw 'Fresh payload unexpectedly bundles .NET' }

# Reproduce an in-place upgrade from the former self-contained distribution.
# Fresh installs on an SDK-equipped runner do not expose app-local hostfxr shadowing.
$legacy = Join-Path $env:RUNNER_TEMP 'MdReader-legacy-publish'
& dotnet publish "$root/src/MdReader/MdReader.csproj" -c Release -r win-x64 `
    --self-contained true -p:PublishSingleFile=false -p:PublishReadyToRun=false -o $legacy
if ($LASTEXITCODE -ne 0) { throw 'Could not prepare self-contained upgrade fixture' }
Copy-Item "$legacy/*" $target -Recurse -Force
if (-not (Test-Path "$target/hostfxr.dll")) { throw 'Upgrade fixture has no app-local hostfxr' }
# Restore the old payload's framework-dependent config only through setup below.
foreach ($attempt in 1..2) {
    $log = Invoke-Setup $installer "install-$attempt.log" @("/DIR=`"$target`"")
    if ($log -notmatch 'Desktop Runtime already installed; skipping download') {
        throw 'Installed runtime was not detected or was downloaded again'
    }
    foreach ($file in @('MdReader.exe', 'MdReader.runtimeconfig.json', 'Assets/Mermaid/mermaid.min.js')) {
        if (-not (Test-Path "$target/$file")) { throw "Missing installed file: $file" }
    }
    if (Test-Path "$target/hostfxr.dll") { throw 'Upgrade left an app-local runtime loader behind' }
    $configuration = Get-Content "$target/MdReader.runtimeconfig.json" -Raw | ConvertFrom-Json
    if ($configuration.runtimeOptions.includedFrameworks -or
        -not ($configuration.runtimeOptions.frameworks | Where-Object name -EQ 'Microsoft.WindowsDesktop.App')) {
        throw 'Installed payload is not a framework-dependent Desktop application'
    }
    $productVersion = (Get-Item "$target/MdReader.exe").VersionInfo.ProductVersion.Split('+')[0]
    if ($productVersion -ne $Version) { throw "Installed version $productVersion does not match $Version" }
    $runtimes = & "$env:ProgramFiles/dotnet/dotnet.exe" --list-runtimes
    if ($LASTEXITCODE -ne 0) { throw 'Could not query x64 runtimes' }
    foreach ($framework in $configuration.runtimeOptions.frameworks) {
        $name = [regex]::Escape($framework.name)
        if (-not ($runtimes -match "^$name 9\.0\.\d+ ")) {
            throw "Required x64 $($framework.name) 9.0.x runtime is missing"
        }
    }
}

$document = Join-Path $env:RUNNER_TEMP 'MdReader-smoke.md'
$workflow = Get-Content "$PSScriptRoot/fixtures/workflow.mmd" -Raw
[IO.File]::WriteAllText($document, "# Installer smoke test`n`n" + '```mermaid' + "`n$workflow`n" + '```' + "`n")
$app = Start-Process "$target/MdReader.exe" -ArgumentList "`"$document`"" -PassThru
try {
    $deadline = [DateTime]::UtcNow.AddSeconds(20)
    do {
        Start-Sleep -Milliseconds 250
        $app.Refresh()
        if ($app.HasExited) { throw "Application exited during startup: $($app.ExitCode)" }
    } while ($app.MainWindowHandle -eq 0 -and [DateTime]::UtcNow -lt $deadline)
    if ($app.MainWindowHandle -eq 0) { throw 'Application did not create its window' }
} finally {
    if (-not $app.HasExited) {
        $null = $app.CloseMainWindow()
        if (-not $app.WaitForExit(5000)) { $app.Kill(); $app.WaitForExit() }
    }
}
$null = Invoke-Setup "$target/unins000.exe" 'uninstall.log' @()
if (Test-Path "$target/MdReader.exe") { throw 'Uninstall left the application executable behind' }
Write-Host 'Bootstrap, runtime reuse, install, reinstall, startup, and uninstall checks passed.'
