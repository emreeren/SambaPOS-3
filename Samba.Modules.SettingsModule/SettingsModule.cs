using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Localization.Properties;
using Samba.Presentation.Common;

namespace Samba.Modules.SettingsModule
{
    [ModuleExport(typeof(SettingsModule))]
    public class SettingsModule : ModuleBase
    {
        [ImportingConstructor]
        public SettingsModule()
        {
            AddDashboardCommand<SettingsViewModel>(Resources.LocalSettings, Resources.Settings);
            AddDashboardCommand<TerminalListViewModel>(Resources.Terminals, Resources.Settings);
            AddDashboardCommand<PrinterListViewModel>(Resources.Printers, Resources.Settings);
            AddDashboardCommand<PrintJobListViewModel>(Resources.PrintJobs, Resources.Settings);
            AddDashboardCommand<PrinterTemplateCollectionViewModel>(Resources.PrinterTemplates, Resources.Settings);
            AddDashboardCommand<NumeratorListViewModel>(Resources.Numerators, Resources.Settings);
            AddDashboardCommand<ProgramSettingsViewModel>(Resources.ProgramSettings, Resources.Settings, 10);
            AddDashboardCommand<RuleActionListViewModel>(Resources.RuleActions, Resources.Settings);
            AddDashboardCommand<RuleListViewModel>(Resources.Rules, Resources.Settings);
            AddDashboardCommand<TriggerListViewModel>(Resources.Triggers, Resources.Settings);
            AddDashboardCommand<BrowserViewModel>(Resources.SambaPosWebsite, Resources.SambaNetwork, 99);
        }

        //private void OnShowBrowser(string obj)
        //{
        //    if (_browserViewModel == null)
        //        _browserViewModel = new BrowserViewModel();
        //    CommonEventPublisher.PublishViewAddedEvent(_browserViewModel);
        //    new Uri("http://network.sambapos.com").PublishEvent(EventTopicNames.BrowseUrl);
        //}
    }
}
