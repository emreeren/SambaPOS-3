using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;

namespace Samba.Presentation.ViewModels
{
    public class PaymentButtonGroupViewModel : ObservableObject
    {
        public PaymentButtonGroupViewModel(ICaptionCommand makePaymentCommand, ICaptionCommand settleCommand, ICaptionCommand closeCommand)
        {
            _makePaymentCommand = makePaymentCommand;
            _settleCommand = settleCommand;
            _closeCommand = closeCommand;
            _paymentButtons = new ObservableCollection<CommandButtonViewModel<PaymentTemplate>>();
        }

        private IEnumerable<PaymentTemplate> _paymentTemplates;
        public IEnumerable<PaymentTemplate> PaymentTemplates
        {
            get { return _paymentTemplates; }
        }

        private readonly ObservableCollection<CommandButtonViewModel<PaymentTemplate>> _paymentButtons;
        public ObservableCollection<CommandButtonViewModel<PaymentTemplate>> PaymentButtons
        {
            get { return _paymentButtons; }
        }

        private readonly ICaptionCommand _makePaymentCommand;
        private readonly ICaptionCommand _settleCommand;
        private readonly ICaptionCommand _closeCommand;

        public void UpdatePaymentButtons(IEnumerable<PaymentTemplate> paymentTemplates)
        {
            _paymentTemplates = paymentTemplates;
            _paymentButtons.Clear();
            _paymentButtons.AddRange(CreatePaymentButtons());
        }

        private IEnumerable<CommandButtonViewModel<PaymentTemplate>> CreatePaymentButtons()
        {
            var result = new List<CommandButtonViewModel<PaymentTemplate>>();
            if (_settleCommand != null)
            {
                result.Add(new CommandButtonViewModel<PaymentTemplate>
                {
                    Caption = Resources.Settle,
                    Command = _settleCommand,
                    Color = "Gainsboro"
                });
            }

            result.AddRange(
                PaymentTemplates
                .OrderBy(x => x.Order)
                .Select(x => new CommandButtonViewModel<PaymentTemplate>
                {
                    Caption = x.Name.Replace(" ", "\r"),
                    Command = _makePaymentCommand,
                    Color = x.ButtonColor,
                    Parameter = x
                }));

            if (_closeCommand != null)
            {
                result.Add(new CommandButtonViewModel<PaymentTemplate>
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
