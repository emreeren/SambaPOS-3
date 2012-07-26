namespace Samba.Services.Common
{
    public static class RuleEventNames
    {
        public const string TicketsMerged = "TicketsMerged";
        public const string PaymentProcessed = "PaymentProcessed";
        public const string TicketResourceChanged = "TicketResourceChanged";
        public const string TicketOpened = "TicketOpened";
        public const string TicketClosing = "TicketClosing";
        public const string AutomationCommandExecuted = "AutomationCommandExecuted";
        public const string ResourceUpdated = "ResourceUpdated";
        public const string ApplicationStarted = "ApplicationStarted";
        public const string ChangeAmountChanged = "ChangeAmountChanged";
        public const string TicketLineAdded = "TicketLineAdded";
        public const string TicketLocationChanged = "TicketLocationChanged";
        public const string TriggerExecuted = "TriggerExecuted";
        public const string TicketTotalChanged = "TicketTotalChanged";
        public const string TicketTagSelected = "TicketTagSelected";
        public const string AccountSelectedForTicket = "AccountSelectedForTicket";
        public const string WorkPeriodStarts = "WorkPeriodStarts";
        public const string WorkPeriodEnds = "WorkPeriodEnds";
        public const string UserLoggedOut = "UserLoggedOut";
        public const string UserLoggedIn = "UserLoggedIn";
        public const string MessageReceived = "MessageReceived";
        public const string OrderTagged = "OrderTagged";
        public const string OrderUntagged = "OrderUntagged";
    }

    public static class EventTopicNames
    {
        public const string CreateTicket = "Create Ticket";
        public const string MoveSelectedOrders = "Move Selected Orders";
        public const string ActivateTicketList = "Activate Ticket List";
        public const string UnlockTicketRequested = "Unlock Ticket Requested";
        public const string TargetAccountSelected = "Target Account Selected";
        public const string SelectResource = "Select Resource";
        public const string ResourceSelected = "Resource Selected";
        public const string EditResourceDetails = "Edit Resource Details";
        public const string ApplicationLockStateChanged = "Application Lock State Changed";
        public const string DisplayTicket = "Display Ticket";
        public const string AccountTransactionDocumentSelected = "Account Transaction Document Selected";
        public const string DisplayAccountTransactions = "Display Account Transactions";
        public const string AddCustomTicketCommand = "Add Custom Ticket Command";
        public const string AddCustomOrderCommand = "Add Custom Order Command";
        public const string TicketClosed = "Ticket Closed";
        public const string DisplayTicketOrderDetails = "Display Ticket Order Details";
        public const string PortionSelected = "Portion Selected";
        public const string OrderTagSelected = "OrderTagSelected";
        public const string ShellInitialized = "Shell Initialized";
        public const string ResetCache = "Reset Cache";
        public const string ScreenMenuItemDataSelected = "Screen Menu Item Data Selected";
        public const string AddLiabilityAmount = "Add Liability Amount";
        public const string AddReceivableAmount = "Add Receivable Amount";
        public const string ExecuteEvent = "ExecuteEvent";
        public const string UpdateDepartment = "Update Department";
        public const string PopupClicked = "Popup Clicked";
        public const string DisplayTicketExplorer = "Display Ticket Explorer";
        public const string TagSelectedForSelectedTicket = "Tag Selected";
        public const string SelectTicketTag = "Select Ticket Tag";
        public const string SelectOrderTag = "Select Order Tag";
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
        public const string GetPaymentFromAccount = "Get Payment From Account";
        public const string MakePaymentToAccount = "Make Payment To Account";
        public const string ActivateAccountSelector = "Activate Account Selector";
    }
}
