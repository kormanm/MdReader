# Building the MdReader Installer

This project includes a complete Windows installer setup using Inno Setup.

## Quick Build Instructions

### Option 1: Automated (Recommended)

Navigate to the installer directory and run:

```cmd
cd src\MdReader.Installer
build-installer.bat
```

Or with PowerShell:

```powershell
cd src\MdReader.Installer
.\build-and-create-installer.ps1
```

### Option 2: Manual

1. **Install Prerequisites:**
   - .NET 9 SDK: https://dotnet.microsoft.com/download/dotnet/9.0
   - Inno Setup 6: https://jrsoftware.org/isdl.php

2. **Build the application:**
   ```cmd
   cd src\MdReader
   dotnet publish -c Release -r win-x64 -p:SelfContained=true
   ```

3. **Create installer:**
   - Open `src\MdReader.Installer\MdReader-Setup.iss` in Inno Setup
   - Click **Build** ? **Compile**

4. **Find installer:**
   - Location: `installer-output\MdReaderSetup-1.0.0.exe`

## Installer Features

? Self-contained (includes .NET 9 runtime)  
? File associations (.md, .markdown)  
? Start menu shortcuts  
? Desktop shortcut (optional)  
? Clean uninstallation  
? "Open With" context menu integration  

## Distribution

The final installer is a single `.exe` file that users can download and run.

For more details, see: [src/MdReader.Installer/README.md](src/MdReader.Installer/README.md)
