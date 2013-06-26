using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Automation;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Helpers;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services;

namespace Samba.Presentation.Services.Common
{
    public class DataCreationService
    {
        private readonly IWorkspace _workspace;

        public DataCreationService()
        {
            _workspace = WorkspaceFactory.Create();
        }

        private bool ShouldCreateData()
        {
            return _workspace.Count<User>() == 0;
        }

        public void CreateData()
        {
            CreateDefaultCurrenciesIfNeeded();

            if (!ShouldCreateData()) return;

            var saleAccountType = new AccountType { Name = string.Format(Resources.Accounts_f, Resources.Sales) };
            var receivableAccountType = new AccountType { Name = string.Format(Resources.Accounts_f, Resources.Receiveable) };
            var paymentAccountType = new AccountType { Name = string.Format(Resources.Accounts_f, Resources.Payment) };
            var discountAccountType = new AccountType { Name = string.Format(Resources.Accounts_f, Resources.Discount) };
            var customerAccountType = new AccountType { Name = string.Format(Resources.Accounts_f, Resources.Customer) };

            _workspace.Add(receivableAccountType);
            _workspace.Add(saleAccountType);
            _workspace.Add(paymentAccountType);
            _workspace.Add(discountAccountType);
            _workspace.Add(customerAccountType);
            _workspace.CommitChanges();

            var customerEntityType = new EntityType { Name = Resources.Customers, EntityName = Resources.Customer, AccountTypeId = customerAccountType.Id };
            customerEntityType.EntityCustomFields.Add(new EntityCustomField { EditingFormat = "(###) ### ####", FieldType = 0, Name = Resources.Phone });
            customerEntityType.AccountNameTemplate = "[Name]-[" + Resources.Phone + "]";
            var tableEntityType = new EntityType { Name = Resources.Tables, EntityName = Resources.Table };

            _workspace.Add(customerEntityType);
            _workspace.Add(tableEntityType);

            _workspace.CommitChanges();

            var accountScreen = new AccountScreen { Name = Resources.General };
            accountScreen.AccountScreenValues.Add(new AccountScreenValue { AccountTypeName = saleAccountType.Name, AccountTypeId = saleAccountType.Id, DisplayDetails = true });
            accountScreen.AccountScreenValues.Add(new AccountScreenValue { AccountTypeName = receivableAccountType.Name, AccountTypeId = receivableAccountType.Id, DisplayDetails = true });
            accountScreen.AccountScreenValues.Add(new AccountScreenValue { AccountTypeName = discountAccountType.Name, AccountTypeId = discountAccountType.Id, DisplayDetails = true });
            accountScreen.AccountScreenValues.Add(new AccountScreenValue { AccountTypeName = paymentAccountType.Name, AccountTypeId = paymentAccountType.Id, DisplayDetails = true });
            _workspace.Add(accountScreen);

            var defaultSaleAccount = new Account { AccountTypeId = saleAccountType.Id, Name = Resources.Sales };
            var defaultReceivableAccount = new Account { AccountTypeId = receivableAccountType.Id, Name = Resources.Receivables };
            var cashAccount = new Account { AccountTypeId = paymentAccountType.Id, Name = Resources.Cash };
            var creditCardAccount = new Account { AccountTypeId = paymentAccountType.Id, Name = Resources.CreditCard };
            var voucherAccount = new Account { AccountTypeId = paymentAccountType.Id, Name = Resources.Voucher };
            var defaultDiscountAccount = new Account { AccountTypeId = discountAccountType.Id, Name = Resources.Discount };
            var defaultRoundingAccount = new Account { AccountTypeId = discountAccountType.Id, Name = Resources.Rounding };

            _workspace.Add(defaultSaleAccount);
            _workspace.Add(defaultReceivableAccount);
            _workspace.Add(defaultDiscountAccount);
            _workspace.Add(defaultRoundingAccount);
            _workspace.Add(cashAccount);
            _workspace.Add(creditCardAccount);
            _workspace.Add(voucherAccount);

            _workspace.CommitChanges();

            var discountTransactionType = new AccountTransactionType
            {
                Name = string.Format(Resources.Transaction_f, Resources.Discount),
                SourceAccountTypeId = receivableAccountType.Id,
                TargetAccountTypeId = discountAccountType.Id,
                DefaultSourceAccountId = defaultReceivableAccount.Id,
                DefaultTargetAccountId = defaultDiscountAccount.Id
            };

            var roundingTransactionType = new AccountTransactionType
            {
                Name = string.Format(Resources.Transaction_f, Resources.Rounding),
                SourceAccountTypeId = receivableAccountType.Id,
                TargetAccountTypeId = discountAccountType.Id,
                DefaultSourceAccountId = defaultReceivableAccount.Id,
                DefaultTargetAccountId = defaultRoundingAccount.Id
            };

            var saleTransactionType = new AccountTransactionType
            {
                Name = string.Format(Resources.Transaction_f, Resources.Sale),
                SourceAccountTypeId = saleAccountType.Id,
                TargetAccountTypeId = receivableAccountType.Id,
                DefaultSourceAccountId = defaultSaleAccount.Id,
                DefaultTargetAccountId = defaultReceivableAccount.Id
            };

            var paymentTransactionType = new AccountTransactionType
            {
                Name = string.Format(Resources.Transaction_f, Resources.Payment),
                SourceAccountTypeId = receivableAccountType.Id,
                TargetAccountTypeId = paymentAccountType.Id,
                DefaultSourceAccountId = defaultReceivableAccount.Id,
                DefaultTargetAccountId = cashAccount.Id
            };

            var customerAccountTransactionType = new AccountTransactionType
            {
                Name = string.Format(Resources.Customer_f, Resources.AccountTransaction),
                SourceAccountTypeId = receivableAccountType.Id,
                TargetAccountTypeId = customerAccountType.Id,
                DefaultSourceAccountId = defaultReceivableAccount.Id
            };

            var customerCashPaymentType = new AccountTransactionType
            {
                Name = string.Format(Resources.Customer_f, Resources.CashPayment),
                SourceAccountTypeId = customerAccountType.Id,
                TargetAccountTypeId = paymentAccountType.Id,
                DefaultTargetAccountId = cashAccount.Id
            };

            var customerCreditCardPaymentType = new AccountTransactionType
            {
                Name = string.Format(Resources.Customer_f, Resources.CreditCardPayment),
                SourceAccountTypeId = customerAccountType.Id,
                TargetAccountTypeId = paymentAccountType.Id,
                DefaultTargetAccountId = creditCardAccount.Id
            };

            _workspace.Add(saleTransactionType);
            _workspace.Add(paymentTransactionType);
            _workspace.Add(discountTransactionType);
            _workspace.Add(roundingTransactionType);
            _workspace.Add(customerAccountTransactionType);
            _workspace.Add(customerCashPaymentType);
            _workspace.Add(customerCreditCardPaymentType);

            var customerCashDocument = new AccountTransactionDocumentType
            {
                Name = string.Format(Resources.Customer_f, Resources.Cash),
                ButtonHeader = Resources.Cash,
                DefaultAmount = string.Format("[{0}]", Resources.Balance),
                DescriptionTemplate = string.Format(Resources.Payment_f, Resources.Cash),
                MasterAccountTypeId = customerAccountType.Id
            };
            customerCashDocument.AddAccountTransactionDocumentTypeMap();
            customerCashDocument.TransactionTypes.Add(customerCashPaymentType);

            var customerCreditCardDocument = new AccountTransactionDocumentType
            {
                Name = string.Format(Resources.Customer_f, Resources.CreditCard),
                ButtonHeader = Resources.CreditCard,
                DefaultAmount = string.Format("[{0}]", Resources.Balance),
                DescriptionTemplate = string.Format(Resources.Payment_f, Resources.CreditCard),
                MasterAccountTypeId = customerAccountType.Id
            };
            customerCreditCardDocument.AddAccountTransactionDocumentTypeMap();
            customerCreditCardDocument.TransactionTypes.Add(customerCreditCardPaymentType);

            _workspace.Add(customerCashDocument);
            _workspace.Add(customerCreditCardDocument);

            var discountService = new CalculationType
            {
                AccountTransactionType = discountTransactionType,
                CalculationMethod = 0,
                DecreaseAmount = true,
                Name = Resources.Discount
            };

            var roundingService = new CalculationType
            {
                AccountTransactionType = roundingTransactionType,
                CalculationMethod = 2,
                DecreaseAmount = true,
                IncludeTax = true,
                Name = Resources.Round
            };

            var discountSelector = new CalculationSelector { Name = Resources.Discount, ButtonHeader = Resources.DiscountPercentSign };
            discountSelector.CalculationTypes.Add(discountService);
            discountSelector.AddCalculationSelectorMap();

            var roundingSelector = new CalculationSelector { Name = Resources.Round, ButtonHeader = Resources.Round };
            roundingSelector.CalculationTypes.Add(roundingService);
            roundingSelector.AddCalculationSelectorMap();


            _workspace.Add(discountService);
            _workspace.Add(roundingService);
            _workspace.Add(discountSelector);
            _workspace.Add(roundingSelector);

            var screen = new ScreenMenu();
            _workspace.Add(screen);

            var ticketNumerator = new Numerator { Name = Resources.TicketNumerator };
            _workspace.Add(ticketNumerator);

            var orderNumerator = new Numerator { Name = Resources.OrderNumerator };
            _workspace.Add(orderNumerator);

            var printBillAutomation = new AutomationCommand { Name = Resources.PrintBill, ButtonHeader = Resources.PrintBill };
            printBillAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = Resources.NewOrders + "," + Resources.Unpaid + ",IsClosed", VisibleStates = "*", DisplayOnTicket = true, DisplayOnPayment = true });
            _workspace.Add(printBillAutomation);

