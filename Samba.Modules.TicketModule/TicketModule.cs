using System;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    [ModuleExport(typeof(TicketModule))]
    public class TicketModule : VisibleModuleBase
    {
        readonly IRegionManager _regionManager;
        private readonly TicketEditorView _ticketEditorView;

        [ImportingConstructor]
        public TicketModule(IRegionManager regionManager, TicketEditorView ticketEditorView)
            : base(regionManager, AppScreens.TicketList)
        {
            SetNavigationCommand("POS", Resources.Common, "Images/Network.png");

            _regionManager = regionManager;
            _ticketEditorView = ticketEditorView;

            AddDashboardCommand<OrderTagGroupListViewModel>(Resources.OrderTags, Resources.Settings,10);
            AddDashboardCommand<TicketTagGroupListViewModel>(Resources.TicketTags, Resources.Settings, 10);

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

            EventServiceFactory.EventService.GetEvent<GenericEvent<Account>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.AccountSelectedForTicket || x.Topic == EventTopicNames.PaymentRequestedForTicket)
                        Activate();
                }
                );

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.ActivateTicketView || x.Topic == EventTopicNames.DisplayTicketView)
                        Activate();
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(
            x =>
            {
                if (x.Topic == EventTopicNames.WorkPeriodStatusChanged)
                {
                    if (x.Value.StartDate < x.Value.EndDate)
                    {
                        using (var v = WorkspaceFactory.Create())
                        {
                            var items = v.All<ScreenMenuItem>().ToList();
                            using (var vr = WorkspaceFactory.CreateReadOnly())
                            {
                                AppServices.ResetCache();
                                var endDate = AppServices.MainDataContext.LastTwoWorkPeriods.Last().EndDate;
                                var startDate = endDate.AddDays(-7);
                                vr.Queryable<Order>()
                                    .Where(y => y.CreatedDateTime >= startDate && y.CreatedDateTime < endDate)
                                    .GroupBy(y => y.MenuItemId)
                                    .ToList().ForEach(
                                        y => items.Where(z => z.MenuItemId == y.Key).ToList().ForEach(z => z.UsageCount = y.Count()));
                            }
                            v.CommitChanges();
                        }
                    }
                }
            });

        }

        protected override bool CanNavigate(string arg)
        {
            return AppServices.MainDataContext.IsCurrentWorkPeriodOpen;
        }

        protected override void OnNavigate(string obj)
        {
            base.OnNavigate(obj);
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicketView);
        }

        public override object GetVisibleView()
        {
            return _ticketEditorView;
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(TicketEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.UserRegion, typeof(DepartmentButtonView));
        }
    }
}
