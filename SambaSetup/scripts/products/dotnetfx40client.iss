[CustomMessages]
dotnetfx40_title=.NET Framework 4.0 Client Profile

en.dotnetfx40_size=48.1 MB
de.dotnetfx40_size=48,1 MB

[Code]
const
	dotnetfx40_url = 'http://download.microsoft.com/download/5/6/2/562A10F9-C9F4-4313-A044-9C94E0A8FAC8/dotNetFx40_Client_x86_x64.exe';

procedure dotnetfx40client();
var
	version: cardinal;
begin
	RegQueryDWordValue(HKLM, 'Software\Microsoft\NET Framework Setup\NDP\v4\Client', 'Install', version);
	if version <> 1 then
		AddProduct('dotNetFx40_Client_x86_x64.exe',
      '/q:a /t:' + ExpandConstant('{tmp}{\}') + 'dotNetFx40_Client_x86_x64.exe /c:"install /qb /l"',
			CustomMessage('dotnetfx40_title'),
			CustomMessage('dotnetfx40_size'),
			dotnetfx40_url,true,true);
end;
