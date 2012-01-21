using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
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
            _workspace = workspace;
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
            set { Model.FontSize = value; RaisePropertyChanged(()=>FontSize);}
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
            set { Model.SubMenuTag = value; RaisePropertyChanged(() => Tag); }
        }

        [LocalizedDisplayName(ResourceStrings.Portion)]
        public string Portion
        {
            get { return Model.ItemPortion; }
            set { Model.ItemPortion = value; RaisePropertyChanged(() => Portion); }
        }

        [LocalizedDisplayName(ResourceStrings.OrderTagTemplate)]
        public string OrderTagTemplateName
        {
            get { return Model.OrderTagTemplate != null ? Model.OrderTagTemplate.Name : ""; }
            set
            {
                Model.OrderTagTemplate = OrderTagTemplates.FirstOrDefault(x => x.Name == value);
                RaisePropertyChanged(() => OrderTagTemplateName);
            }
        }

        private IEnumerable<OrderTagTemplate> _orderTagTemplates;
        private readonly IWorkspace _workspace;

        protected IEnumerable<OrderTagTemplate> OrderTagTemplates
        {
            get { return _orderTagTemplates ?? (_orderTagTemplates = _workspace.All<OrderTagTemplate>()); }
        }
    }
}
