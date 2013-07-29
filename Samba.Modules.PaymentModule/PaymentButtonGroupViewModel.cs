using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.ViewModels;

namespace Samba.Modules.PaymentModule
{
    public class PaymentButtonGroupViewModel : ObservableObject
    {
        public PaymentButtonGroupViewModel()
        {
            _paymentButtons = new ObservableCollection<CommandButtonViewModel<PaymentType>>();
        }

        private readonly ObservableCollection<CommandButtonViewModel<PaymentType>> _paymentButtons;
        public ObservableCollection<CommandButtonViewModel<PaymentType>> PaymentButtons
        {
            get { return _paymentButtons; }
        }

        private ICaptionCommand _makePaymentCommand;
        private ICaptionCommand _settleCommand;
        private ICaptionCommand _closeCommand;

        public void SetButtonCommands(ICaptionCommand makePaymentCommand, ICaptionCommand settleCommand, ICaptionCommand closeCommand)
        {
            _makePaymentCommand = makePaymentCommand;
            _settleCommand = settleCommand;
            _closeCommand = closeCommand;
        }

        public void Update(IEnumerable<PaymentType> paymentTypes, ForeignCurrency foreignCurrency)
        {
            _paymentButtons.Clear();
            _paymentButtons.AddRange(CreatePaymentButtons(paymentTypes, foreignCurrency));
        }

        private IEnumerable<CommandButtonViewModel<PaymentType>> CreatePaymentButtons(IEnumerable<PaymentType> paymentTypes, ForeignCurrency foreignCurrency)
        {
            var result = new List<CommandButtonViewModel<PaymentType>>();
            if (_settleCommand != null)
            {
                result.Add(new CommandButtonViewModel<PaymentType>
                {
                    Caption = Resources.Settle,
                    Command = _settleCommand,
                });
            }

            var pts = foreignCurrency == null ? paymentTypes.Where(x => x.Account == null || x.Account.ForeignCurrencyId == 0) : paymentTypes.Where(x => x.Account != null && x.Account.ForeignCurrencyId == foreignCurrency.Id);
            result.AddRange(pts
                .OrderBy(x => x.SortOrder)
                .Select(x => new CommandButtonViewModel<PaymentType>
                {
                    Caption = x.Name.Replace(" ", "\r"),
                    Command = _makePaymentCommand,
                    Color = x.ButtonColor,
                    FontSize = x.FontSize,
                    Parameter = x
                }));

            if (_closeCommand != null)
            {
                result.Add(new CommandButtonViewModel<PaymentType>
                {
                    Caption = Resources.Close,
                    Command = _closeCommand,
                    Color = "Red"
                });
            }
            return result;
        }
    }
}
