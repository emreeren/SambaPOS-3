using System.ComponentModel;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Helpers;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.Widgets;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.AutomationModule.WidgetCreators
{
    class AutomationButtonWidgetViewModel : WidgetViewModel
    {
        private readonly IAutomationService _automationService;
        private readonly IAutomationServiceBase _automationServiceBase;

        [Browsable(false)]
        public CaptionCommand<AutomationButtonWidgetViewModel> ItemClickedCommand { get; set; }

        public AutomationButtonWidgetViewModel(Widget widget, IApplicationState applicationState, IAutomationService automationService, IAutomationServiceBase automationServiceBase)
            : base(widget, applicationState)
        {
            _automationService = automationService;
            _automationServiceBase = automationServiceBase;
            ItemClickedCommand = new CaptionCommand<AutomationButtonWidgetViewModel>("", OnItemClicked);
        }

        private void OnItemClicked(AutomationButtonWidgetViewModel obj)
        {
            _automationService.NotifyEvent(RuleEventNames.AutomationCommandExecuted, new { AutomationCommandName = obj.Settings.CommandName });
        }

        protected override object CreateSettingsObject()
        {
            return JsonHelper.Deserialize<AutomationButtonWidgetSettings>(_model.Properties);
        }

        public override void Refresh()
        {
            //
        }

        [Browsable(false)]
        public AutomationButtonWidgetSettings Settings { get { return SettingsObject as AutomationButtonWidgetSettings; } }


        protected override void BeforeEditSettings()
        {
            Settings.CommandNameValue.UpdateValues(_automationServiceBase.GetAutomationCommandNames());
        }
    }
}
