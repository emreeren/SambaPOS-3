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
    public class ExtraModifierEditorViewModel : ObservableObject
    {
        public ICaptionCommand CloseCommand { get; set; }
        public ICaptionCommand UpdateExtraPropertiesCommand { get; set; }

        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get { return _selectedOrder; }
            set
            {
                _selectedOrder = value;
                RaisePropertyChanged(() => CustomPropertyName);
                RaisePropertyChanged(() => CustomPropertyPrice);
                RaisePropertyChanged(() => CustomPropertyQuantity);
            }
        }

        [ImportingConstructor]
        public ExtraModifierEditorViewModel()
        {
            CloseCommand = new CaptionCommand<string>(Resources.Close, OnCloseCommandExecuted);
            UpdateExtraPropertiesCommand = new CaptionCommand<string>(Resources.Update, OnUpdateExtraProperties);
            SelectedOrder = Order.Null;
        }

        public string CustomPropertyName
        {
            get { return SelectedOrder.GetCustomOrderTag() != null ? SelectedOrder.GetCustomOrderTag().Name : ""; }
            set
            {
                SelectedOrder.UpdateCustomOrderTag(value, CustomPropertyPrice, CustomPropertyQuantity);
                RaisePropertyChanged(() => CustomPropertyName);
            }
        }

        public decimal CustomPropertyPrice
        {
            get
            {
                var prop = SelectedOrder.GetCustomOrderTag();
                if (prop != null)
                {
                    return SelectedOrder.TaxIncluded ? prop.Price + prop.TaxAmount : prop.Price;
                }
                return 0;
            }
            set
            {
                SelectedOrder.UpdateCustomOrderTag(CustomPropertyName, value, CustomPropertyQuantity);
                RaisePropertyChanged(() => CustomPropertyPrice);
            }
        }

        public decimal CustomPropertyQuantity
        {
            get { return SelectedOrder.GetCustomOrderTag() != null ? SelectedOrder.GetCustomOrderTag().Quantity : 1; }
            set
            {
                SelectedOrder.UpdateCustomOrderTag(CustomPropertyName, CustomPropertyPrice, value);
                RaisePropertyChanged(() => CustomPropertyQuantity);
            }
        }

        private void OnCloseCommandExecuted(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }

        private void OnUpdateExtraProperties(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
        }


    }
}
