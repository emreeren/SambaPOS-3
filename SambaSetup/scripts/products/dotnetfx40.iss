[CustomMessages]
dotnetfx40_title=.NET Framework 4.0

en.dotnetfx40_size=48.1 MB
de.dotnetfx40_size=48,1 MB

[Code]
const
	dotnetfx40_url = 'http://download.microsoft.com/download/9/5/A/95A9616B-7A37-4AF6-BC36-D6EA96C8DAAE/dotNetFx40_Full_x86_x64.exe';

procedure dotnetfx40();
var
	version: cardinal;
begin
	RegQueryDWordValue(HKLM, 'Software\Microsoft\NET Framework Setup\NDP\v4\Full', 'Install', version);
	if version <> 1 then
		AddProduct('dotNetFx40_Full_x86_x64.exe',
			'/q:a /t:' + ExpandConstant('{tmp}{\}') + 'dotNetFx40_Full_x86_x64.exe /c:"install /qb /l"',
			CustomMessage('dotnetfx40_title'),
			CustomMessage('dotnetfx40_size'),
			dotnetfx40_url);
end;
