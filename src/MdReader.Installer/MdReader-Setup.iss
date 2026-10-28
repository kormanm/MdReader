; MdReader Inno Setup Script
; This script creates a Windows installer for MdReader

#define MyAppName "MdReader"
#ifndef MyAppVersion
  #define MyAppVersion "1.2.2"
#endif
#define MyAppPublisher "MdReader"
#define MyAppURL "https://github.com/kormanm/MdReader"
#define MyAppExeName "MdReader.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\..\LICENSE
OutputDir=..\..\installer-output
OutputBaseFilename=MdReaderSetup-{#MyAppVersion}
SetupIconFile=..\MdReader\app.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
ChangesAssociations=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "fileassoc"; Description: "Associate .md and .markdown files with {#MyAppName}"; GroupDescription: "File associations:"; Flags: checkedonce

[Files]
; Trusted local verifier extracted only for prerequisite verification.
Source: "Verify-RuntimeSignature.ps1"; Flags: dontcopy
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
; Framework-dependent application; PrepareToInstall installs the shared runtime if needed.
Source: "..\MdReader\bin\Release\net9.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Icon file
Source: "..\MdReader\app.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
; File associations for .md files
Root: HKCR; Subkey: ".md"; ValueType: string; ValueName: ""; ValueData: "MdReader.MarkdownFile"; Flags: uninsdeletevalue; Tasks: fileassoc
Root: HKCR; Subkey: ".markdown"; ValueType: string; ValueName: ""; ValueData: "MdReader.MarkdownFile"; Flags: uninsdeletevalue; Tasks: fileassoc
Root: HKCR; Subkey: "MdReader.MarkdownFile"; ValueType: string; ValueName: ""; ValueData: "Markdown Document"; Flags: uninsdeletekey; Tasks: fileassoc
Root: HKCR; Subkey: "MdReader.MarkdownFile\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKCR; Subkey: "MdReader.MarkdownFile\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; Add to "Open With" menu
Root: HKCR; Subkey: "Applications\{#MyAppExeName}"; ValueType: string; ValueName: ""; ValueData: ""; Flags: uninsdeletekey; Tasks: fileassoc
Root: HKCR; Subkey: "Applications\{#MyAppExeName}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc
Root: HKCR; Subkey: "Applications\{#MyAppExeName}\SupportedTypes"; ValueType: string; ValueName: ".md"; ValueData: ""; Tasks: fileassoc
Root: HKCR; Subkey: "Applications\{#MyAppExeName}\SupportedTypes"; ValueType: string; ValueName: ".markdown"; ValueData: ""; Tasks: fileassoc
Root: HKCR; Subkey: "Applications\{#MyAppExeName}\SupportedTypes"; ValueType: string; ValueName: ".txt"; ValueData: ""; Tasks: fileassoc

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent; Check: CanLaunchApplication

[Code]
const
  DesktopRuntimeKey = 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App';
  DesktopRuntimeUrl = 'https://aka.ms/dotnet/9.0/windowsdesktop-runtime-win-x64.exe';
  DesktopRuntimeFile = 'windowsdesktop-runtime-win-x64.exe';

var
  RuntimeDownloadPage: TDownloadWizardPage;
  RuntimeRestartRequired: Boolean;

function DesktopRuntimeInstalled: Boolean;
var
  Versions: TArrayOfString;
  I: Integer;
  Installed: Cardinal;
begin
  Result := False;
  { WPF requires the x64 Windows Desktop runtime, not just Microsoft.NETCore.App.
    .NET 10 alone does not satisfy the app's default major-version roll-forward policy. }
  if RegGetValueNames(HKLM64, DesktopRuntimeKey, Versions) then
    for I := 0 to GetArrayLength(Versions) - 1 do
      if (Copy(Versions[I], 1, 4) = '9.0.') and
         (StrToIntDef(Copy(Versions[I], 5, Length(Versions[I])), -1) >= 0) then
        if RegQueryDWordValue(HKLM64, DesktopRuntimeKey, Versions[I], Installed) then
          if Installed = 1 then begin
            Result := True;
            Exit;
          end;
end;

procedure InitializeWizard;
begin
  RuntimeDownloadPage := CreateDownloadPage(
    'Installing .NET 9 Desktop Runtime',
    'MdReader needs the x64 .NET 9 Desktop Runtime. Downloading it from Microsoft.', nil);
  RuntimeDownloadPage.ShowBaseNameInsteadOfUrl := True;
end;

function HasMicrosoftSignature(const FileName: String): Boolean;
var
  Command, LogPath: String;
  Details: AnsiString;
  ExitCode: Integer;
begin
  ExtractTemporaryFile('Verify-RuntimeSignature.ps1');
  LogPath := ExpandConstant('{tmp}\runtime-signature.log');
  { ExecutionPolicy is scoped to this child process for our bundled local script.
    The script still requires a trusted, valid Authenticode signature. }
  Command := '-NoProfile -NonInteractive -ExecutionPolicy Bypass -File ' +
    AddQuotes(ExpandConstant('{tmp}\Verify-RuntimeSignature.ps1')) +
    ' -InstallerPath ' + AddQuotes(FileName) + ' -LogPath ' + AddQuotes(LogPath);
  Result := False;
  if Exec(ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'),
    Command, '', SW_HIDE, ewWaitUntilTerminated, ExitCode) then
    Result := ExitCode = 0;
  if LoadStringFromFile(LogPath, Details) then Log('Runtime signature verification: ' + Details);
  if not Result then Log(Format('Runtime signature verification failed (process/error code %d).', [ExitCode]));
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  RuntimePath: String;
  ExitCode: Integer;
begin
  Result := '';
  if DesktopRuntimeInstalled then Exit;
  RuntimeDownloadPage.Clear;
  RuntimeDownloadPage.Add(DesktopRuntimeUrl, DesktopRuntimeFile, '');
  RuntimeDownloadPage.Show;
  try
    try
      RuntimeDownloadPage.Download;
    except
      Result := 'Could not download .NET 9 Desktop Runtime. Check your Internet connection ' +
        'and try again, or install the x64 .NET 9 Desktop Runtime manually. ' + GetExceptionMessage;
      Exit;
    end;
  finally
    RuntimeDownloadPage.Hide;
  end;

  RuntimePath := ExpandConstant('{tmp}\') + DesktopRuntimeFile;
  if not HasMicrosoftSignature(RuntimePath) then begin
    DeleteFile(RuntimePath);
    Result := 'The .NET runtime download does not have a valid Microsoft signature. ' +
      'Installation was stopped. Check Windows certificate trust and try again.';
    Exit;
  end;

  WizardForm.StatusLabel.Caption := 'Installing .NET 9 Desktop Runtime...';
  if not Exec(RuntimePath, '/install /quiet /norestart', '', SW_HIDE,
    ewWaitUntilTerminated, ExitCode) then begin
    Result := 'Could not start the .NET 9 Desktop Runtime installer.';
    Exit;
  end;
  if (ExitCode <> 0) and (ExitCode <> 3010) then begin
    Result := Format('.NET 9 Desktop Runtime installation failed (exit code %d). ' +
      'Install the x64 .NET 9 Desktop Runtime manually, then run MdReader setup again.', [ExitCode]);
    Exit;
  end;
  if not DesktopRuntimeInstalled then begin
    Result := 'The x64 .NET 9 Desktop Runtime was not detected after installation. ' +
      'Restart Windows if requested, then run MdReader setup again.';
    Exit;
  end;
  RuntimeRestartRequired := ExitCode = 3010;
  { Setup can finish copying the app before requesting the restart. }
end;

function NeedRestart: Boolean;
begin
  Result := RuntimeRestartRequired;
end;

function CanLaunchApplication: Boolean;
begin
  Result := not RuntimeRestartRequired;
end;
