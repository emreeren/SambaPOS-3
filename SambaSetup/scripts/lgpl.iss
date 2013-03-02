[Code]
var
  LGPLPage: TOutputMsgMemoWizardPage;
  LGPLAccept: TNewRadioButton;
  LGPLRefuse: TNewRadioButton;

procedure LGPLPageActivate(Sender: TWizardPage); forward;
procedure LGPLAcceptClick(Sender: TObject); forward;

procedure LGPL_InitializeWizard();
var 
  LGPLText: AnsiString;

begin
  // Create the page
  LGPLPage := CreateOutputMsgMemoPage(wpLicense, SetupMessage(msgWizardLicense), SetupMessage(msgLicenseLabel), CustomMessage('LGPLHeader'), '');

  // Adjust the memo and add the confirm/refuse options
  LGPLPage.RichEditViewer.Height := ScaleY(148);
  LGPLAccept := TNewRadioButton.Create(LGPLPage);
  LGPLAccept.Left := LGPLPage.RichEditViewer.Left;
  LGPLAccept.Top := LGPLPage.Surface.ClientHeight - ScaleY(41);
  LGPLAccept.Width := LGPLPage.RichEditViewer.Width;
  LGPLAccept.Parent := LGPLPage.Surface;
  LGPLAccept.Caption := SetupMessage(msgLicenseAccepted);
  LGPLRefuse := TNewRadioButton.Create(LGPLPage);
  LGPLRefuse.Left := LGPLPage.RichEditViewer.Left;
  LGPLRefuse.Top := LGPLPage.Surface.ClientHeight - ScaleY(21);
  LGPLRefuse.Width := LGPLPage.RichEditViewer.Width;
  LGPLRefuse.Parent := LGPLPage.Surface;
  LGPLRefuse.Caption := SetupMessage(msgLicenseNotAccepted);
  
  // Set the states and event handlers
  LGPLPage.OnActivate := @LGPLPageActivate;
  LGPLAccept.OnClick := @LGPLAcceptClick;
  LGPLRefuse.OnClick := @LGPLAcceptClick;
  LGPLRefuse.Checked := true;
	
  // Load the LGPL text into the new page
  ExtractTemporaryFile('gpl-3.0.txt');
  LoadStringFromFile(ExpandConstant('{tmp}/gpl-3.0.txt'), LGPLText);
  LGPLPage.RichEditViewer.RTFText := LGPLText;
end;

procedure LGPLPageActivate(Sender: TWizardPage);
begin
  WizardForm.NextButton.Enabled := LGPLAccept.Checked;
end;

procedure LGPLAcceptClick(Sender: TObject);
begin
  WizardForm.NextButton.Enabled := LGPLAccept.Checked;
end;

[Files]
Source: src\gpl-3.0.txt; DestDir: {app}; Flags: ignoreversion

[CustomMessages]
LGPLHeader=Please read the following License Agreement. Some components are licensed under the GNU Lesser General Public License.