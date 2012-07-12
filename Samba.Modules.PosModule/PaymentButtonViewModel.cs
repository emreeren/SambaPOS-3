using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
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
        public CaptionCommand<PaymentTemplate> MakeFastPaymentCommand { get; set; }

        private Ticket _selectedTicket;
        public Ticket SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _selectedTicket = value;

                if (SelectedDepartment != null && _selectedTicket != Ticket.Empty)
                {
                    PaymentButtonGroup.UpdatePaymentButtons(_cacheService.GetUnderTicketPaymentTemplates());
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
            MakeFastPaymentCommand = new CaptionCommand<PaymentTemplate>("[FastPayment]", OnMakeFastPaymentExecute, CanMakeFastPayment);

            PaymentButtonGroup = new PaymentButtonGroupViewModel(MakeFastPaymentCommand, MakePaymentCommand, CloseTicketCommand);
            SelectedTicket = Ticket.Empty;

            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(OnDepartmentChanged);
        }

        private void OnDepartmentChanged(EventParameters<Department> obj)
        {
            PaymentButtonGroup.UpdatePaymentButtons(_cacheService.GetUnderTicketPaymentTemplates());
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

        private void OnMakeFastPaymentExecute(PaymentTemplate obj)
        {
            _ticketService.PayTicket(SelectedTicket, obj);
            CloseTicket();
        }

        private bool CanMakeFastPayment(PaymentTemplate arg)
        {
            return SelectedTicket != Ticket.Empty && SelectedTicket.GetRemainingAmount() > 0;
        }

        private bool CanCloseTicket(string arg)
        {
            return SelectedTicket != Ticket.Empty && SelectedTicket.CanCloseTicket();
        }

        private void OnMakePaymentExecute(string obj)
        {
            SelectedTicket.PublishEvent(EventTopicNames.MakePayment);
        }

        private void OnCloseTicketExecute(string obj)
        {
            CloseTicket();
        }

        private void CloseTicket()
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.CloseTicketRequested);
        }
    }
}
