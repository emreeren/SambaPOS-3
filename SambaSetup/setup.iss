#include "scripts\products.iss"

#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"
#include "scripts\products\msi31.iss"
#include "scripts\lgpl.iss"
#include "scripts\products\ssce40.iss"
#include "scripts\products\dotnetfx40client.iss"
#include "scripts\products\wic.iss"

[CustomMessages]
win2000sp3_title=Windows 2000 Service Pack 3
winxpsp2_title=Windows XP Service Pack 2
winxpsp3_title=Windows XP Service Pack 3
en.full_setup=Full Setup
en.compact_setup=Compact Setup
en.custom_setup=Custom Setup
en.sample_data=Sample Data
en.handheld_terminal_app=Handheld terminal app
en.ce_install_sp3_required=Compact SQL 4.0 removed from packages list because Service Pack 3 required for Compact SQL 4.0 installation. Program will run with TXT database.

en.InstallService=install WindowsService %1
en.StartService=start WindowsService %1

tr.full_setup=Tam Kurulum
tr.compact_setup=Normal Kurulum
tr.custom_setup=Özel Kurulum
tr.sample_data=Örnek Veri
tr.handheld_terminal_app=El terminali uygulamasý
tr.ce_install_sp3_required=Compact SQL 4.0 çalýþtýrmak için Service Pack 3 gerektiðinden kurulum listesinden kaldýrýldý. Program TXT dosya veritabaný üzerinden çalýþacak.

#define Version "3.0.35"
#define FileVersion "3035"
#define DbVersion "24"

#define VersionInfo "3.0.35 BETA"
#define VersionTime GetDateTimeString('yyyy-mm-dd hhnn', '-', ':');

