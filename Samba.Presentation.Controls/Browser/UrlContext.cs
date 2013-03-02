using System;

namespace Samba.Presentation.Controls.Browser
{
  /// <summary>
  /// Flags used by INewWindowManager::EvaluateNewWindow. 
  /// These values are taken into account in the decision of whether to display a pop-up window.
  /// </summary>
  [Flags]
  enum UrlContext
  {
    /// <summary>
    /// No information Present
    /// </summary>
    None = 0x0,
    /// <summary>
    /// The page is unloading. This flag is set in response to the onbeforeunload and onunload events. 
    /// Some pages load pop-up windows when you leave them rather than when you enter. This flag is used to identify those situations.
    /// </summary>
    Unloading = 0x1,
    /// <summary>
    /// The call to INewWindowManager::EvaluateNewWindow is the result of a user-initiated action 
    /// (a mouse click or key press). Use this flag in conjunction with the NWMF_FIRST_USERINITED flag 
    /// to determine whether the call is a direct or indirect result of the user-initiated action.
    /// </summary>
    UserInited = 0x2,
    /// <summary>
    /// When NWMF_USERINITED is present, this flag indicates that the call to 
    /// INewWindowManager::EvaluateNewWindow is the first query that results from this user-initiated action. 
    /// Always use this flag in conjunction with NWMF_USERINITED.
    /// </summary>
    UserFirstInited = 0x4,
    /// <summary>
    /// The override key (ALT) was pressed. The override key is used to bypass the pop-up manager—allowing 
    /// all pop-up windows to display—and must be held down at the time that INewWindowManager::EvaluateNewWindow is called. 
    /// </summary>
    OverrideKey = 0x8,
    /// <summary>
    /// The new window attempting to load is the result of a call to the showHelp method. Help is sometimes displayed in a separate window, 
    /// and this flag is valuable in those cases.
    /// </summary>
    ShowHelp = 0x10,
    /// <summary>
    /// The new window is a dialog box that displays HTML content.
    /// </summary>
    HtmlDialog = 0x20,
    /// <summary>
    /// Indicates that the EvaluateNewWindow method is being called through a marshalled Component Object Model (COM) proxy 
    /// from another thread. In this situation, the method should make a decision and return immediately without performing 
    /// blocking operations such as showing modal user interface (UI). Lengthy operations will cause the calling thread to 
    /// appear unresponsive.
    /// </summary>
    FromProxy = 0x40
  }
}