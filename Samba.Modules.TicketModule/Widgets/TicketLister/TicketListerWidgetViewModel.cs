using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Helpers;
using Samba.Infrastructure.Messaging;
using Samba.Persistance;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Widgets;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.TicketModule.Widgets.TicketLister
{
    class TicketListerWidgetViewModel : WidgetViewModel
    {
        private readonly IApplicationState _applicationState;
        private readonly ITicketServiceBase _ticketService;
        private readonly IPrinterService _printerService;
        private readonly ICacheService _cacheService;
        private readonly IAutomationDao _automationDao;

        public DelegateCommand<TicketViewData> ItemSelectionCommand { get; set; }

        public TicketListerWidgetViewModel(Widget model, IApplicationState applicationState, ITicketServiceBase ticketService,
            IPrinterService printerService, ICacheService cacheService, IAutomationDao automationDao)
            : base(model, applicationState)
        {
            _applicationState = applicationState;
            _ticketService = ticketService;
            _printerService = printerService;
            _cacheService = cacheService;
            _automationDao = automationDao;
            ItemSelectionCommand = new DelegateCommand<TicketViewData>(OnItemSelection);

            EventServiceFactory.EventService.GetEvent<GenericEvent<Message>>().Subscribe(
            x =>
            {
                if (_applicationState.ActiveAppScreen == AppScreens.EntityView
                    && x.Topic == EventTopicNames.MessageReceivedEvent
                    && x.Value.Command == Messages.TicketRefreshMessage)
                {
                    Refresh();
                }
            });
        }

        private void OnItemSelection(TicketViewData obj)
        {
            if (obj == null) return;

            if (!Settings.MultiSelection)
            {
                TicketList.ToList().ForEach(x => x.IsSelected = false);
                obj.IsSelected = true;
            }
            else
            {
                obj.IsSelected = !obj.IsSelected;
            }

            if (!string.IsNullOrEmpty(Settings.CommandName))
            {
                var val = GetCommandValues();

                _applicationState.NotifyEvent(RuleEventNames.AutomationCommandExecuted,
                    new
                    {
                        Ticket = Ticket.Empty,
                        AutomationCommandName = Settings.CommandName,
                        Value = val
                    });
            }
        }

        private string GetCommandValues()
        {
            return !string.IsNullOrEmpty(Settings.CommandValue)
                       ? string.Join(",",
                                     TicketList.Where(x => x.IsSelected)
                                               .Select(x => _printerService.GetPrintingContent(x.Ticket, Settings.CommandValue, 0)))
                       : string.Join(",",
                                     TicketList.Where(x => x.IsSelected)
                                               .Select(x => x.Ticket.Id));
        }

        [Browsable(false)]
        public bool IsRefreshing { get; set; }
        [Browsable(false)]
        public string FontName { get { return Settings.FontName; } }
        [Browsable(false)]
        public int FontSize { get { return Settings.FontSize; } }
        [Browsable(false)]
        public string Background { get { return Settings.Background; } }
        [Browsable(false)]
        public string Foreground { get { return Settings.Foreground; } }

        [Browsable(false)]
        public IList<TicketViewData> TicketList { get; set; }

        internal IList<TicketViewData> GetTicketList()
        {
            var tickets = _ticketService.GetTicketsByState(Settings.State);
      
            if (!string.IsNullOrEmpty(Settings.OrderBy))
            {
                if (Settings.OrderBy.Contains("{"))
                {
                    tickets = tickets.OrderBy(x => _printerService.GetPrintingContent(x, Settings.OrderBy, 0));
                }
                else switch (Settings.OrderBy)
                {
                    case "Id":
                        tickets = tickets.OrderBy(x => x.Id);
                        break;
                    case "Ticket No":
                        tickets = tickets.OrderBy(x => x.TicketNumber);
                        break;
                    case "Last Order":
                        tickets = tickets.OrderBy(x => x.LastOrderDate);
                        break;
                    default:
                        {
                            var entityTypeName = Settings.OrderBy;
                            var entityFieldName = "";
                            if (entityTypeName.Contains(":"))
                            {
                                var parts = entityTypeName.Split(':');
                                entityTypeName = parts[0];
                                entityFieldName = parts[1];
                            }
                            var entityType = _cacheService.GetEntityTypeIdByEntityName(entityTypeName);
                            if (entityType > 0)
                            {
                                if (string.IsNullOrEmpty(entityFieldName))
                                    tickets = tickets.OrderBy(x => x.GetEntityName(entityType));
                                else
                                {
                                    tickets =
                                        tickets.OrderBy(
                                            x =>
                                            x.TicketEntities.First(y => y.EntityTypeId == entityType)
                                             .GetCustomData(entityFieldName));
                                }
                            }
                        }
                        break;
                }
            }

            return tickets.Select(x => new TicketViewData
                                           {
                                               ItemSelectionCommand = ItemSelectionCommand,
                                               Background = Settings.Background,
                                               Foreground = Settings.Foreground,
                                               SelectedBackground = Settings.SelectedBackground,
                                               SelectedForeground = Settings.SelectedForeground,
                                               MinWidth = Settings.MinWidth,
                                               Border = Settings.Border,
                                               TicketData = _printerService.GetPrintingContent(x, Settings.Format, Settings.Width),
                                               Ticket = x
                                           })
                                           .ToList();
        }

        [Browsable(false)]
        public TicketListerWidgetSettings Settings { get { return SettingsObject as TicketListerWidgetSettings; } }

        protected override object CreateSettingsObject()
        {
            return JsonHelper.Deserialize<TicketListerWidgetSettings>(_model.Properties);
        }

        protected override void BeforeEditSettings()
        {
            var values = new List<string> { "Id", "Ticket No", "Last Order" };
            values.AddRange(_cacheService.GetEntityTypes().Select(x => x.EntityName).ToList());
            Settings.OrderByNameValue.UpdateValues(values);
            Settings.CommandNameValue.UpdateValues(_automationDao.GetAutomationCommandNames());
        }

        public override void Refresh()
        {
            if (IsRefreshing) return;
            using (var worker = new BackgroundWorker())
            {
                worker.DoWork += delegate
                                     {
                                         IsRefreshing = true;
                                         TicketList = GetTicketList();
                                     };

                worker.RunWorkerCompleted += (sender, eventArgs) =>
                {
                    IsRefreshing = false;
                    RaisePropertyChanged(() => TicketList);
                };

                worker.RunWorkerAsync();
            }
        }
    }

    public class TicketViewData : ObservableObject
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged(() => BackgroundValue);
                RaisePropertyChanged(() => ForegroundValue);
            }
        }

        public Ticket Ticket { get; set; }
        public string TicketData { get; set; }
        public string Background { get; set; }
        public string Foreground { get; set; }
        public string SelectedBackground { get; set; }
        public string SelectedForeground { get; set; }
        public string BackgroundValue { get { return IsSelected ? SelectedBackground : Background; } }
        public string ForegroundValue { get { return IsSelected ? SelectedForeground : Foreground; } }
        public string Border { get; set; }
        public int MinWidth { get; set; }
        public DelegateCommand<TicketViewData> ItemSelectionCommand { get; set; }
    }
}
