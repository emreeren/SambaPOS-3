#include "isxdl\isxdl.iss"


[CustomMessages]
DependenciesDir=kur

en.depdownload_msg=The following applications are required before setup can continue:%n%n%1%nDownload and install now?
de.depdownload_msg=Die folgenden Programme werden benötigt bevor das Setup fortfahren kann:%n%n%1%nJetzt downloaden und installieren?
tr.depdownload_msg=Kurulumun devam edebilmesi için aþaðýdaki eksik bileþenlerin bilgisayarýnýza yüklenmesi gerekiyor:%n%1%nÞimdi indirilip kurulsun mu?

en.depdownload_memo_title=Download dependencies
de.depdownload_memo_title=Abhängigkeiten downloaden
tr.depdownload_memo_title=Gerekli bileþenleri indir

en.depinstall_memo_title=Install dependencies
de.depinstall_memo_title=Abhängigkeiten installieren
tr.depinstall_memo_title=Gerekli bileþenleri kur

en.depinstall_title=Installing dependencies
de.depinstall_title=Installiere Abhängigkeiten
tr.depinstall_title=Gerekli bileþenler kuruluyor

en.depinstall_description=Please wait while Setup installs dependencies on your computer.
de.depinstall_description=Warten Sie bitte während Abhängigkeiten auf Ihrem Computer installiert wird.
tr.depinstall_description=Kurulum gerekli bileþenleri yüklüyor. Lütfen bekleyiniz.

en.depinstall_status=Installing %1...
de.depinstall_status=Installiere %1...
tr.depinstall_status=Kuruluyor %1... (Birkaç dakika sürebilir)

en.depinstall_missing=%1 must be installed before setup can continue. Please install %1 and run Setup again.
de.depinstall_missing=%1 muss installiert werden bevor das Setup fortfahren kann. Bitte installieren Sie %1 und starten Sie das Setup erneut.
tr.depinstall_missing=%1 kurulum devam etmeden önce bilgisayarýnýza kurulmasý gerekiyor. Lüften %1 yükleyin ve Kurulumu tekrar baþlatýn.

en.depinstall_error=An error occured while installing the dependencies. Please restart the computer and run the setup again or install the following dependencies manually:%n
de.depinstall_error=Ein Fehler ist während der Installation der Abghängigkeiten aufgetreten. Bitte starten Sie den Computer neu und führen Sie das Setup erneut aus oder installieren Sie die folgenden Abhängigkeiten per Hand:%n
tr.depinstall_error=Gerekli bileþenler kurulurken bir hata oluþtu. Lütfen bilgisayarýnýzý yeniden baþlatýn ve kurulumu yeniden çalýþtýrýn veya aþaðýda listelenen bileþenleri kurun:%n

de.isxdl_langfile=german2.ini
tr.isxdl_langfile=turkish.ini


[Files]
Source: "scripts\isxdl\german2.ini"; Flags: dontcopy
Source: "scripts\isxdl\turkish.ini"; Flags: dontcopy

[Code]
type
	TProduct = record
		File: String;
		Title: String;
		Parameters: String;
		InstallClean : Boolean;
		MustRebootAfter : Boolean;
	end;
	
var
	installMemo, downloadMemo, downloadMessage: string;
	products: array of TProduct;
	DependencyPage: TOutputProgressWizardPage;
	
	rebootRequired : boolean;
	rebootMessage : string;

procedure RemoveProducts();
begin
   installMemo:='';
   downloadMemo:='';
   downloadMessage:='';
   SetArrayLength(products, 0);
   isxdl_ClearFiles();
end;

procedure AddProduct(FileName, Parameters, Title, Size, URL: string; InstallClean : Boolean; MustRebootAfter : Boolean);
var
	path: string;
	i: Integer;
  x: Integer;
  canInstall: Boolean;
begin
  installMemo := installMemo + '%1' + Title + #13;
  
  path := ExpandConstant('{src}{\}') + CustomMessage('DependenciesDir') + '\' + FileName;
  if not FileExists(path) then begin
    path := ExpandConstant('{tmp}{\}') + FileName;
    
    isxdl_AddFile(URL, path);
    
    downloadMemo := downloadMemo + '%1' + Title + #13;
    downloadMessage := downloadMessage + '    ' + Title + ' (' + Size + ')' + #13;
  end;

  i := GetArrayLength(products);
  SetArrayLength(products, i + 1);
  products[i].File := path;
  products[i].Title := Title;
  products[i].Parameters := Parameters;
  products[i].InstallClean := InstallClean;
  products[i].MustRebootAfter := MustRebootAfter;
end;

