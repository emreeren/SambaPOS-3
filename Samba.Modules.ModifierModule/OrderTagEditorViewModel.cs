using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services.Common;

namespace Samba.Modules.ModifierModule
{
    [Export]
    public class OrderTagEditorViewModel : ObservableObject
    {

        [ImportingConstructor]
        public OrderTagEditorViewModel()
        {
            CloseCommand = new CaptionCommand<string>(Resources.Close, OnCloseCommandExecuted);
            OrderTagSelectedCommand = new DelegateCommand<OrderTagButtonViewModel>(OnOrderTagSelected);
            OrderTags = new ObservableCollection<OrderTagButtonViewModel>();
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderTagData>>().Subscribe(OnOrderTagDataSelected);
        }

        private void OnOrderTagDataSelected(EventParameters<OrderTagData> obj)
        {
            if (obj.Topic == EventTopicNames.SelectOrderTag)
            {
                ResetValues(obj.Value.Ticket);
                SelectedOrderTagData = obj.Value;
                OrderTags.AddRange(obj.Value.OrderTagGroup.OrderTags.Select(x => new OrderTagButtonViewModel(obj.Value.SelectedOrders, obj.Value.OrderTagGroup, x)));
                if (OrderTags.Count == 1)
                {
                    obj.Value.SelectedOrderTag = OrderTags[0].Model;
                    obj.Value.PublishEvent(EventTopicNames.OrderTagSelected);
                    return;
                }
                RaisePropertyChanged(() => OrderTagColumnCount);
            }
        }

        private void ResetValues(Ticket selectedTicket)
        {
            SelectedTicket = null;
            SelectedOrder = null;
            SelectedOrderTagData = null;
            OrderTags.Clear();
            SetSelectedTicket(selectedTicket);
        }

        public Ticket SelectedTicket { get; private set; }
        public Order SelectedOrder { get; private set; }
        public OrderTagData SelectedOrderTagData { get; set; }

        public ICaptionCommand CloseCommand { get; set; }

        public DelegateCommand<OrderTagButtonViewModel> OrderTagSelectedCommand { get; set; }

        public ObservableCollection<OrderTagButtonViewModel> OrderTags { get; set; }
        public int OrderTagColumnCount { get { return OrderTags.Count % 7 == 0 ? OrderTags.Count / 7 : (OrderTags.Count / 7) + 1; } }


        private static void OnCloseCommandExecuted(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }

        private void OnOrderTagSelected(OrderTagButtonViewModel orderTag)
        {
            var orderTagData = new OrderTagData
                                   {
                                       OrderTagGroup = SelectedOrderTagData.OrderTagGroup,
                                       SelectedOrderTag = orderTag.Model,
                                       Ticket = SelectedTicket
                                   };

            orderTagData.PublishEvent(EventTopicNames.OrderTagSelected, true);

            OrderTags.ToList().ForEach(x => x.Refresh());
        }

        private void SetSelectedTicket(Ticket ticketViewModel)
        {
            SelectedTicket = ticketViewModel;
            RaisePropertyChanged(() => SelectedTicket);
            RaisePropertyChanged(() => SelectedOrder);
        }

    }
}
