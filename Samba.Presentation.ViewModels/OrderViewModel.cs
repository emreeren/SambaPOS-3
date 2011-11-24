using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Presentation.ViewModels
{
    public class OrderViewModel : ObservableObject
    {
        public bool IsSelectedQuantityModified { get; set; }

        public OrderViewModel(Order model)
        {
            _model = model;
            ResetSelectedQuantity();
            ItemSelectedCommand = new DelegateCommand<OrderViewModel>(OnItemSelected);
            UpdateItemColor();
        }

        public DelegateCommand<OrderViewModel> ItemSelectedCommand { get; set; }

        public string Description
        {
            get
            {
                var desc = _model.MenuItemName + _model.GetPortionDesc();
                if (IsSelectedQuantityModified)
                    desc = string.Format("({0:#.##}) {1}", Model.SelectedQuantity, desc);
                return desc;
            }
        }

        private readonly Order _model;
        public Order Model { get { return _model; } }

        public decimal Quantity
        {
            get { return _model.Quantity; }
            set
            {
                _model.Quantity = value;
                RaisePropertyChanged(() => Quantity);
                RaisePropertyChanged(() => TotalPrice);
                ResetSelectedQuantity();
            }
        }

        public decimal SelectedQuantity { get { return Model.SelectedQuantity; } }

        public void IncSelectedQuantity()
        {
            Model.IncSelectedQuantity();
            IsSelectedQuantityModified = true;
            RefreshSelectedItem();
        }
        public void DecSelectedQuantity()
        {
            Model.DecSelectedQuantity();
            IsSelectedQuantityModified = true;
            RefreshSelectedItem();
        }

        public void ResetSelectedQuantity()
        {
            Model.ResetSelectedQuantity();
            IsLastSelected = false;
            IsSelectedQuantityModified = false;
            RefreshSelectedItem();
        }

        private void RefreshSelectedItem()
        {
            RaisePropertyChanged(() => SelectedQuantity);
            RaisePropertyChanged(() => Description);
            RaisePropertyChanged(() => Background);
            RaisePropertyChanged(() => Foreground);
            RaisePropertyChanged(() => BorderThickness);
        }

        public decimal Price
        {
            get { return Model.GetPlainPrice() + Model.GetMenuItemOrderTagPrice(); }
        }

        public decimal TotalPrice
        {
            get { return Price * Quantity; }
        }

        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; UpdateItemColor(); RaisePropertyChanged(() => Selected); }
        }

        private Brush _background;
        public Brush Background
        {
            get { return _background; }
            set { _background = value; RaisePropertyChanged(() => Background); }
        }

        private Brush _foreground;
        public Brush Foreground
        {
            get { return _foreground; }
            set { _foreground = value; RaisePropertyChanged(() => Foreground); }
        }

        public int BorderThickness { get { return IsLastSelected ? 1 : 0; } }

        public string OrderNumber
        {
            get
            {
                return Model.OrderNumber > 0 ? string.Format(Resources.OrderNumber_f,
                    Model.OrderNumber, CreatingUserName) : Resources.NewOrder;
            }
        }

        public object GroupObject { get { return new { OrderNumber, Time = Model.Id > 0 ? Model.CreatedDateTime.ToShortTimeString() : "" }; } }

        public string CreatingUserName { get { return AppServices.MainDataContext.GetUserName(Model.CreatingUserId); } }

        public string CustomPropertyName
        {
            get { return Model.GetCustomOrderTag() != null ? Model.GetCustomOrderTag().Name : ""; }
            set
            {
                Model.UpdateCustomOrderTag(value, CustomPropertyPrice, CustomPropertyQuantity);
                RefreshProperties();
            }
        }

        public decimal CustomPropertyPrice
        {
            get
            {
                var prop = Model.GetCustomOrderTag();
                if (prop != null)
                {
                    return Model.TaxIncluded ? prop.Price + prop.TaxAmount : prop.Price;
                }
                return 0;
            }
            set
            {
                Model.UpdateCustomOrderTag(CustomPropertyName, value, CustomPropertyQuantity);
                RefreshProperties();
            }
        }

        public decimal CustomPropertyQuantity
        {
            get { return Model.GetCustomOrderTag() != null ? Model.GetCustomOrderTag().Quantity : 1; }
            set
            {
                Model.UpdateCustomOrderTag(CustomPropertyName, CustomPropertyPrice, value);
                RefreshProperties();
            }
        }

        public TextDecorationCollection TextDecoration
        {
            get
            {
                return !Model.DecreaseInventory ? TextDecorations.Strikethrough : null;
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                return _model.Locked ? FontWeights.Bold : FontWeights.Normal;
            }
        }

        public string PriceTag { get { return Model.PriceTag; } }

        private ObservableCollection<OrderTagValueViewModel> _orderTagValues;
        public ObservableCollection<OrderTagValueViewModel> OrderTagValues
        {
            get { return _orderTagValues ?? (_orderTagValues = new ObservableCollection<OrderTagValueViewModel>(Model.OrderTagValues.Select(x => new OrderTagValueViewModel(x)))); }
        }

        public bool IsLocked { get { return Model.Locked; } }
        private bool _isLastSelected;
        public bool IsLastSelected
        {
            get { return _isLastSelected; }
            set
            {
                _isLastSelected = value;
                RaisePropertyChanged(() => BorderThickness);
            }
        }

        private MenuItem _menuItem;
        public MenuItem MenuItem
        {
            get { return _menuItem ?? (_menuItem = AppServices.DataAccessService.GetMenuItem(Model.MenuItemId)); }
        }

        private void OnItemSelected(OrderViewModel obj)
        {
            ToggleSelection();
        }

        public void ToggleSelection()
        {
            Selected = !Selected;
            if (!Selected) ResetSelectedQuantity();
            this.PublishEvent(EventTopicNames.SelectedOrdersChanged);
        }

        private void UpdateItemColor()
        {
            if (Selected)
            {
                Background = SystemColors.HighlightBrush;
                Foreground = SystemColors.HighlightTextBrush;
            }
            else
            {
                Background = null;
                Foreground = SystemColors.WindowTextBrush;

                if (IsLocked)
                    Foreground = Brushes.DarkRed;
                if (!Model.DecreaseInventory)
                    Foreground = Brushes.Gray;
                if (!Model.CalculatePrice && Model.DecreaseInventory)
                    Foreground = Brushes.DarkBlue;
            }
        }

        public void NotSelected()
        {
            if (_selected)
            {
                _selected = false;
                ResetSelectedQuantity();
                UpdateItemColor();
                RaisePropertyChanged(() => Quantity);
            }
        }

        public void UpdatePortion(MenuItemPortion portion, string priceTag)
        {
            _model.UpdatePortion(portion, priceTag, AppServices.MainDataContext.GetTaxTemplate(portion.MenuItemId));
            RaisePropertyChanged(() => Description);
            RaisePropertyChanged(() => TotalPrice);
        }

        public void ToggleOrderTag(OrderTagGroup orderTagGroup, OrderTag orderTag, int userId)
        {
            var result = _model.ToggleOrderTag(orderTagGroup, orderTag, userId);

            RuleExecutor.NotifyEvent(result ? RuleEventNames.OrderTagged : RuleEventNames.OrderUntagged,
            new
            {
                Order = Model,
                OrderTagName = orderTagGroup.Name,
                OrderTagValue = orderTag.Name
            });

            RefreshProperties();
            RaisePropertyChanged(() => TotalPrice);
            RaisePropertyChanged(() => Quantity);
            RaisePropertyChanged(() => Description);
            RaisePropertyChanged(() => FontWeight);
            RaisePropertyChanged(() => IsLocked);

        }

        private void RefreshProperties()
        {
            OrderTagValues.Clear();
            OrderTagValues.AddRange(Model.OrderTagValues.Select(x => new OrderTagValueViewModel(x)));
        }

        public void UpdatePrice(decimal value)
        {
            Model.UpdatePrice(value, AppServices.MainDataContext.SelectedDepartment.PriceTag);
            RaisePropertyChanged(() => Price);
            RaisePropertyChanged(() => TotalPrice);
        }

        public bool IsTaggedWith(OrderTagGroup orderTagGroup)
        {
            return OrderTagValues.FirstOrDefault(x => x.Model.OrderTagGroupId == orderTagGroup.Id) != null;
        }
    }
}
