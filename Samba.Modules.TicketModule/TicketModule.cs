using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    [ModuleExport(typeof(TicketModule))]
    public class TicketModule : ModuleBase
    {
        [ImportingConstructor]
        public TicketModule()
        {
            AddDashboardCommand<EntityCollectionViewModelBase<TicketTemplateViewModel, TicketTemplate>>(Resources.TicketTemplates, Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<TicketTagGroupViewModel, TicketTagGroup>>(Resources.TicketTags, Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<OrderStateGroupViewModel, OrderStateGroup>>(Resources.OrderState.ToPlural(), Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<OrderTagGroupViewModel, OrderTagGroup>>(Resources.OrderTags, Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<OrderTagTemplateViewModel, OrderTagTemplate>>(Resources.OrderTagTemplates, Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<CalculationTemplateViewModel, CalculationTemplate>>(Resources.CalculationTemplates, Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<CalculationSelectorViewModel, CalculationSelector>>(Resources.CalculationSelector.ToPlural(), Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<PaymentTemplateViewModel, PaymentTemplate>>(Resources.PaymentTemplates, Resources.Tickets, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<ChangePaymentTemplateViewModel, ChangePaymentTemplate>>(Resources.ChangePaymentTemplate.ToPlural(), Resources.Tickets, 35);

            PermissionRegistry.RegisterPermission(PermissionNames.AddItemsToLockedTickets, PermissionCategories.Ticket, Resources.CanReleaseTicketLock);
            PermissionRegistry.RegisterPermission(PermissionNames.RemoveTicketTag, PermissionCategories.Ticket, Resources.CanRemoveTicketTag);
            PermissionRegistry.RegisterPermission(PermissionNames.MoveOrders, PermissionCategories.Ticket, Resources.CanMoveTicketLines);
            PermissionRegistry.RegisterPermission(PermissionNames.MergeTickets, PermissionCategories.Ticket, Resources.CanMergeTickets);
            PermissionRegistry.RegisterPermission(PermissionNames.DisplayOldTickets, PermissionCategories.Ticket, Resources.CanDisplayOldTickets);
            PermissionRegistry.RegisterPermission(PermissionNames.MoveUnlockedOrders, PermissionCategories.Ticket, Resources.CanMoveUnlockedTicketLines);
            PermissionRegistry.RegisterPermission(PermissionNames.ChangeExtraProperty, PermissionCategories.Ticket, Resources.CanUpdateExtraModifiers);
        }

        protected override void OnInitialization()
        {

        }

    }
}
