using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Localization;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.PosModule
{
    public class OrderViewModel : ObservableObject
    {
        public OrderViewModel(Order model, ICacheService cacheService, IApplicationState applicationState)
        {
            _model = model;
            _cacheService = cacheService;
            _applicationState = applicationState;
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
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;
        public Order Model { get { return _model; } }

        public decimal Quantity
        {
            get { return _model.Quantity; }
            set
            {
                _model.Quantity = value;
                _model.ResetSelectedQuantity();
                RaisePropertyChanged(() => Quantity);
                RaisePropertyChanged(() => TotalPrice);
                ResetSelectedQuantity();
            }
        }

        public bool IsTimerVisible { get { return Model.ProductTimerValue != null; } }
        public string TimerDescription
        {
            get
            {
                if (Model.ProductTimerValue == null) return "";
                const string fmt = "{0} {1} - {2} ({3:N}) {4}";
                return String.Format(fmt,
                    Model.ProductTimerValue.Start.Date != DateTime.Now.Date ? Model.ProductTimerValue.Start.ToShortDateString() : "",
                    Model.ProductTimerValue.Start.Date == DateTime.Now.Date ? Model.ProductTimerValue.Start.ToShortTimeString() : "",
                    Model.ProductTimerValue.GetDuration().ToShortDuration(),
                    Model.Price,
                    Model.ProductTimerValue.IsActive ? Resources.Active : "");
            }
        }

        public string TimerColor { get { return IsTimerVisible && Model.ProductTimerValue.IsActive ? "Blue" : "Gray"; } }

        public bool IsStateVisible { get { return !String.IsNullOrEmpty(State); } }

        public string State { get { return Model.GetStateDesc(x => _applicationState.CurrentLoggedInUser.UserRole.IsAdmin || _cacheService.CanShowStateOnTicket(x.StateName, x.State)); } }

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
            RaisePropertyChanged(() => IsTimerVisible);
            RaisePropertyChanged(() => TimerDescription);
            RaisePropertyChanged(() => TimerColor);
        }

        public decimal Price
        {
            get { return Model.GetVisiblePrice(); }
        }

        public decimal TotalPrice
        {
            get { return Price * Quantity; }
        }

        public string TotalPriceStr { get { return TotalPrice.ToString(LocalSettings.ReportCurrencyFormat); } }

        public bool Selected
        {
            get { return Model.IsSelected; }
            set { Model.IsSelected = value; UpdateItemColor(); RaisePropertyChanged(() => Selected); }
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
                return Model.OrderNumber > 0 ? String.Format(Resources.OrderNumber_f,
                    Model.OrderNumber, CreatingUserName) : Resources.NewOrder;
            }
        }

        public object GroupObject { get { return new { OrderNumber, Time = Model.Id > 0 ? Model.CreatedDateTime.ToShortTimeString() : "" }; } }

        public string CreatingUserName { get { return Model.CreatingUserName; } }

        public TextDecorationCollection TextDecoration
        {
            get
            {
                return Model.DecreaseInventory || Model.IncreaseInventory ? null : TextDecorations.Strikethrough;
            }
        }

        public FontWeight FontWeight { get { return _model.Locked ? FontWeights.Bold : FontWeights.Normal; } }

        public string PriceTag { get { return Model.PriceTag; } }

        private ObservableCollection<OrderTagValueViewModel> _orderTagValues;
        public ObservableCollection<OrderTagValueViewModel> OrderTagValues
        {
            get { return _orderTagValues ?? (_orderTagValues = new ObservableCollection<OrderTagValueViewModel>(Model.GetOrderTagValues().Select(x => new OrderTagValueViewModel(x)))); }
        }

        private string _orderKey;
        public string OrderKey { get { return _orderKey ?? (_orderKey = Model.OrderKey); } }

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
            this.PublishEvent(EventTopicNames.SelectedOrdersChanged, true);
        }

        public void UpdateItemColor()
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
            if (Selected)
            {
                Selected = false;
                ResetSelectedQuantity();
                UpdateItemColor();
                RefreshOrder();
            }
        }

        public void RefreshOrder()
        {
            RefreshProperties();
            RaisePropertyChanged(() => State);
            RaisePropertyChanged(() => TotalPriceStr);
            RaisePropertyChanged(() => Quantity);
            RaisePropertyChanged(() => Description);
            RaisePropertyChanged(() => FontWeight);
            RaisePropertyChanged(() => IsLocked);
        }

        private void RefreshProperties()
        {
            _orderTagValues = null;
            _orderKey = null;
            RaisePropertyChanged(() => OrderTagValues);
            RaisePropertyChanged(() => OrderKey);
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
