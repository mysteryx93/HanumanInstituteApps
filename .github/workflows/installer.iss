#define AppName "__APP_NAME__"
#define AppInternal "__APP_INTERNAL__"
#define AppVersion "__APP_VERSION__"
#define PublishDir "__PUBLISH_DIR__"
#define OutputDir "__OUTPUT_DIR__"
#define OutputFile "__OUTPUT_FILE__"

[Setup]
AppId={{YOUR-STABLE-APP-ID}}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} v{#AppVersion}
UninstallDisplayName={#AppName}
__ARCH_FLAGS__
DefaultDirName={autopf}\Hanuman Institute\{#AppName}
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
