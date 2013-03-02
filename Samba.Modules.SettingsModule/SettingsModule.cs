using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.SettingsModule
{
    [ModuleExport(typeof(SettingsModule))]
    public class SettingsModule : ModuleBase
    {
        [ImportingConstructor]
        public SettingsModule()
        {
            AddDashboardCommand<SettingsViewModel>(Resources.LocalSettings, Resources.Settings, 20);
            AddDashboardCommand<TerminalListViewModel>(Resources.Terminals, Resources.Settings, 21);
            AddDashboardCommand<EntityCollectionViewModelBase<NumeratorViewModel, Numerator>>(Resources.Numerators, Resources.Settings, 21);
            AddDashboardCommand<EntityCollectionViewModelBase<ForeignCurrencyViewModel, ForeignCurrency>>(string.Format(Resources.List_f, Resources.Currency), Resources.Settings, 21);
            AddDashboardCommand<EntityCollectionViewModelBase<StateViewModel, State>>(Resources.State.ToPlural(), Resources.Settings, 21);
            AddDashboardCommand<ProgramSettingsViewModel>(Resources.ProgramSettings, Resources.Settings, 22);
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
