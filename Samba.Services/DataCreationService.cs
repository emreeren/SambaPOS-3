using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Actions;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services.Common;

namespace Samba.Services
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

            var saleAccountTemplate = new AccountTemplate { Name = "Sales Accounts" };
            var receivableAccountTemplate = new AccountTemplate { Name = "Receivable Accounts" };
            var paymentAccountTemplate = new AccountTemplate { Name = "Payment Accounts" };
            var discountAccountTemplate = new AccountTemplate { Name = "Discount Accounts" };

            _workspace.Add(receivableAccountTemplate);
            _workspace.Add(saleAccountTemplate);
            _workspace.Add(paymentAccountTemplate);
            _workspace.Add(discountAccountTemplate);

            var customerResourceTemplate = new ResourceTemplate { Name = "Customers", EntityName = "Customer" };
            var tableResourceTemplate = new ResourceTemplate { Name = "Tables", EntityName = Resources.Table };

            _workspace.Add(customerResourceTemplate);
            _workspace.Add(tableResourceTemplate);

            _workspace.CommitChanges();

            var accountScreen = new AccountScreen
                                    {
                                        Name = "General",
                                        AccountTemplateNames = string.Join(";", saleAccountTemplate.Name,
                                                                           receivableAccountTemplate.Name,
                                                                           discountAccountTemplate.Name,
                                                                           paymentAccountTemplate.Name)
                                    };
            _workspace.Add(accountScreen);

            var defaultSaleAccount = new Account { AccountTemplateId = saleAccountTemplate.Id, Name = "Sales" };
            var defaultReceivableAccount = new Account { AccountTemplateId = receivableAccountTemplate.Id, Name = "Receivables" };
            var cashAccount = new Account { AccountTemplateId = paymentAccountTemplate.Id, Name = Resources.Cash };
            var creditCardAccount = new Account { AccountTemplateId = paymentAccountTemplate.Id, Name = Resources.CreditCard };
            var voucherAccount = new Account { AccountTemplateId = paymentAccountTemplate.Id, Name = Resources.Voucher };
            var defaultDiscountAccount = new Account { AccountTemplateId = discountAccountTemplate.Id, Name = "Discount" };
            var defaultRoundingAccount = new Account { AccountTemplateId = discountAccountTemplate.Id, Name = Resources.Rounding };

            _workspace.Add(defaultSaleAccount);
            _workspace.Add(defaultReceivableAccount);
            _workspace.Add(defaultDiscountAccount);
            _workspace.Add(defaultRoundingAccount);
            _workspace.Add(cashAccount);
            _workspace.Add(creditCardAccount);
            _workspace.Add(voucherAccount);

            _workspace.CommitChanges();

            var discountTransactionTemplate = new AccountTransactionTemplate
            {
                Name = "Discount Transaction",
                SourceAccountTemplateId = receivableAccountTemplate.Id,
                TargetAccountTemplateId = discountAccountTemplate.Id,
                DefaultSourceAccountId = defaultReceivableAccount.Id,
                DefaultTargetAccountId = defaultDiscountAccount.Id
            };

            var roundingTransactionTemplate = new AccountTransactionTemplate
            {
                Name = "Rounding Transaction",
                SourceAccountTemplateId = receivableAccountTemplate.Id,
                TargetAccountTemplateId = discountAccountTemplate.Id,
                DefaultSourceAccountId = defaultReceivableAccount.Id,
                DefaultTargetAccountId = defaultRoundingAccount.Id
            };

            var saleTransactionTemplate = new AccountTransactionTemplate
            {
                Name = "Sale Transaction",
                SourceAccountTemplateId = saleAccountTemplate.Id,
                TargetAccountTemplateId = receivableAccountTemplate.Id,
                DefaultSourceAccountId = defaultSaleAccount.Id,
                DefaultTargetAccountId = defaultReceivableAccount.Id
            };

            var paymentTransactionTemplate = new AccountTransactionTemplate
            {
                Name = "Payment Transaction",
                SourceAccountTemplateId = receivableAccountTemplate.Id,
                TargetAccountTemplateId = paymentAccountTemplate.Id,
                DefaultSourceAccountId = defaultReceivableAccount.Id,
                DefaultTargetAccountId = cashAccount.Id
            };

            _workspace.Add(saleTransactionTemplate);
            _workspace.Add(paymentTransactionTemplate);
            _workspace.Add(discountTransactionTemplate);
            _workspace.Add(roundingTransactionTemplate);

            var discountService = new CalculationTemplate
            {
                AccountTransactionTemplate = discountTransactionTemplate,
                ButtonHeader = Resources.DiscountPercentSign,
                CalculationMethod = 0,
                DecreaseAmount = true,
                Name = Resources.Discount
            };
            discountService.AddCalculationTemplateMap();

            var roundingService = new CalculationTemplate
            {
                AccountTransactionTemplate = roundingTransactionTemplate,
                ButtonHeader = Resources.Round,
                CalculationMethod = 2,
                DecreaseAmount = true,
                IncludeTax = true,
                Name = Resources.Round
            };
            roundingService.AddCalculationTemplateMap();

            _workspace.Add(discountService);
            _workspace.Add(roundingService);

            var screen = new ScreenMenu();
            _workspace.Add(screen);

            var ticketNumerator = new Numerator { Name = Resources.TicketNumerator };
            _workspace.Add(ticketNumerator);

            var orderNumerator = new Numerator { Name = Resources.OrderNumerator };
            _workspace.Add(orderNumerator);

            var printBillAutomation = new AutomationCommand { Name = Resources.PrintBill, ButtonHeader = Resources.PrintBill };
            printBillAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { VisualBehaviour = 1 });
            _workspace.Add(printBillAutomation);

            var unlockTicketAutomation = new AutomationCommand { Name = "Unlock Ticket", ButtonHeader = "Unlock Ticket" };
            unlockTicketAutomation.AutomationCommandMaps.Add(new AutomationCommandMap { VisualBehaviour = 2 });
            _workspace.Add(unlockTicketAutomation);

            var addTicketAutomation = new AutomationCommand { Name = "Add Ticket", ButtonHeader = "Add Ticket" };
            addTicketAutomation.AddAutomationCommandMap();
            _workspace.Add(addTicketAutomation);

            _workspace.CommitChanges();

            var ticketTemplate = new TicketTemplate
                                     {
                                         Name = Resources.TicketTemplate,
                                         TicketNumerator = ticketNumerator,
                                         OrderNumerator = orderNumerator,
                                         SaleTransactionTemplate = saleTransactionTemplate,
                                     };

            var cashPayment = new PaymentTemplate
            {
                AccountTransactionTemplate = paymentTransactionTemplate,
                Account = cashAccount,
                Name = cashAccount.Name
            };
            cashPayment.PaymentTemplateMaps.Add(new PaymentTemplateMap { DisplayAtPaymentScreen = true });

            var creditCardPayment = new PaymentTemplate
            {
                AccountTransactionTemplate = paymentTransactionTemplate,
                Account = creditCardAccount,
                Name = creditCardAccount.Name
            };
            creditCardPayment.PaymentTemplateMaps.Add(new PaymentTemplateMap { DisplayAtPaymentScreen = true });

            var voucherPayment = new PaymentTemplate
            {
                AccountTransactionTemplate = paymentTransactionTemplate,
                Account = voucherAccount,
                Name = voucherAccount.Name
            };
            voucherPayment.PaymentTemplateMaps.Add(new PaymentTemplateMap { DisplayAtPaymentScreen = true });

            _workspace.Add(cashPayment);
            _workspace.Add(creditCardPayment);
            _workspace.Add(voucherPayment);
            _workspace.Add(ticketTemplate);

            var department = new Department
            {
                Name = Resources.Restaurant,
                TicketTemplate = ticketTemplate,
                ScreenMenuId = screen.Id,
            };

            _workspace.Add(department);

            var role = new UserRole("Admin") { IsAdmin = true, DepartmentId = 1 };
            _workspace.Add(role);

            var u = new User("Administrator", "1234") { UserRole = role };
            _workspace.Add(u);

            var ticketPrinterTemplate = new PrinterTemplate();
            ticketPrinterTemplate.Name = Resources.TicketTemplate;
            ticketPrinterTemplate.HeaderTemplate = Resources.TicketTemplateHeaderValue;
            ticketPrinterTemplate.LineTemplate = Resources.TicketTempleteLineTemplateValue;
            ticketPrinterTemplate.FooterTemplate = Resources.TicketTemplateFooterValue;

            var kitchenPrinterTemplate = new PrinterTemplate();
            kitchenPrinterTemplate.Name = Resources.KitchenOrderTemplate;
            kitchenPrinterTemplate.HeaderTemplate = Resources.KitchenTemplateHeaderValue;

            kitchenPrinterTemplate.LineTemplate = Resources.KitchenTemplateLineTemplateValue;
            kitchenPrinterTemplate.FooterTemplate = "<F>-";

            var invoicePrinterTemplate = new PrinterTemplate();
            invoicePrinterTemplate.Name = Resources.InvoicePrinterTemplate;
            invoicePrinterTemplate.HeaderTemplate = Resources.InvoiceTemplateHeaderValue;
            invoicePrinterTemplate.LineTemplate = Resources.InvoiceTemplateLineTemplateValue;
            invoicePrinterTemplate.FooterTemplate = "<F>-";

            _workspace.Add(ticketPrinterTemplate);
            _workspace.Add(kitchenPrinterTemplate);
            _workspace.Add(invoicePrinterTemplate);

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
                SlipReportPrinter = printer1,
            };

            var pm1 = new PrinterMap { PrinterId = printer1.Id, PrinterTemplateId = ticketPrinterTemplate.Id };
            _workspace.Add(pm1);

            var pj1 = new PrintJob
            {
                Name = Resources.PrintBill,
                LocksTicket = true,
                UseFromPaymentScreen = true,
                UseFromTerminal = true,
                UseFromPos = true,
                WhatToPrint = (int)WhatToPrintTypes.Everything,
            };
            pj1.PrinterMaps.Add(pm1);


            _workspace.Add(pj1);

            var pm2 = new PrinterMap { PrinterId = printer2.Id, PrinterTemplateId = kitchenPrinterTemplate.Id };
            var pj2 = new PrintJob
            {
                Name = Resources.PrintOrdersToKitchenPrinter,
                WhatToPrint = (int)WhatToPrintTypes.NewLines,
            };
            pj2.PrinterMaps.Add(pm2);

            _workspace.Add(pj2);
            _workspace.Add(t);

            var orderTag1 = new OrderTagGroup { Name = Resources.Gift, ButtonHeader = Resources.Gift, CalculateOrderPrice = false, DecreaseOrderInventory = true, SelectionType = 1 };
            orderTag1.OrderTags.Add(new OrderTag { Name = Resources.Gift });
            orderTag1.AddOrderTagMap();
            _workspace.Add(orderTag1);

            var orderTag2 = new OrderTagGroup { Name = Resources.Void, ButtonHeader = Resources.Void, CalculateOrderPrice = false, DecreaseOrderInventory = false, SelectionType = 1 };
            orderTag2.OrderTags.Add(new OrderTag { Name = Resources.Void });
            orderTag2.UnlocksOrder = true;
            orderTag2.AddOrderTagMap();
            _workspace.Add(orderTag2);

            var orderTagTemplate = new OrderTagTemplate { Name = Resources.Gift };
            orderTagTemplate.OrderTagTemplateValues.Add(new OrderTagTemplateValue { OrderTagGroup = orderTag1, OrderTag = orderTag1.OrderTags[0] });

            _workspace.Add(orderTagTemplate);

            const string parameterFormat = "[{{\"Key\":\"{0}\",\"Value\":\"{1}\"}}]";
            const string doubleParameterFormat = "[{{\"Key\":\"{0}\",\"Value\":\"{1}\"}},{{\"Key\":\"{2}\",\"Value\":\"{3}\"}}]";

            var action = new AppAction { ActionType = "RemoveOrderTag", Name = Resources.RemoveGiftTag, Parameter = string.Format(parameterFormat, "OrderTagName", Resources.Gift) };
            _workspace.Add(action);
            _workspace.CommitChanges();

            var rule = new AppRule
                           {
                               Name = Resources.RemoveGiftTagWhenVoided,
                               EventName = "OrderTagged",
                               EventConstraints = "OrderTagName;=;" + Resources.Void
                           };

            var actionContainer = new ActionContainer(action) { ParameterValues = "" };
            rule.Actions.Add(actionContainer);

            _workspace.Add(rule);

            var newOrderState = new ResourceState { Name = "New Orders", Color = "Orange" };
            _workspace.Add(newOrderState);

            var availableState = new ResourceState { Name = "Available", Color = "White" };
            _workspace.Add(availableState);

            var billRequestedState = new ResourceState { Name = "Bill Requested", Color = "Maroon" };
            _workspace.Add(billRequestedState);

            var newOrderAction = new AppAction { ActionType = "UpdateResourceState", Name = "Update New Order State", Parameter = string.Format(parameterFormat, "ResourceState", "New Orders") };
            _workspace.Add(newOrderAction);
            var availableAction = new AppAction { ActionType = "UpdateResourceState", Name = "Update Available State", Parameter = string.Format(parameterFormat, "ResourceState", "Available") };
            _workspace.Add(availableAction);
            var billRequestedAction = new AppAction { ActionType = "UpdateResourceState", Name = "Update Bill Requested State", Parameter = string.Format(parameterFormat, "ResourceState", "Bill Requested") };
            _workspace.Add(billRequestedAction);
            var createTicketAction = new AppAction { ActionType = "CreateTicket", Name = "Create New Ticket", Parameter = "" };
            _workspace.Add(createTicketAction);
            var closeTicketAction = new AppAction { ActionType = "CloseActiveTicket", Name = "Close Ticket", Parameter = "" };
            _workspace.Add(closeTicketAction);
            var printBillAction = new AppAction { ActionType = "ExecutePrintJob", Name = "Execute Bill Print Job", Parameter = string.Format(parameterFormat, "PrintJobName", Resources.PrintBill) };
            _workspace.Add(printBillAction);
            var printKitchenOrdersAction = new AppAction { ActionType = "ExecutePrintJob", Name = "Execute Kitchen Orders Print Job", Parameter = string.Format(parameterFormat, "PrintJobName", Resources.PrintOrdersToKitchenPrinter) };
            _workspace.Add(printKitchenOrdersAction);
            var unlockTicketAction = new AppAction { ActionType = "UnlockTicket", Name = "Unlock Ticket", Parameter = "" };
            _workspace.Add(unlockTicketAction);
            _workspace.CommitChanges();

            var newOrderRule = new AppRule { Name = "Update New Order Resource Color", EventName = "TicketClosing", EventConstraints = "NewOrderCount;>;0" };
            newOrderRule.Actions.Add(new ActionContainer(printKitchenOrdersAction));
            newOrderRule.Actions.Add(new ActionContainer(newOrderAction));
            _workspace.Add(newOrderRule);

            var availableRule = new AppRule { Name = "Update Available Resource Color", EventName = "ResourceUpdated", EventConstraints = "OpenTicketCount;=;0" };
            var ac2 = new ActionContainer(availableAction);
            availableRule.Actions.Add(ac2);
            _workspace.Add(availableRule);

            var movingRule = new AppRule { Name = "Update Moved Resource Color", EventName = "TicketResourceChanged", EventConstraints = "OrderCount;>;0" };
            var ac3 = new ActionContainer(newOrderAction);
            movingRule.Actions.Add(ac3);
            _workspace.Add(movingRule);

            var printBillRule = new AppRule { Name = "Print Bill Rule", EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = "AutomationCommandName;=;" + Resources.PrintBill };
            printBillRule.Actions.Add(new ActionContainer(printBillAction));
            printBillRule.Actions.Add(new ActionContainer(billRequestedAction));
            printBillRule.Actions.Add(new ActionContainer(closeTicketAction));
            _workspace.Add(printBillRule);

            var unlockTicketRule = new AppRule { Name = "Unlock Ticket Rule", EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = "AutomationCommandName;=;Unlock Ticket" };
            unlockTicketRule.Actions.Add(new ActionContainer(unlockTicketAction));
            _workspace.Add(unlockTicketRule);

            var createTicketRule = new AppRule { Name = "Create Ticket Rule", EventName = RuleEventNames.AutomationCommandExecuted, EventConstraints = "AutomationCommandName;=;Add Ticket" };
            createTicketRule.Actions.Add(new ActionContainer(createTicketAction));
            _workspace.Add(createTicketRule);

            var updateMergedTicket = new AppRule { Name = "Update Merged Tickets State", EventName = RuleEventNames.TicketsMerged };
            updateMergedTicket.Actions.Add(new ActionContainer(newOrderAction));
            _workspace.Add(updateMergedTicket);

            ImportMenus(screen);
            ImportTableResources(department, tableResourceTemplate, availableState.Id);

            ImportItems(BatchCreateResources);
            ImportItems(BatchCreateTransactionTemplates);
            ImportItems(BatchCreateTransactionTemplateDocuments);

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

        private void ImportTableResources(Department department, ResourceTemplate tableTemplate, int defaultStateId)
        {
            var fileName = string.Format("{0}/Imports/table{1}.txt", LocalSettings.AppPath, "_" + LocalSettings.CurrentLanguage);

            if (!File.Exists(fileName))
                fileName = string.Format("{0}/Imports/table.txt", LocalSettings.AppPath);

            if (!File.Exists(fileName)) return;

            var lines = File.ReadAllLines(fileName);
            var items = BatchCreateResourcesWithTemplate(lines, _workspace, tableTemplate).ToList();
            items.ForEach(_workspace.Add);

            _workspace.CommitChanges();

            var screen = new ResourceScreen { Name = "All Tables", ColumnCount = 7, ResourceTemplateId = tableTemplate.Id, DisplayOpenTickets = false };
            _workspace.Add(screen);

            foreach (var resource in items)
            {
                resource.ResourceTemplateId = tableTemplate.Id;
                screen.AddScreenItem(new ResourceScreenItem { Name = resource.Name, ResourceId = resource.Id });
                var state = new ResourceStateValue { Date = DateTime.Now, ResoruceId = resource.Id, StateId = defaultStateId };
                _workspace.Add(state);
            }

            _workspace.CommitChanges();

            department.ResourceScreens.Add(screen);
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

        public IEnumerable<Resource> BatchCreateResourcesWithTemplate(string[] values, IWorkspace workspace, ResourceTemplate template)
        {
            IList<Resource> result = new List<Resource>();
            if (values.Length > 0)
            {
                foreach (var resource in from value in values
                                         where !value.StartsWith("#")
                                         let resourceName = value
                                         let count = Dao.Count<Resource>(y => y.Name == resourceName.Trim())
                                         where count == 0
                                         select new Resource { Name = value.Trim(), ResourceTemplateId = template.Id }
                                             into resource
                                             where result.Count(x => x.Name.ToLower() == resource.Name.ToLower()) == 0
                                             select resource)
                {
                    result.Add(resource);
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
                var templates = workspace.All<AccountTemplate>().ToList();
                AccountTemplate currentTemplate = null;

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
                                currentTemplate = new AccountTemplate { Name = templateName };
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
                            var account = new Account { Name = item, AccountTemplateId = currentTemplate.Id };
                            result.Add(account);
                        }
                    }
                }
            }
            return result;
        }


        public IEnumerable<Resource> BatchCreateResources(string[] values, IWorkspace workspace)
        {
            IList<Resource> result = new List<Resource>();
            if (values.Length > 0)
            {
                var templates = workspace.All<ResourceTemplate>().ToList();
                ResourceTemplate currentTemplate = null;

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
                                currentTemplate = new ResourceTemplate { Name = templateName };
                                w.Add(currentTemplate);
                                w.CommitChanges();
                            }
                        }
                    }
                    else if (currentTemplate != null)
                    {
                        var accountName = item.ToLower().Trim();
                        if (workspace.Single<Resource>(x => x.Name.ToLower() == accountName) == null)
                        {
                            var account = new Resource { Name = item, ResourceTemplateId = currentTemplate.Id };
                            result.Add(account);
                        }
                    }
                }
            }
            return result;
        }

        public IEnumerable<AccountTransactionTemplate> BatchCreateTransactionTemplates(string[] values, IWorkspace workspace)
        {
            IList<AccountTransactionTemplate> result = new List<AccountTransactionTemplate>();
            if (values.Length > 0)
            {
                foreach (var item in values)
                {
                    var parts = item.Split(';');
                    if (parts.Count() > 2)
                    {
                        var name = parts[0].Trim();

                        if (workspace.Single<AccountTransactionTemplate>(x => x.Name.ToLower() == name.ToLower()) != null) continue;

                        var sTempName = parts[1].Trim();
                        var tTempName = parts[2].Trim();
                        var dsa = parts.Length > 2 ? parts[3].Trim() : "";
                        var dta = parts.Length > 3 ? parts[4].Trim() : "";

                        var sAccTemplate = workspace.Single<AccountTemplate>(x => x.Name.ToLower() == sTempName.ToLower());
                        if (sAccTemplate == null)
                        {
                            using (var w = WorkspaceFactory.Create())
                            {
                                sAccTemplate = new AccountTemplate { Name = sTempName };
                                w.Add(sAccTemplate);
                                w.CommitChanges();
                            }
                        }

                        var tAccTemplate = workspace.Single<AccountTemplate>(x => x.Name.ToLower() == tTempName.ToLower());
                        if (tAccTemplate == null)
                        {
                            using (var w = WorkspaceFactory.Create())
                            {
                                tAccTemplate = new AccountTemplate { Name = tTempName };
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
                                sa = new Account { Name = dsa, AccountTemplateId = sAccTemplate.Id };
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
                                ta = new Account { Name = dta, AccountTemplateId = tAccTemplate.Id };
                                w.Add(ta);
                                w.CommitChanges();
                            }
                        }

                        var resultItem = new AccountTransactionTemplate
                                             {
                                                 Name = name,
                                                 SourceAccountTemplateId = sAccTemplate.Id,
                                                 TargetAccountTemplateId = tAccTemplate.Id
                                             };

                        if (sa != null) resultItem.DefaultSourceAccountId = sa.Id;
                        if (ta != null) resultItem.DefaultTargetAccountId = ta.Id;

                        result.Add(resultItem);
                    }
                }
            }
            return result;
        }

        public IEnumerable<AccountTransactionDocumentTemplate> BatchCreateTransactionTemplateDocuments(string[] values, IWorkspace workspace)
        {
            IList<AccountTransactionDocumentTemplate> result = new List<AccountTransactionDocumentTemplate>();
            if (values.Length > 0)
            {
                foreach (var item in values)
                {
                    var parts = item.Split(';');
                    if (parts.Count() > 3)
                    {
                        var name = parts[0].Trim();
                        if (workspace.Single<AccountTransactionDocumentTemplate>(x => x.Name.ToLower() == name.ToLower()) != null) continue;

                        var atName = parts[1].Trim();
                        var header = parts[2].Trim();

                        var accTemplate = workspace.Single<AccountTemplate>(x => x.Name.ToLower() == atName.ToLower());
                        if (accTemplate == null)
                        {
                            using (var w = WorkspaceFactory.Create())
                            {
                                accTemplate = new AccountTemplate { Name = atName };
                                w.Add(accTemplate);
                                w.CommitChanges();
                            }
                        }

                        var resultItem = new AccountTransactionDocumentTemplate
                                             {
                                                 Name = name,
                                                 MasterAccountTemplateId = accTemplate.Id,
                                                 ButtonHeader = header,
                                                 ButtonColor = "Gainsboro"
                                             };

                        for (var i = 3; i < parts.Length; i++)
                        {
                            var n = parts[i].ToLower();
                            var tt = workspace.Single<AccountTransactionTemplate>(x => x.Name.ToLower() == n);
                            if (tt != null) resultItem.TransactionTemplates.Add(tt);
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
            LocalSettings.DefaultCurrencyFormat = "#,0.00;(#,0.00);-";
        }
    }
}
