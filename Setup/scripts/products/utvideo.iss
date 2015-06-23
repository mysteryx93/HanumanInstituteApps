// requires Windows 7, Windows 7 Service Pack 1, Windows Server 2003 Service Pack 2, Windows Server 2008, Windows Server 2008 R2, Windows Server 2008 R2 SP1, Windows Vista Service Pack 1, Windows XP Service Pack 3
// requires Windows Installer 3.1 or later
// requires Internet Explorer 5.01 or later
// http://www.microsoft.com/downloads/en/details.aspx?FamilyID=9cfb2d51-5ff4-4491-b0e5-b386f32c0992

[CustomMessages]
utvideo_title=UT Video Codec Suite
utvideo_size=0.6 MB

[Code]
const
	utvideo_url = 'http://www.videohelp.com/download/utvideo-15.2.0-win.exe';

procedure utvideo();
var
	version: cardinal;
begin
  if not RegKeyExists(HKEY_CLASSES_ROOT, 'AppID\UtVideoMFT.dll') then
  begin
		if (not IsIA64()) then
			AddProduct('utvideo.exe',
				'',
				CustomMessage('utvideo_title'),
				CustomMessage('utvideo_size'),
        utvideo_url,
				false, false);
	end;
end;