            var unlockTicketAutomation = new AutomationCommand { Name = Resources.UnlockTicket, ButtonHeader = Resources.UnlockTicket };
            unlockTicketAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = Resources.Locked, VisibleStates = Resources.Locked, DisplayOnTicket = true });
            _workspace.Add(unlockTicketAutomation);

            var addTicketAutomation = new AutomationCommand { Name = string.Format(Resources.Add_f, Resources.Ticket), ButtonHeader = string.Format(Resources.Add_f, Resources.Ticket) };
            addTicketAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = string.Format("{0},{1}", Resources.Unpaid, Resources.Locked), VisibleStates = "*", DisplayOnTicket = true });
            _workspace.Add(addTicketAutomation);

            var giftItemAutomation = new AutomationCommand { Name = Resources.Gift, ButtonHeader = Resources.Gift };
            giftItemAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = "GStatus=", VisibleStates = "GStatus=", DisplayOnOrders = true });
            _workspace.Add(giftItemAutomation);

            var cancelGiftItemAutomation = new AutomationCommand { Name = string.Format(Resources.Cancel_f, Resources.Gift), ButtonHeader = string.Format(Resources.Cancel_f, Resources.Gift) };
            cancelGiftItemAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = Resources.Gift, VisibleStates = Resources.Gift, DisplayOnOrders = true });
            _workspace.Add(cancelGiftItemAutomation);

            var voidItemAutomation = new AutomationCommand { Name = Resources.Void, ButtonHeader = Resources.Void };
            voidItemAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = string.Format("GStatus={0}," + Resources.Submitted, Resources.Gift), VisibleStates = string.Format("GStatus=,GStatus={0}", Resources.Gift), DisplayOnOrders = true });
            _workspace.Add(voidItemAutomation);

            var cancelVoidItemAutomation = new AutomationCommand { Name = string.Format(Resources.Cancel_f, Resources.Void), ButtonHeader = string.Format(Resources.Cancel_f, Resources.Void) };
            cancelVoidItemAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { EnabledStates = Resources.New, VisibleStates = Resources.Void, DisplayOnOrders = true });
            _workspace.Add(cancelVoidItemAutomation);


            _workspace.CommitChanges();

            var ticketType = new TicketType
                                     {
                                         Name = Resources.Ticket,
                                         TicketNumerator = ticketNumerator,
                                         OrderNumerator = orderNumerator,
                                         SaleTransactionType = saleTransactionType,
                                         ScreenMenuId = screen.Id,
                                     };

            ticketType.EntityTypeAssignments.Add(new EntityTypeAssignment { EntityTypeId = tableEntityType.Id, EntityTypeName = tableEntityType.Name, SortOrder = 10 });
            ticketType.EntityTypeAssignments.Add(new EntityTypeAssignment { EntityTypeId = customerEntityType.Id, EntityTypeName = customerEntityType.Name, SortOrder = 20 });

            var cashPayment = new PaymentType
            {
                AccountTransactionType = paymentTransactionType,
                Account = cashAccount,
                Name = cashAccount.Name
            };
            cashPayment.PaymentTypeMaps.Add(new PaymentTypeMap { DisplayAtPaymentScreen = true });

            var creditCardPayment = new PaymentType
            {
                AccountTransactionType = paymentTransactionType,
                Account = creditCardAccount,
                Name = creditCardAccount.Name
            };
            creditCardPayment.PaymentTypeMaps.Add(new PaymentTypeMap { DisplayAtPaymentScreen = true });

            var voucherPayment = new PaymentType
            {
                AccountTransactionType = paymentTransactionType,
                Account = voucherAccount,
                Name = voucherAccount.Name
            };
            voucherPayment.PaymentTypeMaps.Add(new PaymentTypeMap { DisplayAtPaymentScreen = true });

            var accountPayment = new PaymentType
            {
                AccountTransactionType = customerAccountTransactionType,
                Name = Resources.CustomerAccount
            };
            accountPayment.PaymentTypeMaps.Add(new PaymentTypeMap { DisplayAtPaymentScreen = true });

            _workspace.Add(cashPayment);
            _workspace.Add(creditCardPayment);
            _workspace.Add(voucherPayment);
            _workspace.Add(accountPayment);
            _workspace.Add(ticketType);

            var warehouseType = new WarehouseType { Name = Resources.Warehouses };
            _workspace.Add(warehouseType);
            _workspace.CommitChanges();

            var localWarehouse = new Warehouse
            {
                Name = Resources.LocalWarehouse,
                WarehouseTypeId = warehouseType.Id
            };

            _workspace.Add(localWarehouse);
            _workspace.CommitChanges();

            var department = new Department
            {
                Name = Resources.Restaurant,
                TicketTypeId = ticketType.Id,
                WarehouseId = localWarehouse.Id
            };
            _workspace.Add(department);

            var transactionType = new InventoryTransactionType
                                      {
                                          Name = Resources.PurchaseTransactionType,
                                          TargetWarehouseTypeId = warehouseType.Id,
                                          DefaultTargetWarehouseId = localWarehouse.Id
                                      };

            _workspace.Add(transactionType);

            var transactionDocumentType = new InventoryTransactionDocumentType
                {
                    Name = Resources.PurchaseTransaction,
                    InventoryTransactionType = transactionType
                };

            _workspace.Add(transactionDocumentType);

            var role = new UserRole("Admin") { IsAdmin = true, DepartmentId = 1 };
            _workspace.Add(role);

            var u = new User("Administrator", "1234") { UserRole = role };
            _workspace.Add(u);

            var ticketPrinterTemplate = new PrinterTemplate { Name = Resources.TicketTemplate, Template = GetDefaultTicketPrintTemplate() };
            var kitchenPrinterTemplate = new PrinterTemplate { Name = Resources.KitchenOrderTemplate, Template = GetDefaultKitchenPrintTemplate() };

            _workspace.Add(ticketPrinterTemplate);
            _workspace.Add(kitchenPrinterTemplate);

            var printer1 = new Printer { Name = Resources.TicketPrinter };
            var printer2 = new Printer { Name = Resources.KitchenPrinter };
            var printer3 = new Printer { Name = Resources.InvoicePrinter };

            _workspace.Add(printer1);
            _workspace.Add(printer2);
            _workspace.Add(printer3);

            _workspace.CommitChanges();

            var t = new Terminal
            {
                IsDefault = true,
                Name = Resources.Server,
                ReportPrinter = printer1,
            };

            var pm1 = new PrinterMap { PrinterId = printer1.Id, PrinterTemplateId = ticketPrinterTemplate.Id };
            _workspace.Add(pm1);

            var pj1 = new PrintJob
            {
                Name = Resources.PrintBill,
                WhatToPrint = (int)WhatToPrintTypes.Everything,
            };
            pj1.PrinterMaps.Add(pm1);


            _workspace.Add(pj1);

            var pm2 = new PrinterMap { PrinterId = printer2.Id, PrinterTemplateId = kitchenPrinterTemplate.Id };
            var pj2 = new PrintJob
            {
                Name = Resources.PrintOrdersToKitchenPrinter,
                WhatToPrint = (int)WhatToPrintTypes.Everything,
            };
            pj2.PrinterMaps.Add(pm2);

            _workspace.Add(pj2);
            _workspace.Add(t);

            var newOrderState = new State { Name = Resources.NewOrders, Color = "Orange", GroupName = "Status" };
            _workspace.Add(newOrderState);

            var availableState = new State { Name = "Available", Color = "White", GroupName = "Status" };
            _workspace.Add(availableState);

            var billRequestedState = new State { Name = "Bill Requested", Color = "Maroon", GroupName = "Status" };
            _workspace.Add(billRequestedState);

            var updateOrderAction = new AppAction
                                        {
                                            ActionType = ActionNames.UpdateOrder,
                                            Name = "Update Order",
                                            Parameter = Params()
                                                .Add("IncreaseInventory", "[:Increase]").Add("DecreaseInventory", "[:Decrease]")
                                                .Add("CalculatePrice", "[:Calculate Price]").Add("Locked", "[:Locked]").
                                                ToString()
                                        };
            _workspace.Add(updateOrderAction);
            var updateTicketStatusAction = new AppAction { ActionType = ActionNames.UpdateTicketState, Name = Resources.UpdateTicketStatus, Parameter = Params().Add("StateName", Resources.Status).Add("State", "[:Status]").Add("CurrentState", "[:Current Status]").ToString() };
            _workspace.Add(updateTicketStatusAction);
            var updateOrderStatusAction = new AppAction { ActionType = ActionNames.UpdateOrderState, Name = "Update Order Status", Parameter = Params().Add("StateName", Resources.Status).Add("State", "[:Status]").Add("CurrentState", "[:Current Status]").ToString() };
            _workspace.Add(updateOrderStatusAction);
            var updateOrderGiftStatusAction = new AppAction
                                                  {
                                                      ActionType = ActionNames.UpdateOrderState,
                                                      Name = "Update Order Gift State",
                                                      Parameter = Params()
                                                        .Add("StateName", "GStatus").Add("GroupOrder", "1")
                                                        .Add("CurrentState", "[:Current Status]").Add("State", "[:Status]")
                                                        .Add("StateOrder", "1").Add("StateValue", "[:Value]")
                                                      .ToString()
                                                  };
            _workspace.Add(updateOrderGiftStatusAction);

            var updateEntityStateAction = new AppAction { ActionType = ActionNames.UpdateEntityState, Name = "Update Entity State", Parameter = Params().Add("EntityStateName", "Status").Add("EntityState", "[:Status]").ToString() };
            _workspace.Add(updateEntityStateAction);

            var createTicketAction = new AppAction { ActionType = ActionNames.CreateTicket, Name = string.Format(Resources.Create_f, Resources.Ticket), Parameter = "" };
            _workspace.Add(createTicketAction);
            var closeTicketAction = new AppAction { ActionType = ActionNames.CloseActiveTicket, Name = Resources.CloseTicket, Parameter = "" };
            _workspace.Add(closeTicketAction);
            var printBillAction = new AppAction { ActionType = ActionNames.ExecutePrintJob, Name = "Execute Bill Print Job", Parameter = Params().Add("PrintJobName", Resources.PrintBill).ToString() };
            _workspace.Add(printBillAction);
            var printKitchenOrdersAction = new AppAction { ActionType = ActionNames.ExecutePrintJob, Name = "Execute Kitchen Orders Print Job", Parameter = Params().Add("PrintJobName", Resources.PrintOrdersToKitchenPrinter).Add("OrderStateName", Resources.Status).Add("OrderState", Resources.New).ToString() };
            _workspace.Add(printKitchenOrdersAction);
            var lockTicketAction = new AppAction { ActionType = ActionNames.LockTicket, Name = Resources.LockTicket, Parameter = "" };
            _workspace.Add(lockTicketAction);
            var unlockTicketAction = new AppAction { ActionType = ActionNames.UnlockTicket, Name = Resources.UnlockTicket, Parameter = "" };
            _workspace.Add(unlockTicketAction);
            var markTicketAsClosed = new AppAction { ActionType = ActionNames.MarkTicketAsClosed, Name = Resources.MarkTicketAsClosed, Parameter = "" };
            _workspace.Add(markTicketAsClosed);
            _workspace.CommitChanges();

            var newTicketRule = new AppRule { Name = "New Ticket Creating Rule", EventName = RuleEventNames.TicketCreated };
            newTicketRule.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("{0}={1}", Resources.Status, Resources.New) });
            newTicketRule.AddRuleMap();
            _workspace.Add(newTicketRule);

            var newOrderAddingRule = new AppRule { Name = "New Order Adding Rule", EventName = RuleEventNames.OrderAdded };
            newOrderAddingRule.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("Status={0}", Resources.NewOrders) });
            newOrderAddingRule.Actions.Add(new ActionContainer(updateOrderStatusAction) { ParameterValues = string.Format("Status={0}", Resources.New) });
            newOrderAddingRule.AddRuleMap();
            _workspace.Add(newOrderAddingRule);

            var ticketPayCheckRule = new AppRule { Name = "Ticket Payment Check", EventName = RuleEventNames.BeforeTicketClosing, EventConstraints = "RemainingAmount;=;0" };
            ticketPayCheckRule.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = "Status=" + Resources.Paid });
            ticketPayCheckRule.Actions.Add(new ActionContainer(markTicketAsClosed));
            ticketPayCheckRule.AddRuleMap();
            _workspace.Add(ticketPayCheckRule);

            var ticketMovedRule = new AppRule { Name = Resources.TicketMovedRule, EventName = RuleEventNames.TicketMoved };
            ticketMovedRule.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("Status={0}", Resources.NewOrders) });
            ticketMovedRule.AddRuleMap();
            _workspace.Add(ticketMovedRule);

            var ticketClosingRule = new AppRule { Name = string.Format(Resources.Rule_f, Resources.TicketClosing), EventName = RuleEventNames.TicketClosing };
            ticketClosingRule.Actions.Add(new ActionContainer(printKitchenOrdersAction));
            ticketClosingRule.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("Status={0}#Current Status={1}", Resources.Unpaid, Resources.NewOrders) });
            ticketClosingRule.Actions.Add(new ActionContainer(updateOrderStatusAction) { ParameterValues = string.Format("Status={0}#Current Status={1}", Resources.Submitted, Resources.New) });
            ticketClosingRule.AddRuleMap();
            _workspace.Add(ticketClosingRule);

            var giftOrderRule = new AppRule { Name = string.Format(Resources.Rule_f, Resources.Gift), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = string.Format("AutomationCommandName;=;{0}", giftItemAutomation.Name) };
            giftOrderRule.Actions.Add(new ActionContainer(updateOrderAction) { ParameterValues = "Decrease=True#Calculate Price=False" });
            giftOrderRule.Actions.Add(new ActionContainer(updateOrderGiftStatusAction) { ParameterValues = string.Format("Status={0}#Value=[:Value]", Resources.Gift) });
            giftOrderRule.AddRuleMap();
            _workspace.Add(giftOrderRule);

            var cancelGiftOrderRule = new AppRule { Name = string.Format(Resources.Rule_f, string.Format(Resources.Cancel_f, Resources.Gift)), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = string.Format("AutomationCommandName;=;{0}", cancelGiftItemAutomation.Name) };
            cancelGiftOrderRule.Actions.Add(new ActionContainer(updateOrderAction) { ParameterValues = "Decrease=True#Calculate Price=True" });
            cancelGiftOrderRule.Actions.Add(new ActionContainer(updateOrderGiftStatusAction) { ParameterValues = string.Format("Current Status={0}#Status=#Value=", Resources.Gift) });
            cancelGiftOrderRule.AddRuleMap();
            _workspace.Add(cancelGiftOrderRule);

            var voidOrderRule = new AppRule { Name = string.Format(Resources.Rule_f, Resources.Void), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = string.Format("AutomationCommandName;=;{0}", voidItemAutomation.Name) };
            voidOrderRule.Actions.Add(new ActionContainer(updateOrderAction) { ParameterValues = "Decrease=False#Calculate Price=False" });
            voidOrderRule.Actions.Add(new ActionContainer(updateOrderGiftStatusAction) { ParameterValues = string.Format("Status={0}#Value=[:Value]", Resources.Void) });
            voidOrderRule.Actions.Add(new ActionContainer(updateOrderStatusAction) { ParameterValues = string.Format("Status={0}", Resources.New) });
            voidOrderRule.AddRuleMap();
            _workspace.Add(voidOrderRule);

            var cancelVoidOrderRule = new AppRule { Name = string.Format(Resources.Rule_f, string.Format(Resources.Cancel_f, Resources.Void)), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = string.Format("AutomationCommandName;=;{0}", cancelVoidItemAutomation.Name) };
            cancelVoidOrderRule.Actions.Add(new ActionContainer(updateOrderAction) { ParameterValues = "Decrease=True#Calculate Price=True" });
            cancelVoidOrderRule.Actions.Add(new ActionContainer(updateOrderGiftStatusAction) { ParameterValues = string.Format("Current Status={0}#Status=#Value=", Resources.Void) });
            cancelVoidOrderRule.Actions.Add(new ActionContainer(updateOrderStatusAction) { ParameterValues = string.Format("Status={0}", Resources.Submitted) });
            cancelVoidOrderRule.AddRuleMap();
            _workspace.Add(cancelVoidOrderRule);

            var newOrderRule = new AppRule { Name = "Update New Order Entity Color", EventName = RuleEventNames.TicketStateUpdated, EventConstraints = "State;=;" + Resources.Unpaid };
            newOrderRule.Actions.Add(new ActionContainer(updateEntityStateAction) { ParameterValues = "Status=" + Resources.NewOrders });
            newOrderRule.AddRuleMap();
            _workspace.Add(newOrderRule);

            var availableRule = new AppRule { Name = "Update Available Entity Color", EventName = RuleEventNames.EntityUpdated, EventConstraints = "OpenTicketCount;=;0" };
            var ac2 = new ActionContainer(updateEntityStateAction) { ParameterValues = string.Format("Status={0}", "Available") };
            availableRule.Actions.Add(ac2);
            availableRule.AddRuleMap();
            _workspace.Add(availableRule);

            var movingRule = new AppRule { Name = "Update Moved Entity Color", EventName = "TicketEntityChanged", EventConstraints = "OrderCount;>;0" };
            var ac3 = new ActionContainer(updateEntityStateAction) { ParameterValues = string.Format("Status={0}", Resources.NewOrders) };
            movingRule.Actions.Add(ac3);
            movingRule.AddRuleMap();
            _workspace.Add(movingRule);

            var printBillRule = new AppRule { Name = string.Format(Resources.Rule_f, Resources.PrintBill), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = "AutomationCommandName;=;" + Resources.PrintBill };
            printBillRule.Actions.Add(new ActionContainer(printBillAction));
            printBillRule.Actions.Add(new ActionContainer(lockTicketAction));
            printBillRule.Actions.Add(new ActionContainer(updateEntityStateAction) { ParameterValues = string.Format("Status={0}", "Bill Requested") });
            printBillRule.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("Status={0}", Resources.Locked) });
            printBillRule.Actions.Add(new ActionContainer(closeTicketAction));
            printBillRule.AddRuleMap();
            _workspace.Add(printBillRule);

            var unlockTicketRule = new AppRule { Name = string.Format(Resources.Rule_f, Resources.UnlockTicket), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = "AutomationCommandName;=;" + Resources.UnlockTicket };
            unlockTicketRule.Actions.Add(new ActionContainer(unlockTicketAction));
            unlockTicketRule.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("Status={0}", Resources.Unpaid) });
            unlockTicketRule.AddRuleMap();
            _workspace.Add(unlockTicketRule);

            var createTicketRule = new AppRule { Name = string.Format(Resources.Rule_f, "Create Ticket"), EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = "AutomationCommandName;=;" + string.Format(Resources.Add_f, Resources.Ticket) };
            createTicketRule.Actions.Add(new ActionContainer(createTicketAction));
            createTicketRule.AddRuleMap();
            _workspace.Add(createTicketRule);

            var updateMergedTicket = new AppRule { Name = Resources.UpdateMergedTicketsState, EventName = RuleEventNames.TicketsMerged };
            updateMergedTicket.Actions.Add(new ActionContainer(updateEntityStateAction) { ParameterValues = string.Format("Status={0}", Resources.NewOrders) });
            updateMergedTicket.Actions.Add(new ActionContainer(updateTicketStatusAction) { ParameterValues = string.Format("Status={0}", Resources.NewOrders) });
            updateMergedTicket.AddRuleMap();
            _workspace.Add(updateMergedTicket);

            ImportMenus(screen);
            ImportTableResources(tableEntityType, ticketType);

            var customerScreen = new EntityScreen { Name = string.Format(Resources.Customer_f, Resources.Search), DisplayMode = 1, EntityTypeId = customerEntityType.Id, TicketTypeId = ticketType.Id };
            customerScreen.EntityScreenMaps.Add(new EntityScreenMap());
            _workspace.Add(customerScreen);

            var customerTicketScreen = new EntityScreen { Name = Resources.CustomerTickets, DisplayMode = 0, EntityTypeId = customerEntityType.Id, StateFilter = newOrderState.Name, ColumnCount = 6, RowCount = 6, TicketTypeId = ticketType.Id };
            customerTicketScreen.EntityScreenMaps.Add(new EntityScreenMap());
            _workspace.Add(customerTicketScreen);

            ImportItems(BatchCreateEntities);
            ImportItems(BatchCreateTransactionTypes);
            ImportItems(BatchCreateTransactionTypeDocuments);

            _workspace.CommitChanges();
            _workspace.Dispose();


        }

        private void ImportItems<T>(Func<string[], IWorkspace, IEnumerable<T>> func) where T : class
        {
            var fileName = string.Format("{0}\\Imports\\" + typeof(T).Name.ToLower() + "{1}.txt", LocalSettings.AppPath, "_" + LocalSettings.CurrentLanguage);
            if (!File.Exists(fileName))
                fileName = string.Format("{0}\\Imports\\" + typeof(T).Name.ToLower() + ".txt", LocalSettings.AppPath);
            if (!File.Exists(fileName)) return;
            var lines = File.ReadAllLines(fileName);
            var items = func(lines, _workspace);
            items.ToList().ForEach(x => _workspace.Add(x));
            _workspace.CommitChanges();
        }

        private void ImportTableResources(EntityType tableTemplate, TicketType ticketType)
        {
            var fileName = string.Format("{0}/Imports/table{1}.txt", LocalSettings.AppPath, "_" + LocalSettings.CurrentLanguage);

            if (!File.Exists(fileName))
                fileName = string.Format("{0}/Imports/table.txt", LocalSettings.AppPath);

            if (!File.Exists(fileName)) return;

            var lines = File.ReadAllLines(fileName);
            var items = BatchCreateEntitiesWithTemplate(lines, _workspace, tableTemplate).ToList();
            items.ForEach(_workspace.Add);

            _workspace.CommitChanges();

            var screen = new EntityScreen { Name = Resources.All_Tables, DisplayState = "Status", TicketTypeId = ticketType.Id, ColumnCount = 7, EntityTypeId = tableTemplate.Id, FontSize = 50 };
            screen.EntityScreenMaps.Add(new EntityScreenMap());
            _workspace.Add(screen);

            foreach (var resource in items)
            {
                resource.EntityTypeId = tableTemplate.Id;
                screen.AddScreenItem(new EntityScreenItem { Name = resource.Name, EntityId = resource.Id });
                var state = new EntityStateValue { EntityId = resource.Id };
                state.SetStateValue("Status", "Available");
                _workspace.Add(state);
            }

            _workspace.CommitChanges();
        }

        private void ImportMenus(ScreenMenu screenMenu)
        {
            var fileName = string.Format("{0}/Imports/menu{1}.txt", LocalSettings.AppPath, "_" + LocalSettings.CurrentLanguage);

            if (!File.Exists(fileName))
                fileName = string.Format("{0}/Imports/menu.txt", LocalSettings.AppPath);

            if (!File.Exists(fileName)) return;

            var lines = File.ReadAllLines(fileName, Encoding.UTF8);

            var items = BatchCreateMenuItems(lines, _workspace).ToList();
            items.ForEach(_workspace.Add);
            _workspace.CommitChanges();
            var groupCodes = items.Select(x => x.GroupCode).Distinct().Where(x => !string.IsNullOrEmpty(x));

            foreach (var groupCode in groupCodes)
            {
                var code = groupCode;
                screenMenu.AddCategory(code);
                screenMenu.AddItemsToCategory(groupCode, items.Where(x => x.GroupCode == code).ToList());
            }
        }

        public IEnumerable<Entity> BatchCreateEntitiesWithTemplate(string[] values, IWorkspace workspace, EntityType template)
        {
            IList<Entity> result = new List<Entity>();
            if (values.Length > 0)
            {
                foreach (var entity in from value in values
                                       where !value.StartsWith("#")
                                       let entityName = value
                                       let count = Dao.Count<Entity>(y => y.Name == entityName.Trim())
                                       where count == 0
                                       select new Entity { Name = value.Trim(), EntityTypeId = template.Id }
                                           into resource
                                           where result.Count(x => x.Name.ToLower() == resource.Name.ToLower()) == 0
                                           select resource)
                {
                    result.Add(entity);
                }
            }
            return result;
        }

        public IEnumerable<MenuItem> BatchCreateMenuItems(string[] values, IWorkspace workspace)
        {
            var ds = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            IList<MenuItem> result = new List<MenuItem>();
            if (values.Length > 0)
            {
                var currentCategory = Resources.Common;

                foreach (var item in values)
                {
                    if (string.IsNullOrWhiteSpace(item)) continue;

                    if (item.StartsWith("#"))
                    {
                        currentCategory = item.Trim('#', ' ');
                    }
                    else if (item.Contains(" "))
                    {
                        IList<string> parts = new List<string>(item.Split(' '));
                        var price = ConvertToDecimal(parts[parts.Count - 1], ds);
                        parts.RemoveAt(parts.Count - 1);

                        var itemName = string.Join(" ", parts.ToArray());
                        var mi = MenuItem.Create();
                        mi.Name = itemName;
                        mi.Portions[0].Price = price;
                        mi.GroupCode = currentCategory;
                        result.Add(mi);
                    }
                }
            }
            return result;
        }

        public IEnumerable<Account> BatchCreateAccounts(string[] values, IWorkspace workspace)
        {
            IList<Account> result = new List<Account>();
            if (values.Length > 0)
            {
                var templates = workspace.All<AccountType>().ToList();
                AccountType currentTemplate = null;

                foreach (var item in values)
                {
                    if (item.StartsWith("#"))
                    {
                        var templateName = item.Trim('#', ' ');
                        currentTemplate = templates.SingleOrDefault(x => x.Name.ToLower() == templateName.ToLower());
                        if (currentTemplate == null)
                        {
                            using (var w = WorkspaceFactory.Create())
                            {
                                currentTemplate = new AccountType { Name = templateName };
                                w.Add(currentTemplate);
                                w.CommitChanges();
                            }
                        }
                    }
                    else if (currentTemplate != null)
                    {
                        var accountName = item.ToLower().Trim();
                        if (workspace.Single<Account>(x => x.Name.ToLower() == accountName) == null)
                        {
                            var account = new Account { Name = item, AccountTypeId = currentTemplate.Id };
                            result.Add(account);
                        }
                    }
                }
            }
            return result;
        }


        public IEnumerable<Entity> BatchCreateEntities(string[] values, IWorkspace workspace)
        {
            IList<Entity> result = new List<Entity>();
            if (values.Length > 0)
            {
                var templates = workspace.All<EntityType>().ToList();
                EntityType currentTemplate = null;

                foreach (var item in values)
                {
                    if (item.StartsWith("#"))
                    {
                        var templateName = item.Trim('#', ' ');
                        currentTemplate = templates.SingleOrDefault(x => x.Name.ToLower() == templateName.ToLower());
                        if (currentTemplate == null)
                        {
                            using (var w = WorkspaceFactory.Create())
                            {
                                currentTemplate = new EntityType { Name = templateName };
                                w.Add(currentTemplate);
                                w.CommitChanges();
                            }
                        }
                    }
                    else if (currentTemplate != null)
                    {
                        var accountName = item.ToLower().Trim();
                        if (workspace.Single<Entity>(x => x.Name.ToLower() == accountName) == null)
                        {
                            var account = new Entity { Name = item, EntityTypeId = currentTemplate.Id };
                            result.Add(account);
                        }
                    }
                }
            }
            return result;
        }

        public IEnumerable<AccountTransactionType> BatchCreateTransactionTypes(string[] values, IWorkspace workspace)
        {
            IList<AccountTransactionType> result = new List<AccountTransactionType>();
            if (values.Length > 0)
            {
                foreach (var item in values)
                {
                    var parts = item.Split(';');
                    if (parts.Count() > 2)
                    {
                        var name = parts[0].Trim();

                        if (workspace.Single<AccountTransactionType>(x => x.Name.ToLower() == name.ToLower()) != null) continue;

                        var sTempName = parts[1].Trim();
                        var tTempName = parts[2].Trim();
                        var dsa = parts.Length > 2 ? parts[3].Trim() : "";
                        var dta = parts.Length > 3 ? parts[4].Trim() : "";

                        var sAccTemplate = workspace.Single<AccountType>(x => x.Name.ToLower() == sTempName.ToLower());
                        if (sAccTemplate == null)
                        {
                            using (var w = WorkspaceFactory.Create())
                            {
                                sAccTemplate = new AccountType { Name = sTempName };
                                w.Add(sAccTemplate);
                                w.CommitChanges();
                            }
                        }

                        var tAccTemplate = workspace.Single<AccountType>(x => x.Name.ToLower() == tTempName.ToLower());
                        if (tAccTemplate == null)
                        {
                            using (var w = WorkspaceFactory.Create())
                            {
                                tAccTemplate = new AccountType { Name = tTempName };
                                w.Add(tAccTemplate);
                                w.CommitChanges();
                            }
                        }

                        var sa = !string.IsNullOrEmpty(dsa)
                            ? workspace.Single<Account>(x => x.Name.ToLower() == dsa.ToLower())
                            : null;

                        if (!string.IsNullOrEmpty(dsa) && sa == null)
                        {
                            using (var w = WorkspaceFactory.Create())
                            {
                                sa = new Account { Name = dsa, AccountTypeId = sAccTemplate.Id };
                                w.Add(sa);
                                w.CommitChanges();
                            }
                        }

                        var ta = !string.IsNullOrEmpty(dta)
                            ? workspace.Single<Account>(x => x.Name.ToLower() == dta.ToLower())
                            : null;

                        if (!string.IsNullOrEmpty(dta) && ta == null)
                        {
                            using (var w = WorkspaceFactory.Create())
                            {
                                ta = new Account { Name = dta, AccountTypeId = tAccTemplate.Id };
                                w.Add(ta);
                                w.CommitChanges();
                            }
                        }

                        var resultItem = new AccountTransactionType
                                             {
                                                 Name = name,
                                                 SourceAccountTypeId = sAccTemplate.Id,
                                                 TargetAccountTypeId = tAccTemplate.Id
                                             };

                        if (sa != null) resultItem.DefaultSourceAccountId = sa.Id;
                        if (ta != null) resultItem.DefaultTargetAccountId = ta.Id;

                        result.Add(resultItem);
                    }
                }
            }
            return result;
        }

        public IEnumerable<AccountTransactionDocumentType> BatchCreateTransactionTypeDocuments(string[] values, IWorkspace workspace)
        {
            IList<AccountTransactionDocumentType> result = new List<AccountTransactionDocumentType>();
            if (values.Length > 0)
            {
                foreach (var item in values)
                {
                    var parts = item.Split(';');
                    if (parts.Count() > 3)
                    {
                        var name = parts[0].Trim();
                        if (workspace.Single<AccountTransactionDocumentType>(x => x.Name.ToLower() == name.ToLower()) != null) continue;

                        var atName = parts[1].Trim();
                        var header = parts[2].Trim();

                        var accTemplate = workspace.Single<AccountType>(x => x.Name.ToLower() == atName.ToLower());
                        if (accTemplate == null)
                        {
                            using (var w = WorkspaceFactory.Create())
                            {
                                accTemplate = new AccountType { Name = atName };
                                w.Add(accTemplate);
                                w.CommitChanges();
                            }
                        }

                        var resultItem = new AccountTransactionDocumentType
                                             {
                                                 Name = name,
                                                 MasterAccountTypeId = accTemplate.Id,
                                                 ButtonHeader = header,
                                                 ButtonColor = "Gainsboro"
                                             };

                        for (var i = 3; i < parts.Length; i++)
                        {
                            var n = parts[i].ToLower();
                            var tt = workspace.Single<AccountTransactionType>(x => x.Name.ToLower() == n);
                            if (tt != null) resultItem.TransactionTypes.Add(tt);
                        }

                        result.Add(resultItem);
                    }
                }
            }
            return result;
        }

        private static decimal ConvertToDecimal(string priceStr, string decimalSeperator)
        {
            try
            {
                priceStr = priceStr.Replace(".", decimalSeperator);
                priceStr = priceStr.Replace(",", decimalSeperator);

                var price = Convert.ToDecimal(priceStr);
                return price;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static void CreateDefaultCurrenciesIfNeeded()
        {
            LocalSettings.ReportCurrencyFormat = "#,0.00;(#,0.00);-";
            LocalSettings.ReportQuantityFormat = "#.##;-#.##;-";
            LocalSettings.CurrencyFormat = "#,#0.00";
            LocalSettings.QuantityFormat = "#,##.##";
            LocalSettings.PrintoutCurrencyFormat = "#,#0.00;-#,#0.00;";
        }

        private ParameterBuilder Params()
        {
            return new ParameterBuilder();
        }

        public static string GetDefaultTicketPrintTemplate()
        {
            const string template = @"[LAYOUT]
-- General layout
<T><%TICKET>
<L00><%DATE>:{TICKET DATE}
<L00><%TIME>:{TIME}
{ENTITIES}
<L00><%TICKET> No:{TICKET NO}
<F>-
{ORDERS}
<F>=
<EB>
{DISCOUNTS}
[<J10><%TOTAL> <%GIFT>:|{ORDER STATE TOTAL:<%GIFT>}]
<J10><%TOTAL>:|{TICKET TOTAL}
{PAYMENTS}
<DB>
<F>=
<C10>T H A N K   Y O U
 
[DISCOUNTS]
<J00>{CALCULATION NAME} %{CALCULATION AMOUNT}|{CALCULATION TOTAL}
 
[PAYMENTS]
<J00>{PAYMENT NAME}|{PAYMENT AMOUNT}
 
[ORDERS]
-- Default format for orders
<J00>- {QUANTITY} {NAME}|{PRICE}
{ORDER TAGS}
 
[ORDERS:<%GIFT>]
-- Format for gifted orders
<J00>- {QUANTITY} {NAME}|**GIFT**
{ORDER TAGS}
 
[ORDERS:<%VOID>]
-- Nothing will print for void lines
 
[ORDER TAGS]
-- Format for order tags
<J00> * {ORDER TAG NAME} | {ORDER TAG PRICE}
 
[ENTITIES:<%TABLE>]
-- Table entity format
<L00><%TABLE>: {ENTITY NAME}
 
[ENTITIES:<%CUSTOMER>]
-- Customer entity format
<J00><%CUSTOMER>: {ENTITY NAME} | {ENTITY DATA:Phone}";
            return ReplaceTemplateValues(template);
        }

        public static string GetDefaultKitchenPrintTemplate()
        {
            const string template = @"[LAYOUT]
<T><%TICKET>
<L00><%DATE>:{TICKET DATE}
<L00><%TIME>:{TIME}
<L00><%TABLE>:{ENTITY NAME:<%TABLE>}
<L00><%TICKET> No:{TICKET NO}
<F>-
{ORDERS}

[ORDERS]
<L00>- {QUANTITY} {NAME}
{ORDER TAGS}

[ORDERS:<%VOID>]
<J00>- {QUANTITY} {NAME}|**<%VOID>**
{ORDER TAGS}

[ORDER TAGS]
-- Format for order tags
<L00>     * {ORDER TAG NAME}";

            return ReplaceTemplateValues(template);
        }

        private static string ReplaceTemplateValues(string template)
        {
            template = template.Replace("<%TICKET>", Resources.Ticket);
            template = template.Replace("<%DATE>", Resources.Date);
            template = template.Replace("<%TIME>", Resources.Time);
            template = template.Replace("<%GIFT>", Resources.Gift);
            template = template.Replace("<%VOID>", Resources.Void);
            template = template.Replace("<%TABLE>", Resources.Table);
            template = template.Replace("<%CUSTOMER>", Resources.Customer);
            template = template.Replace("<%PHONE>", Resources.Phone);
            template = template.Replace("<%TOTAL>", Resources.Total);
            return template;
        }
    }

    internal class ParameterBuilder
    {
        private readonly IDictionary<string, string> _values = new Dictionary<string, string>();
        public ParameterBuilder Add(string key, string value)
        {
            _values.Add(key, value);
            return this;
        }

        public override string ToString()
        {
            return JsonHelper.Serialize(_values);
        }
    }
}
