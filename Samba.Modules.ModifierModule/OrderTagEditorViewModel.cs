using System;
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
    public class OrderTagEditorViewModel : ObservableObject
    {
        [ImportingConstructor]
        public OrderTagEditorViewModel()
        {
            ToggleRemoveModeCommand = new CaptionCommand<string>(Resources.Remove, OnToggleRemoveMode);
            CloseCommand = new CaptionCommand<string>(Resources.Close, OnCloseCommandExecuted);
            OrderTagSelectedCommand = new DelegateCommand<OrderTagButtonViewModel>(OnOrderTagSelected);
            FreeTagSelectedCommand = new DelegateCommand<string>(OnFreeTagSelected);
            OrderTags = new ObservableCollection<OrderTagButtonViewModel>();
            EventServiceFactory.EventService.GetEvent<GenericEvent<OrderTagData>>().Subscribe(OnOrderTagDataSelected);
        }

        private void OnToggleRemoveMode(string obj)
        {
            RemoveMode = !RemoveMode;
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
                if (obj.Value.SelectedOrders.Count() == 1 && obj.Value.OrderTagGroup.FreeTagging)
                {
                    var so = obj.Value.SelectedOrders.ElementAt(0);
                    if (so.OrderTagValues.Where(x => x.OrderTagGroupId == obj.Value.OrderTagGroup.Id).Any(x => OrderTags.All(y => y.Name != x.TagValue)))
                    {
                        var ov = so.OrderTagValues.Where(x => x.OrderTagGroupId == obj.Value.OrderTagGroup.Id && OrderTags.All(y => y.Name != x.TagValue));
                        foreach (var orderTagValue in ov)
                        {
                            OrderTags.Add(new OrderTagButtonViewModel(obj.Value.SelectedOrders, obj.Value.OrderTagGroup,
                                new OrderTag { Name = orderTagValue.TagValue, Price = orderTagValue.Price }));
                        }
                    }
                }
                RaisePropertyChanged(() => OrderTagColumnCount);
                RaisePropertyChanged(() => FreeTagging);
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

        public bool FreeTagging { get { return SelectedOrderTagData == null || SelectedOrderTagData.OrderTagGroup.FreeTagging; } }

        private string _freeTagName;
        public string FreeTagName
        {
            get { return _freeTagName; }
            set { _freeTagName = value; RaisePropertyChanged(() => FreeTagName); }
        }

        public string FreeTagPriceStr
        {
            get { return FreeTagPrice.ToString(); }
            set { FreeTagPrice = Convert.ToDecimal(value); RaisePropertyChanged(() => FreeTagPriceStr); }
        }

        private decimal _freeTagPrice;
        public decimal FreeTagPrice
        {
            get { return _freeTagPrice; }
            set
            {
                _freeTagPrice = value;
                RaisePropertyChanged(() => FreeTagPrice);
            }
        }

        private bool _removeMode;
        public bool RemoveMode
        {
            get { return _removeMode; }
            set
            {
                _removeMode = value;
                RaisePropertyChanged(() => RemoveMode);
                RaisePropertyChanged(() => RemoveModeButtonColor);
            }
        }

        public string RemoveModeButtonColor { get { return RemoveMode ? "Black" : "Gainsboro"; } }

        public ICaptionCommand CloseCommand { get; set; }
        public CaptionCommand<string> ToggleRemoveModeCommand { get; set; }
        public DelegateCommand<OrderTagButtonViewModel> OrderTagSelectedCommand { get; set; }
        public DelegateCommand<string> FreeTagSelectedCommand { get; set; }
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

            orderTagData.PublishEvent(RemoveMode ? EventTopicNames.OrderTagRemoved : EventTopicNames.OrderTagSelected, true);
            RemoveMode = false;
            OrderTags.ToList().ForEach(x => x.Refresh());
        }

        private void OnFreeTagSelected(string obj)
        {
            if (string.IsNullOrEmpty(FreeTagName) || string.IsNullOrEmpty(FreeTagName.Trim())) return;
            if (OrderTags.Any(x => x.Name.ToLower() == FreeTagName.ToLower()))
            {
                var b = OrderTags.First(x => x.Name == FreeTagName.ToLower());
                OnOrderTagSelected(b);
                return;
            }
            var orderTagData = new OrderTagData
            {
                SelectedOrders = SelectedOrderTagData.SelectedOrders,
                OrderTagGroup = SelectedOrderTagData.OrderTagGroup,
                SelectedOrderTag = new OrderTag { Name = FreeTagName, Price = FreeTagPrice },
                Ticket = SelectedTicket
            };
            FreeTagName = "";
            FreeTagPriceStr = "0";
            OrderTags.Add(new OrderTagButtonViewModel(orderTagData.SelectedOrders, orderTagData.OrderTagGroup, orderTagData.SelectedOrderTag));
            orderTagData.PublishEvent(RemoveMode ? EventTopicNames.OrderTagRemoved : EventTopicNames.OrderTagSelected, true);
            RemoveMode = false;
            RaisePropertyChanged(() => OrderTagColumnCount);
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
