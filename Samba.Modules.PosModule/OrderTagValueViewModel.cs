using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;

namespace Samba.Modules.PosModule
{
    public class OrderTagValueViewModel : ObservableObject
    {
        public OrderTagValue Model { get; set; }

        public OrderTagValueViewModel(OrderTagValue model)
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
                return Model.Price != 0 && !Model.AddTagPriceToOrderPrice ? ((Model.Price + Model.TaxAmount) * Model.Quantity).ToString("#,#0.00") : "";
            }
        }

        public string ShortName { get { return Model.ShortName; } }
    }
}
