using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
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

        [ImportingConstructor]
        public TicketTotalsViewModel(ICacheService cacheService)
        {
            _cacheService = cacheService;
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

        private List<ServiceViewModel> _preServices;
        public List<ServiceViewModel> PreServices
        {
            get { return _preServices ?? (_preServices = new List<ServiceViewModel>(Model.Calculations.Where(x => !x.IncludeTax).Select(x => new ServiceViewModel(x)))); }
        }

        private List<ServiceViewModel> _postServices;
        public List<ServiceViewModel> PostServices
        {
            get { return _postServices ?? (_postServices = new List<ServiceViewModel>(Model.Calculations.Where(x => x.IncludeTax).Select(x => new ServiceViewModel(x)))); }
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

        public string TicketPlainTotalLabel { get { return TicketPlainTotalValue.ToString(LocalSettings.DefaultCurrencyFormat); } }
        public string TicketTotalLabel { get { return TicketTotalValue.ToString(LocalSettings.DefaultCurrencyFormat); } }
        public string TicketTaxLabel { get { return TicketTaxValue.ToString(LocalSettings.DefaultCurrencyFormat); } }
        public string TicketSubTotalLabel { get { return TicketSubTotalValue.ToString(LocalSettings.DefaultCurrencyFormat); } }
        public string TicketPaymentLabel { get { return TicketPaymentValue.ToString(LocalSettings.DefaultCurrencyFormat); } }
        public string TicketRemainingLabel { get { return TicketRemainingValue.ToString(LocalSettings.DefaultCurrencyFormat); } }

        public string Title
        {
            get
            {
                var sb = new StringBuilder();
                if (Model == null) return "";
                if (Model.Id > 0) sb.AppendFormat("# {0} ", Model.TicketNumber);
                foreach (var ticketResource in Model.TicketResources)
                {
                    var rs = _cacheService.GetResourceTemplateById(ticketResource.ResourceTemplateId);
                    sb.AppendLine(string.Format("{0}: {1}", rs.EntityName, ticketResource.ResourceName));
                }
                var selectedTicketTitle = sb.ToString().Trim(new[] { '\r', '\n' });

                if (string.IsNullOrEmpty(selectedTicketTitle)) selectedTicketTitle = string.Format(Resources.New_f, Resources.Ticket);

                return selectedTicketTitle;
            }
        }

        public void ResetCache()
        {
            _payments = null;
            _preServices = null;
            _postServices = null;
        }

        public void Refresh()
        {
            ThreadPool.QueueUserWorkItem(RefreshAll, string.Empty);
        }

        private void RefreshAll(object state)
        {
            RaisePropertyChanged(() => Title);
            RaisePropertyChanged(() => TicketRemainingLabel);
            RaisePropertyChanged(() => TicketPaymentLabel);
            RaisePropertyChanged(() => TicketSubTotalLabel);
            RaisePropertyChanged(() => TicketTaxLabel);
            RaisePropertyChanged(() => TicketTotalLabel);
            RaisePropertyChanged(() => TicketPlainTotalLabel);

            RaisePropertyChanged(() => IsTicketSubTotalVisible);
            RaisePropertyChanged(() => IsPlainTotalVisible);
            RaisePropertyChanged(() => IsTicketTaxTotalVisible);
            RaisePropertyChanged(() => IsTicketRemainingVisible);
            RaisePropertyChanged(() => IsTicketPaymentVisible);
            RaisePropertyChanged(() => IsTicketTotalVisible);

            RaisePropertyChanged(() => TicketPlainTotalValue);
            RaisePropertyChanged(() => TicketRemainingValue);
            RaisePropertyChanged(() => TicketPaymentValue);
            RaisePropertyChanged(() => TicketSubTotalValue);
            RaisePropertyChanged(() => TicketTaxValue);
            RaisePropertyChanged(() => TicketTotalValue);

            RaisePropertyChanged(() => PostServices);
            RaisePropertyChanged(() => PreServices);
            RaisePropertyChanged(() => Payments);

            PostServices.ForEach(x => x.Refresh());
            PreServices.ForEach(x => x.Refresh());

        }
    }
}