[Setup]
AppName=SambaPOS
Uninstallable=true
DirExistsWarning=no
CreateAppDir=true
OutputDir=bin
OutputBaseFilename=SambaSetup{#FileVersion}_Beta
SourceDir=.
AppCopyright=Copyright © Açýk Yazýlým Platformu 2013
AppVerName=Samba POS {#VersionInfo}

DefaultGroupName=SambaPOS3
AllowNoIcons=true
AppPublisher=Açýk Yazýlým
AppVersion={#VersionInfo}
UninstallDisplayIcon={app}\Samba.Presentation.exe
UninstallDisplayName=SambaPOS3
UsePreviousGroup=true
UsePreviousAppDir=true
DefaultDirName={pf}\SambaPOS3
VersionInfoVersion={#Version}
VersionInfoCompany=Açýk Yazýlým
VersionInfoCopyright=Copyright © AYP 2010
ShowUndisplayableLanguages=false
LanguageDetectionMethod=locale
InternalCompressLevel=fast
SolidCompression=true
Compression=lzma/fast

;required by products
PrivilegesRequired=admin
ArchitecturesAllowed=
VersionInfoProductName=Samba POS Setup
AppID={{9447659F-1795-44B2-B8A2-E0FA049A5F6E}


[Languages]
Name: en; MessagesFile: compiler:Default.isl
Name: de; MessagesFile: compiler:Languages\German.isl
Name: tr; MessagesFile: compiler:Languages\Turkish.isl

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}; Languages: ; Components: 
Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked

[Files]
Source: src\EntityFramework.dll; DestDir: {app}
Source: src\Microsoft.Practices.Prism.dll; DestDir: {app}
Source: src\Microsoft.Practices.Prism.MefExtensions.dll; DestDir: {app}
Source: src\Microsoft.Practices.Prism.Interactivity.dll; DestDir: {app}
Source: src\Microsoft.Practices.ServiceLocation.dll; DestDir: {app}
Source: src\PropertyTools.dll; DestDir: {app}; Flags: ignoreversion
Source: src\PropertyTools.Wpf.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Domain.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Infrastructure.Data.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Infrastructure.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.MessagingServer.exe; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.MessagingServerServiceTool.exe; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.MessagingServer.WindowsService.exe; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.AccountModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.AutomationModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.BasicReports.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.BasicReports.pdb; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.DepartmentModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.EntityModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.InventoryModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.LoginModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.ManagementModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.MarketModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.MenuModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.ModifierModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.NavigationModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.PaymentModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.PosModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.PrinterModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.SettingsModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.TaskModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.TicketModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.UserModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.WorkperiodModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Persistance.DBMigration.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Persistance.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Presentation.Common.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Presentation.Controls.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Presentation.exe; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Presentation.exe.config; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Presentation.exe.manifest; DestDir: {app}; Flags: ignoreversion
Source: src\AxInterop.cidv5callerid.dll; DestDir: {app}; Flags: ignoreversion; Components: cid
Source: src\Interop.cidv5callerid.dll; DestDir: {app}; Flags: ignoreversion; Components: cid
Source: src\Samba.Modules.CidMonitor.dll; DestDir: {app}; Flags: ignoreversion; Components: cid
Source: src\Samba.Presentation.ViewModels.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Presentation.Services.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Services.dll; DestDir: {app}; Flags: ignoreversion
Source: src\System.Windows.Interactivity.dll; DestDir: {app}
Source: src\Images\apple-icon.png; DestDir: {app}\Images\
Source: src\Images\apple.ico; DestDir: {app}\Images\
Source: src\Images\empty.png; DestDir: {app}\Images\
Source: src\Images\logo.png; DestDir: {app}\Images\; Flags: onlyifdoesntexist
Source: src\Imports\menu.txt; DestDir: {app}\Imports\; Components: veri
Source: src\Imports\table.txt; DestDir: {app}\Imports\; Components: veri
Source: src\Imports\menu_tr.txt; DestDir: {app}\Imports\; Components: veri
Source: src\Imports\table_tr.txt; DestDir: {app}\Imports\; Components: veri
Source: src\FlexButton.dll; DestDir: {app}; Flags: ignoreversion
Source: src\FastButton.dll; DestDir: {app}; Flags: ignoreversion
Source: src\DataGridFilterLibrary.dll; DestDir: {app}; Flags: ignoreversion
Source: src\FluentValidation.dll; DestDir: {app}; Flags: ignoreversion
Source: src\FluentMigrator.dll; DestDir: {app}; Flags: ignoreversion
Source: src\FluentMigrator.Runner.dll; DestDir: {app}; Flags: ignoreversion
Source: src\GongSolutions.Wpf.DragDrop.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Omu.ValueInjecter.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Gma.QrCodeNet.Encoding.Net35.dll; DestDir: {app}; Flags: ignoreversion
Source: src\ICSharpCode.AvalonEdit.dll; DestDir: {app}; Flags: ignoreversion
Source: src\FluentScript.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Stateless.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Zen.Barcode.Core.dll; DestDir: {app}; Flags: ignoreversion
Source: src\migrate.txt; DestDir: {userappdata}\Ozgu Tech\SambaPOS3; Flags: ignoreversion
Source: C:\Windows\Fonts\lucon.ttf; DestDir: {fonts}; Flags: onlyifdoesntexist uninsneveruninstall; FontInstall: Lucida Console
Source: src\Samba.Localization.dll; DestDir: {app}; Flags: ignoreversion
Source: src\tr\Samba.Localization.resources.dll; DestDir: {app}\tr\; Flags: ignoreversion
Source: src\it\Samba.Localization.resources.dll; DestDir: {app}\it\; Flags: ignoreversion
Source: src\pt-BR\Samba.Localization.resources.dll; DestDir: {app}\pt-BR\; Flags: ignoreversion
Source: src\hr\Samba.Localization.resources.dll; DestDir: {app}\hr\; Flags: ignoreversion
Source: src\ar\Samba.Localization.resources.dll; DestDir: {app}\ar\; Flags: ignoreversion
Source: src\hu\Samba.Localization.resources.dll; DestDir: {app}\hu\; Flags: ignoreversion
Source: src\es\Samba.Localization.resources.dll; DestDir: {app}\es\; Flags: ignoreversion
Source: src\id\Samba.Localization.resources.dll; DestDir: {app}\id\; Flags: ignoreversion
Source: src\el\Samba.Localization.resources.dll; DestDir: {app}\el\; Flags: ignoreversion
Source: src\zh-CN\Samba.Localization.resources.dll; DestDir: {app}\zh-CN\; Flags: ignoreversion
Source: src\sq\Samba.Localization.resources.dll; DestDir: {app}\sq\; Flags: ignoreversion
Source: src\de\Samba.Localization.resources.dll; DestDir: {app}\de\; Flags: ignoreversion
Source: src\cs\Samba.Localization.resources.dll; DestDir: {app}\cs\; Flags: ignoreversion
Source: src\nl\Samba.Localization.resources.dll; DestDir: {app}\nl\; Flags: ignoreversion
Source: src\he\Samba.Localization.resources.dll; DestDir: {app}\he\; Flags: ignoreversion
Source: src\fr\Samba.Localization.resources.dll; DestDir: {app}\fr\; Flags: ignoreversion
Source: src\ru-RU\Samba.Localization.resources.dll; DestDir: {app}\ru-RU\; Flags: ignoreversion
Source: src\da\Samba.Localization.resources.dll; DestDir: {app}\da\; Flags: ignoreversion
Source: src\fa\Samba.Localization.resources.dll; DestDir: {app}\fa\; Flags: ignoreversion
Source: src\tk-TM\Samba.Localization.resources.dll; DestDir: {app}\tk-TM\; Flags: ignoreversion
                                                              
[Components]
Name: pos; Description: Samba POS; Types: full compact custom; Flags: fixed
Name: terminal; Description: {cm:handheld_terminal_app}; Languages: ; Types: full
Name: sqlce; Description: Compact SQL 4.0; Languages: ; Types: full compact custom
Name: veri; Description: {cm:sample_data}; Languages: ; Types: full compact custom
Name: cid; Description: Caller Id; Languages: ; Types: full custom

[Types]
Name: compact; Description: {cm:compact_setup}
Name: full; Description: {cm:full_setup}
Name: custom; Description: {cm:custom_setup}; Flags: iscustom

[Icons]
Name: {group}\Samba POS 3; Filename: {app}\Samba.Presentation.exe
Name: {group}\{cm:UninstallProgram,Samba POS}; Filename: {uninstallexe}
Name: {commondesktop}\SambaPOS3; Filename: {app}\Samba.Presentation.exe; IconIndex: 0; Flags: createonlyiffileexists; Components: 
Name: {commondesktop}\Samba Terminal; Filename: {app}\Samba.Presentation.Terminal.exe; Flags: createonlyiffileexists
Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\Samba POS; Filename: {app}\Samba.Presentation.exe; Tasks: quicklaunchicon
Name: {group}\Samba Data; Filename: {commonappdata}\Ozgu Tech\SambaPOS3\

[Run]
Filename: {app}\Samba.Presentation.exe; Description: {cm:LaunchProgram,Samba POS}; Flags: nowait postinstall skipifsilent unchecked
Filename: {app}\Samba.MessagingServerServiceTool.exe; Description: {cm:LaunchProgram,MessagingServer ServiceTool}; Flags: nowait postinstall skipifsilent unchecked runascurrentuser
Filename: {app}\Samba.MessagingServer.exe; Description: {cm:LaunchProgram,MessagingServer (Standalone)}; Flags: nowait postinstall skipifsilent unchecked
;Filename: {app}\Samba.MessagingServer.WindowsService.exe; Parameters: "--install"; Description: {cm:InstallService,MessagingServer.WindowsService}; Flags: nowait postinstall skipifsilent unchecked*/
;Filename: {app}\Samba.MessagingServer.WindowsService.exe; Parameters: "--start"; Description: {cm:StartService,MessagingServer.WindowsService}; Flags: nowait postinstall skipifsilent unchecked

;[UninstallRun]
;Filename: {app}\Samba.MessagingServer.exe; Parameters: "--uninstall"; Flags: runascurrentuser

[Code]
function CreateVersion(): boolean;
var
  fileName : string;
  lines : TArrayOfString;
begin
  Result := true;
  fileName := ExpandConstant('{commonappdata}\Ozgu Tech\SambaPOS3\version.dat');
  SetArrayLength(lines, 5);
  lines[0] := ExpandConstant('Version={#Version}');
  lines[1] := ExpandConstant('FileVersion={#FileVersion}');
  lines[2] := ExpandConstant('DbVersion={#DbVersion}');
  lines[3] := ExpandConstant('AppVersion={#VersionInfo}');
  lines[4] := ExpandConstant('VersionTime={#VersionTime}');
  
  Result := SaveStringsToFile(filename,lines,false);
  
  exit;
end;
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if  CurStep=ssPostInstall then
    begin
         CreateVersion();
    end
end;
 
procedure CurPageChanged(CurPageID: Integer);
begin
if (CurPageId = wpSelectProgramGroup) then
  begin
    RemoveProducts();
    msi31('3.0');
    wic();
    dotnetfx40client();
    if IsComponentSelected('sqlce') then
    if not minwinspversion(5, 1, 3) then begin
		MsgBox(FmtMessage(CustomMessage('ce_install_sp3_required'), [CustomMessage('winxpsp3_title')]), mbError, MB_OK);
    end else begin
		ssce40();
    end;
  end;
end;

procedure InitializeWizard();
begin
  LGPL_InitializeWizard();
end;

function InitializeSetup(): Boolean;
begin
	initwinversion();

	if not minwinspversion(5, 0, 3) then begin
		MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('win2000sp3_title')]), mbError, MB_OK);
		exit;
	end;
	if not minwinspversion(5, 1, 2) then begin
		MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('winxpsp2_title')]), mbError, MB_OK);
		exit;
	end;

	//if (not iis()) then exit;

	//msi20('2.0');

	//ie6('5.0.2919');

	//dotnetfx11();
	//dotnetfx11lp();
	//dotnetfx11sp1();

	//kb835732();

	//if (minwinversion(5, 0) and minspversion(5, 0, 4)) then begin
	//	dotnetfx20sp1();
		//dotnetfx20sp1lp();
	//end else begin
	//	dotnetfx20();
		//dotnetfx20lp();
	//end;

	//dotnetfx35();
	//dotnetfx35lp();
	//dotnetfx35sp1();
	//dotnetfx35sp1lp();

	//mdac28('2.7');
	//jet4sp8('4.0.8015');

  Result := true;
end;

[Dirs]
Name: {app}\Images
Name: {app}\Imports
Name: {app}\tr
Name: {app}\it
Name: {app}\pt-BR
Name: {app}\hr
Name: {app}\ar
Name: {app}\hu
Name: {app}\es
Name: {app}\id
Name: {app}\el
Name: {app}\zh-CN
Name: {app}\sq
Name: {app}\de
Name: {app}\cs
Name: {app}\nl
Name: {app}\he
Name: {app}\fr
Name: {app}\ru-RU

Name: {commonappdata}\Ozgu Tech\SambaPOS3

[Registry]
Root: HKLM; Subkey: SOFTWARE\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION; ValueType: dword; ValueName: samba.presentation.exe; ValueData: 10000

