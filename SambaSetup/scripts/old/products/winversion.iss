[Code]
var
	WindowsVersion: TWindowsVersion;
	
function minwinversion(MajorVersion, MinorVersion: integer): boolean;
begin
	Result := (WindowsVersion.Major >= MajorVersion) and (WindowsVersion.Minor >= MinorVersion)
end;

function maxwinversion(MajorVersion, MinorVersion: integer): boolean;
begin
	Result := (WindowsVersion.Major <= MajorVersion) and (WindowsVersion.Minor <= MinorVersion)
end;

function exactwinversion(MajorVersion, MinorVersion: integer): boolean;
begin
	Result := (WindowsVersion.Major = MajorVersion) and (WindowsVersion.Minor = MinorVersion)
end;

function minspversion(MajorVersion, MinorVersion, SpVersion: integer): boolean;
begin
	if (exactwinversion(MajorVersion, MinorVersion)) then
	   Result := (WindowsVersion.ServicePackMajor >= SpVersion)
	else
	   Result := true;
end;

function maxspversion(MajorVersion, MinorVersion, SpVersion: integer): boolean;
begin
	if (exactwinversion(MajorVersion, MinorVersion)) then
	   Result := (WindowsVersion.ServicePackMajor <= SpVersion)
	else
	   Result := true;
end;

function exactspversion(MajorVersion, MinorVersion, SpVersion: integer): boolean;
begin
	if (exactwinversion(MajorVersion, MinorVersion)) then
	   Result := (WindowsVersion.ServicePackMajor = SpVersion)
	else
	   Result := true;
end;


procedure initwinversion();
begin
	GetWindowsVersionEx(WindowsVersion);
end;