using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.Services;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    [Export]
    public class PaymentButtonViewModel : ObservableObject
    {
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;
        private readonly ITicketService _ticketService;
        public PaymentButtonGroupViewModel PaymentButtonGroup { get; set; }
        public CaptionCommand<string> CloseTicketCommand { get; set; }
        public CaptionCommand<string> MakePaymentCommand { get; set; }
        public CaptionCommand<PaymentType> MakeFastPaymentCommand { get; set; }

        private Ticket _selectedTicket;
        public Ticket SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _selectedTicket = value;

                if (SelectedDepartment != null && _selectedTicket != Ticket.Empty)
                {
                    PaymentButtonGroup.Update(_cacheService.GetUnderTicketPaymentTypes(), null);
                }
                RaisePropertyChanged(() => PaymentButtonGroup);
                RaisePropertyChanged(() => SelectedTicket);
            }
        }

        [ImportingConstructor]
        public PaymentButtonViewModel(ICacheService cacheService, IApplicationState applicationState, ITicketService ticketService)
        {
            _cacheService = cacheService;
            _applicationState = applicationState;
            _ticketService = ticketService;

            CloseTicketCommand = new CaptionCommand<string>(Resources.CloseTicket_r, OnCloseTicketExecute, CanCloseTicket);
            MakePaymentCommand = new CaptionCommand<string>(Resources.Settle, OnMakePaymentExecute, CanMakePayment);
            MakeFastPaymentCommand = new CaptionCommand<PaymentType>("[FastPayment]", OnMakeFastPaymentExecute, CanMakeFastPayment);

            PaymentButtonGroup = new PaymentButtonGroupViewModel();
            PaymentButtonGroup.SetButtonCommands(MakeFastPaymentCommand, MakePaymentCommand, CloseTicketCommand);
            SelectedTicket = Ticket.Empty;

            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(OnDepartmentChanged);
        }

        private void OnDepartmentChanged(EventParameters<Department> obj)
        {
            PaymentButtonGroup.Update(_cacheService.GetUnderTicketPaymentTypes(), null);
            RaisePropertyChanged(() => PaymentButtonGroup);
        }

        public Department SelectedDepartment
        {
            get { return _applicationState.CurrentDepartment != null ? _applicationState.CurrentDepartment.Model : null; }
        }

        private bool CanMakePayment(string arg)
        {
            return SelectedTicket != Ticket.Empty
                && (SelectedTicket.GetRemainingAmount() > 0 || SelectedTicket.Orders.Count > 0);
        }

        private void OnMakeFastPaymentExecute(PaymentType obj)
        {
            if (!CanCloseTicket()) return;
            _ticketService.PayTicket(SelectedTicket, obj);
            CloseTicket();
        }

        private bool CanMakeFastPayment(PaymentType arg)
        {
            return SelectedTicket != Ticket.Empty && SelectedTicket.GetRemainingAmount() > 0;
        }

        private bool CanCloseTicket(string arg)
        {
            return SelectedTicket != Ticket.Empty && SelectedTicket.CanCloseTicket();
        }

        private void OnMakePaymentExecute(string obj)
        {
            if (CanCloseTicket())
                SelectedTicket.PublishEvent(EventTopicNames.MakePayment);
        }

        private void OnCloseTicketExecute(string obj)
        {
            CloseTicket();
        }

        private void CloseTicket()
        {
            if (CanCloseTicket())
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.CloseTicketRequested);
        }

        private bool CanCloseTicket()
        {
            if (!_ticketService.CanCloseTicket(SelectedTicket))
            {
                foreach (var order in SelectedTicket.Orders)
                {
                    var ot = _ticketService.GetMandantoryOrderTagGroup(order);
                    if (ot != null)
                    {
                        InteractionService.UserIntraction.GiveFeedback(
                            string.Format("Select at least {0} {1} tag{2} for {3}",
                                          ot.MinSelectedItems, ot.Name,
                                          ot.MinSelectedItems == 1 ? "" : Resources.PluralCurrencySuffix,
                                          order.MenuItemName));
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
