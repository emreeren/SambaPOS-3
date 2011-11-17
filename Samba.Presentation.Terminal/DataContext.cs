using System.Diagnostics;
using System.Linq;
using Samba.Domain.Models.Tables;
using Samba.Infrastructure;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Presentation.Terminal
{
    public static class DataContext
    {
        public static TicketViewModel SelectedTicket { get; private set; }

        private static TicketViewModel CreateSelectedTicket()
        {
            return new TicketViewModel(AppServices.MainDataContext.SelectedTicket, false);
        }

        public static OrderViewModel SelectedOrder { get; set; }

        public static void UpdateSelectedTicket(Table table)
        {
            Debug.Assert(SelectedTicket == null);
            if (table.TicketId == 0)
                TicketViewModel.AssignLocationToSelectedTicket(table.Id);
            else AppServices.MainDataContext.OpenTicket(table.TicketId);
            RefreshSelectedTicket();
        }

        public static void OpenTicket(int ticketId)
        {
            Debug.Assert(SelectedTicket == null);
            if (ticketId == 0) TicketViewModel.CreateNewTicket(); 
            else AppServices.MainDataContext.OpenTicket(ticketId);
            RefreshSelectedTicket();
        }

        public static TicketCommitResult CloseSelectedTicket()
        {
            Debug.Assert(SelectedTicket != null);
            SelectedTicket = null;
            var result = AppServices.MainDataContext.CloseTicket();
            AppServices.MessagingService.SendMessage(Messages.TicketRefreshMessage, result.TicketId.ToString());
            return result;
        }

        internal static int MoveSelectedOrdersToNewTicket()
        {
            var result= AppServices.MainDataContext.MoveOrders(SelectedTicket.SelectedOrders.Select(x => x.Model), 0).TicketId;
            SelectedTicket = null;
            return result;
        }

        public static void RefreshSelectedTicket()
        {
            SelectedTicket = CreateSelectedTicket();
        }
    }
}
