[Setup]
AppName=Asteroids
AppVersion=1.0.0
DefaultDirName={autopf}\Radish\Asteroids
DefaultGroupName=Radish
SetupIconFile=images\Asteroid3.ico
UninstallDisplayIcon={app}\Asteroids.exe
LicenseFile=LICENSE.txt
OutputBaseFilename=AsteroidsSetup
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible
AppPublisher=Radish
AppPublisherURL=https://radish-vert.vercel.app
AppId={{cc6249ab-a099-4e9d-a741-1547ffebd1c2}

[Files]
Source: "bin\Release\net10.0-windows\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Asteroids"; Filename: "{app}\Asteroids.exe"
Name: "{commondesktop}\Asteroids"; Filename: "{app}\Asteroids.exe"; Tasks: desktopicon

[Tasks]
Name: desktopicon; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\Asteroids.exe"; Description: "Launch Asteroids"; Flags: nowait postinstall skipifsilent
