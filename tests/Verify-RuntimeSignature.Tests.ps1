$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../src/MdReader.Installer/Verify-RuntimeSignature.ps1"

function Assert-SignatureDecision($Subject, $Status, $Expected) {
    $key = [System.Security.Cryptography.RSA]::Create(2048)
    $certificate = $null
    try {
        $request = [System.Security.Cryptography.X509Certificates.CertificateRequest]::new(
            $Subject, $key, [System.Security.Cryptography.HashAlgorithmName]::SHA256,
            [System.Security.Cryptography.RSASignaturePadding]::Pkcs1)
        $certificate = $request.CreateSelfSigned([DateTimeOffset]::Now.AddDays(-1), [DateTimeOffset]::Now.AddDays(1))
        # Trust outcomes are synthetic here; production obtains Status from
        # Get-AuthenticodeSignature, never from certificate subject alone.
        $actual = Test-MicrosoftRuntimeSignature ([pscustomobject]@{
            Status = $Status; SignerCertificate = $certificate
        })
        if ($actual -ne $Expected) { throw "Unexpected decision for $Subject / $Status : $actual" }
    } finally {
        if ($certificate) { $certificate.Dispose() }
        $key.Dispose()
    }
}

Assert-SignatureDecision 'CN=.NET, O=Microsoft Corporation, C=US' 'Valid' $true
Assert-SignatureDecision 'CN=Microsoft Corporation, O=Microsoft Corporation, C=US' 'Valid' $true
Assert-SignatureDecision 'CN=.NET, O=Other Publisher, C=US' 'Valid' $false
Assert-SignatureDecision 'CN=Microsoft Corporation, O=Other Publisher, C=US' 'Valid' $false
Assert-SignatureDecision 'CN="Example, O=Microsoft Corporation", O=Other Publisher, C=US' 'Valid' $false
Assert-SignatureDecision 'CN=.NET, O=Microsoft Corporation, C=US' 'NotTrusted' $false
Assert-SignatureDecision 'CN=.NET, O=Microsoft Corporation, C=US' 'HashMismatch' $false
Assert-SignatureDecision 'CN=.NET, O=Microsoft Corporation, C=US' 'NotSigned' $false
if (Test-MicrosoftRuntimeSignature ([pscustomobject]@{ Status = 'Valid'; SignerCertificate = $null })) {
    throw 'A missing signer certificate must be rejected'
}
Write-Host 'All 9 runtime publisher checks passed.'