function SmartExec(fileName, parameters : string; var ResultCode : Integer) : Boolean;
begin
  if (UpperCase(Copy(fileName,Length(fileName)-2,3)) <> 'EXE') then begin
    Result := ShellExec('', fileName, parameters, '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
  end else begin
    Result := Exec(fileName, parameters, '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
  end;
  // MSI Deferred boot code 3010 is a success
  if (ResultCode = 3010) then ResultCode := 0;
  end;

function PendingReboot : Boolean;
var	Names: String;
begin
  if (RegQueryMultiStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager', 'PendingFileRenameOperations', Names)) then begin
      Result := true;
  end else if ((RegQueryMultiStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager', 'SetupExecute', Names)) and (Names <> ''))  then begin
		Result := true;
	end
	else begin
	  Result := false;
  end;		
end;

function InstallProducts: Boolean;
var
	ResultCode, i, productCount, finishCount: Integer;
begin
	Result := true;
	productCount := GetArrayLength(products);
		
	if productCount > 0 then begin
		DependencyPage := CreateOutputProgressPage(CustomMessage('depinstall_title'), CustomMessage('depinstall_description'));
		DependencyPage.Show;
		
		for i := 0 to productCount - 1 do begin		
		  if ((products[i].InstallClean = true) and PendingReboot)  then
		  begin
		    rebootRequired := true;
		    rebootmessage := products[i].Title;
		    exit;
		  end;
		
			DependencyPage.SetText(FmtMessage(CustomMessage('depinstall_status'), [products[i].Title]), '');
			DependencyPage.SetProgress(i, productCount);			
       		
      if SmartExec(products[i].File, products[i].Parameters, ResultCode) then begin
				//success; ResultCode contains the exit code
				if ResultCode = 0 then begin
					finishCount := finishCount + 1
					if (products[i].MustRebootAfter = true) then begin
					   rebootRequired := true;
					   rebootmessage := products[i].Title;
					    if not PendingReboot then begin
  					       RegWriteMultiStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager', 'PendingFileRenameOperations', '');
              end;
              exit;
          end;
        end
				else begin
					Result := false;
					break;
				end;
			end else begin
				//failure; ResultCode contains the error code
				Result := false;
				break;
			end;
		end;
		
		//only leave not installed products for error message
		for i := 0 to productCount - finishCount - 1 do begin
			products[i] := products[i+finishCount];
		end;
		SetArrayLength(products, productCount - finishCount);
		
		DependencyPage.Hide;
	end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
	i: Integer;
	s: string;
begin
  if not InstallProducts() then begin
		s := CustomMessage('depinstall_error');
		
		for i := 0 to GetArrayLength(products) - 1 do begin
			s := s + #13 + '    ' + products[i].Title;
		end;
		
		Result := s;
	end
  else if (rebootrequired) then
	begin
	   Result := RebootMessage;
	   NeedsRestart := true;
	    RegWriteStringValue(HKEY_CURRENT_USER, 'SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce',
                           'InstallBootstrap', ExpandConstant('{srcexe}'));
	end;
end;


function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
var
	s: string;
begin
	if downloadMemo <> '' then
		s := s + CustomMessage('depdownload_memo_title') + ':' + NewLine + FmtMessage(downloadMemo, [Space]) + NewLine;
	if installMemo <> '' then
		s := s + CustomMessage('depinstall_memo_title') + ':' + NewLine + FmtMessage(installMemo, [Space]) + NewLine;

	s := s + MemoDirInfo + NewLine + NewLine + MemoGroupInfo
	
	if MemoTasksInfo <> '' then
		s := s + NewLine + NewLine + MemoTasksInfo;

	Result := s
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
	Result := true;

	if CurPageID = wpReady then begin

		if downloadMemo <> '' then begin
			//change isxdl language only if it is not english because isxdl default language is already english
			if ActiveLanguage() <> 'en' then begin
				ExtractTemporaryFile(CustomMessage('isxdl_langfile'));
				isxdl_SetOption('language', ExpandConstant('{tmp}{\}') + CustomMessage('isxdl_langfile'));
			end;
			//isxdl_SetOption('title', FmtMessage(SetupMessage(msgSetupWindowTitle), [CustomMessage('appname')]));
			
			if SuppressibleMsgBox(FmtMessage(CustomMessage('depdownload_msg'), [downloadMessage]), mbConfirmation, MB_YESNO, IDYES) = IDNO then
				Result := false
			else if isxdl_DownloadFiles(StrToInt(ExpandConstant('{wizardhwnd}'))) = 0 then
				Result := false;
		end;
	end;
end;

function IsX64: Boolean;
begin
	Result := Is64BitInstallMode and (ProcessorArchitecture = paX64);
end;

function IsIA64: Boolean;
begin
	Result := Is64BitInstallMode and (ProcessorArchitecture = paIA64);
end;

function GetURL(x86, x64, ia64: String): String;
begin
	if IsX64() and (x64 <> '') then
		Result := x64;
	if IsIA64() and (ia64 <> '') then
		Result := ia64;
	
	if Result = '' then
		Result := x86;
end;

function BackButtonClick(CurPageID: Integer): Boolean;
begin
   if (CurPageID = 11) then
       Result := false
      else
      Result := true;
end;
