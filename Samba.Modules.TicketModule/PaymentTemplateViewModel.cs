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
    public class PaymentTemplateViewModel : EntityViewModelBase<PaymentTemplate>
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;
        private readonly ISettingService _settingService;

        [ImportingConstructor]
        public PaymentTemplateViewModel(IUserService userService, IDepartmentService departmentService, ISettingService settingService)
        {
            _userService = userService;
            _departmentService = departmentService;
            _settingService = settingService;

            AddPaymentTemplateMapCommand = new CaptionCommand<string>(Resources.Add, OnAddPaymentTemplateMap);
            DeletePaymentTemplateMapCommand = new CaptionCommand<string>(Resources.Delete, OnDeletePaymentTemplateMap, CanDeletePaymentTemplateMap);
        }

        public PaymentTemplateMapViewModel SelectedPaymentTemplateMap { get; set; }

        public CaptionCommand<string> DeletePaymentTemplateMapCommand { get; set; }
        public CaptionCommand<string> AddPaymentTemplateMapCommand { get; set; }

        private IEnumerable<AccountTransactionTemplate> _accountTransactionTemplates;
        public IEnumerable<AccountTransactionTemplate> AccountTransactionTemplates
        {
            get
            {
                return _accountTransactionTemplates ?? (_accountTransactionTemplates =
                    Workspace.All<AccountTransactionTemplate>().ToList());
            }
        }

        private IEnumerable<Account> _accounts;
        public IEnumerable<Account> Accounts
        {
            get
            {
                return _accounts ?? (_accounts =
                    AccountTransactionTemplate != null
                    ? Workspace.All<Account>(x => x.AccountTemplateId == AccountTransactionTemplate.TargetAccountTemplateId).ToList()
                    : new List<Account>());
            }
        }

        public AccountTransactionTemplate AccountTransactionTemplate
        {
            get { return Model.AccountTransactionTemplate; }
            set
            {
                Model.AccountTransactionTemplate = value;
                Account = null;
                _accounts = null;
                RaisePropertyChanged(() => Accounts);
            }
        }

        public Account Account
        {
            get { return Model.Account; }
            set
            {
                Model.Account = value;
                RaisePropertyChanged(() => Account);
            }
        }

        public string ButtonColor { get { return Model.ButtonColor; } set { Model.ButtonColor = value; } }

        private ObservableCollection<PaymentTemplateMapViewModel> _paymentTemplateMaps;
        public ObservableCollection<PaymentTemplateMapViewModel> PaymentTemplateMaps
        {
            get { return _paymentTemplateMaps ?? (_paymentTemplateMaps = new ObservableCollection<PaymentTemplateMapViewModel>(Model.PaymentTemplateMaps.Select(x => new PaymentTemplateMapViewModel(x, _userService, _departmentService, _settingService)))); }
        }

        private bool CanDeletePaymentTemplateMap(string arg)
        {
            return SelectedPaymentTemplateMap != null;
        }

        private void OnDeletePaymentTemplateMap(string obj)
        {
            if (SelectedPaymentTemplateMap.Id > 0)
                Workspace.Delete(SelectedPaymentTemplateMap.Model);
            Model.PaymentTemplateMaps.Remove(SelectedPaymentTemplateMap.Model);
            PaymentTemplateMaps.Remove(SelectedPaymentTemplateMap);
        }

        private void OnAddPaymentTemplateMap(string obj)
        {
            PaymentTemplateMaps.Add(new PaymentTemplateMapViewModel(Model.AddPaymentTemplateMap(), _userService, _departmentService, _settingService));
        }

        public override Type GetViewType()
        {
            return typeof(PaymentTemplateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.PaymentTemplate;
        }

        protected override AbstractValidator<PaymentTemplate> GetValidator()
        {
            return new PaymentTemplateValidator();
        }
    }

    internal class PaymentTemplateValidator : EntityValidator<PaymentTemplate>
    {
        public PaymentTemplateValidator()
        {
            RuleFor(x => x.AccountTransactionTemplate).NotNull();
            //RuleFor(x => x.Account).NotNull();
        }
    }
}
