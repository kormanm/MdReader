{ Mirrors .NET host lookup: x64 InstallLocation is read from the 32-bit view.
  https://github.com/dotnet/runtime/blob/main/src/native/corehost/hostmisc/pal.windows.cpp }
function HasFrameworkIn(const Root, Framework, RequiredFile: String): Boolean;
var
  Entry: TFindRec;
  Frameworks, VersionPath: String;
begin
  Result := False;
  Frameworks := AddBackslash(Root) + 'shared\' + Framework + '\';
  Log('Checking runtime files in ' + Frameworks);
  if FindFirst(Frameworks + '9.0.*', Entry) then begin
    try
      repeat
        if (Copy(Entry.Name, 1, 4) = '9.0.') and
           (StrToIntDef(Copy(Entry.Name, 5, Length(Entry.Name)), -1) >= 0) then begin
          VersionPath := Frameworks + Entry.Name + '\';
          if FileExists(VersionPath + RequiredFile) and
             FileExists(VersionPath + Framework + '.deps.json') then begin
            Log('Detected x64 ' + Framework + ' ' + Entry.Name);
            Result := True;
            Exit;
          end;
        end;
      until not FindNext(Entry);
    finally
      FindClose(Entry);
    end;
  end;
end;

function HasDesktopRuntimeIn(const Root: String): Boolean;
begin
  { WPF runtimeconfig requires BOTH shared frameworks at the same host root.
    Desktop files alone do not establish that Microsoft.NETCore.App is present. }
  Result := HasFrameworkIn(Root, 'Microsoft.NETCore.App', 'coreclr.dll');
  if not Result then begin
    Log('Missing or incomplete x64 Microsoft.NETCore.App 9.0.x in ' + Root);
    Exit;
  end;
  Result := HasFrameworkIn(Root, 'Microsoft.WindowsDesktop.App', 'PresentationFramework.dll');
  if not Result then
    Log('Missing or incomplete x64 Microsoft.WindowsDesktop.App 9.0.x in ' + Root);
end;

function DesktopRuntimeInstalled: Boolean;
var
  RegisteredRoot, DefaultRoot: String;
begin
  Result := False;
  if RegQueryStringValue(HKLM32,
    'SOFTWARE\dotnet\Setup\InstalledVersions\x64', 'InstallLocation', RegisteredRoot) then begin
    Log('x64 .NET InstallLocation (32-bit registry view): ' + RegisteredRoot);
    Result := HasDesktopRuntimeIn(RegisteredRoot);
    { Do not combine frameworks from two roots: apphost uses the registered root. }
    Exit;
  end else
    Log('No registered x64 .NET InstallLocation in the 32-bit registry view.');
  DefaultRoot := ExpandConstant('{commonpf64}\dotnet');
  Result := HasDesktopRuntimeIn(DefaultRoot);
end;
