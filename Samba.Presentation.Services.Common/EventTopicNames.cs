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
        public const string AddOrder = "AddOrder";
        public const string SendEmail = "SendEmail";
        public const string ExecutePowershellScript = "ExecutePowershellScript";
        public const string UpdateOrder = "UpdateOrder";
        public const string ExecuteScript = "ExecuteScript";
        public const string UpdateTicketState = "UpdateTicketState";
        public const string UpdateOrderState = "UpdateOrderState";
    }

    public static class RuleEventNames
    {
        public const string EntityStateUpdated = "EntityStateUpdated";
        public const string ApplicationScreenChanged = "ApplicationScreenChanged";
        public const string TicketsMerged = "TicketsMerged";
        public const string PaymentProcessed = "PaymentProcessed";
        public const string TicketEntityChanged = "TicketEntityChanged";
        public const string TicketOpened = "TicketOpened";
        public const string TicketClosing = "TicketClosing";
        public const string AutomationCommandExecuted = "AutomationCommandExecuted";
        public const string EntityUpdated = "EntityUpdated";
        public const string ApplicationStarted = "ApplicationStarted";
        public const string ChangeAmountChanged = "ChangeAmountChanged";
        public const string OrderAdded = "OrderAdded";
        public const string OrderMoved = "OrderMoved";
        public const string TicketMoving = "TicketMoving";
        public const string TicketMoved = "TicketMoved";
        public const string TriggerExecuted = "TriggerExecuted";
        public const string TicketTotalChanged = "TicketTotalChanged";
        public const string TicketTagSelected = "TicketTagSelected";
        public const string WorkPeriodStarts = "WorkPeriodStarts";
        public const string WorkPeriodEnds = "WorkPeriodEnds";
        public const string BeforeWorkPeriodEnds = "BeforeWorkPeriodEnds";
        public const string UserLoggedOut = "UserLoggedOut";
        public const string UserLoggedIn = "UserLoggedIn";
        public const string MessageReceived = "MessageReceived";
        public const string OrderTagged = "OrderTagged";
        public const string OrderUntagged = "OrderUntagged";
        public const string TicketStateUpdated = "TicketStateUpdated";
        public const string TicketCreated = "TicketCreated";
        public const string OrderStateUpdated = "OrderStateUpdated";
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
        public const string TicketTagSelected = "Ticket Tag Selected";
        public const string LogData = "Log Data";
        public const string ResetNumerator = "Reset Numerator";
        public const string WorkPeriodStatusChanged = "WorkPeriod Status Changed";
        public const string BrowseUrl = "Browse Url";
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
        public const string PaymentRequestedForTicket = "Payment Requested For Ticket";
        public const string ActivateAccountSelector = "Activate Account Selector";
        public const string Activate = "Activate";
        public const string HandlerRequested = "HandlerRequested";
        public const string SelectAutomationCommandValue = "SelectAutomationCommandValue";
        public const string DisplayInventory = "DisplayInventory";
   
    }

    public static class FunctionNames
    {
        public const string CanExecuteAction = "CanExecuteAction";
        public const string CanExecuteAutomationCommand = "CanExecuteAutomationCommand";
    }
}
