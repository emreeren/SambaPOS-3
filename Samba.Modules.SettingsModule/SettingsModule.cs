using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Modules.SettingsModule.BrowserViews;
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
            AddDashboardCommand<SambaPosWebsite>(Resources.SambaPosWebsite, Resources.SambaNetwork, 90);
            AddDashboardCommand<SambaPosDocumentation>("SambaPOS Documentation", Resources.SambaNetwork, 91); //TODO: make localisation-string
            AddDashboardCommand<SambaPosForum>("SambaPOS Forum", Resources.SambaNetwork, 92); //TODO: make localisation-string
            AddDashboardCommand<SambaPosDevelopment>("SambaPOS Development", Resources.SambaNetwork, 93); //TODO: make localisation-string
            AddDashboardCommand<SambaPosWiki>("SambaPOS Wiki", Resources.SambaNetwork, 94); //TODO: make localisation-string
        }
    }
}
