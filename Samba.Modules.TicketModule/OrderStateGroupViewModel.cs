using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class OrderStateGroupViewModel : EntityViewModelBaseWithMap<OrderStateGroup, OrderStateMap, OrderStateMapViewModel>
    {
        [ImportingConstructor]
        public OrderStateGroupViewModel()
        {
            AddOrderStateCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.OrderState), OnAddPropertyExecuted);
            DeleteOrderStateCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.OrderState), OnDeletePropertyExecuted, CanDeleteProperty);
        }

        private ObservableCollection<OrderStateViewModel> _orderStates;
        public ObservableCollection<OrderStateViewModel> OrderStates { get { return _orderStates ?? (_orderStates = new ObservableCollection<OrderStateViewModel>(GetOrderStates(Model))); } }

        public ICaptionCommand AddOrderStateCommand { get; set; }
        public ICaptionCommand DeleteOrderStateCommand { get; set; }

        public string ButtonHeader { get { return Model.ButtonHeader; } set { Model.ButtonHeader = value; } }
        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }
        public bool UnlocksOrder { get { return Model.UnlocksOrder; } set { Model.UnlocksOrder = value; } }
        public bool CalculateOrderPrice { get { return Model.CalculateOrderPrice; } set { Model.CalculateOrderPrice = value; } }
        public bool DecreaseOrderInventory { get { return Model.DecreaseOrderInventory; } set { Model.DecreaseOrderInventory = value; } }
        public bool IncreaseOrderInventory { get { return Model.IncreaseOrderInventory; } set { Model.IncreaseOrderInventory = value; } }
        public int AccountTransactionTemplateId { get { return Model.AccountTransactionTemplateId; } set { Model.AccountTransactionTemplateId = value; } }

        public OrderStateViewModel SelectedOrderState { get; set; }

        private IEnumerable<AccountTransactionTemplate> _accountTransactionTemplates;
        public IEnumerable<AccountTransactionTemplate> AccountTransactionTemplates { get { return _accountTransactionTemplates ?? (_accountTransactionTemplates = Workspace.All<AccountTransactionTemplate>()); } }

        public AccountTransactionTemplate AccountTransactionTemplate
        {
            get
            {
                return AccountTransactionTemplateId == 0 ? null : AccountTransactionTemplates.FirstOrDefault(x => x.Id == AccountTransactionTemplateId);
            }
            set
            {
                AccountTransactionTemplateId = value != null ? value.Id : 0;
            }
        }

        private void OnDeletePropertyExecuted(string obj)
        {
            if (SelectedOrderState == null) return;
            if (SelectedOrderState.Model.Id > 0)
                Workspace.Delete(SelectedOrderState.Model);
            Model.OrderStates.Remove(SelectedOrderState.Model);
            OrderStates.Remove(SelectedOrderState);
        }

        private bool CanDeleteProperty(string arg)
        {
            return SelectedOrderState != null;
        }

        private void OnAddPropertyExecuted(string obj)
        {
            OrderStates.Add(new OrderStateViewModel(Model.AddOrderState("")));
        }

        private IEnumerable<OrderStateViewModel> GetOrderStates(OrderStateGroup baseModel)
        {
            return baseModel.OrderStates.Select(item => new OrderStateViewModel(item));
        }

        public override string GetModelTypeString()
        {
            return Resources.OrderStateGroup;
        }

        public override Type GetViewType()
        {
            return typeof(OrderStateGroupView);
        }

        protected override void Initialize()
        {
            base.Initialize();
            MapController = new MapController<OrderStateMap, OrderStateMapViewModel>(Model.OrderStateMaps, Workspace);
        }

        protected override AbstractValidator<OrderStateGroup> GetValidator()
        {
            return new OrderStateGroupValidator();
        }
    }

    internal class OrderStateGroupValidator : EntityValidator<OrderStateGroup>
    {
        public OrderStateGroupValidator()
        {
            RuleFor(x => x.AccountTransactionTemplateId).GreaterThan(0).When(x => x.IncreaseOrderInventory);
            RuleFor(x => x.DecreaseOrderInventory).Equal(false).When(x => x.IncreaseOrderInventory);
        }
    }
}
