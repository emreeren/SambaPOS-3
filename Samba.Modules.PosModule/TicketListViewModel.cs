using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    [Export]
    public class TicketListViewModel : ObservableObject
    {
        private readonly ICaptionCommand _executeAutomationCommand;
        private readonly ITicketService _ticketService;
        private readonly ITicketServiceBase _ticketServiceBase;
        private readonly IApplicationState _applicationState;
        private readonly IUserService _userService;

        [ImportingConstructor]
        public TicketListViewModel(ITicketService ticketService, ITicketServiceBase ticketServiceBase,
            IApplicationState applicationState, IUserService userService)
        {
            _ticketService = ticketService;
            _ticketServiceBase = ticketServiceBase;
            _applicationState = applicationState;
            _userService = userService;
            _tickets = new List<TicketButtonViewModel>();
            AddTicketCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Ticket).Replace(" ", "\r"), OnAddTicket, CanAddTicket);
            MergeTicketsCommand = new CaptionCommand<string>(Resources.MergeTickets.Replace(" ", "\r"), OnMergeTickets, CanMergeTickets);
            CloseCommand = new CaptionCommand<string>(Resources.Close.Replace(" ", "\r"), OnCloseCommand);
            _executeAutomationCommand = new CaptionCommand<AutomationCommandData>("", OnExecuteAutomationCommand, CanExecuteAutomationCommand);
        }

        public IEnumerable<CommandButtonViewModel<object>> CommandButtons { get; set; }

        private IEnumerable<CommandButtonViewModel<object>> CreateCommandButtons()
        {
            var result = new List<CommandButtonViewModel<object>>();

            if (SelectedEntity != null)
            {

                result.AddRange(_applicationState.GetAutomationCommands().Where(x => x.DisplayOnTicketList)
                    .Select(x => new CommandButtonViewModel<object>
                    {
                        Caption = x.AutomationCommand.Name,
                        Command = _executeAutomationCommand,
                        Color = x.AutomationCommand.Color,
                        FontSize = x.AutomationCommand.FontSize,
                        Parameter = x
                    }));

                result.Add(new CommandButtonViewModel<object>
                               {
                                   Caption = MergeTicketsCommand.Caption,
                                   Command = MergeTicketsCommand,
                                   FontSize = 40,
                                   Color = "Gainsboro"
                               });

                result.Add(new CommandButtonViewModel<object>
                               {
                                   Caption = CloseCommand.Caption,
                                   Command = CloseCommand,
                                   FontSize = 40,
                                   Color = "Red"
                               });
            }
            return result;
        }

        private bool CanExecuteAutomationCommand(AutomationCommandData arg)
        {
            return SelectedEntity != null;
        }

        private void OnExecuteAutomationCommand(AutomationCommandData obj)
        {
            _applicationState.NotifyEvent(RuleEventNames.AutomationCommandExecuted, new { TicketId = GetLastTicketId(), AutomationCommandName = obj.AutomationCommand.Name, CommandValue = GetTicketIds() });
        }

        protected int GetLastTicketId()
        {
            if (Tickets.Any())
                return Tickets.Last().TicketId;
            return 0;
        }

        private string GetTicketIds()
        {
            if (Tickets.Any(x => x.IsSelected))
                return string.Join(",", Tickets.Where(x => x.IsSelected).Select(x => x.TicketId));
            return string.Join(",", Tickets.Select(x => x.TicketId));
        }

        private bool CanAddTicket(string arg)
        {
            return SelectedEntity != null;
        }

        private List<TicketButtonViewModel> _tickets;
        public List<TicketButtonViewModel> Tickets
        {
            get { return _tickets; }
        }

        public IEntityClass SelectedEntity { get; set; }
        public string ListName { get { return SelectedEntity != null ? SelectedEntity.Name : ""; } }
        public ICaptionCommand AddTicketCommand { get; set; }
        public ICaptionCommand MergeTicketsCommand { get; set; }
        public ICaptionCommand CloseCommand { get; set; }

        public string TotalRemainingAmountLabel { get { return _tickets != null ? Tickets.Sum(x => x.RemainingAmount).ToString(LocalSettings.ReportCurrencyFormat) : ""; } }
        public int RowCount { get { return _tickets.Count() > 8 ? _tickets.Count() : 8; } }
        public int SelectedItemsCount { get { return _tickets.Count(x => x.IsSelected); } }

        private bool CanMergeTickets(string arg)
        {
            return SelectedItemsCount > 1 && _userService.IsUserPermittedFor(PermissionNames.MergeTickets);
        }

        private void OnCloseCommand(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }

        private void OnMergeTickets(string obj)
        {
            var result = _ticketService.MergeTickets(Tickets.Where(x => x.IsSelected).Select(x => x.TicketId));
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                InteractionService.UserIntraction.GiveFeedback(result.ErrorMessage);
                Tickets.ForEach(x => x.IsSelected = false);
            }
            else
                ExtensionMethods.PublishIdEvent(result.TicketId, EventTopicNames.DisplayTicket);
        }

        private void OnAddTicket(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.CreateTicket, true);
            var r = SelectedEntity as Entity;
            new OperationRequest<Entity>(r, null).PublishEvent(EventTopicNames.EntitySelected, true);
        }

        public void UpdateListByEntity(Entity entity)
        {
            if (entity != null)
            {
                SelectedEntity = entity;
                _tickets = _ticketServiceBase.GetOpenTickets(entity.Id).Select(x => new TicketButtonViewModel(x, entity)).ToList();
                Refresh();
            }
        }

        public void UpdateListByTicketTagGroup(TicketTagGroup tagGroup)
        {
            SelectedEntity = tagGroup;
            var tagValue = string.Format("\"TN\":\"{0}\"", tagGroup.Name);
            _tickets = _ticketServiceBase.GetOpenTickets(x => !x.IsClosed && x.TicketTags.Contains(tagValue)).Select(x => new TicketButtonViewModel(x, null)).ToList();
            Refresh();
        }

        public void UpdateListByTicketState(TicketStateData value)
        {
            SelectedEntity = null;
            var stateValue = string.Format("\"S\":\"{0}\"", value.StateName);
            _tickets = _ticketServiceBase.GetOpenTickets(x => x.TicketStates.Contains(stateValue)).Select(x => new TicketButtonViewModel(x, null)).ToList();
            Refresh();
        }

        private void Refresh()
        {
            _tickets.ForEach(x => x.SelectionChanged = ItemSelectionChanged);
            RaisePropertyChanged(() => Tickets);
            RaisePropertyChanged(() => RowCount);
            RaisePropertyChanged(() => TotalRemainingAmountLabel);
            RaisePropertyChanged(() => ListName);
            CommandButtons = CreateCommandButtons();
            RaisePropertyChanged(() => CommandButtons);
        }

        private void ItemSelectionChanged()
        {
            RaisePropertyChanged(() => SelectedItemsCount);
        }

    }
}
