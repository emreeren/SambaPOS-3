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
                return _payments ?? (_payments = new ObservableCollection<PaymentViewModel>(Model.AccountTransactions.AccountTransactions
                    .Where(x => x.AccountTransactionTemplateId == Model.PaymentTransactionTemplateId)
                    .Select(x => new PaymentViewModel(x.TargetTransactionValue))));
            }
        }

        private ObservableCollection<DiscountViewModel> _discounts;
        public ObservableCollection<DiscountViewModel> Discounts
        {
            get
            {
                return _discounts ??
                       (_discounts =
                        new ObservableCollection<DiscountViewModel>(Model.AccountTransactions.AccountTransactions
                                                                        .Where(
                                                                            x =>
                                                                            x.AccountTransactionTemplateId ==
                                                                            Model.DiscountTransactionTemplateId ||
                                                                            x.AccountTransactionTemplateId ==
                                                                            Model.RoundingTransactionTemplateId)
                .Select(x => new DiscountViewModel(x.SourceTransactionValue))));
            }
        }

        public decimal TicketTotalValue { get { return Model.GetSum(); } }
        public decimal TicketTaxValue { get { return Model.CalculateTax(); } }
        public decimal TicketServiceValue { get { return Model.GetServicesTotal(); } }
        public decimal TicketPaymentValue { get { return Model.GetPaymentAmount(); } }
        public decimal TicketRemainingValue { get { return Model.GetRemainingAmount(); } }
        public decimal TicketPlainTotalValue { get { return Model.GetPlainSum(); } }
        public decimal TicketDiscountAmount { get { return Model.GetDiscountTotal(); } }
        public decimal TicketRoundingAmount { get { return Model.GetRoundingTotal(); } }

        public bool IsTicketTotalVisible { get { return TicketPaymentValue > 0 && TicketTotalValue > 0; } }
        public bool IsTicketPaymentVisible { get { return TicketPaymentValue > 0; } }
        public bool IsTicketRemainingVisible { get { return TicketRemainingValue > 0; } }
        public bool IsTicketTaxTotalVisible { get { return TicketTaxValue > 0; } }
        public bool IsPlainTotalVisible { get { return IsTicketDiscountVisible || IsTicketTaxTotalVisible || IsTicketRoundingVisible || IsTicketServiceVisible; } }
        public bool IsTicketDiscountVisible { get { return TicketDiscountAmount != 0; } }
        public bool IsTicketRoundingVisible { get { return TicketRoundingAmount != 0; } }
        public bool IsTicketServiceVisible { get { return TicketServiceValue > 0; } }

        public string TicketPlainTotalLabel
        {
            get { return TicketPlainTotalValue.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketTotalLabel
        {
            get { return TicketTotalValue.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketDiscountLabel
        {
            get { return TicketDiscountAmount.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketRoundingLabel
        {
            get { return TicketRoundingAmount.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketTaxLabel
        {
            get { return TicketTaxValue.ToString(LocalSettings.DefaultCurrencyFormat); }
        }

        public string TicketServiceLabel
        {
            get { return TicketServiceValue.ToString(LocalSettings.DefaultCurrencyFormat); }
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
                else if (!string.IsNullOrEmpty(Model.SaleTransaction.TargetTransactionValue.AccountName) && Model.Id == 0)
                    selectedTicketTitle = string.Format(Resources.Account_f, Model.SaleTransaction.TargetTransactionValue.AccountName);
                else if (string.IsNullOrEmpty(Model.SaleTransaction.TargetTransactionValue.AccountName)) selectedTicketTitle = string.IsNullOrEmpty(Model.LocationName)
                     ? string.Format("# {0}", Model.TicketNumber)
                     : string.Format(Resources.TicketNumberAndLocation_f, Model.TicketNumber, Model.LocationName);
                else if (string.IsNullOrEmpty(Model.LocationName)) selectedTicketTitle = string.IsNullOrEmpty(Model.SaleTransaction.TargetTransactionValue.AccountName)
                     ? string.Format("# {0}", Model.TicketNumber)
                     : string.Format(Resources.TicketNumberAndAccount_f, Model.TicketNumber, Model.SaleTransaction.TargetTransactionValue.AccountName);
                else selectedTicketTitle = string.Format(Resources.AccountNameAndLocationName_f, Model.TicketNumber, Model.SaleTransaction.TargetTransactionValue.AccountName, Model.LocationName);

                return selectedTicketTitle;
            }
        }

        public void ResetCache()
        {
            _payments = null;
            _discounts = null;
        }
    }
}
