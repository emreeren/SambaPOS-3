using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;

namespace Samba.Presentation.ViewModels
{
    public class TicketItemPropertyViewModel : ObservableObject
    {
        public TicketItemProperty Model { get; set; }

        public TicketItemPropertyViewModel(TicketItemProperty model)
        {
            Model = model;
        }

        public string DisplayString
        {
            get
            {
                if (Model.Quantity > 1)
                    return Model.Name + " x " + Model.Quantity.ToString("#");
                return Model.Name;
            }
        }

        public string PriceString
        {
            get
            {
                return Model.PropertyPrice.Amount != 0 && !Model.CalculateWithParentPrice ? ((Model.PropertyPrice.Amount + Model.TaxAmount) * Model.Quantity).ToString("#,#0.00") : "";
            }
        }
    }
}
