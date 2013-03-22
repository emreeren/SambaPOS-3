using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.PaymentModule
{
    [Export]
    public class ChangeTemplatesViewModel : ObservableObject
    {
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public ChangeTemplatesViewModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
            ChangeTemplates = new ObservableCollection<CommandButtonViewModel<PaymentData>>();
        }

        public ObservableCollection<CommandButtonViewModel<PaymentData>> ChangeTemplates { get; set; }

        public void Display(IList<ChangePaymentType> changeTemplates, decimal tenderedAmount, decimal paymentDueAmount, PaymentType paymentType, ICaptionCommand selectChangePaymentTypeCommand)
        {
            ChangeTemplates.Clear();
            ChangeTemplates.AddRange(changeTemplates.Select(x => new CommandButtonViewModel<PaymentData>
            {
                Caption = GetChangeAmountCaption(paymentDueAmount, tenderedAmount, x),
                Parameter = new PaymentData
                {
                    ChangePaymentType = x,
                    PaymentDueAmount = paymentDueAmount,
                    TenderedAmount = tenderedAmount,
                    PaymentType = paymentType
                },
                Command = selectChangePaymentTypeCommand
            }));

            this.PublishEvent(EventTopicNames.Activate);
        }

        public string GetChangeAmountCaption(decimal paymentDueAmount, decimal tenderedAmount, ChangePaymentType changeTemplate)
        {
            var returningAmount = (tenderedAmount - paymentDueAmount);
            if (changeTemplate != null)
            {
                var currency =_cacheService.GetCurrencyById(changeTemplate.Account.ForeignCurrencyId);
             
                if (currency != null)
                {
                    returningAmount = returningAmount / currency.ExchangeRate;
                    return string.Format(currency.CurrencySymbol, returningAmount);
                }
            }

            return returningAmount.ToString(LocalSettings.ReportCurrencyFormat);
        }
    }
}
