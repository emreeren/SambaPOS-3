using System;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.PosModule
{
    public class OpenTicketButtonViewModel : ObservableObject
    {
        public OpenTicketButtonViewModel(OpenTicketData openTicketData, bool shouldWrap)
        {
            Id = openTicketData.Id;
            LastOrderDate = openTicketData.LastOrderDate;
            TicketNumber = openTicketData.TicketNumber;
            TargetAccountName = openTicketData.TargetAccountName;
            AccountName = openTicketData.AccountName;
            RemainingAmount = openTicketData.RemainingAmount;
            Date = openTicketData.Date;
            WrapText = shouldWrap;
        }

        public int Id { get; set; }
        public string TargetAccountName { get; set; }
        public string AccountName { get; set; }
        public decimal RemainingAmount { get; set; }
        public string TicketNumber { get; set; }
        public DateTime Date { get; set; }
        public DateTime LastOrderDate { get; set; }
        public bool WrapText { get; set; }

        public bool IsLocked { get; set; }
        public string TicketTime
        {
            get
            {
                var difference = Convert.ToInt32(new TimeSpan(DateTime.Now.Ticks - LastOrderDate.Ticks).TotalMinutes);
                return difference == 0 ? "-" : string.Format(Resources.OpenTicketButtonDuration, difference.ToString("#"));
            }
        }

        public string Title
        {
            get
            {
                var result = !string.IsNullOrEmpty(TargetAccountName) ? TargetAccountName : TicketNumber;
                result = result + " ";
                result = WrapText ? result.Replace(" ", "\r") : result;
                if (!string.IsNullOrEmpty(AccountName)) result += AccountName;
                return result.TrimEnd('\r');
            }
        }

        public string TitleTextColor { get { return !string.IsNullOrEmpty(TargetAccountName) || !string.IsNullOrEmpty(AccountName) ? "DarkBlue" : "Maroon"; } }

        public string Total
        {
            get
            {
                return RemainingAmount > 0 ? RemainingAmount.ToString(LocalSettings.DefaultCurrencyFormat) : "";
            }
        }

        public void Refresh()
        {
            RaisePropertyChanged(() => TicketTime);
            RaisePropertyChanged(() => Title);
            RaisePropertyChanged(() => Total);
        }
    }
}