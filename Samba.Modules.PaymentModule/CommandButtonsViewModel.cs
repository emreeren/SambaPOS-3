using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
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
        private readonly IApplicationState _applicationState;
        private readonly TenderedValueViewModel _tenderedValueViewModel;
        private readonly OrderSelectorViewModel _orderSelectorViewModel;
        private readonly NumberPadViewModel _numberPadViewModel;
        private readonly IExpressionService _expressionService;

        [ImportingConstructor]
        public CommandButtonsViewModel(PaymentEditor paymentEditor, IApplicationState applicationState,
            TenderedValueViewModel tenderedValueViewModel, OrderSelectorViewModel orderSelectorViewModel, NumberPadViewModel numberPadViewModel,
            IExpressionService expressionService)
        {
            _paymentEditor = paymentEditor;
            _applicationState = applicationState;
            _tenderedValueViewModel = tenderedValueViewModel;

            _orderSelectorViewModel = orderSelectorViewModel;
            _numberPadViewModel = numberPadViewModel;
            _expressionService = expressionService;

            _executeAutomationCommand = new CaptionCommand<AutomationCommandData>("", OnExecuteAutomationCommand, CanExecuteAutomationCommand);
            _serviceSelectedCommand = new CaptionCommand<CalculationSelector>("", OnSelectCalculationSelector, CanSelectCalculationSelector);
        }

        public IEnumerable<CommandButtonViewModel<object>> CommandButtons { get; set; }

        private IEnumerable<CommandButtonViewModel<object>> CreateCommandButtons()
        {
            var result = new List<CommandButtonViewModel<object>>();

            if (_paymentEditor.SelectedTicket != null)
            {
                result.AddRange(_applicationState.GetCalculationSelectors().Where(x => !string.IsNullOrEmpty(x.ButtonHeader))
                    .Select(x => new CommandButtonViewModel<object>
                    {
                        Caption = x.ButtonHeader,
                        Command = _serviceSelectedCommand,
                        Color = x.ButtonColor,
                        FontSize = x.FontSize,
                        Parameter = x
                    }));

                result.AddRange(_applicationState.GetAutomationCommands().Where(x => x.AutomationCommand != null && !string.IsNullOrEmpty(x.AutomationCommand.ButtonHeader) && x.DisplayOnPayment && x.CanDisplay(_paymentEditor.SelectedTicket))
                    .Select(x => new CommandButtonViewModel<object>
                    {
                        Caption = x.AutomationCommand.ButtonHeader,
                        Command = _executeAutomationCommand,
                        Color = x.AutomationCommand.Color,
                        FontSize = x.AutomationCommand.FontSize,
                        Parameter = x
                    }));
            }
            return result;
        }

        private void OnExecuteAutomationCommand(AutomationCommandData obj)
        {
            _applicationState.NotifyEvent(RuleEventNames.AutomationCommandExecuted, new { Ticket = _paymentEditor.SelectedTicket, AutomationCommandName = obj.AutomationCommand.Name });
        }

        private bool CanExecuteAutomationCommand(AutomationCommandData arg)
        {
            if (arg == null) return false;
            if (_paymentEditor.SelectedTicket != null && _paymentEditor.SelectedTicket.IsLocked && arg.VisualBehaviour == 1) return false;
            if (!arg.CanExecute(_paymentEditor.SelectedTicket)) return false;
            return _expressionService.EvalCommand(FunctionNames.CanExecuteAutomationCommand, arg.AutomationCommand, new { Ticket = _paymentEditor.SelectedTicket }, true);
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
            if (_paymentEditor.SelectedTicket != null && (_paymentEditor.SelectedTicket.IsLocked || _paymentEditor.SelectedTicket.IsClosed)) return false;
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
