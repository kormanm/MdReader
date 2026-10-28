@echo off
setlocal
set "PSModulePath=%SystemRoot%\System32\WindowsPowerShell\v1.0\Modules"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0src\MdReader.Installer\build-installer.ps1"
exit /b %ERRORLEVEL%
