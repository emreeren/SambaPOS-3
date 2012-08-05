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
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class CalculationSelectorViewModel : EntityViewModelBase<CalculationSelector>
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;
        private readonly ISettingService _settingService;

        [ImportingConstructor]
        public CalculationSelectorViewModel(IUserService userService, IDepartmentService departmentService, ISettingService settingService)
        {
            _userService = userService;
            _departmentService = departmentService;
            _settingService = settingService;

            AddCalculationSelectorMapCommand = new CaptionCommand<string>(Resources.Add, OnAddCalculationSelectorMap);
            DeleteCalculationSelectorMapCommand = new CaptionCommand<string>(Resources.Delete, OnDeleteCalculationSelectorMap, CanDeleteCalculationSelectorMap);

            AddCalculationTemplateCommand = new CaptionCommand<string>(Resources.Add, OnAddCalculationTemplate);
            DeleteCalculationTemplateCommand = new CaptionCommand<string>(Resources.Delete, OnDeleteCalculationTemplate, CanDeleteCalculationTemplate);
        }

        public CalculationSelectorMapViewModel SelectedCalculationSelectorMap { get; set; }
        public CaptionCommand<string> DeleteCalculationSelectorMapCommand { get; set; }
        public CaptionCommand<string> AddCalculationSelectorMapCommand { get; set; }

        public CalculationTemplate SelectedCalculationTemplate { get; set; }
        public ICaptionCommand AddCalculationTemplateCommand { get; set; }
        public ICaptionCommand DeleteCalculationTemplateCommand { get; set; }

        public string ButtonHeader { get { return Model.ButtonHeader; } set { Model.ButtonHeader = value; } }
        public string ButtonColor { get { return Model.ButtonColor; } set { Model.ButtonColor = value; } }

        private ObservableCollection<CalculationSelectorMapViewModel> _calculationSelectorMaps;
        public ObservableCollection<CalculationSelectorMapViewModel> CalculationSelectorMaps
        {
            get { return _calculationSelectorMaps ?? (_calculationSelectorMaps = new ObservableCollection<CalculationSelectorMapViewModel>(Model.CalculationSelectorMaps.Select(x => new CalculationSelectorMapViewModel(x, _userService, _departmentService, _settingService)))); }
        }

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

        private void OnDeleteCalculationSelectorMap(string obj)
        {
            if (SelectedCalculationSelectorMap.Id > 0)
                Workspace.Delete(SelectedCalculationSelectorMap.Model);
            Model.CalculationSelectorMaps.Remove(SelectedCalculationSelectorMap.Model);
            CalculationSelectorMaps.Remove(SelectedCalculationSelectorMap);
        }

        private bool CanDeleteCalculationSelectorMap(string arg)
        {
            return SelectedCalculationSelectorMap != null;
        }

        private void OnAddCalculationSelectorMap(string obj)
        {
            CalculationSelectorMaps.Add(new CalculationSelectorMapViewModel(Model.AddCalculationSelectorMap(), _userService, _departmentService, _settingService));
        }

        public override Type GetViewType()
        {
            return typeof(CalculationSelectorView);
        }

        public override string GetModelTypeString()
        {
            return Resources.CalculationSelector;
        }
    }
}
