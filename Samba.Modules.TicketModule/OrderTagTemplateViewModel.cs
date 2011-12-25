using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    class OrderTagTemplateViewModel : EntityViewModelBase<OrderTagTemplate>
    {
        public OrderTagTemplateViewModel()
        {
            AddOrderTagTemplateCommand = new CaptionCommand<string>(Resources.Add, OnAddOrderTagTemplate);
            DeleteOrderTagTemplateCommand = new CaptionCommand<string>(Resources.Delete, OnDeleteOrderTagTemplate, CanDeleteOrderTagTemplate);
        }

        public ICaptionCommand AddOrderTagTemplateCommand { get; set; }
        public ICaptionCommand DeleteOrderTagTemplateCommand { get; set; }

        private ObservableCollection<OrderTagTemplateValueViewModel> _orderTagTemplateValues;
        public ObservableCollection<OrderTagTemplateValueViewModel> OrderTagTemplateValues
        {
            get
            {
                return _orderTagTemplateValues ??
                       (_orderTagTemplateValues = new ObservableCollection<OrderTagTemplateValueViewModel>(Model.OrderTagTemplateValues.Select(x => new OrderTagTemplateValueViewModel(Workspace, x))));
            }
        }
        public OrderTagTemplateValueViewModel SelectedOrderTagTemplateValue { get; set; }


        private bool CanDeleteOrderTagTemplate(string arg)
        {
            return SelectedOrderTagTemplateValue != null;
        }

        private void OnDeleteOrderTagTemplate(string obj)
        {
            Model.OrderTagTemplateValues.Remove(SelectedOrderTagTemplateValue.Model);
            if (SelectedOrderTagTemplateValue.Model.Id > 0)
                Workspace.Delete(SelectedOrderTagTemplateValue.Model);
            OrderTagTemplateValues.Remove(SelectedOrderTagTemplateValue);
        }

        private void OnAddOrderTagTemplate(string obj)
        {
            var orderTagTemplateValue = new OrderTagTemplateValue();
            Model.OrderTagTemplateValues.Add(orderTagTemplateValue);
            _orderTagTemplateValues.Add(new OrderTagTemplateValueViewModel(Workspace, orderTagTemplateValue));
        }

        public override Type GetViewType()
        {
            return typeof(OrderTagTemplateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.OrderTagTemplate;
        }

        protected override bool CanSave(string arg)
        {
            if (OrderTagTemplateValues.Any(x => x.OrderTag == null || x.OrderTagGroup == null)) return false;
            return base.CanSave(arg);
        }
    }
}
