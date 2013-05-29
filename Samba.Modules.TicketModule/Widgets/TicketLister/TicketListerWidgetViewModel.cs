using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        public bool IsRefreshing { get; set; }
        private readonly IApplicationState _applicationState;
        private readonly ITicketServiceBase _ticketService;
        private readonly IPrinterService _printerService;
        private readonly ICacheService _cacheService;
        private readonly IAutomationDao _automationDao;

        public TicketListerWidgetViewModel(Widget model, IApplicationState applicationState, ITicketServiceBase ticketService,
            IPrinterService printerService, ICacheService cacheService, IAutomationDao automationDao)
            : base(model, applicationState)
        {
            _applicationState = applicationState;
            _ticketService = ticketService;
            _printerService = printerService;
            _cacheService = cacheService;
            _automationDao = automationDao;

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

        [Browsable(false)]
        public string FontName { get { return Settings.FontName; } }
        [Browsable(false)]
        public int FontSize { get { return Settings.FontSize; } }
        [Browsable(false)]
        public string Background { get { return Settings.Background; } }
        [Browsable(false)]
        public string Foreground { get { return Settings.Foreground; } }

        private TicketViewData _selectedItem;

        [Browsable(false)]
        public TicketViewData SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                // _settingService.ReadLocalSetting(Settings.SelectedTicketSettingName).IntegerValue = value != null ? value.Ticket.Id : 0;
                if (!string.IsNullOrEmpty(Settings.CommandName))
                {
                    var val = "";

                    if (value != null)
                    {
                        val = _printerService.GetPrintingContent(value.Ticket, Settings.CommandValue, 0);
                    }

                    _applicationState.NotifyEvent(RuleEventNames.AutomationCommandExecuted, new
                                                                                                {
                                                                                                    Ticket = Ticket.Empty,
                                                                                                    AutomationCommandName = Settings.CommandName,
                                                                                                    Value = val
                                                                                                });
                }
            }
        }

        [Browsable(false)]
        public IList<TicketViewData> TicketList { get; set; }

        internal IList<TicketViewData> GetTicketList()
        {
            var tickets = _ticketService.GetTicketsByState(Settings.State);

            if (!string.IsNullOrEmpty(Settings.OrderBy))
            {
                if (Settings.OrderBy == "Id")
                    tickets = tickets.OrderBy(x => x.Id);
                else if (Settings.OrderBy == "Ticket No")
                    tickets = tickets.OrderBy(x => x.TicketNumber);
                else if (Settings.OrderBy == "Last Order")
                    tickets = tickets.OrderBy(x => x.LastOrderDate);
                else
                {
                    var entityType = _cacheService.GetEntityTypeIdByEntityName(Settings.OrderBy);
                    if (entityType > 0)
                        tickets = tickets.OrderBy(x => x.GetEntityName(entityType));
                }
            }

            return tickets.Select(x => new TicketViewData
                                           {
                                               Background = Settings.Background,
                                               Foreground = Settings.Foreground,
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

    public class TicketViewData
    {
        public Ticket Ticket { get; set; }
        public string TicketData { get; set; }
        public string Background { get; set; }
        public string Foreground { get; set; }
        public string Border { get; set; }
    }
}
