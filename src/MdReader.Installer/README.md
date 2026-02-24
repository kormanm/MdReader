# MdReader Installer

This directory contains the installer configuration for MdReader.

## Prerequisites

1. **.NET 9 SDK** - Required to build the application
   - Download from: https://dotnet.microsoft.com/download/dotnet/9.0

2. **Inno Setup 6** - Required to create the Windows installer
- Download from: https://jrsoftware.org/isdl.php
- The installer will auto-detect Inno Setup in standard locations
- If you added Inno Setup to PATH, the scripts will find it automatically

## Building the Installer

### Option 1: Automated Build (Recommended)

Run the automated build script:

```powershell
.\build-and-create-installer.ps1
```

This script will:
1. Build and publish the MdReader application
2. Create the Windows installer using Inno Setup
3. Output the installer to `../../installer-output/`

### Option 2: Manual Build

#### Step 1: Build and Publish the Application

```powershell
.\build-installer.ps1
```

Or manually:

```powershell
cd ..\MdReader
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=false -p:SelfContained=true -p:PublishReadyToRun=true
```

The output will be in: `..\MdReader\bin\Release\net9.0-windows\win-x64\publish\`

#### Step 2: Create the Installer

1. Open `MdReader-Setup.iss` in Inno Setup Compiler
2. Click **Build** ? **Compile**
3. The installer will be created in `../../installer-output/`

## Installer Features

The installer includes:

- ? **Application Installation** - Installs MdReader to `Program Files\MdReader`
- ? **Start Menu Shortcuts** - Adds shortcuts to the Start Menu
- ? **Desktop Icon** - Optional desktop shortcut
- ? **File Associations** - Associates .md and .markdown files with MdReader
- ? **Open With Menu** - Adds MdReader to the "Open With" context menu
- ? **Self-Contained** - Includes .NET 9 runtime (no separate installation needed)
- ? **Uninstaller** - Clean uninstallation with registry cleanup

## File Associations

When the "Associate .md and .markdown files" option is selected during installation:

- Double-clicking .md or .markdown files will open them in MdReader
- Right-click ? "Open With" will show MdReader as an option for .md, .markdown, and .txt files
- File icons will be updated to show the MdReader icon (for .md and .markdown only)

**Note**: .txt files are added to the "Open With" menu but are NOT automatically associated. This allows you to manually choose to open text files in MdReader when needed, without changing your default text editor.

## Customization

### Change Version Number

Edit `MdReader-Setup.iss` and modify:

```pascal
#define MyAppVersion "1.0.0"
```

### Change Installation Directory

The default is `{autopf}\MdReader` (Program Files\MdReader).

To change, edit `MdReader-Setup.iss`:

```pascal
DefaultDirName={autopf}\MdReader
```

### Change Installer Icon

Edit `MdReader-Setup.iss`:

```pascal
SetupIconFile=..\MdReader\app.ico
```

## Troubleshooting

### Error: "Cannot find publish directory"

Make sure you've built the application first:

```powershell
.\build-installer.ps1
```

### Error: "Inno Setup not found"

The scripts check these locations automatically:
- `C:\Program Files (x86)\Inno Setup 6\`
- `C:\Program Files\Inno Setup 6\`
- `%LOCALAPPDATA%\Programs\Inno Setup 6\`
- System PATH environment variable

If Inno Setup is installed in a non-standard location, specify the path:

```powershell
.\build-and-create-installer.ps1 -InnoSetupPath "C:\path\to\ISCC.exe"
```

### Error: "Access denied" during installation

Run the installer as Administrator (right-click ? Run as administrator)

## Alternative: WiX Installer

For advanced scenarios, a WiX installer configuration is also provided:

- `MdReader.Installer.wixproj` - WiX project file
- `Product.wxs` - WiX source file

To use WiX:

1. Install WiX Toolset v5: https://wixtoolset.org/
2. Build the WiX project:

```powershell
dotnet build MdReader.Installer.wixproj -c Release
```

## License

The installer script is provided as-is for building MdReader installers.
