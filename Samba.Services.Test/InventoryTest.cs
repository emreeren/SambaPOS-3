using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Samba.Domain.Foundation;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Settings;
using Samba.Persistance.Data;

namespace Samba.Services.Test
{
    [TestClass]
    public class InventoryTest
    {
        [TestMethod]
        public void TestCost()
        {
            var workspace = PrepareMenu("c:\\sd1.txt");

            var iskender = workspace.Single<MenuItem>(x => x.Name.ToLower().Contains("iskender"));
            iskender.Portions[0].MenuItemId = iskender.Id;

            Assert.IsTrue(workspace.All<MenuItem>().Count() > 0);
            Assert.IsNotNull(iskender);
            Assert.IsTrue(iskender.Portions.Count == 1);

            var donerEti = new InventoryItem { Name = "Döner Eti", BaseUnit = "GR", GroupCode = "", TransactionUnit = "KG", TransactionUnitMultiplier = 1000 };
            var yogurt = new InventoryItem { Name = "Yoğurt", BaseUnit = "GR", GroupCode = "", TransactionUnit = "KG", TransactionUnitMultiplier = 1000 };
            var pide = new InventoryItem { Name = "Pide", BaseUnit = "Yarım", GroupCode = "", TransactionUnit = "Adet", TransactionUnitMultiplier = 2 };
            var zeytinYagi = new InventoryItem { Name = "Zeytin Yağı", BaseUnit = "Ölçü", GroupCode = "", TransactionUnit = "Litre", TransactionUnitMultiplier = 100 };

            workspace.Add(donerEti);
            workspace.Add(yogurt);
            workspace.Add(pide);
            workspace.Add(zeytinYagi);

            var rp = new Recipe { Name = "İskender Reçetesi", Portion = iskender.Portions[0] };
            workspace.Add(rp);

            rp.RecipeItems.Add(new RecipeItem { InventoryItem = donerEti, Quantity = 120 });
            rp.RecipeItems.Add(new RecipeItem { InventoryItem = yogurt, Quantity = 50 });
            rp.RecipeItems.Add(new RecipeItem { InventoryItem = pide, Quantity = 2 });
            rp.RecipeItems.Add(new RecipeItem { InventoryItem = zeytinYagi, Quantity = 1 });

            AppServices.MainDataContext.StartWorkPeriod("", 0, 0, 0);

            var transaction = new InventoryTransaction { Date = DateTime.Now, Name = "1" };
            workspace.Add(transaction);

            transaction.TransactionItems.Add(new InventoryTransactionItem { InventoryItem = donerEti, Multiplier = 1000, Price = 16, Quantity = 10, Unit = "KG" });
            transaction.TransactionItems.Add(new InventoryTransactionItem { InventoryItem = pide, Multiplier = 2, Price = 1, Quantity = 50, Unit = "Adet" });
            transaction.TransactionItems.Add(new InventoryTransactionItem { InventoryItem = yogurt, Multiplier = 1000, Price = 4, Quantity = 30, Unit = "KG" });
            transaction.TransactionItems.Add(new InventoryTransactionItem { InventoryItem = zeytinYagi, Multiplier = 100, Price = 5, Quantity = 5, Unit = "Litre" });

            var ticket = new Ticket();
            workspace.Add(ticket);
            ticket.AddTicketItem(0, iskender, iskender.Portions[0].Name);
            ticket.AddTicketItem(0, iskender, iskender.Portions[0].Name);
            ticket.AddTicketItem(0, iskender, iskender.Portions[0].Name);

            var pc = InventoryService.GetCurrentPeriodicConsumption(workspace);
            workspace.Add(pc);

            var iskenderCostItem = pc.CostItems.Single(x => x.Portion.MenuItemId == iskender.Id);
            Assert.AreEqual(iskenderCostItem.Quantity, 3);

            var etCost = ((16m / 1000m) * 120m);
            var pideCost = ((1m / 2m) * 2m);
            var yogurtCost = ((4m / 1000m) * 50m);
            var zeytinYagiCost = ((5m / 100m) * 1m);
            var iskenderCost = decimal.Round(etCost + pideCost + yogurtCost + zeytinYagiCost, 2);

            Assert.AreEqual(iskenderCost, iskenderCostItem.CostPrediction);
            var etpc = pc.PeriodicConsumptionItems.Single(x => x.InventoryItem.Id == donerEti.Id);
            var pidepc = pc.PeriodicConsumptionItems.Single(x => x.InventoryItem.Id == pide.Id);
            var yogurtpc = pc.PeriodicConsumptionItems.Single(x => x.InventoryItem.Id == yogurt.Id);
            var zeytinYagipc = pc.PeriodicConsumptionItems.Single(x => x.InventoryItem.Id == zeytinYagi.Id);

            etpc.PhysicalInventory = 9.5m;
            yogurtpc.PhysicalInventory = 28;
            zeytinYagipc.PhysicalInventory = 4.5m;

            InventoryService.CalculateCost(pc, AppServices.MainDataContext.CurrentWorkPeriod);

            etCost = (etpc.GetConsumption() * etCost) / etpc.GetPredictedConsumption();
            pideCost = (pidepc.GetConsumption() * pideCost) / pidepc.GetPredictedConsumption();
            yogurtCost = (yogurtpc.GetConsumption() * yogurtCost) / yogurtpc.GetPredictedConsumption();
            zeytinYagiCost = (zeytinYagipc.GetConsumption() * zeytinYagiCost) / zeytinYagipc.GetPredictedConsumption();

            Assert.AreEqual(iskenderCostItem.Cost, decimal.Round(etCost + pideCost + yogurtCost + zeytinYagiCost, 2));
        }

