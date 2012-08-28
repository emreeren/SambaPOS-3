using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class OrderTagGroupViewModel : EntityViewModelBaseWithMap<OrderTagGroup, OrderTagMap, OrderTagMapViewModel>
    {
        private readonly IMenuService _menuService;

        [ImportingConstructor]
        public OrderTagGroupViewModel(IMenuService menuService)
        {
            _menuService = menuService;
            AddOrderTagCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.OrderTag), OnAddPropertyExecuted);
            DeleteOrderTagCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.OrderTag), OnDeletePropertyExecuted, CanDeleteProperty);
            SortOrderTagsCommand = new CaptionCommand<string>(string.Format(Resources.Sort_f, Resources.OrderTag), OnSortPropertyExecuted);
        }

        private ObservableCollection<OrderTagViewModel> _orderTags;
        public ObservableCollection<OrderTagViewModel> OrderTags { get { return _orderTags ?? (_orderTags = new ObservableCollection<OrderTagViewModel>(GetOrderTags(Model))); } }

        public ICaptionCommand AddOrderTagCommand { get; set; }
        public ICaptionCommand DeleteOrderTagCommand { get; set; }
        public ICaptionCommand SortOrderTagsCommand { get; set; }

        public string ButtonHeader { get { return Model.ButtonHeader; } set { Model.ButtonHeader = value; } }
        public bool AddTagPriceToOrderPrice { get { return Model.AddTagPriceToOrderPrice; } set { Model.AddTagPriceToOrderPrice = value; } }
        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }
        public int MaxSelectedItems { get { return Model.MaxSelectedItems; } set { Model.MaxSelectedItems = value; } }
        public int MinSelectedItems { get { return Model.MinSelectedItems; } set { Model.MinSelectedItems = value; } }
        public bool FreeTagging { get { return Model.FreeTagging; } set { Model.FreeTagging = value; } }
        public bool SaveFreeTags { get { return Model.SaveFreeTags; } set { Model.SaveFreeTags = value; } }

        public OrderTagViewModel SelectedOrderTag { get; set; }

        private void OnSortPropertyExecuted(string obj)
        {
            InteractionService.UserIntraction.SortItems(Model.OrderTags, string.Format(Resources.Sort_f, Resources.OrderTags), "");
            _orderTags = null;
            RaisePropertyChanged(() => OrderTags);
        }

        private void OnDeletePropertyExecuted(string obj)
        {
            if (SelectedOrderTag == null) return;
            if (SelectedOrderTag.Model.Id > 0)
                Workspace.Delete(SelectedOrderTag.Model);
            Model.OrderTags.Remove(SelectedOrderTag.Model);
            OrderTags.Remove(SelectedOrderTag);
        }

        private bool CanDeleteProperty(string arg)
        {
            return SelectedOrderTag != null;
        }

        private void OnAddPropertyExecuted(string obj)
        {
            OrderTags.Add(new OrderTagViewModel(MenuItem.AddDefaultMenuItemProperty(Model), _menuService));
        }

        private IEnumerable<OrderTagViewModel> GetOrderTags(OrderTagGroup baseModel)
        {
            return baseModel.OrderTags.OrderBy(x => x.Order).Select(item => new OrderTagViewModel(item, _menuService));
        }

        public override string GetModelTypeString()
        {
            return Resources.OrderTagGroup;
        }

        public override Type GetViewType()
        {
            return typeof(OrderTagGroupView);
        }

        protected override void Initialize()
        {
            base.Initialize();
            MapController = new MapController<OrderTagMap, OrderTagMapViewModel>(Model.OrderTagMaps, Workspace);
        }

        protected override void OnSave(string value)
        {
            ReorderItems(Model.OrderTags.OrderBy(x => x.Order).ThenBy(x => x.Id));
            base.OnSave(value);
        }

        private static void ReorderItems(IEnumerable<IOrderable> list)
        {
            var order = 10;
            foreach (var orderable in list)
            {
                orderable.Order = order;
                order += 10;
            }
        }

    }
}
