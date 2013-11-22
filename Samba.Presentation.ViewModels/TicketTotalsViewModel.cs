using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Presentation.ViewModels
{
    [Export]
    public class TicketTotalsViewModel : ObservableObject
    {
        private readonly ICacheService _cacheService;
        private readonly AccountBalances _accountBalances;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public TicketTotalsViewModel(ICacheService cacheService, AccountBalances accountBalances,
            IApplicationState applicationState)
        {
            _cacheService = cacheService;
            _accountBalances = accountBalances;
            _applicationState = applicationState;
            ResetCache();
            _model = Ticket.Empty;
        }

        private Ticket _model;
        public Ticket Model
        {
            get { return _model; }
            set
            {
                if (_model != value)
                {
                    ResetCache();
                    _model = value;
                }
            }
        }

        private List<PaymentViewModel> _payments;
        public List<PaymentViewModel> Payments
        {
            get
            {
                return _payments ?? (_payments = new List<PaymentViewModel>(
                    Model.Payments.Select(x => new PaymentViewModel(x))));
            }
        }

        private List<ChangePaymentViewModel> _changePayments;
        public List<ChangePaymentViewModel> ChangePayments
        {
            get
            {
                return _changePayments ?? (_changePayments = new List<ChangePaymentViewModel>(
                    Model.ChangePayments.Select(x => new ChangePaymentViewModel(x))));
            }
        }

        private List<ServiceViewModel> _preServices;
        public List<ServiceViewModel> PreServices
        {
            get { return _preServices ?? (_preServices = new List<ServiceViewModel>(Model.Calculations.Where(x => !x.IncludeTax).Select(x => new ServiceViewModel(x))).ToList()); }
        }

        private List<ServiceViewModel> _postServices;
        public List<ServiceViewModel> PostServices
        {
            get { return _postServices ?? (_postServices = new List<ServiceViewModel>(Model.Calculations.Where(x => x.IncludeTax).Select(x => new ServiceViewModel(x))).ToList()); }
        }

        public IEnumerable<ServiceViewModel> PreServicesList { get { return PreServices.Where(x => x.CalculationAmount != 0); } }
        public IEnumerable<ServiceViewModel> PostServicesList { get { return PostServices.Where(x => x.CalculationAmount != 0); } }

        public decimal TicketTotalValue { get { return Model.GetSum(); } }
        public decimal TicketTaxValue { get { return Model.CalculateTax(Model.GetPlainSum(), Model.GetPreTaxServicesTotal()); } }
        public decimal TicketSubTotalValue { get { return Model.GetPlainSum() + Model.GetPreTaxServicesTotal(); } }
        public decimal TicketPaymentValue { get { return Model.GetPaymentAmount(); } }
        public decimal TicketChangePaymentValue { get { return Model.GetChangeAmount(); } }
        public decimal TicketRemainingValue { get { return Model.GetRemainingAmount(); } }
        public decimal TicketPlainTotalValue { get { return Model.GetPlainSum(); } }

        public bool IsTicketTotalVisible { get { return TicketPaymentValue > 0 && TicketTotalValue > 0; } }
        public bool IsTicketPaymentVisible { get { return TicketPaymentValue > 0; } }
        public bool IsTicketChangePaymentVisible { get { return TicketChangePaymentValue > 0; } }
        public bool IsTicketRemainingVisible { get { return TicketRemainingValue > 0; } }
        public bool IsTicketTaxTotalVisible { get { return TicketTaxValue > 0; } }
        public bool IsPlainTotalVisible { get { return PostServicesList.Any() || PreServicesList.Any() || IsTicketTaxTotalVisible; } }
        public bool IsTicketSubTotalVisible { get { return PostServicesList.Any() && PreServicesList.Any(); } }

        public string TicketPlainTotalLabel { get { return TicketPlainTotalValue.ToString(LocalSettings.ReportCurrencyFormat); } }
        public string TicketTotalLabel { get { return TicketTotalValue.ToString(LocalSettings.ReportCurrencyFormat); } }
        public string TicketTaxLabel { get { return TicketTaxValue.ToString(LocalSettings.ReportCurrencyFormat); } }
        public string TicketSubTotalLabel { get { return TicketSubTotalValue.ToString(LocalSettings.ReportCurrencyFormat); } }
        public string TicketPaymentLabel { get { return TicketPaymentValue.ToString(LocalSettings.ReportCurrencyFormat); } }
        public string TicketChangePaymentLabel { get { return TicketChangePaymentValue.ToString(LocalSettings.ReportCurrencyFormat); } }
        public string TicketRemainingLabel { get { return TicketRemainingValue.ToString(LocalSettings.ReportCurrencyFormat); } }

        public string Title
        {
            get
            {
                var sb = new StringBuilder();
                if (Model == null) return "";
                if (Model.Id > 0) sb.AppendFormat("# {0} ", Model.TicketNumber);
                foreach (var ticketEntity in Model.TicketEntities)
                {
                    var entityType = _cacheService.GetEntityTypeById(ticketEntity.EntityTypeId);
                    var entityName = entityType.FormatEntityName(ticketEntity.EntityName);
                    sb.AppendLine(string.Format("{0}: {1}", entityType.EntityName, entityName));
                }
                var selectedTicketTitle = sb.ToString().Trim(new[] { '\r', '\n' });

                if (string.IsNullOrEmpty(selectedTicketTitle)) selectedTicketTitle = string.Format(Resources.New_f, Resources.Ticket);
                var state = Model.GetStateData(x => _applicationState.CurrentLoggedInUser.UserRole.IsAdmin || _cacheService.CanShowStateOnTicket(x.StateName, x.State));
                if (!string.IsNullOrEmpty(state)) selectedTicketTitle += Environment.NewLine + state;
                return selectedTicketTitle;
            }
        }

        public string TitleWithAccountBalancesAndState
        {
            get
            {
                var result = TitleWithAccountBalances;
                var state = Model.GetStateData(x => _applicationState.CurrentLoggedInUser.UserRole.IsAdmin || _cacheService.CanShowStateOnTicket(x.StateName, x.State));
                if (!string.IsNullOrEmpty(state)) result += Environment.NewLine + state;
                return result;
            }
        }

        public string TitleWithAccountBalances
        {
            get
            {
                var sb = new StringBuilder();
                if (Model == null) return "";
                if (Model.Id > 0) sb.AppendFormat("# {0} ", Model.TicketNumber);
                foreach (var ticketEntity in Model.TicketEntities)
                {
                    var entityType = _cacheService.GetEntityTypeById(ticketEntity.EntityTypeId);
                    var entityName = entityType.GetFormattedDisplayName(ticketEntity.EntityName, ticketEntity);
                    if (ticketEntity.AccountId > 0)
                    {
                        var balance = _accountBalances.GetAccountBalance(ticketEntity.AccountId);
                        if (balance != 0 || !string.IsNullOrEmpty(entityType.AccountBalanceDisplayFormat))
                        {
                            var format = !string.IsNullOrEmpty(entityType.AccountBalanceDisplayFormat)
                                             ? entityType.AccountBalanceDisplayFormat
                                             : LocalSettings.ReportCurrencyFormat;
                            entityName = string.Format("{0} {1}", entityName, balance.ToString(format));
                        }
                    }
                    sb.AppendLine(string.Format("{0}: {1}", entityType.EntityName, entityName));
                }
                var selectedTicketTitle = sb.ToString().Trim(new[] { '\r', '\n' });

                if (string.IsNullOrEmpty(selectedTicketTitle)) selectedTicketTitle = string.Format(Resources.New_f, Resources.Ticket);

                return selectedTicketTitle;
            }
        }

        public void ResetCache()
        {
            _changePayments = null;
            _payments = null;
            _preServices = null;
            _postServices = null;
        }

        public void Refresh()
        {
            RefreshAll("");
            //ThreadPool.QueueUserWorkItem(RefreshAll, string.Empty);
        }

        private void RefreshAll(object state)
        {
            RaisePropertyChanged(() => Title);
            RaisePropertyChanged(() => TitleWithAccountBalances);
            RaisePropertyChanged(() => TicketRemainingLabel);
            RaisePropertyChanged(() => TicketPaymentLabel);
            RaisePropertyChanged(() => TicketChangePaymentLabel);
            RaisePropertyChanged(() => TicketSubTotalLabel);
            RaisePropertyChanged(() => TicketTaxLabel);
            RaisePropertyChanged(() => TicketTotalLabel);
            RaisePropertyChanged(() => TicketPlainTotalLabel);

            RaisePropertyChanged(() => IsTicketSubTotalVisible);
            RaisePropertyChanged(() => IsPlainTotalVisible);
            RaisePropertyChanged(() => IsTicketTaxTotalVisible);
            RaisePropertyChanged(() => IsTicketRemainingVisible);
            RaisePropertyChanged(() => IsTicketPaymentVisible);
            RaisePropertyChanged(() => IsTicketChangePaymentVisible);
            RaisePropertyChanged(() => IsTicketTotalVisible);

            RaisePropertyChanged(() => TicketPlainTotalValue);
            RaisePropertyChanged(() => TicketRemainingValue);
            RaisePropertyChanged(() => TicketPaymentValue);
            RaisePropertyChanged(() => TicketChangePaymentValue);
            RaisePropertyChanged(() => TicketSubTotalValue);
            RaisePropertyChanged(() => TicketTaxValue);
            RaisePropertyChanged(() => TicketTotalValue);

            RaisePropertyChanged(() => PostServices);
            RaisePropertyChanged(() => PreServices);
            RaisePropertyChanged(() => PreServicesList);
            RaisePropertyChanged(() => PostServicesList);
            RaisePropertyChanged(() => Payments);
            RaisePropertyChanged(() => ChangePayments);

            PostServices.ForEach(x => x.Refresh());
            PreServices.ForEach(x => x.Refresh());

        }
    }
}
