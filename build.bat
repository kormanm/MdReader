@echo off
REM Build script for MdReader
REM Requires .NET 9.0 SDK

echo Building MdReader...
echo.

cd src\MdReader

echo Restoring NuGet packages...
dotnet restore
if %ERRORLEVEL% neq 0 (
    echo Failed to restore packages
    exit /b %ERRORLEVEL%
)

echo.
echo Building in Release mode...
dotnet build -c Release
if %ERRORLEVEL% neq 0 (
    echo Build failed
    exit /b %ERRORLEVEL%
)

echo.
echo Build completed successfully!
echo Executable location: src\MdReader\bin\Release\net9.0-windows\MdReader.exe
echo.

pause
