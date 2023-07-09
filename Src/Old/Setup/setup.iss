;contribute on github.com/stfx/innodependencyinstaller or codeproject.com/Articles/20868/NET-Framework-1-1-2-0-3-5-Installer-for-InnoSetup

;comment out product defines to disable installing them
;#define use_iis
; #define use_kb835732

; #define use_msi20
; #define use_msi31
#define use_msi45

; #define use_ie6

; #define use_dotnetfx11
; #define use_dotnetfx11lp

; #define use_dotnetfx20
; #define use_dotnetfx20lp

; #define use_dotnetfx35
; #define use_dotnetfx35lp

; #define use_dotnetfx40
; #define use_wic

#define use_dotnetfx46
#define use_utvideo

; #define use_vc2010

; #define use_mdac28
; #define use_jet4sp8

; #define use_sqlcompact35sp2

; #define use_sql2005express
; #define use_sql2008express
; #define use_sql2014localdb

#define MyAppSetupName 'Natural Grounding Player'
#define MyAppVersion '1.4'

[Setup]
AppName={#MyAppSetupName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppSetupName} {#MyAppVersion}
AppCopyright=Copyright © 2016-2017, Spiritual Self Transformation
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany=Spiritual Self Transformation
AppPublisher=Spiritual Self Transformation
AppPublisherURL=https://www.spiritualselftransformation.com
;AppSupportURL=http://...
;AppUpdatesURL=http://...
;OutputBaseFilename={#MyAppSetupName}-{#MyAppVersion}
OutputBaseFilename=natural-grounding-player
DefaultGroupName={#MyAppSetupName}
DefaultDirName={pf}\{#MyAppSetupName}
UninstallDisplayIcon={app}\NaturalGroundingPlayer.exe
OutputDir=bin
SourceDir=.
AllowNoIcons=yes
;SetupIconFile=MyProgramIcon
SolidCompression=yes

;MinVersion default value: "0,5.0 (Windows 2000+) if Unicode Inno Setup, else 4.0,4.0 (Windows 95+)"
;MinVersion=0,5.0
PrivilegesRequired=admin
;ArchitecturesAllowed=x86 x64
;ArchitecturesInstallIn64BitMode=x64
ShowLanguageDialog=no
LicenseFile=LICENSE.md

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\NaturalGroundingPlayer\bin\Release\NaturalGroundingPlayer.exe"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\NaturalGroundingPlayer.exe.config"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\AxInterop.WMPLib.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\Business.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\DataAccess.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\EntityFramework.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\EntityFramework.SqlServer.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\License.txt"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\System.Data.SQLite.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\System.Data.SQLite.EF6.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\System.Data.SQLite.Linq.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\x86\SQLite.Interop.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\Interop.WMPLib.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\MediaPlayer.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\MediaInfo.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\Microsoft.Threading.Tasks.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\Microsoft.Threading.Tasks.Extensions.Desktop.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\Microsoft.Threading.Tasks.Extensions.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\MPC_API_LIB.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\System.Linq.Dynamic.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\Xceed.Wpf.Toolkit.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\bin\Release\YoutubeExtractor.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\Player432hz\bin\Release\432hzPlayer.exe"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\Player432hz\bin\Release\432hzPlayer.exe.config"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\PowerliminalsPlayer\bin\Release\PowerliminalsPlayer.exe"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\PowerliminalsPlayer\bin\Release\PowerliminalsPlayer.exe.config"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\YinMediaEncoder\bin\Release\YinMediaEncoder.exe"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\YinMediaEncoder\bin\Release\YinMediaEncoder.exe.config"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\AudioVideoMuxer\bin\Release\AudioVideoMuxer.exe"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\AudioVideoMuxer\bin\Release\AudioVideoMuxer.exe.config"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\NaturalGroundingPlayer\InitialDatabase.db"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\Encoder\AviSynth.dll"; DestDir: "{app}"; Flags: replacesameversion
Source: "..\Encoder\AviSynth.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\AviSynthMT.avsi"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\avs2yuv.exe"; DestDir: "{app}\Encoder"; Flags: replacesameversion ignoreversion
Source: "..\Encoder\dfttest.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\dither.avsi"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\dither.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\edi_rpow2.avsi"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\ffmpeg.exe"; DestDir: "{app}\Encoder"; DestName: "ffmpeg.exe"; Check: IsWin64; Flags: replacesameversion ignoreversion
Source: "..\Encoder\ffmpeg-x86.exe"; DestDir: "{app}\Encoder"; DestName: "ffmpeg.exe"; Check: not IsWin64; Flags: replacesameversion ignoreversion
Source: "..\Encoder\HQDeringmod.avsi"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\InterFrame2.avsi"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\KNLMeansCL.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\KNLMeansCL-6.11.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\libfftw3f-3.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\LSMASHSource.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\masktools2.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\MedianBlur2.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\mvtools2.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\nnedi3.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\opusenc.exe"; DestDir: "{app}\Encoder"; Flags: replacesameversion ignoreversion
Source: "..\Encoder\ResizeX.avsi"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\RgTools.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\RoboCrop26.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\Shader.avsi"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\Shader.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\smdegrain.avsi"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\SmoothAdjust.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\svpflow1.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\svpflow2.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\TimeStretch.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\TWriteAVI.dll"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\UUSize4.avsi"; DestDir: "{app}\Encoder"; Flags: replacesameversion
Source: "..\Encoder\DevIL.dll"; DestDir: "{syswow64}";

Source: "Dependencies\utvideo.exe"; DestDir: "{app}"; Flags: dontcopy

[Icons]
Name: "{group}\Natural Grounding Player"; Filename: "{app}\NaturalGroundingPlayer.exe"; IconFilename: "{app}\NaturalGroundingPlayer.exe"
Name: "{group}\432hz Player"; Filename: "{app}\432hzPlayer.exe"; IconFilename: "{app}\432hzPlayer.exe"
Name: "{group}\Powerliminals Player"; Filename: "{app}\PowerliminalsPlayer.exe"; IconFilename: "{app}\PowerliminalsPlayer.exe"
Name: "{group}\Yin Media Encoder"; Filename: "{app}\YinMediaEncoder.exe"; IconFilename: "{app}\YinMediaEncoder.exe"
Name: "{group}\Audio Video Muxer"; Filename: "{app}\AudioVideoMuxer.exe"; IconFilename: "{app}\AudioVideoMuxer.exe"
Name: "{commondesktop}\Natural Grounding Player"; Filename: "{app}\NaturalGroundingPlayer.exe"; IconFilename: "{app}\NaturalGroundingPlayer.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\MyProgram"; Filename: "{app}\NaturalGroundingPlayer.exe"; IconFilename: "{app}\NaturalGroundingPlayer.exe"; Tasks: quicklaunchicon
;Name: "{group}\{cm:UninstallProgram,MyProgram}"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\NaturalGroundingPlayer.exe"; Description: "{cm:LaunchProgram,Natural Grounding Player}"; Flags: nowait postinstall skipifsilent

#include "scripts\products.iss"

#include "scripts\products\stringversion.iss"
#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"
#include "scripts\products\dotnetfxversion.iss"

#ifdef use_iis
#include "scripts\products\iis.iss"
#endif

#ifdef use_kb835732
#include "scripts\products\kb835732.iss"
#endif

#ifdef use_msi20
#include "scripts\products\msi20.iss"
#endif
#ifdef use_msi31
#include "scripts\products\msi31.iss"
#endif
#ifdef use_msi45
#include "scripts\products\msi45.iss"
#endif

#ifdef use_ie6
#include "scripts\products\ie6.iss"
#endif

#ifdef use_dotnetfx11
#include "scripts\products\dotnetfx11.iss"
#include "scripts\products\dotnetfx11sp1.iss"
#ifdef use_dotnetfx11lp
#include "scripts\products\dotnetfx11lp.iss"
#endif
#endif

#ifdef use_dotnetfx20
#include "scripts\products\dotnetfx20.iss"
#include "scripts\products\dotnetfx20sp1.iss"
#include "scripts\products\dotnetfx20sp2.iss"
#ifdef use_dotnetfx20lp
#include "scripts\products\dotnetfx20lp.iss"
#include "scripts\products\dotnetfx20sp1lp.iss"
#include "scripts\products\dotnetfx20sp2lp.iss"
#endif
#endif

#ifdef use_dotnetfx35
#include "scripts\products\dotnetfx35sp1.iss"
#ifdef use_dotnetfx35lp
#include "scripts\products\dotnetfx35sp1lp.iss"
#endif
#endif

#ifdef use_dotnetfx40
#include "scripts\products\dotnetfx40client.iss"
#include "scripts\products\dotnetfx40full.iss"
#endif

#ifdef use_dotnetfx46
#include "scripts\products\dotnetfx46.iss"
#endif

#ifdef use_utvideo
#include "scripts\products\utvideo.iss"
#endif

#ifdef use_wic
#include "scripts\products\wic.iss"
#endif

#ifdef use_vc2010
#include "scripts\products\vcredist2010.iss"
#endif

#ifdef use_mdac28
#include "scripts\products\mdac28.iss"
#endif
#ifdef use_jet4sp8
#include "scripts\products\jet4sp8.iss"
#endif

#ifdef use_sqlcompact35sp2
#include "scripts\products\sqlcompact35sp2.iss"
#endif

#ifdef use_sql2005express
#include "scripts\products\sql2005express.iss"
#endif
#ifdef use_sql2008express
#include "scripts\products\sql2008express.iss"
#endif
#ifdef use_sql2014localdb
#include "scripts\products\sql2014localdb.iss"
#endif

[CustomMessages]
win_sp_title=Windows %1 Service Pack %2

[Dirs]
Name: "{app}\Encoder"

[Registry]
Root: "HKLM32"; Subkey: "SOFTWARE\Microsoft\Multimedia\WMPlayer\Extensions\.avs\"; ValueType: string; ValueName: "PerceivedType"; ValueData: "video"; Flags: createvalueifdoesntexist
Root: "HKLM32"; Subkey: "SOFTWARE\Microsoft\Multimedia\WMPlayer\Extensions\.avs\"; ValueType: dword; ValueName: "Permissions"; ValueData: "$0000000f"; Flags: createvalueifdoesntexist
Root: "HKLM32"; Subkey: "SOFTWARE\Microsoft\Multimedia\WMPlayer\Extensions\.avs\"; ValueType: dword; ValueName: "Runtime"; ValueData: "$00000007"; Flags: createvalueifdoesntexist

[InstallDelete]
Type: files; Name: "{app}\InitialDatabase.mdf"
Type: files; Name: "{app}\InitialDatabase_log.ldf"
Type: files; Name: "{app}\Encoder\svpflow_cpu.dll"
Type: files; Name: "{app}\Encoder\svpflow_gpu.dll"

[Code]
function InitializeSetup(): boolean;
begin
	//init windows version
	initwinversion();

#ifdef use_iis
	if (not iis()) then exit;
#endif

#ifdef use_msi20
	msi20('2.0');
#endif
#ifdef use_msi31
	msi31('3.1');
#endif
#ifdef use_msi45
	msi45('4.5');
#endif
#ifdef use_ie6
	ie6('5.0.2919');
#endif

#ifdef use_dotnetfx11
	dotnetfx11();
#ifdef use_dotnetfx11lp
	dotnetfx11lp();
#endif
	dotnetfx11sp1();
#endif

	//install .netfx 2.0 sp2 if possible; if not sp1 if possible; if not .netfx 2.0
#ifdef use_dotnetfx20
	//check if .netfx 2.0 can be installed on this OS
	if not minwinspversion(5, 0, 3) then begin
		msgbox(fmtmessage(custommessage('depinstall_missing'), [fmtmessage(custommessage('win_sp_title'), ['2000', '3'])]), mberror, mb_ok);
		exit;
	end;
	if not minwinspversion(5, 1, 2) then begin
		msgbox(fmtmessage(custommessage('depinstall_missing'), [fmtmessage(custommessage('win_sp_title'), ['XP', '2'])]), mberror, mb_ok);
		exit;
	end;

	if minwinversion(5, 1) then begin
		dotnetfx20sp2();
#ifdef use_dotnetfx20lp
		dotnetfx20sp2lp();
#endif
	end else begin
		if minwinversion(5, 0) and minwinspversion(5, 0, 4) then begin
#ifdef use_kb835732
			kb835732();
#endif
			dotnetfx20sp1();
#ifdef use_dotnetfx20lp
			dotnetfx20sp1lp();
#endif
		end else begin
			dotnetfx20();
#ifdef use_dotnetfx20lp
			dotnetfx20lp();
#endif
		end;
	end;
#endif

#ifdef use_dotnetfx35
	//dotnetfx35();
	dotnetfx35sp1();
#ifdef use_dotnetfx35lp
	//dotnetfx35lp();
	dotnetfx35sp1lp();
#endif
#endif

#ifdef use_wic
	wic();
#endif

	// if no .netfx 4.0 is found, install the client (smallest)
#ifdef use_dotnetfx40
	if (not netfxinstalled(NetFx40Client, '') and not netfxinstalled(NetFx40Full, '')) then
		dotnetfx40client();
#endif

#ifdef use_dotnetfx45
    //dotnetfx45(2); // min allowed version is .netfx 4.5.2
    dotnetfx45(0); // min allowed version is .netfx 4.5.0
#endif

#ifdef use_utvideo
	utvideo();
#endif

#ifdef use_vc2010
	vcredist2010();
#endif

#ifdef use_mdac28
	mdac28('2.7');
#endif
#ifdef use_jet4sp8
	jet4sp8('4.0.8015');
#endif

#ifdef use_sqlcompact35sp2
	sqlcompact35sp2();
#endif

#ifdef use_sql2005express
	sql2005express();
#endif
#ifdef use_sql2008express
	sql2008express();
#endif
#ifdef use_sql2014localdb
	sql2014localdb();
#endif

	Result := true;
end;
