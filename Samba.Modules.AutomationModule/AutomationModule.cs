using System.ComponentModel.Composition;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Automation;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.AutomationModule
{
    [ModuleExport(typeof(AutomationModule))]
    class AutomationModule : ModuleBase
    {
        private readonly IExpressionService _expressionService;

        [ImportingConstructor]
        public AutomationModule(IExpressionService expressionService, IAutomationService automationService)
        {
            _expressionService = expressionService;
            AddDashboardCommand<EntityCollectionViewModelBase<RuleActionViewModel, AppAction>>(Resources.RuleActions, Resources.Automation, 45);
            AddDashboardCommand<EntityCollectionViewModelBase<RuleViewModel, AppRule>>(Resources.Rules, Resources.Automation, 45);
            AddDashboardCommand<TriggerListViewModel>(Resources.Trigger.ToPlural(), Resources.Automation, 45);
            AddDashboardCommand<EntityCollectionViewModelBase<AutomationCommandViewModel, AutomationCommand>>(Resources.AutomationCommand.ToPlural(), Resources.Automation, 45);
            AddDashboardCommand<EntityCollectionViewModelBase<ScriptViewModel, Script>>(Resources.Script.ToPlural(), Resources.Automation, 45);

            automationService.RegisterActionType(ActionNames.ExecuteScript, Resources.ExecuteScript, new { ScriptName = "" });

            HighlightingManager.Instance.RegisterHighlighting("SambaDSL", null, () => LoadHighlightingDefinition("SambaDSL.xshd"));
        }

        protected override void OnInitialization()
        {
            base.OnInitialization();
            EventServiceFactory.EventService.GetEvent<GenericEvent<IActionData>>().Subscribe(OnActionData);
        }

        private void OnActionData(EventParameters<IActionData> obj)
        {
            if (obj.Value.Action.ActionType == ActionNames.ExecuteScript)
            {
                var script = obj.Value.GetAsString("ScriptName");
                if (!string.IsNullOrEmpty(script))
                {
                    _expressionService.EvalCommand(script, null, obj.Value.DataObject, true);
                }
            }
        }

        public static IHighlightingDefinition LoadHighlightingDefinition(string resourceName)
        {
            var type = typeof(AutomationModule);
            var fullName = type.Namespace + "." + resourceName;
            using (var stream = type.Assembly.GetManifestResourceStream(fullName))
            using (var reader = new XmlTextReader(stream))
                return HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
    }
}
