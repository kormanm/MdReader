@echo off
REM Publish script for MdReader
REM Creates a framework-dependent application; the installer handles .NET setup.

echo Publishing MdReader as a framework-dependent application...
echo.

cd /d "%~dp0src\MdReader"

echo Publishing for Windows x64...
if exist "bin\Release\net9.0-windows\win-x64\publish" rmdir /s /q "bin\Release\net9.0-windows\win-x64\publish"
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=false -p:PublishReadyToRun=false -p:DebugType=None -p:DebugSymbols=false
if %ERRORLEVEL% neq 0 (
    echo Publish failed
    exit /b %ERRORLEVEL%
)

echo.
echo Publish completed successfully!
echo Executable location: src\MdReader\bin\Release\net9.0-windows\win-x64\publish\MdReader.exe
echo.
echo Requires the x64 .NET 9 Desktop Runtime and WebView2 Runtime.
echo Distribute the whole publish folder, or build the installer to handle .NET installation.
echo.

pause
