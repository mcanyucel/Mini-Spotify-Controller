#define MyAppName "Mini Spotify Controller"
#define MyAppVersion "3.0.0.0"
#define MyAppPublisher "MCY"
#define MyAppExeName "Mini Spotify Controller.exe"

[Setup]
AppId={{7484317c-3ccd-4d40-8fa1-bcb3e9cf81c0}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=LICENSE.txt
OutputDir=Output
OutputBaseFilename=MiniSpotifyControllerSetup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "PublishOutput\*"; Excludes: "*.pdb"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Code]
function HasDotNet8: Boolean;
var
  Path: string;
  FindRec: TFindRec;
begin
  Result := False;
  Path := ExpandConstant('{commonpf}\dotnet\shared\Microsoft.WindowsDesktop.App');
  
  if DirExists(Path) then
  begin
    if FindFirst(Path + '\8.*', FindRec) then
    begin
      try
        Result := True;
      finally
        FindClose(FindRec);
      end;
    end;
  end;
end;

function InitializeSetup: Boolean;
begin
    Result := False;
    
    if not HasDotNet8 then
    begin
        MsgBox('.NET 8.0 Desktop Runtime is required to run this application.' + #13#10 +
               'Please install it from https://dotnet.microsoft.com/download/dotnet/8.0', 
               mbCriticalError, MB_OK);
        Exit;
    end;
    
    Result := True;
end;