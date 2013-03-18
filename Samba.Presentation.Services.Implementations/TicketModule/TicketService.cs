using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data.Serializer;
using Samba.Localization.Properties;
using Samba.Persistance;
using Samba.Persistance.DaoClasses;
using Samba.Persistance.Data;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Services.Implementations.TicketModule
{
    [Export(typeof(ITicketService))]
    public class TicketService : ITicketService
    {
        private readonly ITicketDao _ticketDao;
        private readonly IApplicationState _applicationState;
        private readonly IAutomationService _automationService;
        private readonly IExpressionService _expressionService;
        private readonly IUserService _userService;
        private readonly ISettingService _settingService;
        private readonly IAccountService _accountService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public TicketService(ITicketDao ticketDao, IDepartmentService departmentService, IApplicationState applicationState,
            IAutomationService automationService, IUserService userService, ISettingService settingService, IExpressionService expressionService,
            IAccountService accountService, ICacheService cacheService)
        {
            _ticketDao = ticketDao;
            _expressionService = expressionService;
            _applicationState = applicationState;
            _automationService = automationService;
            _userService = userService;
            _settingService = settingService;
            _accountService = accountService;
            _cacheService = cacheService;
        }

        private decimal GetExchangeRate(Account account)
        {
            if (account.ForeignCurrencyId == 0) return 1;
            return _cacheService.GetCurrencyById(account.ForeignCurrencyId).ExchangeRate;
        }

        public void UpdateEntity(Ticket ticket, int entityTypeId, int entityId, string entityName, int accountTypeId, int accountId, string entityCustomData)
        {
            var currentEntity = ticket.TicketEntities.SingleOrDefault(x => x.EntityTypeId == entityTypeId);
            var currentEntityId = currentEntity != null ? currentEntity.EntityId : 0;
            var newEntityName = entityName;
            var oldEntityName = currentEntity != null ? currentEntity.EntityName : "";

            if (currentEntity != null && currentEntity.EntityId != entityId)
            {
                var entityType = _cacheService.GetEntityTypeById(currentEntity.EntityTypeId);
                _automationService.NotifyEvent(RuleEventNames.EntityUpdated, new
                {
                    EntityTypeId = currentEntity.EntityTypeId,
                    EntityId = currentEntity.EntityId,
                    EntityTypeName = entityType.Name,
                    OpenTicketCount = GetOpenTicketCount(currentEntity.EntityId, ticket.Id)
                });
            }

            ticket.UpdateEntity(entityTypeId, entityId, entityName, accountTypeId, accountId, entityCustomData);

            if (currentEntityId != entityId)
            {
                var entityType = _cacheService.GetEntityTypeById(entityTypeId);
                _automationService.NotifyEvent(RuleEventNames.TicketEntityChanged,
                    new
                    {
                        Ticket = ticket,
                        EntityTypeId = entityTypeId,
                        EntityId = entityId,
                        EntityTypeName = entityType.Name,
                        OldEntityName = oldEntityName,
                        NewEntityName = newEntityName,
                        OrderCount = ticket.Orders.Count
                    });
            }
        }

        private int GetOpenTicketCount(int entityId, int ticketId)
        {
            var ids = GetOpenTicketIds(entityId).ToList();
            if (ticketId > 0 && !ids.Contains(ticketId)) ids.Add(ticketId);
            return ids.Count - (ticketId > 0 ? 1 : 0);
        }

        public void UpdateEntity(Ticket ticket, Entity entity)
        {
            if (entity == null) return;
            var entityType = _cacheService.GetEntityTypeById(entity.EntityTypeId);
            UpdateEntity(ticket, entityType.Id, entity.Id, entity.Name, entityType.AccountTypeId, entity.AccountId, entity.CustomData);
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
            var account = _cacheService.GetAccountById(_applicationState.CurrentTicketType.SaleTransactionType.DefaultTargetAccountId);
            var result = Ticket.Create(
                _applicationState.CurrentDepartment.Model,
                _applicationState.CurrentTicketType,
                account,
                GetExchangeRate(account),
                _applicationState.GetCalculationSelectors().Where(x => string.IsNullOrEmpty(x.ButtonHeader)).SelectMany(y => y.CalculationTypes));
            _automationService.NotifyEvent(RuleEventNames.TicketCreated, new { Ticket = result });
            return result;
        }

        public TicketCommitResult CloseTicket(Ticket ticket)
        {
            var result = _ticketDao.CheckConcurrency(ticket);
            Debug.Assert(ticket != null);
            var changed = !string.IsNullOrEmpty(result.ErrorMessage);
            var canSumbitTicket = !changed && ticket.CanSubmit;

            if (canSumbitTicket)
            {
                RecalculateTicket(ticket);
                ticket.Close();

                if (ticket.Orders.Count > 0)
                {
                    var ticketType = _cacheService.GetTicketTypeById(ticket.TicketTypeId);

                    if (ticket.Orders.FirstOrDefault(x => !x.Locked) != null)
                    {
                        var number = _settingService.GetNextNumber(ticketType.OrderNumerator.Id);
                        ticket.MergeOrdersAndUpdateOrderNumbers(number);
                    }

                    if (ticket.Id == 0)
                    {
                        UpdateTicketNumber(ticket, ticketType.TicketNumerator);
                        _ticketDao.Save(ticket);
                    }

                    Debug.Assert(!string.IsNullOrEmpty(ticket.TicketNumber));
                    Debug.Assert(ticket.Id > 0);
                    _automationService.NotifyEvent(RuleEventNames.TicketClosing, new { Ticket = ticket, TicketId = ticket.Id });
                    ticket.LockTicket();
                }

                ticket.RemoveZeroAmountAccountTransactions();

                if (ticket.Id > 0)// eğer adisyonda satır yoksa ID burada 0 olmalı.
                    _ticketDao.Save(ticket);

                Debug.Assert(ticket.Orders.Count(x => x.OrderNumber == 0) == 0);
            }

            if (ticket.Id > 0)
            {
                foreach (var ticketEntity in ticket.TicketEntities)
                {
                    var entityType = _cacheService.GetEntityTypeById(ticketEntity.EntityTypeId);
                    _automationService.NotifyEvent(RuleEventNames.EntityUpdated, new
                                                                                       {
                                                                                           EntityTypeId = ticketEntity.EntityTypeId,
                                                                                           EntityId = ticketEntity.EntityId,
                                                                                           EntityTypeName = entityType.Name,
                                                                                           OpenTicketCount = GetOpenTicketIds(ticketEntity.EntityId).Count()
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
                    TenderedAmount = tenderedAmount,
                    ProcessedAmount = tenderedAmount - changeAmount,
                    ChangeAmount = changeAmount,
                    SelectedQuantity = ticket.PaidItems.Sum(x => x.Quantity),
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
            ticket.LastOrderDate = DateTime.Now;
        }

        public TicketCommitResult MergeTickets(IEnumerable<int> ticketIds)
        {
            var ticketList = ticketIds.Select(OpenTicket).ToList();

            if (ticketList.Any(x => x.Calculations.Any()))
                return new TicketCommitResult { ErrorMessage = string.Format("Can't merge tickets\r{0}", "contains calculations") };

            var entitiesUnMatches = ticketList.SelectMany(x => x.TicketEntities).GroupBy(x => x.EntityTypeId).Any(x => x.Select(y => y.EntityId).Distinct().Count() > 1);
            if (entitiesUnMatches) return new TicketCommitResult { ErrorMessage = string.Format("Can't merge tickets\r{0}", "Entities doesn't match") };

            var clonedOrders = ticketList.SelectMany(x => x.Orders).Select(ObjectCloner.Clone).ToList();
            var clonedPayments = ticketList.SelectMany(x => x.Payments).Select(ObjectCloner.Clone2).ToList();
            var clonedChangePayments = ticketList.SelectMany(x => x.ChangePayments).Select(ObjectCloner.Clone2).ToList();
            var clonedTags = ticketList.SelectMany(x => x.GetTicketTagValues()).Select(ObjectCloner.Clone).ToList();
            var clonedEntites = ticketList.SelectMany(x => x.TicketEntities).Select(ObjectCloner.Clone).ToList();

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
                ticket.AddChangePayment(_cacheService.GetChangePaymentTypeById(cp.ChangePaymentTypeId), account, cp.Amount, GetExchangeRate(account), 0);
            }

            clonedEntites.ForEach(x => ticket.UpdateEntity(x.EntityTypeId, x.EntityId, x.EntityName, x.AccountTypeId, x.AccountId, x.EntityCustomData));
            clonedTags.ForEach(x => ticket.SetTagValue(x.TagName, x.TagValue));

            RefreshAccountTransactions(ticket);

            _automationService.NotifyEvent(RuleEventNames.TicketsMerged, new { Ticket = ticket });
            return CloseTicket(ticket);
        }

        public TicketCommitResult MoveOrders(Ticket ticket, Order[] selectedOrders, int targetTicketId)
        {
            _automationService.NotifyEvent(RuleEventNames.TicketMoving, new { Ticket = ticket });

            var clonedOrders = selectedOrders.Select(ObjectCloner.Clone2).ToList();
            ticket.RemoveOrders(selectedOrders);

            CloseTicket(ticket);
            ticket = OpenTicket(targetTicketId);

            foreach (var clonedOrder in clonedOrders)
            {
                clonedOrder.TicketId = 0;
                ticket.Orders.Add(clonedOrder);
                _automationService.NotifyEvent(RuleEventNames.OrderMoved, new { Ticket = ticket, Order = clonedOrder, clonedOrder.MenuItemName });
            }

            RefreshAccountTransactions(ticket);
            ticket.LastOrderDate = DateTime.Now;

            _automationService.NotifyEvent(RuleEventNames.TicketMoved, new { Ticket = ticket });

            return CloseTicket(ticket);
        }

        public void RecalculateTicket(Ticket ticket)
        {
            var total = ticket.TotalAmount;
            ticket.Calculations.Where(x => x.CalculationType == 5).ToList().ForEach(
                x => x.Amount = _expressionService.EvalCommand(FunctionNames.Calculation, "_" + x.Name, new { Ticket = ticket }, 0m));
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
                        PaymentTotal = ticket.GetPaymentAmount(),
                        RemainingAmount = ticket.GetRemainingAmount()
                    });
            }
        }

        public void UpdateTicketState(Ticket ticket, string stateName, string currentState, string state, string stateValue, int quantity = 0)
        {
            var sv = ticket.GetStateValue(stateName);
            if (!string.IsNullOrEmpty(currentState) && sv.State != currentState) return;
            if (sv != null && sv.StateName == stateName && sv.StateValue == stateValue && sv.Quantity == quantity && sv.State == state) return;

            ticket.SetStateValue(stateName, state, stateValue, quantity);

            _automationService.NotifyEvent(RuleEventNames.TicketStateUpdated,
            new
            {
                Ticket = ticket,
                StateName = stateName,
                State = state,
                StateValue = stateValue,
                Quantity = quantity,
                TicketState = ticket.GetStateData()
            });
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

        public IEnumerable<int> GetOpenTicketIds(int entityId)
        {
            return _ticketDao.GetOpenTicketIds(entityId);
        }

        public IEnumerable<OpenTicketData> GetOpenTickets(int entityId)
        {
            return GetOpenTickets(x => !x.IsClosed && x.TicketEntities.Any(y => y.EntityId == entityId));
        }

        public IEnumerable<OpenTicketData> GetOpenTickets(Expression<Func<Ticket, bool>> prediction)
        {
            return _ticketDao.GetOpenTickets(prediction);
        }

        public void SaveFreeTicketTag(int tagGroupId, string freeTag)
        {
            _ticketDao.SaveFreeTicketTag(tagGroupId, freeTag);
            _cacheService.ResetTicketTagCache();
        }

        public IEnumerable<Ticket> GetFilteredTickets(DateTime startDate, DateTime endDate, IList<ITicketExplorerFilter> filters)
        {
            return _ticketDao.GetFilteredTickets(startDate, endDate, filters);
        }

        public IList<ITicketExplorerFilter> CreateTicketExplorerFilters()
        {
            var item = new TicketExplorerFilter(_cacheService) { FilterType = Resources.OnlyOpenTickets };
            return new List<ITicketExplorerFilter> { item };
        }

        public void UpdateAccountOfOpenTickets(Entity entity)
        {
            var openTicketDataList = GetOpenTickets(entity.Id).Select(x => x.Id);
            using (var w = WorkspaceFactory.Create())
            {
                var tickets = w.All<Ticket>(x => openTicketDataList.Contains(x.Id), x => x.TicketEntities);
                foreach (var ticket in tickets)
                {
                    ticket.TicketEntities.Where(x => x.EntityId == entity.Id).ToList().ForEach(x =>
                        {
                            var entityType = _cacheService.GetEntityTypeById(x.EntityTypeId);
                            x.AccountTypeId = entityType.AccountTypeId;
                            x.AccountId = entity.AccountId;
                        });
                }
                w.CommitChanges();
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
                foreach (var order in so.Where(x => x.OrderTagExists(y => y.OrderTagGroupId == orderTagGroup.Id && y.TagValue != orderTag.Name)))
                {
                    var orderTagValue = order.GetOrderTagValues().First(x => x.OrderTagGroupId == orderTagGroup.Id);
                    order.UntagOrder(orderTagValue);
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
                    _cacheService.ResetOrderTagCache();
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
            if (!order.DecreaseInventory || order.Locked) return true;
            var ots = _applicationState.GetOrderTagGroups(order.MenuItemId);
            return ots.Where(x => x.MinSelectedItems > 0).All(orderTagGroup => order.GetOrderTagValues(x => x.OrderTagGroupId == orderTagGroup.Id).Count() >= orderTagGroup.MinSelectedItems);
        }

        public OrderTagGroup GetMandantoryOrderTagGroup(Order order)
        {
            if (order.Locked) return null;
            var ots = _applicationState.GetOrderTagGroups(order.MenuItemId);
            return ots.Where(x => x.MinSelectedItems > 0).FirstOrDefault(orderTagGroup => order.GetOrderTagValues(x => x.OrderTagGroupId == orderTagGroup.Id).Count() < orderTagGroup.MinSelectedItems);
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

            foreach (var taxTransactionTemplate in ticket.GetTaxIds().Select(x => _cacheService.GetAccountTransactionTypeById(x)))
            {
                ticket.TransactionDocument.AddSingletonTransaction(taxTransactionTemplate.Id,
                       taxTransactionTemplate,
                       ticket.AccountTypeId, ticket.AccountId);
            }
        }

        public void UpdateOrderStates(Ticket ticket, IList<Order> orders, string stateName, string currentState, int groupOrder, string state, int stateOrder,
                                       string stateValue)
        {
            var so = orders.Where(x => string.IsNullOrEmpty(currentState) || x.IsInState(stateName, currentState)).ToList();
            foreach (var order in so)
            {
                if (order.IsInState(stateName, state) && (string.IsNullOrEmpty(stateValue) || order.IsInState(stateValue))) continue;
                order.SetStateValue(stateName, groupOrder, state, stateOrder, stateValue);
                _automationService.NotifyEvent(RuleEventNames.OrderStateUpdated,
                                               new
                                                   {
                                                       Ticket = ticket,
                                                       Order = order,
                                                       StateName = stateName,
                                                       State = state,
                                                       StateValue = stateValue,
                                                   });
            }
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

        public bool CanMakeAccountTransaction(TicketEntity ticketEntity, AccountTransactionType accountTransactionType, decimal targetBalance)
        {
            if (ticketEntity.AccountId == 0) return false;
            var entityType = _cacheService.GetEntityTypeById(ticketEntity.EntityTypeId);
            var typeId = accountTransactionType.TargetAccountTypeId;
            if (accountTransactionType.DefaultSourceAccountId == 0)
                typeId = accountTransactionType.SourceAccountTypeId;
            var result = entityType.AccountTypeId == typeId;
            if (result)
            {
                var accountType = _cacheService.GetAccountTypeById(entityType.AccountTypeId);
                if (accountType.WorkingRule != 0)
                {
                    if (accountType.WorkingRule == 1 && targetBalance < 0) return false; //disallow credit
                    if (accountType.WorkingRule == 2 && targetBalance > ticketEntity.GetCustomDataAsDecimal(Resources.CreditLimit)) return false; //disallow debit
                }
            }
            return result;
        }


        public Order AddOrder(Ticket ticket, int menuItemId, decimal quantity, string portionName, OrderTagTemplate template)
        {
            if (ticket.IsLocked && !_userService.IsUserPermittedFor(PermissionNames.AddItemsToLockedTickets)) return null;
            if (!ticket.CanSubmit) return null;
            var menuItem = _cacheService.GetMenuItem(x => x.Id == menuItemId);
            var portion = _cacheService.GetMenuItemPortion(menuItemId, portionName);
            if (portion == null) return null;
            var priceTag = _applicationState.CurrentDepartment.PriceTag;
            var productTimer = _applicationState.GetProductTimer(menuItemId);
            var order = ticket.AddOrder(
                _applicationState.CurrentTicketType.SaleTransactionType,
                _applicationState.CurrentDepartment.Model,
                _applicationState.CurrentLoggedInUser.Name, menuItem,
                _applicationState.GetTaxTemplates(menuItem.Id).ToList(), portion, priceTag, productTimer);

            order.Quantity = quantity > 9 ? decimal.Round(quantity / portion.Multiplier, 3, MidpointRounding.AwayFromZero) : quantity;

            if (template != null) template.OrderTagTemplateValues.ToList().ForEach(x => order.ToggleOrderTag(x.OrderTagGroup, x.OrderTag, 0, ""));
            RecalculateTicket(ticket);

            order.PublishEvent(EventTopicNames.OrderAdded);
            _automationService.NotifyEvent(RuleEventNames.OrderAdded, new { Ticket = ticket, Order = order, order.MenuItemName });

            return order;
        }
    }
}
