using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;

namespace Samba.Presentation.ViewModels
{
    public class DiscountViewModel
    {
        public Discount Model { get; set; }

        public DiscountViewModel(Discount model)
        {
            Model = model;
        }

        public string DiscountTypeDisplay
        {
            get
            {
                return Model.DiscountType == (int)DiscountType.Percent ? "%" : "";
            }
        }

        public string AmountDisplay
        {
            get
            {
                switch (Model.DiscountType)
                {
                    case (int)DiscountType.Percent:
                        return Model.Amount.ToString();
                    case (int)DiscountType.Auto:
                        return Resources.AutoFlatten_ab;
                    default:
                        return Model.Amount > 0 ? Resources.Rounding : Resources.Flattening;
                }
            }
        }

        public string DiscountAmountDisplay
        {
            get
            {
                return Model.DiscountAmount.ToString(LocalSettings.DefaultCurrencyFormat);
            }
        }
    }
}
