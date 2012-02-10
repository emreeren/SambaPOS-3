using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Actions;
using Samba.Domain.Models.Locations;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Persistance.Data;

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
            var paymentAccountTemplate = new AccountTemplate { Name = "Payment Accounts" };
            var customerAccountTemplate = new AccountTemplate { Name = "Customer Accounts" };
            var discountAccountTemplate = new AccountTemplate { Name = "Discount Accounts" };

            var defaultSaleAccount = new Account { AccountTemplate = saleAccountTemplate, Name = "Sales" };
            var defaultpaymentAccount = new Account { AccountTemplate = paymentAccountTemplate, Name = Resources.Cash };
            var creditCardAccount = new Account { AccountTemplate = paymentAccountTemplate, Name = Resources.CreditCard };
            var voucherAccount = new Account { AccountTemplate = paymentAccountTemplate, Name = Resources.Voucher };
            var defaultCustomerAccount = new Account { AccountTemplate = customerAccountTemplate, Name = "Customer" };
            var defaultDiscountAccount = new Account { AccountTemplate = discountAccountTemplate, Name = "Discount" };
            var defaultRoundingAccount = new Account { AccountTemplate = discountAccountTemplate, Name = Resources.Rounding };

            _workspace.Add(defaultpaymentAccount);
            _workspace.Add(creditCardAccount);
            _workspace.Add(voucherAccount);

            var discountTransactionTemplate = new AccountTransactionTemplate
            {
                Name = "Discount Transaction",
                SourceAccountTemplate = customerAccountTemplate,
                TargetAccountTemplate = discountAccountTemplate,
                DefaultSourceAccount = defaultCustomerAccount,
                DefaultTargetAccount = defaultDiscountAccount
            };

            var roundingTransactionTemplate = new AccountTransactionTemplate
            {
                Name = "Rounding Transaction",
                SourceAccountTemplate = customerAccountTemplate,
                TargetAccountTemplate = discountAccountTemplate,
                DefaultSourceAccount = defaultCustomerAccount,
                DefaultTargetAccount = defaultRoundingAccount
            };

            var saleTransactionTemplate = new AccountTransactionTemplate
            {
                Name = "Sale Transaction",
                SourceAccountTemplate = saleAccountTemplate,
                TargetAccountTemplate = customerAccountTemplate,
                DefaultSourceAccount = defaultSaleAccount,
                DefaultTargetAccount = defaultCustomerAccount
            };

            var paymentTransactionTemplate = new AccountTransactionTemplate
            {
                Name = "Payment Transaction",
                SourceAccountTemplate = customerAccountTemplate,
                TargetAccountTemplate = paymentAccountTemplate,
                DefaultSourceAccount = defaultCustomerAccount,
                DefaultTargetAccount = defaultpaymentAccount
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
                Name = Resources.DiscountPercentSign
            };

            var roundingService = new CalculationTemplate
            {
                AccountTransactionTemplate = roundingTransactionTemplate,
                ButtonHeader = Resources.Round,
                CalculationMethod = 2,
                DecreaseAmount = true,
                IncludeTax = true,
                Name = Resources.Round
            };

            _workspace.Add(discountService);
            _workspace.Add(roundingService);

            var screen = new ScreenMenu();
            _workspace.Add(screen);

            var ticketNumerator = new Numerator { Name = Resources.TicketNumerator };
            _workspace.Add(ticketNumerator);

            var orderNumerator = new Numerator { Name = Resources.OrderNumerator };
            _workspace.Add(orderNumerator);

            _workspace.CommitChanges();

            var ticketTemplate = new TicketTemplate
                                     {
                                         Name = Resources.TicketTemplate,
                                         TicketNumerator = ticketNumerator,
                                         OrderNumerator = orderNumerator,
                                         SaleTransactionTemplate = saleTransactionTemplate,
                                         PaymentTransactionTemplate = paymentTransactionTemplate
                                     };

            ticketTemplate.CalulationTemplates.Add(discountService);
            ticketTemplate.CalulationTemplates.Add(roundingService);

            _workspace.Add(ticketTemplate);

            var department = new Department
            {
                Name = Resources.Restaurant,
                TicketTemplate = ticketTemplate,
                ScreenMenuId = screen.Id,
                IsAlaCarte = true
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
                ButtonHeader = Resources.PrintBill,
                LocksTicket = true,
                Order = 0,
                UseFromPaymentScreen = true,
                UseFromTerminal = true,
                UseFromPos = true,
                WhatToPrint = (int)WhatToPrintTypes.Everything,
                WhenToPrint = (int)WhenToPrintTypes.Manual
            };
            pj1.PrinterMaps.Add(pm1);


            _workspace.Add(pj1);

            var pm2 = new PrinterMap { PrinterId = printer2.Id, PrinterTemplateId = kitchenPrinterTemplate.Id };
            var pj2 = new PrintJob
            {
                Name = Resources.PrintOrdersToKitchenPrinter,
                ButtonHeader = "",
                Order = 1,
                WhatToPrint = (int)WhatToPrintTypes.NewLines,
                WhenToPrint = (int)WhenToPrintTypes.NewLinesAdded
            };
            pj2.PrinterMaps.Add(pm2);

            _workspace.Add(pj2);

            t.PrintJobs.Add(pj1);
            t.PrintJobs.Add(pj2);
            _workspace.Add(t);

            var orderTag1 = new OrderTagGroup { Name = Resources.Gift, ButtonHeader = Resources.Gift, CalculateOrderPrice = false, DecreaseOrderInventory = true, SelectionType = 1 };
            orderTag1.OrderTags.Add(new OrderTag { Name = Resources.Gift });
            orderTag1.OrderTagMaps.Add(new OrderTagMap());
            _workspace.Add(orderTag1);

            var orderTag2 = new OrderTagGroup { Name = Resources.Void, ButtonHeader = Resources.Void, CalculateOrderPrice = false, DecreaseOrderInventory = false, SelectionType = 1 };
            orderTag2.OrderTags.Add(new OrderTag { Name = Resources.Void });
            orderTag2.OrderTagMaps.Add(new OrderTagMap());
            orderTag2.UnlocksOrder = true;
            _workspace.Add(orderTag2);

            department.TicketTemplate.OrderTagGroups.Add(orderTag1);
            department.TicketTemplate.OrderTagGroups.Add(orderTag2);

            var orderTagTemplate = new OrderTagTemplate { Name = Resources.Gift };
            orderTagTemplate.OrderTagTemplateValues.Add(new OrderTagTemplateValue { OrderTagGroup = orderTag1, OrderTag = orderTag1.OrderTags[0] });

            _workspace.Add(orderTagTemplate);

            var action = new AppAction { ActionType = "RemoveOrderTag", Name = Resources.RemoveGiftTag, Parameter = string.Format("[{{\"Key\":\"OrderTagName\",\"Value\":\"{0}\"}}]", Resources.Gift) };
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

            ImportMenus(screen);
            ImportLocations(department);

            _workspace.CommitChanges();
            _workspace.Dispose();
        }

        private void ImportLocations(Department department)
        {
            var fileName = string.Format("{0}/Imports/table{1}.txt", LocalSettings.AppPath, "_" + LocalSettings.CurrentLanguage);

            if (!File.Exists(fileName))
                fileName = string.Format("{0}/Imports/table.txt", LocalSettings.AppPath);

            if (!File.Exists(fileName)) return;

            var lines = File.ReadAllLines(fileName);
            var items = BatchCreateLocations(lines, _workspace);
            _workspace.CommitChanges();

            var screen = new LocationScreen { Name = Resources.AllLocations, ColumnCount = 8 };
            _workspace.Add(screen);

            foreach (var location in items)
                screen.AddScreenItem(location);

            _workspace.CommitChanges();

            department.LocationScreens.Add(screen);
        }

        private void ImportMenus(ScreenMenu screenMenu)
        {
            var fileName = string.Format("{0}/Imports/menu{1}.txt", LocalSettings.AppPath, "_" + LocalSettings.CurrentLanguage);

            if (!File.Exists(fileName))
                fileName = string.Format("{0}/Imports/menu.txt", LocalSettings.AppPath);

            if (!File.Exists(fileName)) return;

            var lines = File.ReadAllLines(fileName, Encoding.UTF8);

            var items = BatchCreateMenuItems(lines, _workspace);
            _workspace.CommitChanges();
            var groupCodes = items.Select(x => x.GroupCode).Distinct().Where(x => !string.IsNullOrEmpty(x));

            foreach (var groupCode in groupCodes)
            {
                var code = groupCode;
                screenMenu.AddCategory(code);
                screenMenu.AddItemsToCategory(groupCode, items.Where(x => x.GroupCode == code).ToList());
            }
        }

        public IEnumerable<Location> BatchCreateLocations(string[] values, IWorkspace workspace)
        {
            IList<Location> result = new List<Location>();
            if (values.Length > 0)
            {
                var currentCategory = Resources.Common;
                foreach (var value in values)
                {
                    if (value.StartsWith("#"))
                    {
                        currentCategory = value.Trim('#', ' ');
                    }
                    else
                    {
                        var locationName = value;
                        var count = Dao.Count<Location>(y => y.Name == locationName.Trim());
                        if (count == 0)
                        {
                            var location = new Location { Name = value.Trim(), Category = currentCategory };
                            if (result.Count(x => x.Name.ToLower() == location.Name.ToLower()) == 0)
                            {
                                result.Add(location);
                                workspace.Add(location);
                            }
                        }
                    }
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
                        workspace.Add(mi);
                        workspace.Add(mi.Portions[0]);
                        result.Add(mi);
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
            LocalSettings.DefaultCurrencyFormat = "C";
        }
    }
}
