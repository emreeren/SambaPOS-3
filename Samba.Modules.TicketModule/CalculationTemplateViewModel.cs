using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class CalculationTemplateViewModel : EntityViewModelBase<CalculationTemplate>
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;
        private readonly ISettingService _settingService;

        [ImportingConstructor]
        public CalculationTemplateViewModel(IUserService userService, IDepartmentService departmentService, ISettingService settingService)
        {
            _userService = userService;
            _departmentService = departmentService;
            _settingService = settingService;
            AddCalculationTemplateMapCommand = new CaptionCommand<string>(Resources.Add, OnAddCalculationTemplateMap);
            DeleteCalculationTemplateMapCommand = new CaptionCommand<string>(Resources.Delete, OnDeleteCalculationTemplateMap, CanDeleteCalculationTemplateMap);
        }

        private ObservableCollection<CalculationTemplateMapViewModel> _calculationTemplateMaps;
        public ObservableCollection<CalculationTemplateMapViewModel> CalculationTemplateMaps
        {
            get { return _calculationTemplateMaps ?? (_calculationTemplateMaps = new ObservableCollection<CalculationTemplateMapViewModel>(Model.CalculationTemplateMaps.Select(x => new CalculationTemplateMapViewModel(x, _userService, _departmentService, _settingService)))); }
        }

        public CalculationTemplateMapViewModel SelectedCalculationTemplateMap { get; set; }
        public CaptionCommand<string> DeleteCalculationTemplateMapCommand { get; set; }
        public CaptionCommand<string> AddCalculationTemplateMapCommand { get; set; }

        private string[] _calculationMethods;
        public string[] CalculationMethods
        {
            get
            {
                return _calculationMethods ?? (_calculationMethods = new[] {
                    Resources.RateFromTicketAmount,
                    Resources.RateFromPreviousTemplate, 
                    Resources.FixedAmount,
                    Resources.FixedAmountFromTicketTotal});
            }
        }

        public string SelectedCalculationMethod { get { return CalculationMethods[CalculationMethod]; } set { CalculationMethod = Array.IndexOf(CalculationMethods, value); } }

        public int CalculationMethod { get { return Model.CalculationMethod; } set { Model.CalculationMethod = value; } }
        public decimal Amount { get { return Model.Amount; } set { Model.Amount = value; } }
        public decimal MaxAmount { get { return Model.MaxAmount; } set { Model.MaxAmount = value; } }
        public string ButtonHeader { get { return Model.ButtonHeader; } set { Model.ButtonHeader = value; } }
        public string ButtonColor { get { return Model.ButtonColor; } set { Model.ButtonColor = value; } }
        public bool IncludeTax { get { return Model.IncludeTax; } set { Model.IncludeTax = value; } }
        public bool DecreaseAmount { get { return Model.DecreaseAmount; } set { Model.DecreaseAmount = value; } }

        private IEnumerable<AccountTransactionTemplate> _accountTransactionTemplates;
        public IEnumerable<AccountTransactionTemplate> AccountTransactionTemplates { get { return _accountTransactionTemplates ?? (_accountTransactionTemplates = Workspace.All<AccountTransactionTemplate>()); } }

        public AccountTransactionTemplate AccountTransactionTemplate { get { return Model.AccountTransactionTemplate; } set { Model.AccountTransactionTemplate = value; } }

        private void OnDeleteCalculationTemplateMap(string obj)
        {
            if (SelectedCalculationTemplateMap.Id > 0)
                Workspace.Delete(SelectedCalculationTemplateMap.Model);
            Model.CalculationTemplateMaps.Remove(SelectedCalculationTemplateMap.Model);
            CalculationTemplateMaps.Remove(SelectedCalculationTemplateMap);
        }

        private bool CanDeleteCalculationTemplateMap(string arg)
        {
            return SelectedCalculationTemplateMap != null;
        }

        private void OnAddCalculationTemplateMap(string obj)
        {
            CalculationTemplateMaps.Add(new CalculationTemplateMapViewModel(Model.AddCalculationTemplateMap(), _userService, _departmentService, _settingService));
        }

        public override Type GetViewType()
        {
            return typeof(CalculationTemplateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.CalculationTemplate;
        }

        protected override AbstractValidator<CalculationTemplate> GetValidator()
        {
            return new CalculationTemplateValidator();
        }
    }

    internal class CalculationTemplateValidator : EntityValidator<CalculationTemplate>
    {
        public CalculationTemplateValidator()
        {
            RuleFor(x => x.AccountTransactionTemplate).NotNull();
        }
    }
}
