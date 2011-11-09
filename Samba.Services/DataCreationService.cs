using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tables;
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

            var screen = new ScreenMenu();
            _workspace.Add(screen);

            var ticketNumerator = new Numerator { Name = Resources.TicketNumerator };
            _workspace.Add(ticketNumerator);

            var orderNumerator = new Numerator { Name = Resources.OrderNumerator };
            _workspace.Add(orderNumerator);

            _workspace.CommitChanges();

            var department = new Department
            {
                Name = Resources.Restaurant,
                ScreenMenuId = screen.Id,
                TicketNumerator = ticketNumerator,
                OrderNumerator = orderNumerator,
                IsAlaCarte = true
            };

            _workspace.Add(department);

            var role = new UserRole("Admin") { IsAdmin = true, DepartmentId = 1 };
            _workspace.Add(role);

            var u = new User("Administrator", "1234") { UserRole = role };
            _workspace.Add(u);

            var ticketTemplate = new PrinterTemplate();
            ticketTemplate.Name = Resources.TicketTemplate;
            ticketTemplate.HeaderTemplate = Resources.TicketTemplateHeaderValue;
            ticketTemplate.LineTemplate = Resources.TicketTempleteLineTemplateValue;
            ticketTemplate.GiftLineTemplate = Resources.TicketTemplateGiftedLineTemplateValue;
            ticketTemplate.FooterTemplate = Resources.TicketTemplateFooterValue;

            var kitchenTemplate = new PrinterTemplate();
            kitchenTemplate.Name = Resources.KitchenOrderTemplate;
            kitchenTemplate.HeaderTemplate = Resources.KitchenTemplateHeaderValue;

            kitchenTemplate.LineTemplate = Resources.KitchenTemplateLineTemplateValue;
            kitchenTemplate.GiftLineTemplate = Resources.KitchenTemplateLineTemplateValue;
            kitchenTemplate.VoidedLineTemplate = Resources.KitchenTemplateVoidedLineTemplateValue;

            kitchenTemplate.FooterTemplate = "<F>-";

            var invoiceTemplate = new PrinterTemplate();
            invoiceTemplate.Name = Resources.InvoicePrinterTemplate;
            invoiceTemplate.HeaderTemplate = Resources.InvoiceTemplateHeaderValue;
            invoiceTemplate.LineTemplate = Resources.InvoiceTemplateLineTemplateValue;
            invoiceTemplate.VoidedLineTemplate = "";
            invoiceTemplate.FooterTemplate = "<F>-";

            _workspace.Add(ticketTemplate);
            _workspace.Add(kitchenTemplate);
            _workspace.Add(invoiceTemplate);

            var printer1 = new Printer { Name = Resources.TicketPrinter };
            var printer2 = new Printer { Name = Resources.KitchenPrinter };
            var printer3 = new Printer { Name = Resources.InvoicePrinter };

            _workspace.Add(printer1);
            _workspace.Add(printer2);
            _workspace.Add(printer3);

            var t = new Terminal
            {
                IsDefault = true,
                Name = Resources.Server,
                SlipReportPrinter = printer1,
            };

            var pm1 = new PrinterMap { Printer = printer1, PrinterTemplate = ticketTemplate };
            _workspace.Add(pm1);

            var pj1 = new PrintJob
            {
                Name = Resources.PrintBill,
                ButtonText = Resources.PrintBill,
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

            var pm2 = new PrinterMap { Printer = printer2, PrinterTemplate = kitchenTemplate };
            var pj2 = new PrintJob
            {
                Name = Resources.PrintOrdersToKitchenPrinter,
                ButtonText = "",
                Order = 1,
                WhatToPrint = (int)WhatToPrintTypes.NewLines,
                WhenToPrint = (int)WhenToPrintTypes.NewLinesAdded
            };
            pj2.PrinterMaps.Add(pm2);

            _workspace.Add(pj2);

            t.PrintJobs.Add(pj1);
            t.PrintJobs.Add(pj2);
            _workspace.Add(t);

            ImportMenus(screen);
            ImportTables(department);

            _workspace.CommitChanges();
            _workspace.Dispose();
        }

        private void ImportTables(Department department)
        {
            var fileName = string.Format("{0}/Imports/table{1}.txt", LocalSettings.AppPath, "_" + LocalSettings.CurrentLanguage);

            if (!File.Exists(fileName))
                fileName = string.Format("{0}/Imports/table.txt", LocalSettings.AppPath);

            if (!File.Exists(fileName)) return;

            var lines = File.ReadAllLines(fileName);
            var items = BatchCreateTables(lines, _workspace);
            _workspace.CommitChanges();

            var screen = new TableScreen { Name = Resources.AllTables, ColumnCount = 8 };
            _workspace.Add(screen);

            foreach (var table in items)
                screen.AddScreenItem(table);

            _workspace.CommitChanges();

            department.TableScreenId = screen.Id;
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

        public IEnumerable<Table> BatchCreateTables(string[] values, IWorkspace workspace)
        {
            IList<Table> result = new List<Table>();
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
                        var tableName = value;
                        var count = Dao.Count<Table>(y => y.Name == tableName.Trim());
                        if (count == 0)
                        {
                            var table = new Table { Name = value.Trim(), Category = currentCategory };
                            if (result.Count(x => x.Name.ToLower() == table.Name.ToLower()) == 0)
                            {
                                result.Add(table);
                                workspace.Add(table);
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
                        mi.Portions[0].Price= price;
                        mi.GroupCode = currentCategory;
                        workspace.Add(mi);
                        workspace.Add(mi.Portions[0]);
                        result.Add(mi);
                    }
                }
            }
            return result;
        }

        public IEnumerable<Reason> BatchCreateReasons(string[] values, int reasonType, IWorkspace workspace)
        {
            IList<Reason> result = new List<Reason>();
            if (values.Length > 0)
            {
                foreach (var reason in values.Select(value => new Reason { Name = value, ReasonType = reasonType }))
                {
                    workspace.Add(reason);
                    result.Add(reason);
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
