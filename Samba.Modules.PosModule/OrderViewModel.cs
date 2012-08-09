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
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    public class OrderViewModel : ObservableObject
    {
        private readonly IAutomationService _automationService;

        public OrderViewModel(Order model, IAutomationService automationService)
        {
            _model = model;
            _automationService = automationService;
            ResetSelectedQuantity();
            ItemSelectedCommand = new DelegateCommand<OrderViewModel>(OnItemSelected);
            UpdateItemColor();
        }

        public DelegateCommand<OrderViewModel> ItemSelectedCommand { get; set; }

        public string Description
        {
            get { return _model.Description; }
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

        public bool IsStateVisible { get { return !string.IsNullOrEmpty(State); } }
        public string State
        {
            get
            {
                return !string.IsNullOrEmpty(Model.OrderStateGroupName) ? string.Format("[{0}]", Model.OrderStateGroupName + (!string.IsNullOrEmpty(Model.OrderState) && Model.OrderState != Model.OrderStateGroupName ? ": " + Model.OrderState : "")) : "";
            }
        }

        public decimal SelectedQuantity { get { return Model.SelectedQuantity; } }

        public void IncSelectedQuantity()
        {
            Model.IncSelectedQuantity();
            RefreshSelectedItem();
        }
        public void DecSelectedQuantity()
        {
            Model.DecSelectedQuantity();
            RefreshSelectedItem();
        }

        public void ResetSelectedQuantity()
        {
            Model.ResetSelectedQuantity();
            IsLastSelected = false;
            RefreshSelectedItem();
        }

        private void RefreshSelectedItem()
        {
            RaisePropertyChanged(() => SelectedQuantity);
            RaisePropertyChanged(() => Description);
            RaisePropertyChanged(() => Background);
            RaisePropertyChanged(() => Foreground);
            RaisePropertyChanged(() => BorderThickness);
            RaisePropertyChanged(() => State);
            RaisePropertyChanged(() => IsStateVisible);
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

        public string CreatingUserName { get { return Model.CreatingUserName; } }

        public TextDecorationCollection TextDecoration
        {
            get
            {
                return !Model.DecreaseInventory ? TextDecorations.Strikethrough : null;
            }
        }

        public FontWeight FontWeight { get { return _model.Locked ? FontWeights.Bold : FontWeights.Normal; } }

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

        public int MenuItemId
        {
            get { return Model.MenuItemId; }
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
                Background = Brushes.Transparent;
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
                RaisePropertyChanged(() => TotalPrice);
            }
        }

        public void UpdatePortion(MenuItemPortion portion, string priceTag, TaxTemplate taxTemplate)
        {
            _model.UpdatePortion(portion, priceTag, taxTemplate);
            RaisePropertyChanged(() => Description);
            RaisePropertyChanged(() => TotalPrice);
        }

        public void ToggleOrderTag(OrderTagGroup orderTagGroup, OrderTag orderTag, int userId)
        {
            var result = _model.ToggleOrderTag(orderTagGroup, orderTag, userId);
            _automationService.NotifyEvent(result ? RuleEventNames.OrderTagged : RuleEventNames.OrderUntagged,
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

        public void UpdateOrderState(OrderStateGroup orderStateGroup, OrderState selectedOrderState, int userId)
        {
            Model.UpdateOrderState(orderStateGroup, selectedOrderState, userId);
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
            RaisePropertyChanged(() => OrderTagValues);
        }

        public void UpdatePrice(decimal value, string priceTag)
        {
            Model.UpdatePrice(value, priceTag);
            RaisePropertyChanged(() => Price);
            RaisePropertyChanged(() => TotalPrice);
        }

        public bool IsTaggedWith(OrderTagGroup orderTagGroup)
        {
            return Model.IsTaggedWith(orderTagGroup);
        }


    }
}
