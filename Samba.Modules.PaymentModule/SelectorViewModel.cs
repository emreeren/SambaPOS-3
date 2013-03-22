using Samba.Infrastructure.Settings;
using Samba.Presentation.Common;

namespace Samba.Modules.PaymentModule
{
    public class SelectorViewModel : ObservableObject
    {
        public SelectorViewModel(Selector model)
        {
            Model = model;
        }

        protected Selector Model { get; set; }
        public string Quantity { get { return Model.RemainingQuantity.ToString(LocalSettings.ReportQuantityFormat); } }
        public string Total { get { return Model.RemainingPrice.ToString(LocalSettings.ReportCurrencyFormat); } }
        public string Description { get { return Model.IsSelected ? string.Format("{0} ({1:#.###})", Model.Description, Model.SelectedQuantity) : Model.Description; } }
        public bool IsSelected { get { return Model.IsSelected; } }

        public void Select()
        {
            Model.Select();
            Refresh();
        }

        public void Refresh()
        {
            RaisePropertyChanged(() => Quantity);
            RaisePropertyChanged(() => Description);
            RaisePropertyChanged(() => Total);
            RaisePropertyChanged(() => IsSelected);
        }
    }
}