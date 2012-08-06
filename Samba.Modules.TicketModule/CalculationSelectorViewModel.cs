using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Presentation.ViewModels;

namespace Samba.Modules.TicketModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class CalculationSelectorViewModel : EntityViewModelBase<CalculationSelector>
    {
        [ImportingConstructor]
        public CalculationSelectorViewModel()
        {
            AddCalculationTemplateCommand = new CaptionCommand<string>(Resources.Add, OnAddCalculationTemplate);
            DeleteCalculationTemplateCommand = new CaptionCommand<string>(Resources.Delete, OnDeleteCalculationTemplate, CanDeleteCalculationTemplate);
        }

        public MapController<CalculationSelectorMap, AbstractMapViewModel<CalculationSelectorMap>> MapController { get; set; }

        public CalculationTemplate SelectedCalculationTemplate { get; set; }
        public ICaptionCommand AddCalculationTemplateCommand { get; set; }
        public ICaptionCommand DeleteCalculationTemplateCommand { get; set; }

        public string ButtonHeader { get { return Model.ButtonHeader; } set { Model.ButtonHeader = value; } }
        public string ButtonColor { get { return Model.ButtonColor; } set { Model.ButtonColor = value; } }

        private ObservableCollection<CalculationTemplate> _calculationTemplates;
        public ObservableCollection<CalculationTemplate> CalculationTemplates
        {
            get { return _calculationTemplates ?? (_calculationTemplates = new ObservableCollection<CalculationTemplate>(Model.CalculationTemplates)); }
        }

        private bool CanDeleteCalculationTemplate(string arg)
        {
            return SelectedCalculationTemplate != null;
        }

        private void OnDeleteCalculationTemplate(string obj)
        {
            Model.CalculationTemplates.Remove(SelectedCalculationTemplate);
            CalculationTemplates.Remove(SelectedCalculationTemplate);
        }

        private void OnAddCalculationTemplate(string obj)
        {
            var selectedValues =
                 InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<CalculationTemplate>().ToList<IOrderable>(),
                 Model.CalculationTemplates.ToList<IOrderable>(), Resources.TicketTags, string.Format(Resources.SelectItemsFor_f, Resources.CalculationTemplate, Model.Name, Resources.CalculationSelector),
                 Resources.CalculationTemplate, Resources.CalculationTemplate.ToPlural());

            foreach (CalculationTemplate selectedValue in selectedValues)
            {
                if (!Model.CalculationTemplates.Contains(selectedValue))
                    Model.CalculationTemplates.Add(selectedValue);
            }

            _calculationTemplates = null;
            RaisePropertyChanged(() => CalculationTemplates);
        }

        public override Type GetViewType()
        {
            return typeof(CalculationSelectorView);
        }

        public override string GetModelTypeString()
        {
            return Resources.CalculationSelector;
        }

        protected override void Initialize()
        {
            base.Initialize();
            MapController = new MapController<CalculationSelectorMap, AbstractMapViewModel<CalculationSelectorMap>>(Model.CalculationSelectorMaps, Workspace);
        }
    }
}
