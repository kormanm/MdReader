@echo off
REM Publish script for MdReader
REM Creates a self-contained executable that doesn't require .NET runtime

echo Publishing MdReader as self-contained application...
echo.

cd src\MdReader

echo Publishing for Windows x64...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
if %ERRORLEVEL% neq 0 (
    echo Publish failed
    exit /b %ERRORLEVEL%
)

echo.
echo Publish completed successfully!
echo Executable location: src\MdReader\bin\Release\net9.0-windows\win-x64\publish\MdReader.exe
echo.
echo This is a standalone executable that includes the .NET runtime.
echo You can copy this file and run it on any Windows 10/11 machine.
echo.

pause
