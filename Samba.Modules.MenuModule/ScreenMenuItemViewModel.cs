using System.ComponentModel;
using System.Windows.Media;
using Samba.Domain.Models.Menus;
using Samba.Localization;
using Samba.Presentation.Common;

namespace Samba.Modules.MenuModule
{
    public class ScreenMenuItemViewModel : ObservableObject
    {
        public ScreenMenuItemViewModel(ScreenMenuItem model)
        {
            Model = model;
        }

        [Browsable(false)]
        public ScreenMenuItem Model { get; private set; }

        [LocalizedDisplayName(ResourceStrings.Product)]
        public string MenuItemDisplayString
        {
            get { return Name; }
        }

        [LocalizedDisplayName(ResourceStrings.SortOrder)]
        public int Order
        {
            get { return Model.Order; }
            set
            {
                Model.Order = value;
                RaisePropertyChanged(() => Order);
            }
        }

        [LocalizedDisplayName(ResourceStrings.AutoSelect)]
        public bool AutoSelect
        {
            get { return Model.AutoSelect; }
            set
            {
                Model.AutoSelect = value;
                RaisePropertyChanged(() => AutoSelect);
            }
        }

        [LocalizedDisplayName(ResourceStrings.Color)]
        public SolidColorBrush ButtonColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Model.ButtonColor))
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(Model.ButtonColor));
                return Brushes.Transparent;
            }
            set
            {
                Model.ButtonColor = value != Brushes.Transparent ? value.ToString() : string.Empty;
                RaisePropertyChanged(() => ButtonColor);
            }
        }

        [LocalizedDisplayName(ResourceStrings.ImagePath)]
        public string ImagePath
        {
            get { return Model.ImagePath ?? ""; }
            set
            {
                Model.ImagePath = value != null ? value.Trim('\b') : null;
                RaisePropertyChanged(() => ImagePath);
            }
        }

        [LocalizedDisplayName(ResourceStrings.Header)]
        public string Name
        {
            get { return Model.Name; }
            set { Model.Name = value; RaisePropertyChanged(() => Name); }
        }

        [LocalizedDisplayName(ResourceStrings.Quantity)]
        public int Quantity
        {
            get { return Model.Quantity; }
            set { Model.Quantity = value; RaisePropertyChanged(() => Quantity); }
        }

        [LocalizedDisplayName(ResourceStrings.Tag)]
        public string Tag
        {
            get { return Model.Tag; }
            set { Model.Tag = value; RaisePropertyChanged(() => Tag); }
        }

        [LocalizedDisplayName(ResourceStrings.Portion)]
        public string Portion
        {
            get { return Model.ItemPortion; }
            set { Model.ItemPortion = value; RaisePropertyChanged(() => Portion); }
        }
    }
}
