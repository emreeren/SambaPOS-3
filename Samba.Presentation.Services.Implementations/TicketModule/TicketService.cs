using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data.Serializer;
using Samba.Localization.Properties;
using Samba.Persistance;
using Samba.Persistance.DaoClasses;
using Samba.Persistance.Data;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Presentation.Services.Implementations.TicketModule
{
    [Export(typeof(ITicketService))]
    public class TicketService : ITicketService
    {
        private readonly ITicketDao _ticketDao;
        private readonly IApplicationState _applicationState;
        private readonly IAutomationService _automationService;
        private readonly IUserService _userService;
        private readonly ISettingService _settingService;
        private readonly IPresentationCacheService _presentationCacheService;
        private readonly IAccountService _accountService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public TicketService(ITicketDao ticketDao, IDepartmentService departmentService, IApplicationState applicationState, IAutomationService automationService,
            IUserService userService, ISettingService settingService, IPresentationCacheService presentationCacheService, IAccountService accountService,
            ICacheService cacheService)
        {
            _ticketDao = ticketDao;
            _applicationState = applicationState;
            _automationService = automationService;
            _userService = userService;
            _settingService = settingService;
            _presentationCacheService = presentationCacheService;
            _accountService = accountService;
            _cacheService = cacheService;
        }

        public decimal GetExchangeRate(Account account)
        {
            if (account.ForeignCurrencyId == 0) return 1;
            return _cacheService.GetCurrencyById(account.ForeignCurrencyId).ExchangeRate;
        }

        public void UpdateResource(Ticket ticket, int resourceTypeId, int resourceId, string resourceName, int accountId, string resourceCustomData)
        {
            var currentResource = ticket.TicketResources.SingleOrDefault(x => x.ResourceTypeId == resourceTypeId);
            var currentResourceId = currentResource != null ? currentResource.ResourceId : 0;
            var newResourceName = resourceName;
            var oldResourceName = currentResource != null ? currentResource.ResourceName : "";
            if (currentResource != null && currentResource.ResourceId != resourceId)
            {
                var resourceType = _presentationCacheService.GetResourceTypeById(currentResource.ResourceTypeId);
                _automationService.NotifyEvent(RuleEventNames.ResourceUpdated, new
                {
                    currentResource.ResourceTypeId,
                    currentResource.ResourceId,
                    ResourceTypeName = resourceType.Name,
                    OpenTicketCount = GetOpenTicketIds(currentResource.ResourceId).Count() - (ticket.Id > 0 ? 1 : 0)
                });
            }

            ticket.UpdateResource(resourceTypeId, resourceId, resourceName, accountId, resourceCustomData);

            if (currentResourceId != resourceId)
            {
                _automationService.NotifyEvent(RuleEventNames.TicketResourceChanged,
                    new
                    {
                        Ticket = ticket,
                        ResourceTypeId = resourceTypeId,
                        ResourceId = resourceId,
                        OldResourceName = oldResourceName,
                        NewResourceName = newResourceName,
                        OrderCount = ticket.Orders.Count
                    });
            }
        }

        public void UpdateResource(Ticket ticket, Resource resource)
        {
            if (resource == null) return;
            UpdateResource(ticket, resource.ResourceTypeId, resource.Id, resource.Name, resource.AccountId, resource.CustomData);
        }

        public Ticket OpenTicket(int ticketId)
        {
            Debug.Assert(_applicationState.CurrentDepartment != null);

            var ticket = ticketId == 0
                             ? CreateTicket()
                             : _ticketDao.OpenTicket(ticketId);

            _automationService.NotifyEvent(RuleEventNames.TicketOpened, new { Ticket = ticket, OrderCount = ticket.Orders.Count });

            return ticket;
        }

        private Ticket CreateTicket()
        {
            var account = _presentationCacheService.GetAccountById(_applicationState.CurrentTicketType.SaleTransactionType.DefaultTargetAccountId);
            return Ticket.Create(_applicationState.CurrentDepartment.Model, _applicationState.CurrentTicketType, account, GetExchangeRate(account), _presentationCacheService.GetCalculationSelectors().Where(x => string.IsNullOrEmpty(x.ButtonHeader)).SelectMany(y => y.CalculationTypes));
        }

        public TicketCommitResult CloseTicket(Ticket ticket)
        {
            var result = _ticketDao.CheckConcurrency(ticket);
            Debug.Assert(ticket != null);
            var changed = !string.IsNullOrEmpty(result.ErrorMessage);
            var canSumbitTicket = !changed && ticket.CanSubmit; // Fişi kaydedebilmek için gün sonu yapılmamış ve fişin ödenmemiş olması gerekir.

            if (canSumbitTicket)
            {
                RecalculateTicket(ticket);
                ticket.Close();

                if (ticket.Orders.Count > 0)
                {
                    var ticketType = _presentationCacheService.GetTicketTypeById(ticket.TicketTypeId);

                    if (ticket.Orders.FirstOrDefault(x => !x.Locked) != null)
                    {
                        var number = _settingService.GetNextNumber(ticketType.OrderNumerator.Id);
                        ticket.MergeOrdersAndUpdateOrderNumbers(number);
                        ticket.Orders.Where(x => x.Id == 0).ToList().ForEach(x => x.CreatedDateTime = DateTime.Now);
                    }

                    if (ticket.Id == 0)
                    {
                        UpdateTicketNumber(ticket, ticketType.TicketNumerator);
                        ticket.LastOrderDate = DateTime.Now;
                        _ticketDao.Save(ticket);
                    }

                    Debug.Assert(!string.IsNullOrEmpty(ticket.TicketNumber));
                    Debug.Assert(ticket.Id > 0);
                    _automationService.NotifyEvent(RuleEventNames.TicketClosing, new { Ticket = ticket, TicketId = ticket.Id, NewOrderCount = ticket.GetUnlockedOrders().Count() });
                    ticket.LockTicket();
                }

                if (ticket.IsClosed)
                    ticket.TransactionDocument.AccountTransactions.Where(x => x.Amount == 0).ToList().ForEach(x => ticket.TransactionDocument.AccountTransactions.Remove(x));

                if (ticket.Id > 0)// eğer adisyonda satır yoksa ID burada 0 olmalı.
                    _ticketDao.Save(ticket);

                Debug.Assert(ticket.Orders.Count(x => x.OrderNumber == 0) == 0);
            }

            if (ticket.Id > 0)
            {
                foreach (var ticketResource in ticket.TicketResources)
                {
                    var resourceType = _presentationCacheService.GetResourceTypeById(ticketResource.ResourceTypeId);
                    _automationService.NotifyEvent(RuleEventNames.ResourceUpdated, new
                                                                                       {
                                                                                           ticketResource.ResourceTypeId,
                                                                                           ticketResource.ResourceId,
                                                                                           ResourceTypeName = resourceType.Name,
                                                                                           OpenTicketCount = GetOpenTicketIds(ticketResource.ResourceId).Count()
                                                                                       });
                }
            }

            result.TicketId = ticket.Id;
            return result;
        }

        public void AddPayment(Ticket ticket, PaymentType paymentType, Account account, decimal tenderedAmount)
        {
            if (account == null) return;
            var remainingAmount = ticket.GetRemainingAmount();
            var changeAmount = tenderedAmount > remainingAmount ? tenderedAmount - remainingAmount : 0;
            ticket.AddPayment(paymentType, account, tenderedAmount, GetExchangeRate(account), _applicationState.CurrentLoggedInUser.Id);
            _automationService.NotifyEvent(RuleEventNames.PaymentProcessed,
                new
                {
                    Ticket = ticket,
                    PaymentTypeName = paymentType.Name,
                    Tenderedamount = tenderedAmount,
                    ProcessedAmount = tenderedAmount - changeAmount,
                    ChangeAmount = changeAmount,
                    RemainingAmount = ticket.GetRemainingAmount()
                });
        }

        public void AddChangePayment(Ticket ticket, ChangePaymentType paymentType, Account account, decimal amount)
        {
            if (account == null) return;
            ticket.AddChangePayment(paymentType, account, amount, GetExchangeRate(account), _applicationState.CurrentLoggedInUser.Id);
        }

        public void PayTicket(Ticket ticket, PaymentType template)
        {
            AddPayment(ticket, template, template.Account, ticket.GetRemainingAmount());
        }

        public void UpdateTicketNumber(Ticket ticket, Numerator numerator)
        {
            if (string.IsNullOrEmpty(ticket.TicketNumber))
            {
                ticket.TicketNumber = _settingService.GetNextString(numerator.Id);
            }
        }

        public TicketCommitResult MergeTickets(IEnumerable<int> ticketIds)
        {
            var ticketList = ticketIds.Select(OpenTicket).ToList();

            if (ticketList.Any(x => x.Calculations.Any()))
                return new TicketCommitResult { ErrorMessage = string.Format("Can't merge tickets\r{0}", "contains calculations") };

            var resourcesUnMatches = ticketList.SelectMany(x => x.TicketResources).GroupBy(x => x.ResourceTypeId).Any(x => x.Select(y => y.ResourceId).Distinct().Count() > 1);
            if (resourcesUnMatches) return new TicketCommitResult { ErrorMessage = string.Format("Can't merge tickets\r{0}", "Resources doesn't match") };

            var clonedOrders = ticketList.SelectMany(x => x.Orders).Select(ObjectCloner.Clone).ToList();
            var clonedPayments = ticketList.SelectMany(x => x.Payments).Select(ObjectCloner.Clone2).ToList();
            var clonedChangePayments = ticketList.SelectMany(x => x.ChangePayments).Select(ObjectCloner.Clone2).ToList();
            var clonedTags = ticketList.SelectMany(x => x.GetTicketTagValues()).Select(ObjectCloner.Clone).ToList();
            var clonedResources = ticketList.SelectMany(x => x.TicketResources).Select(ObjectCloner.Clone).ToList();

            ticketList.ForEach(x => x.Orders.ToList().ForEach(x.RemoveOrder));
            ticketList.ForEach(x => x.Payments.ToList().ForEach(x.RemovePayment));
            ticketList.ForEach(x => x.ChangePayments.ToList().ForEach(x.RemoveChangePayment));
            ticketList.ForEach(x => x.Calculations.ToList().ForEach(x.RemoveCalculation));

            ticketList.ForEach(x => CloseTicket(x));

            var ticket = CreateTicket();
            clonedOrders.ForEach(ticket.Orders.Add);
            foreach (var cp in clonedPayments)
            {
                var account = _accountService.GetAccountById(cp.AccountTransaction.TargetTransactionValue.AccountId);
                ticket.AddPayment(_cacheService.GetPaymentTypeById(cp.PaymentTypeId), account, cp.Amount, GetExchangeRate(account), 0);
            }
            foreach (var cp in clonedChangePayments)
            {
                var account = _accountService.GetAccountById(cp.AccountTransaction.TargetTransactionValue.AccountId);
                ticket.AddChangePayment(_presentationCacheService.GetChangePaymentTypeById(cp.ChangePaymentTypeId), account, cp.Amount, GetExchangeRate(account), 0);
            }

            clonedResources.ForEach(x => ticket.UpdateResource(x.ResourceTypeId, x.ResourceId, x.ResourceName, x.AccountId, x.ResourceCustomData));
            clonedTags.ForEach(x => ticket.SetTagValue(x.TagName, x.TagValue));

            foreach (var template in from order in clonedOrders.GroupBy(x => x.AccountTransactionTypeId)
                                     where ticket.TransactionDocument.AccountTransactions.All(x => x.AccountTransactionTypeId != order.Key)
                                     select _cacheService.GetAccountTransactionTypeById(order.Key))
            {
                ticket.TransactionDocument.AddNewTransaction(template, ticket.AccountTypeId, ticket.AccountId);
            }

            var result = CloseTicket(ticket);
            _automationService.NotifyEvent(RuleEventNames.TicketsMerged, new { Ticket = ticket });
            return result;
        }

        public TicketCommitResult MoveOrders(Ticket ticket, Order[] selectedOrders, int targetTicketId)
        {
            var clonedOrders = selectedOrders.Select(ObjectCloner.Clone2).ToList();
            ticket.RemoveOrders(selectedOrders);

            CloseTicket(ticket);
            ticket = OpenTicket(targetTicketId);

            foreach (var clonedOrder in clonedOrders)
            {
                clonedOrder.TicketId = 0;
                ticket.Orders.Add(clonedOrder);
            }

            foreach (var template in from order in clonedOrders.GroupBy(x => x.AccountTransactionTypeId)
                                     where ticket.TransactionDocument.AccountTransactions.All(x => x.AccountTransactionTypeId != order.Key)
                                     select _cacheService.GetAccountTransactionTypeById(order.Key))
            {
                ticket.TransactionDocument.AddNewTransaction(template, ticket.AccountTypeId, ticket.AccountId);
            }

            ticket.LastOrderDate = DateTime.Now;
            return CloseTicket(ticket);
        }

        public void RecalculateTicket(Ticket ticket)
        {
            var total = ticket.TotalAmount;
            ticket.Recalculate();
            if (total != ticket.TotalAmount)
            {
                _automationService.NotifyEvent(RuleEventNames.TicketTotalChanged,
                    new
                    {
                        Ticket = ticket,
                        PreviousTotal = total,
                        TicketTotal = ticket.GetSum(),
                        DiscountTotal = ticket.GetPreTaxServicesTotal(),
                        PaymentTotal = ticket.GetPaymentAmount()
                    });
            }
        }

        public void RegenerateTaxRates(Ticket ticket)
        {
            foreach (var o in ticket.Orders)
            {
                var order = o;
                var mi = _cacheService.GetMenuItem(x => x.Id == order.MenuItemId);
                if (mi == null) continue;
                var portion = mi.Portions.FirstOrDefault(x => x.Name == order.PortionName);
                if (portion != null) order.UpdatePortion(portion, order.PriceTag, mi.TaxTemplate);
            }
        }

        public void UpdateTag(Ticket ticket, TicketTagGroup tagGroup, TicketTag ticketTag)
        {
            ticket.SetTagValue(tagGroup.Name, ticketTag.Name);

            if (tagGroup.FreeTagging && tagGroup.SaveFreeTags)
            {
                SaveFreeTicketTag(tagGroup.Id, ticketTag.Name);
            }

            var tagData = new TicketTagData
            {
                Ticket = ticket,
                TicketTagGroup = tagGroup,
                TagName = tagGroup.Name,
                TagValue = ticketTag.Name
            };

            _automationService.NotifyEvent(RuleEventNames.TicketTagSelected,
                        new
                        {
                            Ticket = ticket,
                            tagData.TagName,
                            tagData.TagValue,
                            NumericValue = tagGroup.IsNumeric ? Convert.ToDecimal(ticketTag.Name) : 0,
                            TicketTag = ticket.GetTagData()
                        });

            tagData.PublishEvent(EventTopicNames.TicketTagSelected);
        }

        public int GetOpenTicketCount()
        {
            return _ticketDao.GetOpenTicketCount();
        }

        public IEnumerable<int> GetOpenTicketIds(int resourceId)
        {
            return _ticketDao.GetOpenTicketIds(resourceId);
        }

        public IEnumerable<OpenTicketData> GetOpenTickets(int resourceId)
        {
            return GetOpenTickets(x => x.State < 2 && x.TicketResources.Any(y => y.ResourceId == resourceId));
        }

        public IEnumerable<OpenTicketData> GetOpenTickets(Expression<Func<Ticket, bool>> prediction)
        {
            return _ticketDao.GetOpenTickets(prediction);
        }

        public void SaveFreeTicketTag(int tagGroupId, string freeTag)
        {
            _ticketDao.SaveFreeTicketTag(tagGroupId, freeTag);
            _presentationCacheService.ResetTicketTagCache();
        }

        public IEnumerable<Ticket> GetFilteredTickets(DateTime startDate, DateTime endDate, IList<ITicketExplorerFilter> filters)
        {
            return _ticketDao.GetFilteredTickets(startDate, endDate, filters);
        }

        public IList<ITicketExplorerFilter> CreateTicketExplorerFilters()
        {
            var item = new TicketExplorerFilter(_presentationCacheService) { FilterType = Resources.OnlyOpenTickets };
            return new List<ITicketExplorerFilter> { item };
        }

        public void UpdateAccountOfOpenTickets(Resource resource)
        {
            var openTicketDataList = GetOpenTickets(resource.Id);
            using (var w = WorkspaceFactory.Create())
            {
                foreach (var ticket in openTicketDataList.Select(data => w.Single<Ticket>(x => x.Id == data.Id, x => x.TicketResources)))
                {
                    ticket.TicketResources.Where(x => x.ResourceId == resource.Id).ToList().ForEach(x => x.AccountId = resource.AccountId);
                    w.CommitChanges();
                }
            }
        }

        public IEnumerable<Order> GetOrders(int ticketId)
        {
            return _ticketDao.GetOrders(ticketId);
        }

        public void TagOrders(Ticket ticket, IEnumerable<Order> selectedOrders, OrderTagGroup orderTagGroup, OrderTag orderTag, string tagNote)
        {
            var so = selectedOrders.ToList();

            if (orderTagGroup.MaxSelectedItems == 1)
            {
                foreach (var order in so.Where(x => x.OrderTagValues.Any(y => y.OrderTagGroupId == orderTagGroup.Id && y.TagValue != orderTag.Name)))
                {
                    var orderTagValue = order.OrderTagValues.First(x => x.OrderTagGroupId == orderTagGroup.Id);
                    order.OrderTagValues.Remove(orderTagValue);
                    _automationService.NotifyEvent(RuleEventNames.OrderUntagged,
                               new
                               {
                                   Ticket = ticket,
                                   Order = order,
                                   OrderTagName = orderTagGroup.Name,
                                   OrderTagValue = orderTagValue.TagValue
                               });

                }
            }

            foreach (var selectedOrder in so)
            {
                var result = selectedOrder.ToggleOrderTag(orderTagGroup, orderTag, _applicationState.CurrentLoggedInUser.Id, tagNote);

                if (orderTagGroup.SaveFreeTags && orderTagGroup.OrderTags.All(x => x.Name != orderTag.Name))
                {
                    _ticketDao.SaveFreeOrderTag(orderTagGroup.Id, orderTag);
                    _presentationCacheService.ResetOrderTagCache();
                }
                _automationService.NotifyEvent(result ? RuleEventNames.OrderTagged : RuleEventNames.OrderUntagged,
                new
                {
                    Ticket = ticket,
                    Order = selectedOrder,
                    OrderTagName = orderTagGroup.Name,
                    OrderTagValue = orderTag.Name
                });
            }
        }

        public void UntagOrders(Ticket ticket, IEnumerable<Order> selectedOrders, OrderTagGroup orderTagGroup, OrderTag orderTag)
        {
            foreach (var selectedOrder in selectedOrders.Where(selectedOrder => selectedOrder.UntagIfTagged(orderTagGroup, orderTag)))
            {
                _automationService.NotifyEvent(RuleEventNames.OrderUntagged,
                                               new
                                                   {
                                                       Ticket = ticket,
                                                       Order = selectedOrder,
                                                       OrderTagName = orderTagGroup.Name,
                                                       OrderTagValue = orderTag.Name
                                                   });
            }
        }

        public bool CanDeselectOrders(IEnumerable<Order> selectedOrders)
        {
            return selectedOrders.All(CanDeselectOrder);
        }

        public bool CanDeselectOrder(Order order)
        {
            if (!order.DecreaseInventory) return true;
            var ots = _presentationCacheService.GetOrderTagGroups(order.MenuItemId);
            if (order.Locked) ots = ots.Where(x => !string.IsNullOrEmpty(x.ButtonHeader));
            return ots.Where(x => x.MinSelectedItems > 0).All(orderTagGroup => order.OrderTagValues.Count(x => x.OrderTagGroupId == orderTagGroup.Id) >= orderTagGroup.MinSelectedItems);
        }

        public OrderTagGroup GetMandantoryOrderTagGroup(Order order)
        {
            var ots = _presentationCacheService.GetOrderTagGroups(order.MenuItemId);
            if (order.Locked) ots = ots.Where(x => !string.IsNullOrEmpty(x.ButtonHeader));
            return ots.Where(x => x.MinSelectedItems > 0).FirstOrDefault(orderTagGroup => order.OrderTagValues.Count(x => x.OrderTagGroupId == orderTagGroup.Id) < orderTagGroup.MinSelectedItems);
        }

        public bool CanCloseTicket(Ticket ticket)
        {
            if (!ticket.IsLocked)
                return CanDeselectOrders(ticket.Orders);
            return true;
        }

        public void RefreshAccountTransactions(Ticket ticket)
        {
            foreach (var template in from order in ticket.Orders.GroupBy(x => x.AccountTransactionTypeId)
                                     where ticket.TransactionDocument.AccountTransactions.All(x => x.AccountTransactionTypeId != order.Key)
                                     select _cacheService.GetAccountTransactionTypeById(order.Key))
            {
                var transaction = ticket.TransactionDocument.AddNewTransaction(template, ticket.AccountTypeId, ticket.AccountId);
                transaction.Reversable = false;
            }
        }

        public void UpdateOrderStates(Ticket ticket, IEnumerable<Order> selectedOrders, OrderStateGroup orderStateGroup, OrderState orderState)
        {
            var so = selectedOrders.ToList();
            var accountTransactionTypeIds = so.GroupBy(x => x.AccountTransactionTypeId).Select(x => x.Key).ToList();
            so.ForEach(x => x.UpdateOrderState(orderStateGroup, orderState, _applicationState.CurrentLoggedInUser.Id));
            accountTransactionTypeIds.Where(x => so.All(y => y.AccountTransactionTypeId != x)).ToList()
                .ForEach(x => ticket.TransactionDocument.AccountTransactions.Where(y => y.AccountTransactionTypeId == x).ToList().ForEach(y => ticket.TransactionDocument.AccountTransactions.Remove(y)));
            RefreshAccountTransactions(ticket);
        }

        public void ChangeOrdersAccountTransactionTypeId(Ticket ticket, IEnumerable<Order> selectedOrders, int accountTransactionTypeId)
        {
            var so = selectedOrders.ToList();
            var accountTransactionTypeIds = so.GroupBy(x => x.AccountTransactionTypeId).Select(x => x.Key).ToList();
            so.ForEach(x => x.AccountTransactionTypeId = accountTransactionTypeId);
            accountTransactionTypeIds.Where(x => so.All(y => y.AccountTransactionTypeId != x)).ToList()
                .ForEach(x => ticket.TransactionDocument.AccountTransactions.Where(y => y.AccountTransactionTypeId == x).ToList().ForEach(y => ticket.TransactionDocument.AccountTransactions.Remove(y)));
            RefreshAccountTransactions(ticket);
        }

        public void AddAccountTransaction(Ticket ticket, Account sourceAccount, Account targetAccount, decimal amount, decimal exchangeRate)
        {
            var transactionType = _cacheService.FindAccountTransactionType(sourceAccount.AccountTypeId, targetAccount.AccountTypeId, sourceAccount.Id, targetAccount.Id);
            if (transactionType != null)
            {
                ticket.TransactionDocument.AddNewTransaction(transactionType, sourceAccount.AccountTypeId, sourceAccount.Id, targetAccount, amount, exchangeRate);
            }
        }

        public Order AddOrder(Ticket ticket, int menuItemId, decimal quantity, string portionName, OrderTagTemplate template)
        {
            if (ticket.IsLocked && !_userService.IsUserPermittedFor(PermissionNames.AddItemsToLockedTickets)) return null;
            if (!ticket.CanSubmit) return null;
            var menuItem = _cacheService.GetMenuItem(x => x.Id == menuItemId);
            var portion = _presentationCacheService.GetMenuItemPortion(menuItemId, portionName);
            if (portion == null) return null;
            var priceTag = _applicationState.CurrentDepartment.PriceTag;
            var productTimer = _presentationCacheService.GetProductTimer(menuItemId);
            var order = ticket.AddOrder(
                _applicationState.CurrentTicketType.SaleTransactionType,
                _applicationState.CurrentLoggedInUser.Name, menuItem, portion, priceTag, productTimer);

            order.Quantity = quantity > 9 ? decimal.Round(quantity / portion.Multiplier, 3, MidpointRounding.AwayFromZero) : quantity;

            if (template != null) template.OrderTagTemplateValues.ToList().ForEach(x => order.ToggleOrderTag(x.OrderTagGroup, x.OrderTag, 0, ""));
            RecalculateTicket(ticket);

            order.PublishEvent(EventTopicNames.OrderAdded);
            _automationService.NotifyEvent(RuleEventNames.TicketLineAdded, new { Ticket = ticket, Order = order, order.MenuItemName });

            return order;
        }

        public IEnumerable<Order> ExtractSelectedOrders(Ticket model, IEnumerable<Order> selectedOrders)
        {
            var selectedItems = selectedOrders.Where(x => x.SelectedQuantity > 0 && x.SelectedQuantity < x.Quantity).ToList();
            var newItems = model.ExtractSelectedOrders(selectedItems);
            return newItems;
        }
    }
}