        public void TestPurchase()
        {
            var workspace = PrepareMenu("c:\\sd2.txt");

            var iskender = workspace.Single<MenuItem>(x => x.Name.ToLower().Contains("iskender"));
            iskender.Portions[0].MenuItemId = iskender.Id;

            Assert.IsTrue(workspace.All<MenuItem>().Count() > 0);
            Assert.IsNotNull(iskender);
            Assert.IsTrue(iskender.Portions.Count == 1);

            var donerEti = new InventoryItem { Name = "Döner Eti", BaseUnit = "GR", GroupCode = "", TransactionUnit = "KG", TransactionUnitMultiplier = 1000 };
            var yogurt = new InventoryItem { Name = "Yoğurt", BaseUnit = "GR", GroupCode = "", TransactionUnit = "KG", TransactionUnitMultiplier = 1000 };
            var pide = new InventoryItem { Name = "Pide", BaseUnit = "Yarım", GroupCode = "", TransactionUnit = "Adet", TransactionUnitMultiplier = 2 };

            workspace.Add(donerEti);
            workspace.Add(yogurt);
            workspace.Add(pide);

            var rp = new Recipe { Name = "İskender Reçetesi", Portion = iskender.Portions[0] };
            workspace.Add(rp);

            rp.RecipeItems.Add(new RecipeItem { InventoryItem = donerEti, Quantity = 120 });
            rp.RecipeItems.Add(new RecipeItem { InventoryItem = yogurt, Quantity = 50 });
            rp.RecipeItems.Add(new RecipeItem { InventoryItem = pide, Quantity = 2 });

            AppServices.MainDataContext.StartWorkPeriod("", 0, 0, 0);

            var transaction = new InventoryTransaction { Date = DateTime.Now, Name = "1" };
            workspace.Add(transaction);

            transaction.TransactionItems.Add(new InventoryTransactionItem { InventoryItem = donerEti, Multiplier = 1000, Price = 16, Quantity = 10, Unit = "KG" });
            transaction.TransactionItems.Add(new InventoryTransactionItem { InventoryItem = pide, Multiplier = 2, Price = 1, Quantity = 50, Unit = "Adet" });
            transaction.TransactionItems.Add(new InventoryTransactionItem { InventoryItem = yogurt, Multiplier = 1000, Price = 4, Quantity = 30, Unit = "KG" });

            var transactionTotal = transaction.TransactionItems.Sum(x => x.Price * x.Quantity);
            Assert.AreEqual(transactionTotal, (16 * 10) + (50 * 1) + (30 * 4));

            var ticket = new Ticket();
            workspace.Add(ticket);
            ticket.AddTicketItem(0, iskender, iskender.Portions[0].Name);
            ticket.AddTicketItem(0, iskender, iskender.Portions[0].Name);

            var transaction2 = new InventoryTransaction { Date = DateTime.Now, Name = "1" };
            workspace.Add(transaction2);
            transaction2.TransactionItems.Add(new InventoryTransactionItem { InventoryItem = donerEti, Multiplier = 1000, Price = 15, Quantity = 10, Unit = "KG" });

            var pc = InventoryService.GetCurrentPeriodicConsumption(workspace);
            workspace.Add(pc);

            var etpc = pc.PeriodicConsumptionItems.Single(x => x.InventoryItem.Id == donerEti.Id);
            Assert.IsNotNull(etpc);
            Assert.AreEqual(etpc.InStock, 0);
            Assert.AreEqual(etpc.Purchase, 20);
            Assert.AreEqual(etpc.Consumption, 0.24m);
            Assert.AreEqual(etpc.Cost, 15.5m);

            var yogurtpc = pc.PeriodicConsumptionItems.Single(x => x.InventoryItem.Id == yogurt.Id);
            Assert.IsNotNull(yogurtpc);
            Assert.AreEqual(yogurtpc.InStock, 0);
            Assert.AreEqual(yogurtpc.Purchase, 30);
            Assert.AreEqual(yogurtpc.Consumption, 0.1m);

            var pidepc = pc.PeriodicConsumptionItems.Single(x => x.InventoryItem.Id == pide.Id);
            Assert.IsNotNull(pidepc);
            Assert.AreEqual(pidepc.InStock, 0);
            Assert.AreEqual(pidepc.Purchase, 50);
            Assert.AreEqual(pidepc.Consumption, 2);

            Assert.AreEqual(pc.CostItems.Count(), 1);

            AppServices.MainDataContext.StopWorkPeriod("");
            Thread.Sleep(1);
            AppServices.MainDataContext.StartWorkPeriod("", 0, 0, 0);
            Thread.Sleep(1);

            transaction = new InventoryTransaction { Date = DateTime.Now, Name = "1" };
            workspace.Add(transaction);
            const int etAlimMiktari = 50;
            var ti = new InventoryTransactionItem { InventoryItem = donerEti, Multiplier = 1000, Price = 12, Quantity = etAlimMiktari, Unit = "KG" };
            transaction.TransactionItems.Add(ti);

            ticket = new Ticket();
            workspace.Add(ticket);
            ticket.AddTicketItem(0, iskender, iskender.Portions[0].Name);
            ticket.AddTicketItem(0, iskender, iskender.Portions[0].Name);

            pc = InventoryService.GetCurrentPeriodicConsumption(workspace);
            workspace.Add(pc);
            var etpc2 = pc.PeriodicConsumptionItems.Single(x => x.InventoryItem.Id == donerEti.Id);
            Assert.IsNotNull(etpc2);
            Assert.AreEqual(etpc2.InStock, etpc.GetInventoryPrediction());
            Assert.AreEqual(etpc2.Purchase, etAlimMiktari);
            Assert.AreEqual(etpc2.GetInventoryPrediction(), etpc.GetInventoryPrediction() + etAlimMiktari - 0.24m);
            var cost = ((etpc.Cost * etpc.GetInventoryPrediction()) + (ti.Price * ti.Quantity)) /
                       (etpc2.InStock + etpc2.Purchase);
            cost = decimal.Round(cost, 2);
            Assert.AreEqual(etpc2.Cost, cost);

            AppServices.MainDataContext.StopWorkPeriod("");
            Thread.Sleep(1);
            AppServices.MainDataContext.StartWorkPeriod("", 0, 0, 0);
            Thread.Sleep(1);

            transaction = new InventoryTransaction { Date = DateTime.Now, Name = "1" };
            workspace.Add(transaction);
            ti = new InventoryTransactionItem { InventoryItem = donerEti, Multiplier = 1000, Price = 10, Quantity = etAlimMiktari, Unit = "KG" };
            transaction.TransactionItems.Add(ti);

            ticket = new Ticket();
            workspace.Add(ticket);
            ticket.AddTicketItem(0, iskender, iskender.Portions[0].Name);
            ticket.AddTicketItem(0, iskender, iskender.Portions[0].Name);

            pc = InventoryService.GetCurrentPeriodicConsumption(workspace);
            workspace.Add(pc);
            var etpc3 = pc.PeriodicConsumptionItems.Single(x => x.InventoryItem.Id == donerEti.Id);
            Assert.IsNotNull(etpc3);
            Assert.AreEqual(etpc3.InStock, etpc2.GetInventoryPrediction());
            Assert.AreEqual(etpc3.Purchase, etAlimMiktari);
            Assert.AreEqual(etpc3.GetInventoryPrediction(), etpc2.GetInventoryPrediction() + etAlimMiktari - 0.24m);
            cost = ((etpc2.Cost * etpc2.GetInventoryPrediction()) + (ti.Price * ti.Quantity)) /
                       (etpc3.InStock + etpc3.Purchase);
            cost = decimal.Round(cost, 2);
            Assert.AreEqual(etpc3.Cost, cost);
        }

        private static IWorkspace PrepareMenu(string fileName)
        {
            var pth = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            pth = Path.Combine(pth, "..\\..\\..\\Samba.Presentation");
            LocalSettings.AppPath = pth;
            if (File.Exists(fileName)) File.Delete(fileName);
            WorkspaceFactory.SetDefaultConnectionString(fileName);
            var dataCreationService = new DataCreationService();
            dataCreationService.CreateData();
            var workspace = WorkspaceFactory.Create();
            return workspace;
        }
    }
}
