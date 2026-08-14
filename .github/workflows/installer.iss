#define AppName "__APP_NAME__"
#define AppInternal "__APP_INTERNAL__"
#define AppVersion "__APP_VERSION__"
#define PublishDir "__PUBLISH_DIR__"
#define OutputDir "__OUTPUT_DIR__"
#define OutputFile "__OUTPUT_FILE__"
#define IconFile "__ICON_FILE__"
#define X64 "__X64__"

[Setup]
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} v{#AppVersion}
UninstallDisplayName={#AppName}
AppPublisher=Hanuman Institute
#if X64 == "1"
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible
#endif
SetupIconFile={#IconFile}
DefaultDirName={autopf}\Hanuman Institute\{#AppInternal}
DefaultGroupName=Hanuman Institute
OutputDir={#OutputDir}
OutputBaseFilename={#OutputFile}
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Hanuman Institute\{#AppName}"; Filename: "{app}\{#AppInternal}.exe"

[Run]
Filename: "{app}\{#AppInternal}.exe"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent
