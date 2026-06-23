; MdReader Inno Setup Script
; This script creates a Windows installer for MdReader

#define MyAppName "MdReader"
#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#define MyAppPublisher "MdReader"
#define MyAppURL "https://github.com/kormanm/MdReader"
#define MyAppExeName "MdReader.exe"
#define DotNetDownloadUrl "https://aka.ms/dotnet/9.0/windowsdesktop-runtime-win-x64.exe"

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
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
; Main executable and DLLs
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
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]

// Import URLDownloadToFile from urlmon.dll for downloading the .NET runtime
function URLDownloadToFile(pCaller: IUnknown; szURL: WideString;
    szFileName: WideString; dwReserved: LongWord; lpfnCB: IUnknown): HResult;
    external 'URLDownloadToFileW@urlmon.dll stdcall';

// Returns True if .NET 9 Windows Desktop Runtime is already installed
function IsDotNet9Installed(): Boolean;
var
  SubkeyNames: TArrayOfString;
  I: Integer;
begin
  Result := False;
  if RegGetSubkeyNames(HKEY_LOCAL_MACHINE,
      'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App',
      SubkeyNames) then
  begin
    for I := 0 to GetArrayLength(SubkeyNames) - 1 do
      if Copy(SubkeyNames[I], 1, 2) = '9.' then
      begin
        Result := True;
        Exit;
      end;
  end;
end;

// Called by Inno Setup before the installation begins.
// Downloads and installs .NET 9 Desktop Runtime when it is missing.
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  TempFile: String;
  ResultCode: Integer;
  DownloadResult: HResult;
begin
  Result := '';
  NeedsRestart := False;

  if IsDotNet9Installed() then
    Exit;

  if MsgBox('.NET 9.0 Windows Desktop Runtime is required to run MdReader.' + #13#10#13#10 +
            'It will be downloaded and installed automatically.' + #13#10 +
            'Click OK to continue or Cancel to abort.',
            mbConfirmation, MB_OKCANCEL) <> IDOK then
  begin
    Result := 'Installation cancelled. .NET 9.0 Windows Desktop Runtime is required.';
    Exit;
  end;

  TempFile := ExpandConstant('{tmp}\windowsdesktop-runtime-9.0-win-x64.exe');

  WizardForm.StatusLabel.Caption := 'Downloading .NET 9.0 Windows Desktop Runtime...';
  DownloadResult := URLDownloadToFile(nil, '{#DotNetDownloadUrl}', TempFile, 0, nil);

  if DownloadResult <> 0 then
  begin
    if MsgBox('Failed to download .NET 9.0 Windows Desktop Runtime.' + #13#10#13#10 +
              'Please install it manually, then run this installer again.' + #13#10#13#10 +
              'Click OK to open the download page in your browser.',
              mbError, MB_OKCANCEL) = IDOK then
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/9.0',
                '', '', SW_SHOW, ewNoWait, ResultCode);
    Result := 'Please install .NET 9.0 Windows Desktop Runtime and run this installer again.';
    Exit;
  end;

  WizardForm.StatusLabel.Caption := 'Installing .NET 9.0 Windows Desktop Runtime...';
  if not Exec(TempFile, '/install /quiet /norestart', '', SW_SHOW,
              ewWaitUntilTerminated, ResultCode) then
  begin
    Result := '.NET 9.0 Windows Desktop Runtime could not be launched.';
    Exit;
  end;

  // Exit code 3010 means success but a restart is needed
  if ResultCode = 3010 then
    NeedsRestart := True
  else if ResultCode <> 0 then
    Result := '.NET 9.0 Windows Desktop Runtime installation failed (exit code ' +
              IntToStr(ResultCode) + '). Please install it manually.';
end;
