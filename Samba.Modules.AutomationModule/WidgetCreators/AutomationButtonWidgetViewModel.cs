using System.ComponentModel;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Helpers;
using Samba.Persistance;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.Widgets;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.AutomationModule.WidgetCreators
{
    class AutomationButtonWidgetViewModel : WidgetViewModel
    {
        private readonly IApplicationState _applicationState;
        private readonly IAutomationDao _automationDao;

        public AutomationButtonWidgetViewModel(Widget widget, IApplicationState applicationState, IAutomationDao automationDao) : base(widget, applicationState)
        {
            _applicationState = applicationState;
            _automationDao = automationDao;
            ItemClickedCommand = new CaptionCommand<AutomationButtonWidgetViewModel>("", OnItemClicked);
        }

        [Browsable(false)]
        public CaptionCommand<AutomationButtonWidgetViewModel> ItemClickedCommand { get; set; }

        [Browsable(false)]
        public AutomationButtonWidgetSettings Settings
        {
            get
            {
                return SettingsObject as AutomationButtonWidgetSettings;
            }
        }

        public override void Refresh()
        {
            //
        }

        protected override object CreateSettingsObject()
        {
            return JsonHelper.Deserialize<AutomationButtonWidgetSettings>(_model.Properties);
        }

        protected override void BeforeEditSettings()
        {
            Settings.CommandNameValue.UpdateValues(_automationDao.GetAutomationCommandNames());
        }

        private void OnItemClicked(AutomationButtonWidgetViewModel obj)
        {
            _applicationState.NotifyEvent(RuleEventNames.AutomationCommandExecuted,
                new 
                    {
                        Ticket = Ticket.Empty,
                        AutomationCommandName = obj.Settings.CommandName,
                        obj.Settings.Value
                    });
        }
        }
}
