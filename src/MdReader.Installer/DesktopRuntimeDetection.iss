{ Mirrors .NET host lookup: x64 InstallLocation is read from the 32-bit view.
  https://github.com/dotnet/runtime/blob/main/src/native/corehost/hostmisc/pal.windows.cpp }
function HasDesktopRuntimeIn(const Root: String): Boolean;
var
  Entry: TFindRec;
  Frameworks, VersionPath: String;
begin
  Result := False;
  Frameworks := AddBackslash(Root) + 'shared\Microsoft.WindowsDesktop.App\';
  Log('Checking Desktop Runtime files in ' + Frameworks);
  if FindFirst(Frameworks + '9.0.*', Entry) then begin
    try
      repeat
        if (Copy(Entry.Name, 1, 4) = '9.0.') and
           (StrToIntDef(Copy(Entry.Name, 5, Length(Entry.Name)), -1) >= 0) then begin
          VersionPath := Frameworks + Entry.Name + '\';
          if FileExists(VersionPath + 'PresentationFramework.dll') and
             FileExists(VersionPath + 'Microsoft.WindowsDesktop.App.deps.json') then begin
            Log('Detected x64 Windows Desktop Runtime ' + Entry.Name);
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

function DesktopRuntimeInstalled: Boolean;
var
  RegisteredRoot, DefaultRoot: String;
begin
  Result := False;
  if RegQueryStringValue(HKLM32,
    'SOFTWARE\dotnet\Setup\InstalledVersions\x64', 'InstallLocation', RegisteredRoot) then begin
    Log('x64 .NET InstallLocation (32-bit registry view): ' + RegisteredRoot);
    Result := HasDesktopRuntimeIn(RegisteredRoot);
    if Result then Exit;
  end else
    Log('No registered x64 .NET InstallLocation in the 32-bit registry view.');
  DefaultRoot := ExpandConstant('{commonpf64}\dotnet');
  Result := HasDesktopRuntimeIn(DefaultRoot);
end;
