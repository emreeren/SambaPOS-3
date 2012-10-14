using System;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PaymentModule
{
    [Export]
    public class ReturningAmountViewModel : ObservableObject
    {
        private readonly ICacheService _cacheService;
        private readonly IAutomationService _automationService;
        private readonly PaymentEditor _paymentEditor;

        [ImportingConstructor]
        public ReturningAmountViewModel(ICacheService cacheService, IAutomationService automationService,
             PaymentEditor paymentEditor)
        {
            _cacheService = cacheService;
            _automationService = automationService;
            _paymentEditor = paymentEditor;
        }

        private string _returningAmount;
        public string ReturningAmount
        {
            get { return _returningAmount; }
            set { _returningAmount = value; RaisePropertyChanged(() => ReturningAmount); }
        }

        public decimal GetReturningAmount(decimal tenderedAmount, decimal paymentDueAmount, ChangePaymentType changeTemplate)
        {
            var returningAmount = 0m;

            if (tenderedAmount > paymentDueAmount)
            {
                ReturningAmount = "";
                returningAmount = (tenderedAmount - paymentDueAmount);
                if (changeTemplate != null)
                {
                    var currency = _cacheService.GetForeignCurrencies()
                        .SingleOrDefault(x => x.Id == changeTemplate.Account.ForeignCurrencyId);

                    ReturningAmount = string.Format(Resources.ChangeAmount_f,
                            currency != null
                                ? string.Format(currency.CurrencySymbol, returningAmount / currency.ExchangeRate)
                                : returningAmount.ToString(LocalSettings.DefaultCurrencyFormat));
                }
            }

            if (string.IsNullOrEmpty(ReturningAmount))
                ReturningAmount = string.Format(Resources.ChangeAmount_f,
                    (returningAmount / _paymentEditor.ExchangeRate).ToString(LocalSettings.DefaultCurrencyFormat));

            if (returningAmount != 0)
            {
                _automationService.NotifyEvent(RuleEventNames.ChangeAmountChanged,
                                               new
                                               {
                                                   Ticket = _paymentEditor.SelectedTicket,
                                                   TicketAmount = _paymentEditor.SelectedTicket.TotalAmount,
                                                   ChangeAmount = returningAmount,
                                                   TenderedAmount = tenderedAmount
                                               });
            }
            return returningAmount;
        }
    }
}
