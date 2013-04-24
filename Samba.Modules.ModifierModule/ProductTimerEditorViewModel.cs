using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Localization;
using Samba.Localization.Properties;
using Samba.Persistance;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services.Common;
using Samba.Services.Common;

namespace Samba.Modules.ModifierModule
{
    [Export]
    public class ProductTimerEditorViewModel : ObservableObject
    {
        [ImportingConstructor]
        public ProductTimerEditorViewModel()
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
        public DateTime StartDate { get { return SelectedOrder.ProductTimerValue.Start; } }
        public DateTime EndDate { get { return IsActive ? DateTime.Now : SelectedOrder.ProductTimerValue.End; } }
        public bool IsActive { get { return SelectedOrder.ProductTimerValue.IsActive; } }
        public string Start { get { return SelectedOrder.ProductTimerValue.Start.ToString(); } }
        public string End { get { return IsActive ? Resources.Active : SelectedOrder.ProductTimerValue.End.ToString(); } }
        public string Duration { get { return TimeSpan.FromTicks(EndDate.Ticks - StartDate.Ticks).ToLongDuration(); } }
        public string Price { get { return GetPriceDescription(); } }
        public string Value { get { return GetValueDescription(); } }

        private string GetPriceDescription()
        {
            var mi = SelectedOrder.ProductTimerValue;
            return string.Format("{0:#} {1} {2}: {3:N}", mi.PriceDuration, GetTimeDescription(mi.PriceType), Resources.Price, SelectedOrder.Price);
        }

        private string GetValueDescription()
        {
            var mi = SelectedOrder.ProductTimerValue;
            return string.Format("{0:#} {1} {2}: {3:N}", mi.GetTime(), GetTimeDescription(mi.PriceType), Resources.Price, SelectedOrder.GetPrice());
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
            if (selectedOrders == null || selectedOrders.Count() > 1 || !selectedOrders.Any()) return false;
            var order = selectedOrders.First();
            return order.Locked && order.ProductTimerValue != null;
        }

        private void OnCloseCommandExecuted(string obj)
        {
            if (StopTimer) SelectedOrder.StopProductTimer();
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }

        private void OnStopTimer(string obj)
        {
            StopTimer = !StopTimer;
        }

        private bool CanStopTimer(string arg)
        {
            return SelectedOrder.ProductTimerValue.IsActive;
        }
    }
}
