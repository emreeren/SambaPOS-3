using System.ComponentModel;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AutomationModule.WidgetCreators
{
    class AutomationButtonWidgetViewModel : WidgetViewModel
    {
        private readonly IAutomationService _automationService;
        [Browsable(false)]
        public CaptionCommand<AutomationButtonWidgetViewModel> ItemClickedCommand { get; set; }

        public AutomationButtonWidgetViewModel(Widget widget, IApplicationState applicationState, IAutomationService automationService)
            : base(widget, applicationState)
        {
            _automationService = automationService;
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
            Settings.CommandNameValue.UpdateValues(_automationService.GetAutomationCommandNames());
        }
    }
}
