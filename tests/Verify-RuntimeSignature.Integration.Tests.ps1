$ErrorActionPreference = 'Stop'
$verifier = [System.IO.Path]::GetFullPath("$PSScriptRoot/../src/MdReader.Installer/Verify-RuntimeSignature.ps1")
$windowsPowerShell = "$env:WINDIR/System32/WindowsPowerShell/v1.0/powershell.exe"
$directory = Join-Path ([System.IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString())
New-Item -ItemType Directory $directory | Out-Null
$savedModulePath = $env:PSModulePath
try {
    # Reproduce setup inheriting PowerShell 7's modules before launching 5.1.
    $env:PSModulePath = Join-Path $PSHOME 'Modules'
    $log = Join-Path $directory 'signature.log'
    & $windowsPowerShell -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $verifier `
        -InstallerPath $windowsPowerShell -LogPath $log
    if ($LASTEXITCODE -ne 0) {
        throw "Windows PowerShell could not verify a Microsoft-signed executable: $(Get-Content $log -Raw)"
    }
    if ((Get-Content $log -Raw) -notmatch 'Status: Valid') { throw 'Trusted signature was not validated' }

    $unsigned = Join-Path $directory 'unsigned.ps1'
    [System.IO.File]::WriteAllText($unsigned, 'Write-Output unsigned')
    & $windowsPowerShell -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $verifier `
        -InstallerPath $unsigned -LogPath $log
    if ($LASTEXITCODE -ne 1 -or (Get-Content $log -Raw) -notmatch 'Status: NotSigned') {
        throw 'Unsigned file was not rejected with NotSigned status'
    }
    # Native exit code 1 above was expected, not a failed test script.
    $global:LASTEXITCODE = 0
    Write-Host 'Windows PowerShell verification passed with an inherited PowerShell 7 module path.'
} finally {
    $env:PSModulePath = $savedModulePath
    Remove-Item $directory -Recurse -Force
}
