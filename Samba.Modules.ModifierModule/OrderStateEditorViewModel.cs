using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.ViewModels;
using Samba.Services.Common;

namespace Samba.Modules.ModifierModule
{
    [Export]
    public class OrderStateEditorViewModel : ObservableObject
    {

        [ImportingConstructor]
        public OrderStateEditorViewModel()
        {
            CloseCommand = new CaptionCommand<string>(Resources.Close, OnCloseCommandExecuted);
            OrderStateSelectedCommand = new DelegateCommand<OrderStateButtonViewModel>(OnOrderStateSelected);
            OrderStates = new ObservableCollection<OrderStateButtonViewModel>();
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderStateData>>().Subscribe(OnOrderStateDataSelected);
        }

        private void OnOrderStateDataSelected(EventParameters<OrderStateData> obj)
        {
            if (obj.Topic == EventTopicNames.SelectOrderState)
            {
                ResetValues(obj.Value.Ticket);
                SelectedOrderStateData = obj.Value;
                OrderStates.AddRange(obj.Value.OrderStateGroup.OrderStates.Select(x => new OrderStateButtonViewModel(obj.Value.SelectedOrders, obj.Value.OrderStateGroup, x)));
                if (OrderStates.Count == 1)
                {
                    obj.Value.SelectedOrderState = OrderStates[0].Model;
                    obj.Value.PublishEvent(EventTopicNames.OrderStateSelected);
                    return;
                }
                RaisePropertyChanged(() => OrderStateColumnCount);
                
            }
        }

        private void ResetValues(Ticket selectedTicket)
        {
            SelectedTicket = null;
            SelectedOrder = null;
            SelectedOrderStateData = null;
            OrderStates.Clear();
            SetSelectedTicket(selectedTicket);
        }

        public Ticket SelectedTicket { get; private set; }
        public Order SelectedOrder { get; private set; }
        public OrderStateData SelectedOrderStateData { get; set; }

        public ICaptionCommand CloseCommand { get; set; }

        public DelegateCommand<OrderStateButtonViewModel> OrderStateSelectedCommand { get; set; }

        public ObservableCollection<OrderStateButtonViewModel> OrderStates { get; set; }
        public int OrderStateColumnCount { get { return OrderStates.Count % 7 == 0 ? OrderStates.Count / 7 : (OrderStates.Count / 7) + 1; } }


        private static void OnCloseCommandExecuted(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }

        private void OnOrderStateSelected(OrderStateButtonViewModel orderState)
        {
            var orderStateData = new OrderStateData
                                   {
                                       OrderStateGroup = SelectedOrderStateData.OrderStateGroup,
                                       SelectedOrderState = orderState.Model,
                                       Ticket = SelectedTicket
                                   };

            orderStateData.PublishEvent(EventTopicNames.OrderStateSelected, true);

            OrderStates.ToList().ForEach(x => x.Refresh());
        }

        private void SetSelectedTicket(Ticket ticketViewModel)
        {
            SelectedTicket = ticketViewModel;
            RaisePropertyChanged(() => SelectedTicket);
            RaisePropertyChanged(() => SelectedOrder);
        }

    }
}
