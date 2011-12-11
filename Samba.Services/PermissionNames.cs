using System;
using Samba.Localization.Properties;

namespace Samba.Services
{
    public static class PermissionNames
    {
        public static string OpenNavigation = "OpenNavigation";
        public static string OpenDashboard = "OpenDashboard";
        public static string OpenWorkPeriods = "OpenWorkPeriods";
        public static string OpenReports = "OpenReports";
        public static string OpenLocations = "OpenLocation";
        public static string UseDepartment = "UseDepartment_";
        public static string ChangeDepartment = "ChangeDepartment";
        public static string AddItemsToLockedTickets = "AddItemsToLockedTickets";
        public static string GiftItems = "GiftItems";
        public static string MakePayment = "MakePayment";
        public static string MakeFastPayment = "MakeFastPayment";
        public static string MoveOrders = "MoveOrders";
        public static string MoveUnlockedOrders = "MoveUnlockedOrders";
        public static string VoidItems = "VoidItems";
        public static string ChangeExtraProperty = "ChangeExtraProperty";
        public static string MakeDiscount = "MakeDiscount";
        public static string RoundPayment = "RoundPayment";
        public static string FixPayment = "FixPayment";
        public static string ChangeLocation = "ChangeLocation";
        public static string NavigateCashView = "NavigateCashView";
        public static string ChangeItemPrice = "ChangeItemPrice";
        public static string RemoveTicketTag = "RemoveTicketTag";
        public static string MergeTickets = "MergeTickets";
        public static string ChangeReportDate = "ChangeReportDate";
        public static string DisplayOldTickets = "DisplayOldTickets";
        public static string MakeCashTransaction = "MakeCashTransaction";
        public static string CreditOrDeptAccount = "CreditOrDeptAccount";
        public static string MakeAccountTransaction = "MakeAccountTransaction";
    }

    public static class PermissionCategories
    {
        public static string Navigation = Resources.NavigationPermissions;
        public static string Department = Resources.DepartmentPermissions;
        public static string Ticket = Resources.TicketPermissions;
        public static string Payment = Resources.SettlePermissions;
        public static string Report = Resources.ReportPermissions;
        public static string Cash = Resources.CashPermissions;
    }
}
