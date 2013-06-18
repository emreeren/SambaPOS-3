using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.TicketModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class CalculationSelectorViewModel : EntityViewModelBaseWithMap<CalculationSelector,CalculationSelectorMap,AbstractMapViewModel<CalculationSelectorMap>>
    {
        [ImportingConstructor]
        public CalculationSelectorViewModel()
        {
            AddCalculationTypeCommand = new CaptionCommand<string>(Resources.Add, OnAddCalculationType);
            DeleteCalculationTypeCommand = new CaptionCommand<string>(Resources.Delete, OnDeleteCalculationType, CanDeleteCalculationType);
        }

        public CalculationType SelectedCalculationType { get; set; }
        public ICaptionCommand AddCalculationTypeCommand { get; set; }
        public ICaptionCommand DeleteCalculationTypeCommand { get; set; }

        public string ButtonHeader { get { return Model.ButtonHeader; } set { Model.ButtonHeader = value; } }
        public string ButtonColor { get { return Model.ButtonColor; } set { Model.ButtonColor = value; } }
        public int FontSize { get { return Model.FontSize; } set { Model.FontSize = value; } }

        private ObservableCollection<CalculationType> _calculationTypes;
        public ObservableCollection<CalculationType> CalculationTypes
        {
            get { return _calculationTypes ?? (_calculationTypes = new ObservableCollection<CalculationType>(Model.CalculationTypes)); }
        }

        private bool CanDeleteCalculationType(string arg)
        {
            return SelectedCalculationType != null;
        }

        private void OnDeleteCalculationType(string obj)
        {
            Model.CalculationTypes.Remove(SelectedCalculationType);
            CalculationTypes.Remove(SelectedCalculationType);
        }

        private void OnAddCalculationType(string obj)
        {
            var selectedValues =
                 InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<CalculationType>().ToList<IOrderable>(),
                 Model.CalculationTypes.ToList<IOrderable>(), Resources.TicketTag.ToPlural(), string.Format(Resources.SelectItemsFor_f, Resources.CalculationType, Model.Name, Resources.CalculationSelector),
                 Resources.CalculationType, Resources.CalculationType.ToPlural());

            foreach (CalculationType selectedValue in selectedValues)
            {
                if (!Model.CalculationTypes.Contains(selectedValue))
                    Model.CalculationTypes.Add(selectedValue);
            }

            _calculationTypes = null;
            RaisePropertyChanged(() => CalculationTypes);
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
