namespace Samba.Presentation.Services.Common
{
    public static class ActionNames
    {
        public const string CreateAccountTransaction = "CreateAccountTransaction";
        public const string ChangeTicketEntity = "ChangeTicketEntity";
        public const string DisplayTicketList = "DisplayTicketList";
        public const string UpdateTicketTag = "UpdateTicketTag";
        public const string TagOrder = "TagOrder";
        public const string UntagOrder = "UntagOrder";
        public const string RemoveOrderTag = "RemoveOrderTag";
        public const string MoveTaggedOrders = "MoveTaggedOrders";
        public const string UpdatePriceTag = "UpdatePriceTag";
        public const string RefreshCache = "RefreshCache";
        public const string SendMessage = "SendMessage";
        public const string UpdateProgramSetting = "UpdateProgramSetting";
        public const string UpdateTicketCalculation = "UpdateTicketService";
        public const string ExecutePrintJob = "ExecutePrintJob";
        public const string UpdateEntityState = "UpdateEntityState";
        public const string CloseActiveTicket = "CloseActiveTicket";
        public const string LockTicket = "LockTicket";
        public const string UnlockTicket = "UnlockTicket";
        public const string CreateTicket = "CreateTicket";
        public const string DisplayTicket = "DisplayTicket";
        public const string DisplayPaymentScreen = "DisplayPaymentScreen";
        public const string CreateAccountTransactionDocument = "CreateAccountTransactionDocument";
        public const string CreateBatchAccountTransactionDocument = "CreateBatchAccountTransactionDocument";
        public const string AddOrder = "AddOrder";
        public const string SendEmail = "SendEmail";
        public const string ExecutePowershellScript = "ExecutePowershellScript";
        public const string UpdateOrder = "UpdateOrder";
        public const string ExecuteScript = "ExecuteScript";
        public const string UpdateTicketState = "UpdateTicketState";
        public const string UpdateOrderState = "UpdateOrderState";
        public const string SetActiveTicketType = "SetActiveTicketType";
        public const string PrintReport = "PrintReport";
        public const string StartProcess = "StartProcess";
        public const string MarkTicketAsClosed = "MarkTicketAsClosed";
        public const string LoadTicket = "LoadTicket";
        public const string LoopValues = "LoopValues";
        public const string PrintAccountTransactionDocument = "PrintAccountTransactionDocument";
        public const string CreateEntity = "CreateEntity";
        public const string SetWidgetValue = "SetWidgetValue";
        public const string PrintEntity = "PrintEntity";
        public const string StopActiveTimers = "StopActiveTimers";
    }

    public static class EventTopicNames
    {
        public const string Changed = "Changed";
        public const string OrderTagRemoved = "Order Tag Removed";
        public const string BatchDocumentsCreated = "Batch Documents Created";
        public const string BatchCreateDocument = "Batch Create Document";
        public const string CreateTicket = "Create Ticket";
        public const string MoveSelectedOrders = "Move Selected Orders";
        public const string ActivateTicketList = "Activate Ticket List";
        public const string UnlockTicketRequested = "Unlock Ticket Requested";
        public const string TargetAccountSelected = "Target Account Selected";
        public const string SelectEntity = "Select Entity";
        public const string EntitySelected = "Entity Selected";
        public const string EditEntityDetails = "Edit Entity Details";
        public const string ApplicationLockStateChanged = "Application Lock State Changed";
        public const string DisplayTicket = "Display Ticket";
        public const string AccountTransactionDocumentSelected = "Account Transaction Document Selected";
        public const string DisplayAccountTransactions = "Display Account Transactions";
        public const string TicketClosed = "Ticket Closed";
        public const string DisplayTicketOrderDetails = "Display Ticket Order Details";
        public const string PortionSelected = "Portion Selected";
        public const string OrderTagSelected = "Order Tag Selected";
        public const string OrderStateSelected = "Order State Selected";
        public const string ShellInitialized = "Shell Initialized";
        public const string ResetCache = "Reset Cache";
        public const string ScreenMenuItemDataSelected = "Screen Menu Item Data Selected";
        public const string ExecuteEvent = "ExecuteEvent";
        public const string UpdateDepartment = "Update Department";
        public const string PopupClicked = "Popup Clicked";
        public const string SelectTicketTag = "Select Ticket Tag";
        public const string LogData = "Log Data";
        public const string ResetNumerator = "Reset Numerator";
        public const string WorkPeriodStatusChanged = "WorkPeriod Status Changed";
        public const string ActivateNavigation = "Activate Navigation";
        public const string NavigationCommandAdded = "Navigation Command Added";
        public const string DashboardCommandAdded = "Dashboard Command Added";
        public const string OrderAdded = "Order Added";
        public const string DashboardClosed = "Dashboard Closed";
        public const string MessageReceivedEvent = "Message Received";
        public const string ViewAdded = "View Added";
        public const string ViewClosed = "View Closed";
        public const string PinSubmitted = "Pin Submitted";
        public const string UserLoggedIn = "User LoggedIn";
        public const string UserLoggedOut = "User LoggedOut";
        public const string AddedModelSaved = "ModelSaved";
        public const string ModelAddedOrDeleted = "Model Added or Deleted";
        public const string MakePayment = "Make Payment";
        public const string CloseTicketRequested = "Close Ticket Requested";
        public const string SelectedOrdersChanged = "Selected Orders Changed";
        public const string SelectedDepartmentChanged = "Selected Department Changed";
        public const string FindLocation = "Find Location";
        public const string ActivatePosView = "Activate POS View";
        public const string RefreshSelectedTicket = "Refresh Selected Ticket";
        public const string EditTicketNote = "Edit Ticket Note";
        public const string DisplayTicketLog = "DisplayTicketLog";
        public const string PaymentRequestedForTicket = "Payment Requested For Ticket";
        public const string ActivateAccountSelector = "Activate Account Selector";
        public const string Activate = "Activate";
        public const string HandlerRequested = "HandlerRequested";
        public const string SelectAutomationCommandValue = "SelectAutomationCommandValue";
        public const string DisplayInventory = "DisplayInventory";
        public const string TicketTypeChanged = "TicketTypeChanged";
        public const string TicketTypeSelected = "TicketTypeSelected";
        public const string LocalSettingsChanged = "LocalSettingsChanged";
        public const string EnableLandscape = "EnableLandscape";
        public const string DisableLandscape = "DisableLandscape";
        public const string RegionActivated = "RegionActivated";
        public const string ActivateMenuView = "ActivateMenuView";
        public const string RegenerateSelectedTicket = "RegenerateSelectedTicket";
        public const string SetSelectedTicket = "SetSelectedTicket";
        public const string SetWidgetValue = "SetWidgetValue";
        public const string SelectAutomationCommand = "SelectAutomationCommand";
    }

    public static class FunctionNames
    {
        public const string CanExecuteAction = "CanExecuteAction";
        public const string CanExecuteAutomationCommand = "CanExecuteAutomationCommand";
        public const string Calculation = "Calculation";
    }
}
