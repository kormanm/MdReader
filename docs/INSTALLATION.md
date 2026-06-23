# MdReader Installation Guide

## System Requirements
- **Operating System**: Windows 10 or Windows 11
- **.NET Runtime**: .NET 9.0 Desktop Runtime (the installer downloads and installs it automatically if not already present)
- **Disk Space**: ~50 MB (plus ~60 MB for the .NET runtime if not already installed)
- **RAM**: 512 MB minimum, 1 GB recommended

## Installation Steps

### Option 1: Installer (Recommended)

1. Go to the **[Releases page](https://github.com/kormanm/MdReader/releases/latest)**
2. Download `MdReaderSetup-<version>.exe`
3. Run the installer and follow the on-screen steps

The installer checks for .NET 9.0 Desktop Runtime and **automatically downloads and installs it** if it is not already present on your machine.

During installation you can optionally:
- Create a desktop shortcut
- Associate `.md` and `.markdown` files with MdReader (recommended)

To uninstall, use *Add or Remove Programs* in Windows Settings.

### Option 2: Build from Source

#### Prerequisites
1. Install [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Install Visual Studio 2022 (or later) with .NET desktop development workload (optional but recommended)

#### Build Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/kormanm/MdReader.git
   cd MdReader
   ```

2. Build the application:
   ```bash
   cd src/MdReader
   dotnet build -c Release
   ```

3. The executable will be located at:
   ```
   src/MdReader/bin/Release/net9.0-windows/MdReader.exe
   ```

4. (Optional) Publish self-contained:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```
   
   This creates a single executable that doesn't require .NET runtime to be installed.

## File Association Setup

When using the installer, file associations can be configured automatically during setup. If you built from source, you can set them up manually.

### Method 1: Manual "Open With" Association

1. Right-click any `.md` file
2. Select "Open with" → "Choose another app"
3. Click "More apps"
4. Scroll down and click "Look for another app on this PC"
5. Navigate to and select `MdReader.exe`
6. Check "Always use this app to open .md files"
7. Click "OK"

### Method 2: Set as Default Program

1. Open Windows Settings
2. Go to "Apps" → "Default apps"
3. Search for "MdReader" or click "Choose default apps by file type"
4. Find `.md` in the list
5. Click the current default app
6. Select "MdReader" from the list

## First Run

1. Launch MdReader
2. The application will create a state file at:
   ```
   %APPDATA%\MdReader\state.json
   ```

3. To open a file:
   - Click "📂 Open" button
   - Drag and drop a .md file onto the window
   - Right-click a .md file in Explorer and select "Open with MdReader"
   - Pass file path as command-line argument

## Uninstallation

### Installer-based installation

Use *Add or Remove Programs* in Windows Settings to uninstall MdReader. The uninstaller removes the application and all registry entries added during installation.

Optionally, delete your personal state folder to remove saved tabs:
```
%APPDATA%\MdReader
```

### Manual (build from source) installation

1. Delete the MdReader folder from your installation location.
2. Delete the state folder:
   ```
   %APPDATA%\MdReader
   ```
3. (Optional) Remove file associations via Windows Settings → Apps → Default apps.

## Troubleshooting

### Application won't start
- **Issue**: Missing .NET 9.0 Desktop Runtime
- **Solution**: Run the installer again — it will detect and install the missing runtime. Alternatively, install it manually from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/9.0)

### Files won't open
- **Issue**: Incorrect file association
- **Solution**: Re-run the file association setup or use "Open With" method

### Tabs not restoring
- **Issue**: State file corrupted or missing
- **Solution**: Delete `%APPDATA%\MdReader\state.json` and restart

### Error opening URLs
- **Issue**: Network connectivity or invalid URL
- **Solution**: 
  - Check internet connection
  - Verify URL points to a .md or .markdown file
  - Try downloading the file manually to verify accessibility

## Security Notes

- MdReader does not execute code from Markdown files
- External links open in your default browser
- State file only contains file paths, no sensitive data
- No telemetry or data collection

## Updates

To update MdReader:
1. Download the new installer from the [Releases page](https://github.com/kormanm/MdReader/releases/latest)
2. Close MdReader
3. Run the new installer — it will upgrade the existing installation
4. Your tabs and settings will be preserved (stored separately in `%APPDATA%\MdReader`)
