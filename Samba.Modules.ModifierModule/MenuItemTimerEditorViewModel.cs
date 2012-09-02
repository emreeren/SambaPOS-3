using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services.Common;

namespace Samba.Modules.ModifierModule
{
    [Export]
    public class MenuItemTimerEditorViewModel : ObservableObject
    {
        [ImportingConstructor]
        public MenuItemTimerEditorViewModel()
        {
            SelectedOrder = Order.Null;
            CloseCommand = new CaptionCommand<string>(Resources.Close, OnCloseCommandExecuted);
            StopTimerCommand = new CaptionCommand<string>(Resources.StopTimer, OnStopTimer,CanStopTimer);
        }

        public CaptionCommand<string> CloseCommand { get; set; }
        public CaptionCommand<string> StopTimerCommand { get; set; }

        public Order SelectedOrder { get; set; }

        private bool _stopTimer;
        public bool StopTimer
        {
            get { return _stopTimer; }
            set
            {
                _stopTimer = value;
                RaisePropertyChanged(() => ButtonColor);
            }
        }

        public string ButtonColor { get { return StopTimer ? "Black" : "Gainsboro"; } }
        public DateTime StartDate { get { return SelectedOrder.MenuItemTimerValue.Start; } }
        public DateTime EndDate { get { return IsActive ? DateTime.Now : SelectedOrder.MenuItemTimerValue.End; } }
        public bool IsActive { get { return SelectedOrder.MenuItemTimerValue.IsActive; } }
        public string Start { get { return SelectedOrder.MenuItemTimerValue.Start.ToString(); } }
        public string End { get { return IsActive ? Resources.Active : SelectedOrder.MenuItemTimerValue.End.ToString(); } }
        public string Duration { get { return TimeSpan.FromTicks(EndDate.Ticks - StartDate.Ticks).ToLongDuration(); } }
        public string Price { get { return GetPriceDescription(); } }
        public string Value { get { return GetValueDescription(); } }

        private string GetPriceDescription()
        {
            var mi = SelectedOrder.MenuItemTimerValue;
            return string.Format("{0:#} {1} {2}: {3:N}", mi.PriceDuration, GetTimeDescription(mi.PriceType), Resources.Price, SelectedOrder.Price);
        }

        private string GetValueDescription()
        {
            var mi = SelectedOrder.MenuItemTimerValue;
            return string.Format("{0:#} {1} {2}: {3:N}", mi.GetTime(), GetTimeDescription(mi.PriceType), Resources.Price, SelectedOrder.GetPlainPrice());
        }

        public static string GetTimeDescription(int priceType)
        {
            switch (priceType)
            {
                case 2: return Resources.Day;
                case 1: return Resources.Hour;
                default: return Resources.Minute;
            }
        }

        public void Update(Order order)
        {
            StopTimer = false;
            SelectedOrder = order;
            RaisePropertyChanged(() => Start);
            RaisePropertyChanged(() => End);
            RaisePropertyChanged(() => Duration);
            RaisePropertyChanged(() => Price);
            RaisePropertyChanged(() => Value);
        }

        public bool ShouldDisplay(Ticket ticket, IList<Order> selectedOrders)
        {
            if (selectedOrders == null || selectedOrders.Count() > 1 || selectedOrders.Count() == 0) return false;
            var order = selectedOrders.First();
            return order.Locked && order.MenuItemTimerValue != null;
        }

        private void OnCloseCommandExecuted(string obj)
        {
            if (StopTimer) SelectedOrder.StopMenuItemTimer();
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }

        private void OnStopTimer(string obj)
        {
            StopTimer = !StopTimer;
        }

        private bool CanStopTimer(string arg)
        {
            return SelectedOrder.MenuItemTimerValue.IsActive;
        }
    }
}
