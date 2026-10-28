param(
    [string]$InstallerPath,
    [string]$LogPath
)

function Test-MicrosoftRuntimeSignature($Signature) {
    if ($null -eq $Signature -or $Signature.Status -ne 'Valid' -or
        $null -eq $Signature.SignerCertificate) {
        return $false
    }

    # Microsoft's .NET installers use CN=.NET, O=Microsoft Corporation.
    # Decode separate RDNs instead of matching CN or a substring of Subject.
    $rdns = $Signature.SignerCertificate.SubjectName.Decode(
        [System.Security.Cryptography.X509Certificates.X500DistinguishedNameFlags]::UseNewLines
    ) -split '\r?\n'
    return $rdns -contains 'O=Microsoft Corporation'
}

# Dot sourcing exposes the predicate to regression tests without running a download.
if ($MyInvocation.InvocationName -ne '.') {
    try {
        $ErrorActionPreference = 'Stop'
        # Actions launches setup from pwsh, whose inherited PSModulePath can point
        # Windows PowerShell 5.1 at incompatible PowerShell 7 module manifests.
        # Only use modules shipped with the engine that is running this verifier.
        $env:PSModulePath = [System.IO.Path]::Combine($PSHOME, 'Modules')
        Import-Module ([System.IO.Path]::Combine($PSHOME,
            'Modules/Microsoft.PowerShell.Security/Microsoft.PowerShell.Security.psd1')) -ErrorAction Stop
        $signature = Get-AuthenticodeSignature -LiteralPath $InstallerPath
        $details = "Status: $($signature.Status)`r`n" +
            "Status message: $($signature.StatusMessage)`r`n" +
            "Signer subject: $($signature.SignerCertificate.Subject)`r`n"
        Set-Content -LiteralPath $LogPath -Value $details -Encoding UTF8
        if (Test-MicrosoftRuntimeSignature $signature) { exit 0 }
        exit 1
    } catch {
        if ($LogPath) {
            [System.IO.File]::WriteAllText($LogPath,
                "Engine: $($PSVersionTable.PSVersion); PSHOME: $PSHOME`r`n$($_.Exception.ToString())")
        }
        exit 1
    }
}
