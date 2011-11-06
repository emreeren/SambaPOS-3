[CustomMessages]
ssce40x86_title=SQL Server Compact 4.0 for Windows Destkop (x86)
ssce40x64_title=SQL Server Compact 4.0 for Windows Destkop (x64)
 
ssce40_size=2.5 MB
 

[Code]
const
	ssce40x86_url = 'http://download.microsoft.com/download/0/5/D/05DCCDB5-57E0-4314-A016-874F228A8FAD/SSCERuntime_x86-ENU.exe';
  ssce40x64_url = 'http://download.microsoft.com/download/0/5/D/05DCCDB5-57E0-4314-A016-874F228A8FAD/SSCERuntime_x64-ENU.exe';

procedure ssce40();
var
	version: cardinal;
begin
	if not RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Microsoft SQL Server Compact Edition\v4.0') then
  begin
    if (IsWin64) then begin
      AddProduct('SSCERuntime_x64-ENU.exe',
        '/i /quiet /norestart',
        CustomMessage('ssce40x64_title'),
        CustomMessage('ssce40_size'),
        ssce40x64_url,false,false);
    end else begin
      AddProduct('SSCERuntime_x86-ENU.exe',
        '/i /quiet /norestart',
        CustomMessage('ssce40x86_title'),
        CustomMessage('ssce40_size'),
        ssce40x86_url,false,false);
    end;
  end;
end;