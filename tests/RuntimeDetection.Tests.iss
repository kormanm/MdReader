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

function InitializeSetup: Boolean;
var
  Root: String;
begin
  Root := ExpandConstant('{tmp}\runtime-fixture');
  AssertDetection(Root, False, 'missing runtime');
  AddFramework(Root, '8.0.22', True);
  AddFramework(Root, '10.0.1', True);
  AddFramework(Root, '9.0.22-preview.1', True);
  AssertDetection(Root, False, 'wrong major and preview runtimes');
  AddFramework(Root, '9.0.22', False);
  AssertDetection(Root, False, 'incomplete runtime');
  AddFramework(Root, '9.0.22', True);
  AssertDetection(Root, True, 'complete stable runtime');
  DeleteFile(Root + '\shared\Microsoft.WindowsDesktop.App\9.0.22\PresentationFramework.dll');
  AssertDetection(Root, False, 'damaged runtime');
  Log('All runtime detection fixture checks passed.');
  Result := True;
end;
