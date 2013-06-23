using Samba.Localization.Properties;

namespace Samba.Presentation.Services.Common
{
    public static class PermissionNames
    {
        public static string OpenNavigation = "OpenNavigation";
        public static string OpenDashboard = "OpenDashboard";
        public static string OpenWorkPeriods = "OpenWorkPeriods";
        public static string OpenReports = "OpenReports";
        public static string OpenInventory = "OpenInventory";
        public static string OpenMarket="OpenMarket";
        public static string UseDepartment = "UseDepartment_";
        public static string ChangeDepartment = "ChangeDepartment";
        public static string AddItemsToLockedTickets = "AddItemsToLockedTickets";
        public static string MoveOrders = "MoveOrders";
        public static string MoveUnlockedOrders = "MoveUnlockedOrders";
        public static string ChangeExtraProperty = "ChangeExtraProperty";
        public static string NavigateAccountView = "NavigateAccountView";
        public static string ChangeItemPrice = "ChangeItemPrice";
        public static string RemoveTicketTag = "RemoveTicketTag";
        public static string MergeTickets = "MergeTickets";
        public static string ChangeReportDate = "ChangeReportDate";
        public static string DisplayOldTickets = "DisplayOldTickets";
        public static string CreateAccount = "CreateAccount";
        public static string DisplayOtherWaitersTickets = "DisplayOtherWaitersTickets";
    }

    public static class PermissionCategories
    {
        public static string Navigation = Resources.NavigationPermissions;
        public static string Department = Resources.DepartmentPermissions;
        public static string Ticket = Resources.TicketPermissions;
        public static string Payment = Resources.SettlePermissions;
        public static string Report = Resources.ReportPermissions;
        public static string Cash = Resources.CashPermissions;
        public static string Account = Resources.AccountPermissions;
    }
}
