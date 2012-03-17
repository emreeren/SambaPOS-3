using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Presentation.ViewModels
{
    [Export]
    public class TicketTotalsViewModel : ObservableObject
    {
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public TicketTotalsViewModel(ICacheService cacheService, IApplicationState applicationState)
        {
            _cacheService = cacheService;
            _applicationState = applicationState;
            ResetCache();
        }

        private Ticket _model;
        public Ticket Model
        {
            get { return _model; }
            set
            {
                ResetCache();
                _model = value;
            }
        }

        private ObservableCollection<PaymentViewModel> _payments;
        public ObservableCollection<PaymentViewModel> Payments
        {
            get
            {
                return _payments ?? (_payments = new ObservableCollection<PaymentViewModel>(
                    Model.Payments.Select(x => new PaymentViewModel(x))));
            }
        }

        private ObservableCollection<ServiceViewModel> _preServices;
        public ObservableCollection<ServiceViewModel> PreServices
        {
            get { return _preServices ?? (_preServices = new ObservableCollection<ServiceViewModel>(Model.Calculations.Where(x => !x.IncludeTax).Select(x => new ServiceViewModel(x)))); }
        }

        private ObservableCollection<ServiceViewModel> _postServices;
        public ObservableCollection<ServiceViewModel> PostServices
        {
            get { return _postServices ?? (_postServices = new ObservableCollection<ServiceViewModel>(Model.Calculations.Where(x => x.IncludeTax).Select(x => new ServiceViewModel(x)))); }
        }

        public decimal TicketTotalValue { get { return Model.GetSum(); } }
        public decimal TicketTaxValue { get { return Model.CalculateTax(Model.GetPlainSum(), Model.GetPreTaxServicesTotal()); } }
        public decimal TicketSubTotalValue { get { return Model.GetPlainSum() + Model.GetPreTaxServicesTotal(); } }
        public decimal TicketPaymentValue { get { return Model.GetPaymentAmount(); } }
        public decimal TicketRemainingValue { get { return Model.GetRemainingAmount(); } }
        public decimal TicketPlainTotalValue { get { return Model.GetPlainSum(); } }

        public bool IsTicketTotalVisible { get { return TicketPaymentValue > 0 && TicketTotalValue > 0; } }
        public bool IsTicketPaymentVisible { get { return TicketPaymentValue > 0; } }
        public bool IsTicketRemainingVisible { get { return TicketRemainingValue > 0; } }
        public bool IsTicketTaxTotalVisible { get { return TicketTaxValue > 0; } }
        public bool IsPlainTotalVisible { get { return PostServices.Count > 0 || PreServices.Count > 0 || IsTicketTaxTotalVisible; } }
        public bool IsTicketSubTotalVisible { get { return PostServices.Count > 0 && PreServices.Count > 0; } }

        public string TicketPlainTotalLabel
        {
            get { return TicketPlainTotalValue.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketTotalLabel
        {
            get { return TicketTotalValue.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketTaxLabel
        {
            get { return TicketTaxValue.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketSubTotalLabel
        {
            get { return TicketSubTotalValue.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketPaymentLabel
        {
            get { return TicketPaymentValue.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketRemainingLabel
        {
            get { return TicketRemainingValue.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        private AccountTemplate _sourceAccountTemplate;
        public AccountTemplate SourceAccountTemplate
        {
            get
            {
                if (_applicationState.CurrentDepartment == null) return null;

                var sourceAccountTemplateId = Model.AccountTemplateId > 0
                                                  ? Model.AccountTemplateId
                                                  : _applicationState.CurrentDepartment.TicketTemplate.
                                                        SaleTransactionTemplate.TargetAccountTemplateId;

                if (_sourceAccountTemplate == null || _sourceAccountTemplate.Id != sourceAccountTemplateId)
                    _sourceAccountTemplate = _cacheService.GetAccountTemplateById(sourceAccountTemplateId);
                return _sourceAccountTemplate;
            }
        }

        private AccountTemplate _targetAccountTemplate;
        public AccountTemplate TargetAccountTemplate
        {
            get
            {
                if (_applicationState.CurrentDepartment == null) return null;

                var targetAccountTemplateId = Model.TargetAccountTemplateId > 0
                                                  ? Model.TargetAccountTemplateId
                                                  : _applicationState.CurrentDepartment.TicketTemplate.
                                                        TargetAccountTemplateId;

                if (targetAccountTemplateId == 0) return null;

                if (_targetAccountTemplate == null || _targetAccountTemplate.Id != targetAccountTemplateId)
                    _targetAccountTemplate = _cacheService.GetAccountTemplateById(targetAccountTemplateId);
                return _targetAccountTemplate;
            }
        }

        public string TargetEntityName { get { return TargetAccountTemplate != null ? TargetAccountTemplate.EntityName : ""; } }
        public string SourceEntityName { get { return SourceAccountTemplate != null ? SourceAccountTemplate.EntityName : ""; } }

        public string Title
        {
            get
            {
                if (Model == null) return "";
                var sourceName = Model.AccountId > 0 ? string.Format("{0}: {1}", SourceEntityName, Model.AccountName) : "";
                var targetName = Model.TargetAccountId > 0 ? string.Format("{0}: {1}", TargetEntityName, Model.TargetAccountName) : ""; ;
                var selectedTicketTitle = sourceName + (!string.IsNullOrEmpty(targetName) ? "\r" + targetName : "");
                if (Model.Id > 0) selectedTicketTitle = string.Format("# {0} {1}", Model.TicketNumber, selectedTicketTitle);
                if (string.IsNullOrEmpty(selectedTicketTitle)) selectedTicketTitle = string.Format(Resources.New_f, Resources.Ticket);

                //if (Model.AccountId > 0 && Model.Id == 0)
                //    selectedTicketTitle = string.Format(Resources.Location_f, Model.LocationName);
                //else if (!string.IsNullOrEmpty(Model.AccountName) && Model.Id == 0)
                //    selectedTicketTitle = string.Format(Resources.Account_f, Model.AccountName);
                //else if (string.IsNullOrEmpty(Model.AccountName)) selectedTicketTitle = string.IsNullOrEmpty(Model.LocationName)
                //     ? string.Format("# {0}", Model.TicketNumber)
                //     : string.Format(Resources.TicketNumberAndLocation_f, Model.TicketNumber, Model.LocationName);
                //else if (string.IsNullOrEmpty(Model.LocationName)) selectedTicketTitle = string.IsNullOrEmpty(Model.AccountName)
                //     ? string.Format("# {0}", Model.TicketNumber)
                //     : string.Format(Resources.TicketNumberAndAccount_f, Model.TicketNumber, Model.AccountName);
                //else selectedTicketTitle = string.Format(Resources.AccountNameAndLocationName_f, Model.TicketNumber, Model.AccountName, Model.LocationName);)

                return selectedTicketTitle;
            }
        }

        public void ResetCache()
        {
            _payments = null;
            _preServices = null;
            _postServices = null;
            _model = Ticket.Empty;
        }
    }
}
