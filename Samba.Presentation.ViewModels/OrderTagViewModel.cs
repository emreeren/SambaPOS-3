using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;

namespace Samba.Presentation.ViewModels
{
    public class OrderTagViewModel : ObservableObject
    {
        public OrderTag Model { get; set; }

        public OrderTagViewModel(OrderTag model)
        {
            Model = model;
        }

        public string Name
        {
            get { return Model.Name; }
            set { Model.Name = value; }
        }

        public decimal Price
        {
            get { return Model.Price; }
            set { Model.Price = value; }
        }

        public void Refresh()
        {
            RaisePropertyChanged(() => Name);
        }
    }
}
