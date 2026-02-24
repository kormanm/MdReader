@echo off
REM Build and Create MdReader Installer
REM This batch file builds the application and creates the installer

echo ========================================
echo Building MdReader Application
echo ========================================
echo.

cd ..\MdReader

REM Build and publish the application
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=false -p:SelfContained=true -p:PublishReadyToRun=true

if %ERRORLEVEL% neq 0 (
    echo.
    echo Build failed!
    pause
    exit /b %ERRORLEVEL%
)

cd ..\MdReader.Installer

echo.
echo ========================================
echo Build successful!
echo ========================================
echo.
echo Next steps:
echo 1. Install Inno Setup from https://jrsoftware.org/isdl.php
echo 2. Open MdReader-Setup.iss in Inno Setup Compiler
echo 3. Click 'Build' -^> 'Compile'
echo 4. Find the installer in ..\..\installer-output\
echo.

REM Try to compile with Inno Setup if installed
set ISCC="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

if not exist %ISCC% (
    set ISCC="C:\Program Files\Inno Setup 6\ISCC.exe"
)

if not exist %ISCC% (
    set ISCC="%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe"
)

REM Try to find ISCC in PATH
if not exist %ISCC% (
    where ISCC.exe >nul 2>&1
    if %ERRORLEVEL% equ 0 (
        set ISCC=ISCC.exe
    )
)

if exist %ISCC% (
    echo ========================================
    echo Creating Installer with Inno Setup
    echo ========================================
    echo.
    %ISCC% MdReader-Setup.iss
    
    if %ERRORLEVEL% equ 0 (
        echo.
        echo ========================================
        echo Installer created successfully!
        echo ========================================
        echo.
        echo Check the installer-output folder for the setup file.
    ) else (
        echo.
        echo Installer creation failed!
    )
) else (
    echo.
    echo Inno Setup not found at standard locations.
    echo Please install it from https://jrsoftware.org/isdl.php
    echo or ensure ISCC.exe is in your PATH environment variable.
    echo.
    echo Or run MdReader-Setup.iss manually with Inno Setup Compiler.
)

pause
