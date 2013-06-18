using System;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Automation;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AutomationModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class AutomationCommandViewModel : EntityViewModelBaseWithMap<AutomationCommand, AutomationCommandMap, AutomationCommandMapViewModel>
    {
        [ImportingConstructor]
        public AutomationCommandViewModel()
        {
        }

        public string ButtonHeader { get { return Model.ButtonHeader; } set { Model.ButtonHeader = value; } }
        public string Color { get { return Model.Color; } set { Model.Color = value; } }
        public int FontSize { get { return Model.FontSize; } set { Model.FontSize = value; } }
        public string Values
        {
            get { return (Model.Values ?? "").Replace("|", Environment.NewLine); }
            set { Model.Values = value.Trim(Environment.NewLine.ToCharArray()).Replace(Environment.NewLine, "|"); }
        }

        public bool ToggleValues { get { return Model.ToggleValues; } set { Model.ToggleValues = value; } }

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
