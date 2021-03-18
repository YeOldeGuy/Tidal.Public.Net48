; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Tidal"
#define MyAppVersion "1.1.0.0"
#define MyAppPublisher "Keith Walker"
#define MyAppExeName "Tidal.exe"
#define BinDir ".\Tidal\bin\Release"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
AppId={{73942E11-CF52-42A8-9A5F-99CB2719D551}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={commonpf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=.\Install
OutputBaseFilename=TidalInstall
SetupIconFile=.\Transmission.ico
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "assocmagnets"; Description: "Associate &Magnet Files"; GroupDescription: "File Associations:"
Name: "assoctorrents"; Description: "Associate &Torrent Files"; GroupDescription: "File Associations:"

[Files]
Source: "{#BinDir}\Tidal.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BinDir}\*.dll"; DestDir: "{app}"; Flags: ignoreversion
;Source: "{#BinDir}\*.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BinDir}\*.config"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]
Root: HKLM; Subkey: "Software\Classes\{#MyAppName}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\Classes\Magnet"; ValueType: none; Flags: uninsdeletekey

Root: HKLM; Subkey: "Software\Classes\Magnet"; ValueName: ""; ValueType:string; ValueData: "Magnet URI"; Tasks: assocmagnets
Root: HKLM; Subkey: "Software\Classes\Magnet"; ValueName: "Content Type"; ValueType:string; ValueData: "application/x-type"; Tasks: assocmagnets
Root: HKLM; Subkey: "Software\Classes\Magnet"; ValueName: "URL Protocol"; ValueType:string; ValueData:""; Tasks: assocmagnets
Root: HKLM; Subkey: "Software\Classes\Magnet\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: assocmagnets
Root: HKLM; Subkey: "Software\Classes\Magnet\shell\open\command"; ValueType: string; ValueName:""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: assocmagnets

Root: HKLM; Subkey:"Software\Classes\.torrent"; ValueName:""; ValueType:string; ValueData: "{#MyAppName}"; Tasks:assoctorrents
Root: HKLM; Subkey:"Software\Classes\{#MyAppName}\DefaultIcon"; ValueName:""; ValueType:string; ValueData: "{app}\{#MyAppExeName},0"; Tasks:assoctorrents
Root: HKLM; Subkey:"Software\Classes\{#MyAppName}\shell\open\command"; ValueType:string; ValueName:""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: assoctorrents 

