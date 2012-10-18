using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PaymentModule
{
    [Export]
    public class CommandButtonsViewModel : ObservableObject
    {
        private readonly ICaptionCommand _executeAutomationCommand;
        private readonly ICaptionCommand _serviceSelectedCommand;

        private readonly PaymentEditor _paymentEditor;
        private readonly ICacheService _cacheService;
        private readonly IAutomationService _automationService;
        private readonly TenderedValueViewModel _tenderedValueViewModel;
        private readonly OrderSelectorViewModel _orderSelectorViewModel;
        private readonly NumberPadViewModel _numberPadViewModel;

        [ImportingConstructor]
        public CommandButtonsViewModel(PaymentEditor paymentEditor, ICacheService cacheService, IAutomationService automationService,
            TenderedValueViewModel tenderedValueViewModel, OrderSelectorViewModel orderSelectorViewModel, NumberPadViewModel numberPadViewModel)
        {
            _paymentEditor = paymentEditor;
            _cacheService = cacheService;
            _automationService = automationService;
            _tenderedValueViewModel = tenderedValueViewModel;

            _orderSelectorViewModel = orderSelectorViewModel;
            _numberPadViewModel = numberPadViewModel;

            _executeAutomationCommand = new CaptionCommand<AutomationCommandData>("", OnExecuteAutomationCommand, CanExecuteAutomationCommand);
            _serviceSelectedCommand = new CaptionCommand<CalculationSelector>("", OnSelectCalculationSelector, CanSelectCalculationSelector);
        }

        public IEnumerable<CommandButtonViewModel<object>> CommandButtons { get; set; }

        private IEnumerable<CommandButtonViewModel<object>> CreateCommandButtons()
        {
            var result = new List<CommandButtonViewModel<object>>();

            if (_paymentEditor.SelectedTicket != null)
            {
                result.AddRange(_cacheService.GetCalculationSelectors().Where(x => !string.IsNullOrEmpty(x.ButtonHeader))
                    .Select(x => new CommandButtonViewModel<object>
                    {
                        Caption = x.ButtonHeader,
                        Command = _serviceSelectedCommand,
                        Color = x.ButtonColor,
                        Parameter = x
                    }));

                result.AddRange(_cacheService.GetAutomationCommands().Where(x => x.DisplayOnPayment)
                    .Select(x => new CommandButtonViewModel<object>
                    {
                        Caption = x.AutomationCommand.Name,
                        Command = _executeAutomationCommand,
                        Color = x.AutomationCommand.Color,
                        Parameter = x
                    }));
            }
            return result;
        }

        private void OnExecuteAutomationCommand(AutomationCommandData obj)
        {
            _automationService.NotifyEvent(RuleEventNames.AutomationCommandExecuted, new { Ticket = _paymentEditor.SelectedTicket, AutomationCommandName = obj.AutomationCommand.Name });
        }

        private bool CanExecuteAutomationCommand(AutomationCommandData arg)
        {
            if (_tenderedValueViewModel.GetTenderedValue() <= 0) return false;
            if (_paymentEditor.SelectedTicket != null && _paymentEditor.SelectedTicket.Locked && arg != null && arg.VisualBehaviour == 1) return false;
            return true;
        }

        private void OnSelectCalculationSelector(CalculationSelector calculationSelector)
        {
            foreach (var calculationType in calculationSelector.CalculationTypes)
            {
                var amount = calculationType.Amount;
                if (amount == 0) amount = _tenderedValueViewModel.GetTenderedValue();
                if (calculationType.CalculationMethod == 0 || calculationType.CalculationMethod == 1) amount = amount / _paymentEditor.ExchangeRate;
                _paymentEditor.SelectedTicket.AddCalculation(calculationType, amount);
            }
            _tenderedValueViewModel.UpdatePaymentAmount(0);
            _orderSelectorViewModel.UpdateTicket(_paymentEditor.SelectedTicket);
            _numberPadViewModel.ResetValues();
        }

        private bool CanSelectCalculationSelector(CalculationSelector calculationSelector)
        {
            if (calculationSelector == null) return false;
            if (_paymentEditor.SelectedTicket != null && (_paymentEditor.SelectedTicket.Locked || _paymentEditor.SelectedTicket.IsClosed)) return false;
            if (_paymentEditor.SelectedTicket != null && _paymentEditor.SelectedTicket.GetRemainingAmount() == 0 && _paymentEditor.SelectedTicket != null && !calculationSelector.CalculationTypes.Any(x => _paymentEditor.SelectedTicket.Calculations.Any(y => y.CalculationTypeId == x.Id))) return false;
            return !calculationSelector.CalculationTypes.Any(x => x.MaxAmount > 0 && _tenderedValueViewModel.GetTenderedValue() > x.MaxAmount);
        }

        public void Update()
        {
            CommandButtons = CreateCommandButtons();
            RaisePropertyChanged(() => CommandButtons);
        }
    }
}
