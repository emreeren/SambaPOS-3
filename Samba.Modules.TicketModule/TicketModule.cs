using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule
{
    [ModuleExport(typeof(TicketModule))]
    public class TicketModule : ModuleBase
    {
        readonly IRegionManager _regionManager;

        [ImportingConstructor]
        public TicketModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            AddDashboardCommand<EntityCollectionViewModelBase<TicketTemplateViewModel, TicketTemplate>>(Resources.TicketTemplates, Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<TicketTagGroupViewModel, TicketTagGroup>>(Resources.TicketTags, Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<OrderTagTemplateViewModel, OrderTagTemplate>>(Resources.OrderTagTemplates, Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<ServiceTemplateViewModel, ServiceTemplate>>(Resources.ServiceTemplates, Resources.Products, 35);

            PermissionRegistry.RegisterPermission(PermissionNames.AddItemsToLockedTickets, PermissionCategories.Ticket, Resources.CanReleaseTicketLock);
            PermissionRegistry.RegisterPermission(PermissionNames.RemoveTicketTag, PermissionCategories.Ticket, Resources.CanRemoveTicketTag);
            PermissionRegistry.RegisterPermission(PermissionNames.GiftItems, PermissionCategories.Ticket, Resources.CanGiftItems);
            PermissionRegistry.RegisterPermission(PermissionNames.VoidItems, PermissionCategories.Ticket, Resources.CanVoidItems);
            PermissionRegistry.RegisterPermission(PermissionNames.MoveOrders, PermissionCategories.Ticket, Resources.CanMoveTicketLines);
            PermissionRegistry.RegisterPermission(PermissionNames.MergeTickets, PermissionCategories.Ticket, Resources.CanMergeTickets);
            PermissionRegistry.RegisterPermission(PermissionNames.DisplayOldTickets, PermissionCategories.Ticket, Resources.CanDisplayOldTickets);
            PermissionRegistry.RegisterPermission(PermissionNames.MoveUnlockedOrders, PermissionCategories.Ticket, Resources.CanMoveUnlockedTicketLines);
            PermissionRegistry.RegisterPermission(PermissionNames.ChangeExtraProperty, PermissionCategories.Ticket, Resources.CanUpdateExtraModifiers);

            PermissionRegistry.RegisterPermission(PermissionNames.MakePayment, PermissionCategories.Payment, Resources.CanGetPayment);
            PermissionRegistry.RegisterPermission(PermissionNames.MakeFastPayment, PermissionCategories.Payment, Resources.CanGetFastPayment);
            PermissionRegistry.RegisterPermission(PermissionNames.MakeDiscount, PermissionCategories.Payment, Resources.CanMakeDiscount);
            PermissionRegistry.RegisterPermission(PermissionNames.RoundPayment, PermissionCategories.Payment, Resources.CanRoundTicketTotal);
            PermissionRegistry.RegisterPermission(PermissionNames.FixPayment, PermissionCategories.Payment, Resources.CanFlattenTicketTotal);
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.TicketRegion, typeof(TicketListView));
            _regionManager.RegisterViewWithRegion(RegionNames.TicketOrdersRegion, typeof(TicketOrdersView));
            _regionManager.RegisterViewWithRegion(RegionNames.OpenTicketRegion, typeof(OpenTicketsView));
        }
    }
}
