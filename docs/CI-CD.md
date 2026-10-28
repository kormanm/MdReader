# Build and release pipeline

`MdReader.csproj`'s `Version` is the source of truth. The installer build reads it;
release input and tag must match it. Release only from a commit merged into main.

## Pull requests and main

- **Build** restores and compiles the WPF app, runs the real Playwright diagram
  tests, and publishes a framework-dependent win-x64 application.
- **Build Installer** runs publisher policy tests and a Windows PowerShell 5.1
  integration test under an inherited PowerShell 7 environment. It publishes via
  the same PowerShell script, compiles Inno Setup, and runs the installer smoke
  suite. Main pushes also run these installer checks; they do not create releases.
- The smoke suite compiles a test-only forced-bootstrap installer to exercise the
  real Microsoft download, signature verification, prerequisite installer, and
  post-install detection even if the runner already has .NET 9. It then runs the
  production installer twice, requiring runtime reuse without another download,
  verifies the installed version and assets, starts the app, and uninstalls it.
- Before testing the production upgrade it overlays an actual self-contained
  publish, reproducing the previous distribution. Setup must remove its app-local
  `hostfxr.dll`; otherwise apphost ignores global .NET and reports missing runtimes.
- Production installers do not expose the forced-bootstrap switch. The test
  executable is excluded from release artifacts.
- Setup logs are retained as `installer-diagnostics` even if the smoke suite fails.
  The tested setup executable is uploaded only after the complete suite passes.

## Release

1. Bump `Version` in `src/MdReader/MdReader.csproj` and update the changelog.
2. Merge the PR after both build checks pass.
3. Run **Build Installer** on **main** with that version, or push a matching
   `v<Version>` tag at the merged commit.
4. A separate release job downloads the tested artifact and publishes it to the
   matching GitHub release. Only this job has repository write permission.

An existing version tag at another commit is rejected; release automation never
moves tags. After a fix to an already-tagged release, increment the patch version.
Failed workflow runs do not publish artifacts as releases.

## Local Windows builds

`publish.bat` delegates to `build-installer.ps1`. The installer batch entry point
delegates to `build-and-create-installer.ps1`; there are no separate batch publish
flags. Run the latter with `-Configuration Debug` if needed; it passes the actual
publish directory to Inno Setup. Publish output is cleaned before each build.

Runtime detection reads x64 `InstallLocation` in the 32-bit registry view, matching
the .NET host, then checks stable 9.0.x Desktop framework directories and WPF files.
The registered root must contain both base `Microsoft.NETCore.App` and Desktop
frameworks; frameworks from separate roots cannot satisfy the check. When no root
is registered it checks the standard x64 Program Files location. Native fixture tests use
the exact detection include compiled into production setup. Signature
verification requires Windows Authenticode trust plus Microsoft organization,
using the modules shipped with the PowerShell engine doing the verification.

## Validation limits

Hosted Windows smoke tests cover bootstrap, runtime reuse, install, reinstall,
startup, and uninstall. They are not a replacement for a clean Windows VM test of
an offline download failure, a genuinely reboot-required install, x86-only or
preview-only runtimes, or interactive Mermaid behavior. Linux checks cannot
establish that Windows installer execution passed; inspect the Windows job results.
