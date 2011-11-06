using Samba.Domain.Foundation;
using Samba.Domain.Models.Menus;
using Samba.Presentation.Common;

namespace Samba.Presentation.ViewModels
{
    public class MenuItemPropertyViewModel : ObservableObject
    {
        public MenuItemProperty Model { get; set; }

        public MenuItemPropertyViewModel(MenuItemProperty model)
        {
            Model = model;
        }

        public string Name
        {
            get { return Model.Name; }
            set { Model.Name = value; }
        }

        public Price Price
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
