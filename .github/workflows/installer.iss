#define AppName "__APP_NAME__"
#define AppVersion "__APP_VERSION__"
#define PublishDir "__PUBLISH_DIR__"
#define OutputDir "__OUTPUT_DIR__"
#define OutputFile "__OUTPUT_FILE__"

[Setup]
AppId={{YOUR-STABLE-APP-ID}}
AppName={#AppName}
AppVersion={#AppVersion}

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
Name: "{autoprograms}\Hanuman Institute\{#AppName}"; Filename: "{app}\{#AppName}.exe"

[Run]
Filename: "{app}\{#AppName}.exe"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent
