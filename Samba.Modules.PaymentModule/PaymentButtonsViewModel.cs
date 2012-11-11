using System.ComponentModel.Composition;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.PaymentModule
{
    [Export]
    public class PaymentButtonsViewModel : ObservableObject
    {
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public PaymentButtonsViewModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
            PaymentButtonGroup = new PaymentButtonGroupViewModel();
        }

        public PaymentButtonGroupViewModel PaymentButtonGroup { get; set; }

        public void Update(ForeignCurrency foreignCurrency)
        {
            PaymentButtonGroup.Update(_cacheService.GetPaymentScreenPaymentTypes(), foreignCurrency);
            RaisePropertyChanged(() => PaymentButtonGroup);
        }

        public void SetButtonCommands(ICaptionCommand makePaymentCommand, ICaptionCommand settleCommand, ICaptionCommand closeCommand)
        {
            PaymentButtonGroup.SetButtonCommands(makePaymentCommand, settleCommand, closeCommand);
        }
    }
}
