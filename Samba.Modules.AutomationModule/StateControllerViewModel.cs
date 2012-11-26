using System;
using System.ComponentModel.Composition;
using ICSharpCode.AvalonEdit.Document;
using Samba.Domain.Models.Automation;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AutomationModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class StateControllerViewModel : EntityViewModelBase<StateController>
    {
        public string StateDefinition { get { return Model.StateDefinition; } set { Model.StateDefinition = value; } }
        public TextDocument StateDefinitionText { get; set; }

        protected override void Initialize()
        {
            base.Initialize();
            StateDefinitionText = new TextDocument(StateDefinition ?? "");
        }

        protected override void OnSave(string value)
        {
            StateDefinition = StateDefinitionText.Text;
            base.OnSave(value);
        }

        public override Type GetViewType()
        {
            return typeof(StateControllerView);
        }

        public override string GetModelTypeString()
        {
            return Resources.StateController;
        }
    }
}
