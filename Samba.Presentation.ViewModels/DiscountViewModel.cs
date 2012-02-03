using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;

namespace Samba.Presentation.ViewModels
{
    public class DiscountViewModel
    {
        public AccountTransactionValue Model { get; set; }

        public DiscountViewModel(AccountTransactionValue model)
        {
            Model = model;
        }

        public string DiscountTypeDisplay
        {
            get
            {
                return Model.Name;
            }
        }

        public string AmountDisplay
        {
            get
            {
                //switch (Model.DiscountType)
                //{
                //    case (int)DiscountType.Percent:
                //        return Model.Amount.ToString();
                //    case (int)DiscountType.Auto:
                //        return Resources.AutoFlatten_ab;
                //    default:
                //        return Model.Amount > 0 ? Resources.Rounding : Resources.Flattening;
                //}
                return Model.AccountName;
            }
        }

        public string DiscountAmountDisplay
        {
            get
            {
                return (Model.Liability + Model.Receivable).ToString(LocalSettings.DefaultCurrencyFormat);
            }
        }
    }
}
