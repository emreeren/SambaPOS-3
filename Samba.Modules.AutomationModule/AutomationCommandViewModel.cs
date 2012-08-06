using System;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Actions;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.ViewModels;

namespace Samba.Modules.AutomationModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class AutomationCommandViewModel : EntityViewModelBase<AutomationCommand>
    {
        public MapController<AutomationCommandMap, AutomationCommandMapViewModel> MapController { get; set; }

        [ImportingConstructor]
        public AutomationCommandViewModel()
        {

        }

        public string ButtonHeader { get { return Model.ButtonHeader; } set { Model.ButtonHeader = value; } }
        public string Color { get { return Model.Color; } set { Model.Color = value; } }

        public override Type GetViewType()
        {
            return typeof(AutomationCommandView);
        }

        public override string GetModelTypeString()
        {
            return Resources.AutomationCommand;
        }

        protected override void Initialize()
        {
            base.Initialize();
            MapController = new MapController<AutomationCommandMap, AutomationCommandMapViewModel>(Model.AutomationCommandMaps, Workspace);
        }
    }
}
