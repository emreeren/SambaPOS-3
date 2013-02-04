using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;

namespace Samba.Modules.EntityModule
{
    public class EntitySearchResultViewModel : ObservableObject
    {
        public Entity Model { get; set; }
        public EntityType EntityType { get; set; }

        private EntityCustomDataViewModel _accountCustomDataViewModel;
        public EntityCustomDataViewModel AccountCustomDataViewModel
        {
            get { return _accountCustomDataViewModel ?? (_accountCustomDataViewModel = new EntityCustomDataViewModel(Model, EntityType)); }
        }

        public EntitySearchResultViewModel(Entity model, EntityType template)
        {
            EntityType = template;
            Model = model;
        }

        public string this[string index]
        {
            get { return AccountCustomDataViewModel.GetValue(index); }
        }

        public int Id { get { return Model.Id; } }
        public string Name { get { return Model.Name; } set { Model.Name = value; RaisePropertyChanged(() => Name); } }
        public string PhoneNumber { get { return Model.SearchString; } set { Model.SearchString = !string.IsNullOrEmpty(value) ? value.Trim() : ""; RaisePropertyChanged(() => PhoneNumber); } }
        public string PhoneNumberText { get { return PhoneNumber != null && PhoneNumber.Length == 10 ? FormatAsPhoneNumber(PhoneNumber) : PhoneNumber; } }
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
            RaisePropertyChanged(() => LastTicket);
            RaisePropertyChanged(() => TotalTicketAmount);
            RaisePropertyChanged(() => LastTicketLines);
            RaisePropertyChanged(() => TicketTotal);
            RaisePropertyChanged(() => LastTicketStateString);
        }

        public IEnumerable<Order> LastTicketLines { get { return LastTicket != null ? LastTicket.Orders.Where(x => x.CalculatePrice) : null; } }
        public decimal TicketTotal { get { return LastTicket != null ? LastTicket.GetSum() : 0; } }
        public string LastTicketStateString { get { return LastTicket != null ? (LastTicket.IsClosed ? Resources.Paid : Resources.Open) : ""; } }
        public decimal TotalTicketAmount { get; private set; }
    }
}
