using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;

namespace Samba.Presentation.ViewModels
{
    public class AccountSearchViewModel : ObservableObject
    {
        public Account Model { get; set; }

        public AccountSearchViewModel(Account model)
        {
            Model = model;
        }

        public int Id { get { return Model.Id; } }
        public string Name { get { return Model.Name; } set { Model.Name = value.Trim(); RaisePropertyChanged(() => Name); } }
        public string PhoneNumber { get { return Model.SearchString; } set { Model.SearchString = !string.IsNullOrEmpty(value) ? value.Trim() : ""; RaisePropertyChanged(() => PhoneNumber); } }
        public string PhoneNumberText { get { return PhoneNumber != null && PhoneNumber.Length == 10 ? FormatAsPhoneNumber(PhoneNumber) : PhoneNumber; } }
        public DateTime AccountOpeningDate { get { return Model.AccountOpeningDate; } set { Model.AccountOpeningDate = value; } }

        public Ticket LastTicket { get; private set; }
        public bool IsNotNew { get { return Model.Id > 0; } }

        private static string FormatAsPhoneNumber(string phoneNumber)
        {
            return string.Format("({0}) {1} {2}", phoneNumber.Substring(0, 3), phoneNumber.Substring(3, 3), phoneNumber.Substring(6));
        }

        public void UpdateDetailedInfo()
        {
            LastTicket = Dao.Last<Ticket>(x => x.AccountId == Model.Id, x => x.Orders);
            TotalTicketAmount = Dao.Sum<Ticket>(x => x.TotalAmount, x => x.AccountId == Model.Id);
        }

        public IEnumerable<OrderViewModel> LastTicketLines { get { return LastTicket != null ? LastTicket.Orders.Where(x => x.CalculatePrice).Select(x => new OrderViewModel(x)) : null; } }
        public decimal TicketTotal { get { return LastTicket != null ? LastTicket.GetSum() : 0; } }
        public string LastTicketStateString { get { return LastTicket != null ? (LastTicket.IsPaid ? Resources.Paid : Resources.Open) : ""; } }
        public decimal TotalTicketAmount { get; private set; }

    }
}
