using System.ComponentModel;
using System.Windows.Media;
using PropertyTools.DataAnnotations;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;
using Samba.Localization;
using Samba.Presentation.Common;

namespace Samba.Modules.MenuModule
{
    public class ScreenMenuItemViewModel : ObservableObject
    {
        public ScreenMenuItemViewModel(IWorkspace workspace, ScreenMenuItem model)
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
            get { return Model.SortOrder; }
            set
            {
                Model.SortOrder = value;
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
        public Color ButtonColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Model.ButtonColor))
                    return (Color)ColorConverter.ConvertFromString(Model.ButtonColor);
                return Colors.Transparent;
            }
            set
            {
                Model.ButtonColor = value != Colors.Transparent ? value.ToString() : string.Empty;
                RaisePropertyChanged(() => ButtonColor);
            }
        }

        [FilePath(".jpg|.png")]
        [FilterProperty("Filter")]
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

        [LocalizedDisplayName(ResourceStrings.FontSize)]
        public double FontSize
        {
            get { return Model.FontSize; }
            set { Model.FontSize = value; RaisePropertyChanged(() => FontSize); }
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

        [LocalizedDisplayName(ResourceStrings.SubMenuTags)]
        public string Tag
        {
            get { return Model.SubMenuTag; }
            set
            {
                Model.SubMenuTag = value.Trim(new[] { ' ', '\b' }); RaisePropertyChanged(() => Tag);
            }
        }

        [LocalizedDisplayName(ResourceStrings.Portion)]
        public string Portion
        {
            get { return Model.ItemPortion; }
            set { Model.ItemPortion = value; RaisePropertyChanged(() => Portion); }
        }
    }
}
