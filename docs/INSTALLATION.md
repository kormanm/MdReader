# MdReader Installation Guide

## System Requirements
- **Operating System**: Windows 10 or Windows 11
- **.NET Runtime**: .NET 9.0 Desktop Runtime
- **Disk Space**: ~50 MB
- **RAM**: 512 MB minimum, 1 GB recommended

## Installation Steps

### Option 1: Build from Source

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

### Option 2: Using Pre-built Binaries (When Available)

1. Download the latest release from the [Releases page](https://github.com/kormanm/MdReader/releases)
2. Extract the ZIP file to your desired location (e.g., `C:\Program Files\MdReader\`)
3. Run `MdReader.exe`

## File Association Setup

To integrate MdReader with Windows Explorer:

### Method 1: Registry File (Requires Administrator Rights)

1. Edit `install-file-association.reg` and update the path to match your installation:
   ```reg
   @="\"C:\\Your\\Installation\\Path\\MdReader.exe\" \"%1\""
   ```

2. Right-click the file and select "Merge"
3. Confirm the UAC prompt
4. Click "Yes" to add the information to registry

### Method 2: Manual "Open With" Association

1. Right-click any `.md` file
2. Select "Open with" → "Choose another app"
3. Click "More apps"
4. Scroll down and click "Look for another app on this PC"
5. Navigate to and select `MdReader.exe`
6. Check "Always use this app to open .md files"
7. Click "OK"

### Method 3: Set as Default Program

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

### Remove Application
1. Delete the MdReader folder from your installation location
2. Delete the state folder:
   ```
   %APPDATA%\MdReader
   ```

### Remove File Association
1. Create a file named `uninstall-file-association.reg` with this content:
   ```reg
   Windows Registry Editor Version 5.00

   [-HKEY_CLASSES_ROOT\.md]
   [-HKEY_CLASSES_ROOT\.markdown]
   [-HKEY_CLASSES_ROOT\MdReader.MarkdownFile]
   [-HKEY_CLASSES_ROOT\Applications\MdReader.exe]
   ```

2. Run the file to remove registry entries

Or use Windows Settings to change the default app for .md files.

## Troubleshooting

### Application won't start
- **Issue**: Missing .NET runtime
- **Solution**: Install [.NET 9.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)

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
1. Download the new version
2. Close MdReader
3. Replace the old executable with the new one
4. Your tabs and settings will be preserved (stored separately)
