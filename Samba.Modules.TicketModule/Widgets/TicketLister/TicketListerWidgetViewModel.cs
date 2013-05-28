using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Helpers;
using Samba.Infrastructure.Messaging;
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
        private readonly ISettingService _settingService;

        public TicketListerWidgetViewModel(Widget model, IApplicationState applicationState, ITicketServiceBase ticketService,
            IPrinterService printerService, ISettingService settingService)
            : base(model, applicationState)
        {
            _applicationState = applicationState;
            _ticketService = ticketService;
            _printerService = printerService;
            _settingService = settingService;

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

        public string FontName { get { return Settings.FontName; } }

        private TicketViewData _selectedItem;
        public TicketViewData SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value;
                _settingService.ReadLocalSetting(Settings.SelectedTicketSettingName).IntegerValue =value!=null? value.TicketId:0;
            }
        }

        public IList<TicketViewData> TicketList { get; set; }
        internal IList<TicketViewData> GetTicketList()
        {
            var tickets = _ticketService.GetTicketsByState(Settings.State);
            return tickets.Select(x => new TicketViewData { TicketData = _printerService.GetPrintingContent(x, Settings.Format, Settings.Width), TicketId = x.Id }).ToList();
        }

        [Browsable(false)]
        public TicketListerWidgetSettings Settings { get { return SettingsObject as TicketListerWidgetSettings; } }

        protected override object CreateSettingsObject()
        {
            return JsonHelper.Deserialize<TicketListerWidgetSettings>(_model.Properties);
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
        public string TicketData { get; set; }
        public int TicketId { get; set; }

        public override string ToString()
        {
            return TicketData;
        }
    }
}
