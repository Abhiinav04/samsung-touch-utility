; Inno Setup Script for Samsung Touchscreen Utility
#define MyAppName "Samsung Touchscreen Utility"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Autonomous Windows Engineering"
#define MyAppExeName "SamsungTouchUtility.exe"

[Setup]
AppId={{E697C118-2FA8-4D26-9E9A-A37289C19901}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\SamsungTouchUtility
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=..\release\installer
OutputBaseFilename=SamsungTouchUtility-Setup-1.0.0
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
DisableProgramGroupPage=yes

[Tasks]
Name: "startwithwindows"; Description: "Start Samsung Touch Utility automatically on Windows startup"; GroupDescription: "Startup Options:"; Flags: unchecked

[Files]
Source: "..\release\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

[Registry]
; Optional Windows Run key for startup (disabled by default)
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "SamsungTouchUtility"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: startwithwindows

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName} in system tray"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\SamsungTouchUtility\logs"
Type: filesandordirs; Name: "{localappdata}\SamsungTouchUtility"
