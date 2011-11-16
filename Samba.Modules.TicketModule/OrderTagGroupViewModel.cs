using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    public class OrderTagGroupViewModel : EntityViewModelBase<OrderTagGroup>
    {
        private ObservableCollection<OrderTagViewModel> _properties;
        public ObservableCollection<OrderTagViewModel> Properties { get { return _properties ?? (_properties = new ObservableCollection<OrderTagViewModel>(GetProperties(Model))); } }

        private ObservableCollection<OrderTagMapViewModel> _orderTagMaps;
        public ObservableCollection<OrderTagMapViewModel> OrderTagMaps { get { return _orderTagMaps ?? (_orderTagMaps = new ObservableCollection<OrderTagMapViewModel>(GetOrderTagMaps(Model))); } }

        public OrderTagViewModel SelectedProperty { get; set; }
        public ICaptionCommand AddPropertyCommand { get; set; }
        public ICaptionCommand DeletePropertyCommand { get; set; }
        public ICaptionCommand AddOrderTagMapCommand { get; set; }
        public ICaptionCommand DeleteOrderTagMapCommand { get; set; }

        public bool SingleSelection { get { return Model.SingleSelection; } set { Model.SingleSelection = value; } }
        public bool MultipleSelection { get { return Model.MultipleSelection; } set { Model.MultipleSelection = value; } }
        public bool CalculateWithParentPrice { get { return Model.CalculateWithParentPrice; } set { Model.CalculateWithParentPrice = value; } }
        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }
        public int TerminalButtonHeight { get { return Model.TerminalButtonHeight; } set { Model.TerminalButtonHeight = value; } }
        public int TerminalColumnCount { get { return Model.TerminalColumnCount; } set { Model.TerminalColumnCount = value; } }

        public OrderTagMapViewModel SelectedOrderTagMap { get; set; }

        public OrderTagGroupViewModel(OrderTagGroup model)
            : base(model)
        {
            AddPropertyCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Modifier), OnAddPropertyExecuted);
            DeletePropertyCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.Modifier), OnDeletePropertyExecuted, CanDeleteProperty);
            AddOrderTagMapCommand = new CaptionCommand<string>(Resources.Add, OnAddOrderTagMap);
            DeleteOrderTagMapCommand = new CaptionCommand<string>(Resources.Delete, OnDeleteOrderTagMap, CanDeleteOrderTagMap);
        }

        private bool CanDeleteOrderTagMap(string arg)
        {
            return SelectedOrderTagMap != null;
        }

        private void OnDeleteOrderTagMap(string obj)
        {
            if (SelectedOrderTagMap.Id > 0)
                Workspace.Delete(SelectedOrderTagMap.Model);
            Model.OrderTagMaps.Remove(SelectedOrderTagMap.Model);
            OrderTagMaps.Remove(SelectedOrderTagMap);
        }

        private void OnAddOrderTagMap(string obj)
        {
            OrderTagMaps.Add(new OrderTagMapViewModel(Model.AddOrderTagMap()));
        }

        private void OnDeletePropertyExecuted(string obj)
        {
            if (SelectedProperty == null) return;
            if (SelectedProperty.Model.Id > 0)
                Workspace.Delete(SelectedProperty.Model);
            Model.OrderTags.Remove(SelectedProperty.Model);
            Properties.Remove(SelectedProperty);
        }

        private bool CanDeleteProperty(string arg)
        {
            return SelectedProperty != null;
        }

        private void OnAddPropertyExecuted(string obj)
        {
            Properties.Add(new OrderTagViewModel(MenuItem.AddDefaultMenuItemProperty(Model)));
        }

        private static IEnumerable<OrderTagViewModel> GetProperties(OrderTagGroup baseModel)
        {
            return baseModel.OrderTags.Select(item => new OrderTagViewModel(item));
        }

        private static IEnumerable<OrderTagMapViewModel> GetOrderTagMaps(OrderTagGroup model)
        {
            return model.OrderTagMaps.Select(x => new OrderTagMapViewModel(x));
        }

        public override string GetModelTypeString()
        {
            return Resources.ModifierGroup;
        }

        public override Type GetViewType()
        {
            return typeof(OrderTagGroupView);
        }
    }
}
