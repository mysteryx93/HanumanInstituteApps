// requires Windows 7, Windows Server 2003, Windows Server 2008, Windows Server 2008 R2, Windows Vista, Windows XP
// requires Microsoft .NET Framework 3.5 SP 1 or later
// requires Windows Installer 4.5 or later
// SQL Server Express is supported on x64 and EMT64 systems in Windows On Windows (WOW). SQL Server Express is not supported on IA64 systems
// SQLEXPR32.EXE is a smaller package that can be used to install SQL Server Express on 32-bit operating systems only. The larger SQLEXPR.EXE package supports installing onto both 32-bit and 64-bit (WOW install) operating systems. There is no other difference between these packages.
// http://www.microsoft.com/download/en/details.aspx?id=3743

[CustomMessages]
sql2014localdb_title=SQL Server 2014 LocalDB

en.sql2014localdb_size=36.6 MB
;de.sql2014localdb_size=36,6 MB

en.sql2014localdb_size_x64=43.1 MB
;de.sql2014localdb_size_x64=43,1 MB


[Code]
const

	sql2014localdb_url = 'http://care.dlservice.microsoft.com/dl/download/E/A/E/EAE6F7FC-767A-4038-A954-49B8B05D04EB/LocalDB%2032BIT/SqlLocalDB.msi';
	sql2014localdb_url_x64 = 'http://care.dlservice.microsoft.com/dl/download/E/A/E/EAE6F7FC-767A-4038-A954-49B8B05D04EB/LocalDB%2064BIT/SqlLocalDB.msi';

procedure sql2014localdb();
var
	version: string;
begin
  if not RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions\12.0') then
  begin
		if (not IsIA64()) then
			AddProduct('SqlLocalDB.msi',
				'/QB IACCEPTSQLLOCALDBLICENSETERMS=YES',
				CustomMessage('sql2014localdb_title'),
				CustomMessage('sql2014localdb_size' + GetArchitectureString()),
				GetString(sql2014localdb_url, sql2014localdb_url_x64, ''),
				false, false);
	end;
end;
