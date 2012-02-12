using System;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;

namespace Samba.Presentation.ViewModels
{
    public class TicketTotalsViewModel : ObservableObject
    {
        public Ticket Model { get; set; }
        public TicketTotalsViewModel(Ticket model)
        {
            Model = model;
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

        public string Title
        {
            get
            {
                if (Model == null) return "";

                string selectedTicketTitle;

                if (!string.IsNullOrEmpty(Model.LocationName) && Model.Id == 0)
                    selectedTicketTitle = string.Format(Resources.Location_f, Model.LocationName);
                else if (!string.IsNullOrEmpty(Model.AccountName) && Model.Id == 0)
                    selectedTicketTitle = string.Format(Resources.Account_f, Model.AccountName);
                else if (string.IsNullOrEmpty(Model.AccountName)) selectedTicketTitle = string.IsNullOrEmpty(Model.LocationName)
                     ? string.Format("# {0}", Model.TicketNumber)
                     : string.Format(Resources.TicketNumberAndLocation_f, Model.TicketNumber, Model.LocationName);
                else if (string.IsNullOrEmpty(Model.LocationName)) selectedTicketTitle = string.IsNullOrEmpty(Model.AccountName)
                     ? string.Format("# {0}", Model.TicketNumber)
                     : string.Format(Resources.TicketNumberAndAccount_f, Model.TicketNumber, Model.AccountName);
                else selectedTicketTitle = string.Format(Resources.AccountNameAndLocationName_f, Model.TicketNumber, Model.AccountName, Model.LocationName);

                return selectedTicketTitle;
            }
        }

        public void ResetCache()
        {
            _payments = null;
            _preServices = null;
            _postServices = null;
        }
    }
}
