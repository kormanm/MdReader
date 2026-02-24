# MdReader Windows Installer Package

This directory contains everything needed to create a Windows installer for MdReader.

## Quick Start

### For Windows Users

**Easiest Method - Double-click the batch file:**

```
build-installer.bat
```

**Or using PowerShell:**

```powershell
.\build-and-create-installer.ps1
```

### What Gets Installed

- **MdReader Application** (in `C:\Program Files\MdReader\`)
- **Start Menu Shortcut**
- **Optional Desktop Shortcut**
- **File Association** for .md and .markdown files
- **.NET 9 Runtime** (bundled, no separate installation needed)

## Files in this Directory

| File | Purpose |
|------|---------|
| `MdReader-Setup.iss` | Inno Setup script (main installer definition) |
| `build-installer.ps1` | PowerShell build script |
| `build-and-create-installer.ps1` | Complete automated build + installer creation |
| `build-installer.bat` | Batch file for easy building |
| `MdReader.Installer.wixproj` | WiX Toolset project (advanced) |
| `Product.wxs` | WiX source file (advanced) |
| `README.md` | Detailed documentation |

## Creating the Installer

### Prerequisites

1. **.NET 9 SDK** installed
2. **Inno Setup 6** installed from https://jrsoftware.org/isdl.php

### Steps

1. **Build the application:**
   ```powershell
   .\build-installer.ps1
   ```

2. **Create the installer:**
   - Option A: Run `.\build-and-create-installer.ps1` (fully automated)
   - Option B: Open `MdReader-Setup.iss` in Inno Setup and click Compile

3. **Find your installer:**
   - Location: `../../installer-output/MdReaderSetup-1.0.0.exe`

## Installer Options

During installation, users can choose:

- ? Installation directory
- ? Create desktop shortcut
- ? Associate .md and .markdown files with MdReader
- ? Start menu folder name

## CI/CD Integration

The `.github/workflows/build-installer.yml` automatically:

- Builds the installer on every push
- Uploads the installer as an artifact
- Creates a release with the installer when you tag a version

To create a release:

```bash
git tag v1.0.0
git push origin v1.0.0
```

## Customization

See [README.md](README.md) for detailed customization options.

## Distribution

After building, you can distribute the installer file:

- Upload to GitHub Releases
- Share directly (single .exe file)
- Host on your website

The installer is a single .exe file that includes everything needed.
