[Setup]
AppName=MdReader Runtime Detection Tests
AppVersion=1.0
DefaultDirName={tmp}\MdReaderDetectionTests
CreateAppDir=no
Uninstallable=no
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
OutputDir=..\installer-output
OutputBaseFilename=runtime-detection-tests

[Code]
#include "..\src\MdReader.Installer\DesktopRuntimeDetection.iss"

procedure AssertDetection(const Root: String; Expected: Boolean; const Scenario: String);
begin
  if HasDesktopRuntimeIn(Root) <> Expected then
    RaiseException('Runtime detection assertion failed: ' + Scenario);
end;

procedure AddFramework(const Root, Version: String; Complete: Boolean);
var
  Path: String;
begin
  Path := AddBackslash(Root) + 'shared\Microsoft.WindowsDesktop.App\' + Version + '\';
  if not ForceDirectories(Path) then RaiseException('Could not create fixture directory');
  if not SaveStringToFile(Path + 'PresentationFramework.dll', 'test fixture, never executed', False) then
    RaiseException('Could not create fixture file');
  if Complete then
    if not SaveStringToFile(Path + 'Microsoft.WindowsDesktop.App.deps.json', '{}', False) then
      RaiseException('Could not create dependency fixture');
end;

procedure AddBaseRuntime(const Root, Version: String);
var
  Path: String;
begin
  Path := AddBackslash(Root) + 'shared\Microsoft.NETCore.App\' + Version + '\';
  if not ForceDirectories(Path) then RaiseException('Could not create base runtime fixture');
  if not SaveStringToFile(Path + 'coreclr.dll', 'test fixture, never executed', False) then
    RaiseException('Could not create base runtime library fixture');
  if not SaveStringToFile(Path + 'Microsoft.NETCore.App.deps.json', '{}', False) then
    RaiseException('Could not create base runtime dependency fixture');
end;

function InitializeSetup: Boolean;
var
  Root: String;
begin
  Root := ExpandConstant('{tmp}\runtime-fixture');
  AssertDetection(Root, False, 'missing runtime');
  AddBaseRuntime(Root, '9.0.21');
  AssertDetection(Root, False, 'base runtime without Desktop');
  AddFramework(Root, '8.0.22', True);
  AddFramework(Root, '10.0.1', True);
  AddFramework(Root, '9.0.22-preview.1', True);
  AssertDetection(Root, False, 'wrong major and preview runtimes');
  AddFramework(Root, '9.0.22', False);
  AssertDetection(Root, False, 'incomplete runtime');
  AddFramework(Root, '9.0.22', True);
  DeleteFile(Root + '\shared\Microsoft.NETCore.App\9.0.21\coreclr.dll');
  DeleteFile(Root + '\shared\Microsoft.NETCore.App\9.0.21\Microsoft.NETCore.App.deps.json');
  AssertDetection(Root, False, 'Desktop without base runtime');
  AddBaseRuntime(Root, '8.0.22');
  AddBaseRuntime(Root, '10.0.1');
  AddBaseRuntime(Root, '9.0.22-preview.1');
  AssertDetection(Root, False, 'Desktop with incompatible base runtime');
  AddBaseRuntime(Root, '9.0.21');
  AssertDetection(Root, True, 'complete stable runtime');
  DeleteFile(Root + '\shared\Microsoft.NETCore.App\9.0.21\coreclr.dll');
  AssertDetection(Root, False, 'damaged base runtime');
  AddBaseRuntime(Root, '9.0.21');
  DeleteFile(Root + '\shared\Microsoft.WindowsDesktop.App\9.0.22\PresentationFramework.dll');
  AssertDetection(Root, False, 'damaged runtime');
  Log('All runtime detection fixture checks passed.');
  Result := True;
end;
