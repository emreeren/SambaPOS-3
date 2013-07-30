using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services.Common;
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

        public bool AddTagPriceToOrderPrice { get { return Model.AddTagPriceToOrderPrice; } set { Model.AddTagPriceToOrderPrice = value; } }
        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }
        public int FontSize { get { return Model.FontSize; } set { Model.FontSize = value; } }
        public string ButtonColor { get { return Model.ButtonColor; } set { Model.ButtonColor = value; } }
        public int MaxSelectedItems { get { return Model.MaxSelectedItems; } set { Model.MaxSelectedItems = value; } }
        public int MinSelectedItems { get { return Model.MinSelectedItems; } set { Model.MinSelectedItems = value; } }
        public bool FreeTagging { get { return Model.FreeTagging; } set { Model.FreeTagging = value; } }
        public bool SaveFreeTags { get { return Model.SaveFreeTags; } set { Model.SaveFreeTags = value; } }
        public string GroupTag { get { return Model.GroupTag; } set { Model.GroupTag = value; } }
        public bool TaxFree { get { return Model.TaxFree; } set { Model.TaxFree = value; } }
        public bool Hidden { get { return Model.Hidden; } set { Model.Hidden = value; } }

        public OrderTagViewModel SelectedOrderTag { get; set; }

        private void OnSortPropertyExecuted(string obj)
        {
            InteractionService.UserIntraction.SortItems(Model.OrderTags, string.Format(Resources.Sort_f, Resources.OrderTag.ToPlural()), "");
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
            var orderTag = MenuItem.AddDefaultMenuItemProperty(Model);
            orderTag.SortOrder = OrderTags.Any() ? OrderTags.Max(x => x.Model.SortOrder) + 1 : 0;
            OrderTags.Add(new OrderTagViewModel(orderTag, _menuService));
        }

        private IEnumerable<OrderTagViewModel> GetOrderTags(OrderTagGroup baseModel)
        {
            return baseModel.OrderTags.OrderBy(x => x.SortOrder).Select(item => new OrderTagViewModel(item, _menuService));
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
            ReorderItems(Model.OrderTags.OrderBy(x => x.SortOrder).ThenBy(x => x.Id));
            base.OnSave(value);
        }

        private static void ReorderItems(IEnumerable<IOrderable> list)
        {
            var order = 10;
            foreach (var orderable in list)
            {
                orderable.SortOrder = order;
                order += 10;
            }
        }

        protected override AbstractValidator<OrderTagGroup> GetValidator()
        {
            return new OrderTagGroupValidator();
        }


    }

    internal class OrderTagGroupValidator : EntityValidator<OrderTagGroup>
    {
        public OrderTagGroupValidator()
        {
            RuleFor(x => x.MaxSelectedItems).GreaterThanOrEqualTo(x => x.MinSelectedItems).When(x => x.MaxSelectedItems > 0);
            RuleFor(x => x.MaxSelectedItems).Equal(1).When(x => !string.IsNullOrEmpty(x.GroupTag));
            RuleFor(x => x.OrderTags).NotEmpty().When(x => !string.IsNullOrEmpty(x.GroupTag));
            Custom(x =>
            {
                if (!string.IsNullOrWhiteSpace(x.GroupTag) && x.OrderTags.Count < 2)
                {
                    return new ValidationFailure("Order Tags", "Order Tag count should be at least 2 when Group Tag entered.");
                }
                if (x.OrderTags.Select(y => y.Name.ToLower()).Distinct().Count(y => !string.IsNullOrEmpty(y)) != x.OrderTags.Count)
                {
                    return new ValidationFailure("Order Tags", "Order Tags should have unique names");
                }
                return null;
            });
        }
    }
}
