[CustomMessages]
wic_title=Windows Imaging Component

en.wic_size=1.2 MB
de.wic_size=1.2 MB

[Code]
const
	wic_url = 'http://download.microsoft.com/download/f/f/1/ff178bb1-da91-48ed-89e5-478a99387d4f/wic_x86_enu.exe';
procedure wic();
var
		installed: boolean;
begin
	installed := FileExists(GetEnv('windir') + '\system32\windowscodecs.dll');
	if not installed then begin
		AddProduct('wic_x86_enu.exe',
			'/q',
			CustomMessage('wic_title'),
			CustomMessage('wic_size'),
			wic_url,
			false,
			false);
	end;
end;
