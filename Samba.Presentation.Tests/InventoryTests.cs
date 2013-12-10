using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Samba.Domain.Builders;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common.DataGeneration;

namespace Samba.Presentation.Tests
{
    [TestFixture]
    public class InventoryTests
    {
        public static IWorkPeriodService WorkPeriodService;
        public static IInventoryService InventoryService;
        public static IApplicationState ApplicationState;
        public static IApplicationStateSetter ApplicationStateSetter;

        [SetUp]
        public void Setup()
        {
            MefBootstrapper.ComposeParts();
            WorkPeriodService = MefBootstrapper.Resolve<IWorkPeriodService>();
            InventoryService = MefBootstrapper.Resolve<IInventoryService>();
            ApplicationState = MefBootstrapper.Resolve<IApplicationState>();
            ApplicationStateSetter = MefBootstrapper.Resolve<IApplicationStateSetter>();
        }

        [Test]
        public void TestCost()
        {
            var workspace = PrepareMenu("sd1.txt");
            var testContext = new WarehouseTestContext();
            CreateWarehouseTestContext(testContext, workspace);

            var transaction = new InventoryTransactionDocument();
            workspace.Add(transaction);

            transaction.Add(testContext.PurchaseTransactionType, testContext.DonerEti, 16, 10, "KG", 1000);
            transaction.Add(testContext.PurchaseTransactionType, testContext.Pide, 1, 50, "Adet", 2);
            transaction.Add(testContext.PurchaseTransactionType, testContext.Yogurt, 4, 30, "KG", 1000);
            transaction.Add(testContext.PurchaseTransactionType, testContext.ZeytinYagi, 5, 5, "Litre", 100);
            transaction.TransactionItems.ToList().ForEach(workspace.Add);

            var ticket = TicketBuilder.Create(TicketType.Default, testContext.Department).Build();
            workspace.Add(ticket);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);

            var pc = InventoryService.GetCurrentPeriodicConsumption();
            workspace.Add(pc);

            var whc = pc.WarehouseConsumptions.Single(x => x.WarehouseId == testContext.LocalWarehouse.Id);

            var iskenderCostItem = whc.CostItems.Single(x => x.MenuItemId == testContext.Iskender.Id);
            Assert.AreEqual(iskenderCostItem.Quantity, 3);

            var etCost = ((16m / 1000m) * 120m);
            var pideCost = ((1m / 2m) * 2m);
            var yogurtCost = ((4m / 1000m) * 50m);
            var zeytinYagiCost = ((5m / 100m) * 1m);
            var iskenderCost = decimal.Round(etCost + pideCost + yogurtCost + zeytinYagiCost, 2);

            Assert.AreEqual(iskenderCost, iskenderCostItem.CostPrediction);
            var etpc = whc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.DonerEti.Id);
            var pidepc = whc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.Pide.Id);
            var yogurtpc = whc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.Yogurt.Id);
            var zeytinYagipc = whc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.ZeytinYagi.Id);

            etpc.PhysicalInventory = 9.5m;
            yogurtpc.PhysicalInventory = 28;
            zeytinYagipc.PhysicalInventory = 4.5m;

            InventoryService.CalculateCost(pc, ApplicationState.CurrentWorkPeriod);

            etCost = (etpc.GetConsumption() * etCost) / etpc.GetPredictedConsumption();
            pideCost = (pidepc.GetConsumption() * pideCost) / pidepc.GetPredictedConsumption();
            yogurtCost = (yogurtpc.GetConsumption() * yogurtCost) / yogurtpc.GetPredictedConsumption();
            zeytinYagiCost = (zeytinYagipc.GetConsumption() * zeytinYagiCost) / zeytinYagipc.GetPredictedConsumption();

            Assert.AreEqual(iskenderCostItem.Cost, decimal.Round(etCost + pideCost + yogurtCost + zeytinYagiCost, 2));
        }

        [Test]
        public void TestPurchase()
        {
            var workspace = PrepareMenu("sd2.txt");

            var testContext = new WarehouseTestContext();
            CreateWarehouseTestContext(testContext, workspace);

            var transaction = new InventoryTransactionDocument();
            workspace.Add(transaction);

            transaction.Add(testContext.PurchaseTransactionType, testContext.DonerEti, 16, 10, "KG", 1000);
            transaction.Add(testContext.PurchaseTransactionType, testContext.Pide, 1, 50, "Adet", 2);
            transaction.Add(testContext.PurchaseTransactionType, testContext.Yogurt, 4, 30, "KG", 1000);
            transaction.TransactionItems.ToList().ForEach(workspace.Add);
            var transactionTotal = transaction.TransactionItems.Sum(x => x.Price * x.Quantity);
            Assert.AreEqual(transactionTotal, (16 * 10) + (50 * 1) + (30 * 4));

            var ticket = TicketBuilder.Create(TicketType.Default, testContext.Department).Build();

            workspace.Add(ticket);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);

            var transaction2 = new InventoryTransactionDocument();
            workspace.Add(transaction2);
            transaction2.Add(testContext.PurchaseTransactionType, testContext.DonerEti, 15, 10, "KG", 1000);
            transaction2.TransactionItems.ToList().ForEach(workspace.Add);
            var pc = InventoryService.GetCurrentPeriodicConsumption();
            workspace.Add(pc);
            var whc = pc.WarehouseConsumptions.Single(x => x.WarehouseId == testContext.LocalWarehouse.Id);

            var etpc = whc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.DonerEti.Id);
            Assert.IsNotNull(etpc);
            Assert.AreEqual(0, etpc.InStock);
            Assert.AreEqual(20, etpc.Purchase);
            Assert.AreEqual(0.24m, etpc.Consumption);
            Assert.AreEqual(15.5m, etpc.Cost);

            var yogurtpc = whc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.Yogurt.Id);
            Assert.IsNotNull(yogurtpc);
            Assert.AreEqual(0, yogurtpc.InStock);
            Assert.AreEqual(30, yogurtpc.Purchase);
            Assert.AreEqual(0.1m, yogurtpc.Consumption);

            var pidepc = whc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.Pide.Id);
            Assert.IsNotNull(pidepc);
            Assert.AreEqual(0, pidepc.InStock);
            Assert.AreEqual(50, pidepc.Purchase);
            Assert.AreEqual(2, pidepc.Consumption);

            RestartWorkperiod(workspace);

            pc = InventoryService.GetCurrentPeriodicConsumption();
            whc = pc.WarehouseConsumptions.Single(x => x.WarehouseId == testContext.LocalWarehouse.Id);
            etpc = whc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.DonerEti.Id);
            Assert.AreEqual(20 - 0.24m, etpc.InStock);
            Assert.AreEqual(0, etpc.Purchase);
            Assert.AreEqual(0, etpc.Consumption);

            transaction = new InventoryTransactionDocument();
            workspace.Add(transaction);
            const int etAlimMiktari = 50;

            var ti = transaction.Add(testContext.PurchaseTransactionType, testContext.DonerEti, 12, etAlimMiktari, "KG", 1000);
            transaction.TransactionItems.ToList().ForEach(workspace.Add);
            ticket = TicketBuilder.Create(TicketType.Default, testContext.Department).Build();

            workspace.Add(ticket);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);

            pc = InventoryService.GetCurrentPeriodicConsumption();
            workspace.Add(pc);
            whc = pc.WarehouseConsumptions.Single(x => x.WarehouseId == testContext.LocalWarehouse.Id);

            var etpc2 = whc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.DonerEti.Id);
            Assert.IsNotNull(etpc2);
            Assert.AreEqual(etpc2.InStock, etpc.GetInventoryPrediction());
            Assert.AreEqual(etpc2.Purchase, etAlimMiktari);
            Assert.AreEqual(etpc2.GetInventoryPrediction(), etpc.GetInventoryPrediction() + etAlimMiktari - 0.24m);
            var cost = ((etpc.Cost * etpc.GetInventoryPrediction()) + (ti.Price * ti.Quantity)) / (etpc2.InStock + etpc2.Purchase);
            cost = decimal.Round(cost, 2);
            Assert.AreEqual(etpc2.Cost, cost);

            RestartWorkperiod(workspace);

            transaction = new InventoryTransactionDocument();
            workspace.Add(transaction);
            ti = transaction.Add(testContext.PurchaseTransactionType, testContext.DonerEti, 10, etAlimMiktari, "KG", 1000);
            transaction.TransactionItems.ToList().ForEach(workspace.Add);
            ticket = TicketBuilder.Create(TicketType.Default, testContext.Department).Build();

            workspace.Add(ticket);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);

            pc = InventoryService.GetCurrentPeriodicConsumption();
            workspace.Add(pc);
            whc = pc.WarehouseConsumptions.Single(x => x.WarehouseId == testContext.LocalWarehouse.Id);

            var etpc3 = whc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.DonerEti.Id);
            Assert.IsNotNull(etpc3);
            Assert.AreEqual(etpc3.InStock, etpc2.GetInventoryPrediction());
            Assert.AreEqual(etpc3.Purchase, etAlimMiktari);
            Assert.AreEqual(etpc3.GetInventoryPrediction(), etpc2.GetInventoryPrediction() + etAlimMiktari - 0.24m);
            cost = ((etpc2.Cost * etpc2.GetInventoryPrediction()) + (ti.Price * ti.Quantity)) / (etpc3.InStock + etpc3.Purchase);
            cost = decimal.Round(cost, 2);
            Assert.AreEqual(etpc3.Cost, cost);
        }

        [Test]
        public void CanReadInventory()
        {
            var workspace = PrepareMenu("sd4.txt");
            var testContext = new WarehouseTestContext();
            CreateWarehouseTestContext(testContext, workspace);
            var inventoryTransaction1 = new InventoryTransactionDocument();
            inventoryTransaction1.Add(testContext.PurchaseTransactionType, testContext.DonerEti, 16, 10, "KG", 1000);
            inventoryTransaction1.Add(testContext.PurchaseTransactionType, testContext.Pide, 1, 50, "Adet", 2);
            inventoryTransaction1.Add(testContext.PurchaseTransactionType, testContext.Yogurt, 4, 30, "KG", 1000);
            inventoryTransaction1.Add(testContext.PurchaseTransactionType, testContext.ZeytinYagi, 5, 5, "Litre", 100);
            workspace.Add(inventoryTransaction1);
            inventoryTransaction1.TransactionItems.ToList().ForEach(workspace.Add);

            Assert.AreEqual(4, inventoryTransaction1.TransactionItems.Count);
            Assert.AreEqual(10, InventoryService.GetInventory(testContext.DonerEti, testContext.LocalWarehouse));
            Assert.AreEqual(50, InventoryService.GetInventory(testContext.Pide, testContext.LocalWarehouse));
            Assert.AreEqual(30, InventoryService.GetInventory(testContext.Yogurt, testContext.LocalWarehouse));
            Assert.AreEqual(5, InventoryService.GetInventory(testContext.ZeytinYagi, testContext.LocalWarehouse));

            inventoryTransaction1.Add(testContext.PurchaseTransactionType, testContext.DonerEti, 16, 15, "KG", 1000);
            inventoryTransaction1.TransactionItems.ToList().ForEach(workspace.Add);
            Assert.AreEqual(25, InventoryService.GetInventory(testContext.DonerEti, testContext.LocalWarehouse));

            var ticket = TicketBuilder.Create(TicketType.Default, testContext.Department).Build();

            workspace.Add(ticket);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);

            Assert.AreEqual(25 - ((120m * 3) / 1000m), InventoryService.GetInventory(testContext.DonerEti, testContext.LocalWarehouse));

            RestartWorkperiod(workspace);

            ticket = TicketBuilder.Create(TicketType.Default, testContext.Department).Build();

            workspace.Add(ticket);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            Assert.AreEqual(25 - ((120m * 6) / 1000m), InventoryService.GetInventory(testContext.DonerEti, testContext.LocalWarehouse));
            Assert.AreEqual(50 - (2 * 6) / 2, InventoryService.GetInventory(testContext.Pide, testContext.LocalWarehouse));
            Assert.AreEqual((-10m * 6) / 100, InventoryService.GetInventory(testContext.Tuz, testContext.LocalWarehouse));
            Assert.AreEqual(0, InventoryService.GetInventory(testContext.Kekik, testContext.LocalWarehouse));
        }

        [Test]
        public void CanFilterUnneededConsumptionItems()
        {
            var workspace = PrepareMenu("sd6.txt");
            var testContext = new WarehouseTestContext();
            CreateWarehouseTestContext(testContext, workspace);
            testContext.Yogurt.Warehouse = "No Warehouse";

            var inventoryTransaction1 = new InventoryTransactionDocument();
            inventoryTransaction1.Add(testContext.PurchaseTransactionType, testContext.Yogurt, 4, 30, "KG", 1000);
            workspace.Add(inventoryTransaction1);
            inventoryTransaction1.TransactionItems.ToList().ForEach(workspace.Add);

            var pc = InventoryService.GetCurrentPeriodicConsumption();
            var whc = pc.WarehouseConsumptions.Single(x => x.WarehouseId == testContext.LocalWarehouse.Id);
            Assert.True(whc.PeriodicConsumptionItems.Any(x => x.InventoryItemId == testContext.Yogurt.Id));
            Assert.AreEqual(30, InventoryService.GetInventory(testContext.Yogurt, testContext.LocalWarehouse));

            var ticket = TicketBuilder.Create(TicketType.Default, testContext.Department).Build();

            workspace.Add(ticket);
            var order = ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            order.Quantity = 600;

            Assert.AreEqual(0, InventoryService.GetInventory(testContext.Yogurt, testContext.LocalWarehouse));

            RestartWorkperiod(workspace);

            pc = InventoryService.GetCurrentPeriodicConsumption();
            whc = pc.WarehouseConsumptions.Single(x => x.WarehouseId == testContext.LocalWarehouse.Id);
            Assert.True(whc.PeriodicConsumptionItems.All(x => x.InventoryItemId != testContext.Yogurt.Id));

            RestartWorkperiod(workspace);
            RestartWorkperiod(workspace);

            pc = InventoryService.GetCurrentPeriodicConsumption();
            whc = pc.WarehouseConsumptions.Single(x => x.WarehouseId == testContext.LocalWarehouse.Id);

            Assert.True(whc.PeriodicConsumptionItems.All(x => x.InventoryItemId != testContext.Yogurt.Id));

            RestartWorkperiod(workspace);

            ticket = TicketBuilder.Create(TicketType.Default, testContext.Department).Build();

            workspace.Add(ticket);
            order = ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            order.Quantity = 600;

            Assert.AreEqual(-30, InventoryService.GetInventory(testContext.Yogurt, testContext.LocalWarehouse));

            RestartWorkperiod(workspace);

            Assert.AreEqual(-30, InventoryService.GetInventory(testContext.Yogurt, testContext.LocalWarehouse));

            var inventoryTransaction2 = new InventoryTransactionDocument();
            inventoryTransaction2.Add(testContext.PurchaseTransactionType, testContext.Yogurt, 4, 30, "KG", 1000);
            workspace.Add(inventoryTransaction2);
            inventoryTransaction2.TransactionItems.ToList().ForEach(workspace.Add);

            Assert.AreEqual(0, InventoryService.GetInventory(testContext.Yogurt, testContext.LocalWarehouse));

            RestartWorkperiod(workspace);

            pc = InventoryService.GetCurrentPeriodicConsumption();
            whc = pc.WarehouseConsumptions.Single(x => x.WarehouseId == testContext.LocalWarehouse.Id);

            Assert.True(whc.PeriodicConsumptionItems.All(x => x.InventoryItemId != testContext.Yogurt.Id));
            Assert.AreEqual(0, InventoryService.GetInventory(testContext.Yogurt, testContext.LocalWarehouse));

            testContext.Yogurt.Warehouse = testContext.BarWarehouse.Name;

            RestartWorkperiod(workspace);

            pc = InventoryService.GetCurrentPeriodicConsumption();
            var bwhc = pc.WarehouseConsumptions.Single(x => x.WarehouseId == testContext.BarWarehouse.Id);

            Assert.True(bwhc.PeriodicConsumptionItems.Any(x => x.InventoryItemId == testContext.Yogurt.Id));
            Assert.AreEqual(0, InventoryService.GetInventory(testContext.Yogurt, testContext.LocalWarehouse));
            Assert.AreEqual(0, InventoryService.GetInventory(testContext.Yogurt, testContext.BarWarehouse));

            whc = pc.WarehouseConsumptions.Single(x => x.WarehouseId == testContext.LocalWarehouse.Id);

            Assert.True(whc.PeriodicConsumptionItems.All(x => x.InventoryItemId != testContext.Yogurt.Id));
            InventoryService.AddMissingItems(whc);
            Assert.True(whc.PeriodicConsumptionItems.Any(x => x.InventoryItemId == testContext.Yogurt.Id));
            InventoryService.FilterUnneededItems(pc);
            Assert.True(whc.PeriodicConsumptionItems.All(x => x.InventoryItemId != testContext.Yogurt.Id));
            InventoryService.AddMissingItems(whc);
            Assert.True(whc.PeriodicConsumptionItems.Any(x => x.InventoryItemId == testContext.Yogurt.Id));
            var pci = whc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.Yogurt.Id);
            pci.PhysicalInventory = 10;
            workspace.Add(pci);
            InventoryService.FilterUnneededItems(pc);
            Assert.True(whc.PeriodicConsumptionItems.Any(x => x.InventoryItemId == testContext.Yogurt.Id));
            Assert.AreEqual(10, InventoryService.GetInventory(testContext.Yogurt, testContext.LocalWarehouse));
        }

        [Test]
        public void CanAssignWarehouse()
        {
            var workspace = PrepareMenu("sd5.txt");
            var testContext = new WarehouseTestContext();
            CreateWarehouseTestContext(testContext, workspace);
            var inventoryTransaction1 = new InventoryTransactionDocument();
            inventoryTransaction1.Add(testContext.PurchaseTransactionType, testContext.DonerEti, 16, 10, "KG", 1000);
            inventoryTransaction1.Add(testContext.PurchaseTransactionType, testContext.Pide, 1, 50, "Adet", 2);
            inventoryTransaction1.Add(testContext.PurchaseTransactionType, testContext.Yogurt, 4, 30, "KG", 1000);
            inventoryTransaction1.Add(testContext.PurchaseTransactionType, testContext.ZeytinYagi, 5, 5, "Litre", 100);
            workspace.Add(inventoryTransaction1);
            inventoryTransaction1.TransactionItems.ToList().ForEach(workspace.Add);

            Assert.AreEqual(10, InventoryService.GetInventory(testContext.DonerEti, testContext.LocalWarehouse));

            var inventoryTransaction2 = new InventoryTransactionDocument();
            inventoryTransaction2.Add(testContext.BarTransferTransactionType, testContext.DonerEti, 16, 5, "KG", 1000);
            workspace.Add(inventoryTransaction2);
            inventoryTransaction2.TransactionItems.ToList().ForEach(workspace.Add);
            Assert.AreEqual(5, InventoryService.GetInventory(testContext.DonerEti, testContext.BarWarehouse));
            Assert.AreEqual(5, InventoryService.GetInventory(testContext.DonerEti, testContext.LocalWarehouse));
        }

        [Test]
        public void CanHandleMultipleCostCalculation()
        {
            var workspace = PrepareMenu("sd7.txt");
            var testContext = new WarehouseTestContext();
            CreateWarehouseTestContext(testContext, workspace);
            var missingItem = testContext.IskenderRecipe.RecipeItems.Last();
            testContext.IskenderRecipe.RecipeItems.Remove(missingItem);
            workspace.CommitChanges();

            var ticket = TicketBuilder.Create(TicketType.Default, testContext.Department).Build();
            workspace.Add(ticket);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            RestartWorkperiod(workspace);

            testContext.IskenderRecipe.RecipeItems.Add(missingItem);
            workspace.CommitChanges();
            ticket = TicketBuilder.Create(TicketType.Default, testContext.Department).Build();
            workspace.Add(ticket);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            RestartWorkperiod(workspace);
        }

        private void RestartWorkperiod(IWorkspace workspace)
        {
            WorkPeriodService.StopWorkPeriod("");
            Thread.Sleep(1);
            var pc = InventoryService.GetCurrentPeriodicConsumption();
            InventoryService.SavePeriodicConsumption(pc);
            foreach (var warehouseConsumption in pc.WarehouseConsumptions)
            {
                warehouseConsumption.PeriodicConsumptionId = pc.Id;
                workspace.Add(warehouseConsumption);
                warehouseConsumption.PeriodicConsumptionItems.ToList().ForEach(x =>
                {
                    x.WarehouseConsumptionId = warehouseConsumption.Id;
                    workspace.Add(x);
                });
            }

            WorkPeriodService.StartWorkPeriod("");
        }

        private static void CreateWarehouseTestContext(WarehouseTestContext testContext, IWorkspace workspace)
        {
            workspace.Delete<InventoryTransactionType>(x => x.Id > 0);
            workspace.Delete<Entity>(x => x.Id > 0);

            testContext.Iskender = workspace.Single<MenuItem>(x => x.Name == "İskender");
            testContext.Iskender.Portions[0].MenuItemId = testContext.Iskender.Id;

            testContext.Doner = workspace.Single<MenuItem>(x => x.Name == "Ankara Döneri");
            testContext.Doner.Portions[0].MenuItemId = testContext.Doner.Id;

            testContext.DonerEti = new InventoryItem { Name = "Döner Eti", BaseUnit = "GR", GroupCode = "", TransactionUnit = "KG", TransactionUnitMultiplier = 1000 };
            testContext.Yogurt = new InventoryItem { Name = "Yoğurt", BaseUnit = "GR", GroupCode = "", TransactionUnit = "KG", TransactionUnitMultiplier = 1000 };
            testContext.Pide = new InventoryItem { Name = "Pide", BaseUnit = "Yarım", GroupCode = "", TransactionUnit = "Adet", TransactionUnitMultiplier = 2 };
            testContext.ZeytinYagi = new InventoryItem { Name = "Zeytin Yağı", BaseUnit = "Ölçü", GroupCode = "", TransactionUnit = "Litre", TransactionUnitMultiplier = 100 };
            testContext.Tuz = new InventoryItem { Name = "Tuz", BaseUnit = "Ölçü", GroupCode = "", TransactionUnit = "Paket", TransactionUnitMultiplier = 100 };
            testContext.Kekik = new InventoryItem { Name = "Kekik", BaseUnit = "Ölçü", GroupCode = "", TransactionUnit = "Paket", TransactionUnitMultiplier = 100 };

            workspace.Add(testContext.DonerEti);
            workspace.Add(testContext.Yogurt);
            workspace.Add(testContext.Pide);
            workspace.Add(testContext.ZeytinYagi);
            workspace.Add(testContext.Tuz);
            workspace.Add(testContext.Kekik);

            testContext.IskenderRecipe = new Recipe { Name = "İskender Reçetesi", Portion = testContext.Iskender.Portions[0] };
            workspace.Add(testContext.IskenderRecipe);

            testContext.IskenderRecipe.RecipeItems.Add(new RecipeItem { InventoryItem = testContext.DonerEti, Quantity = 120 });
            testContext.IskenderRecipe.RecipeItems.Add(new RecipeItem { InventoryItem = testContext.Yogurt, Quantity = 50 });
            testContext.IskenderRecipe.RecipeItems.Add(new RecipeItem { InventoryItem = testContext.Pide, Quantity = 2 });
            testContext.IskenderRecipe.RecipeItems.Add(new RecipeItem { InventoryItem = testContext.ZeytinYagi, Quantity = 1 });
            testContext.IskenderRecipe.RecipeItems.Add(new RecipeItem { InventoryItem = testContext.Tuz, Quantity = 10 });

            testContext.DonerRecipe = new Recipe { Name = "Döner Reçetesi", Portion = testContext.Doner.Portions[0] };
            workspace.Add(testContext.DonerRecipe);
            testContext.DonerRecipe.RecipeItems.Add(new RecipeItem{InventoryItem=testContext.DonerEti,Quantity = 120});

            testContext.LocalWarehouseAccountType = new AccountType { Name = "Local Warehouse Account Type" };
            testContext.SellerWarehouseAccountType = new AccountType { Name = "Seller Warehouse Account Type" };

            workspace.Add(testContext.LocalWarehouseAccountType);
            workspace.Add(testContext.SellerWarehouseAccountType);

            testContext.WarehouseType = workspace.Single<WarehouseType>(x => x.Name == Resources.Warehouses);
            testContext.WarehouseEntityType = new EntityType { Name = "Warehouse Resource Type" };
            workspace.Add(testContext.WarehouseEntityType);

            testContext.LocalWarehouseAccount = new Account { AccountTypeId = testContext.LocalWarehouseAccountType.Id };
            testContext.Seller1Account = new Account { AccountTypeId = testContext.SellerWarehouseAccountType.Id };
            testContext.Seller2Account = new Account { AccountTypeId = testContext.SellerWarehouseAccountType.Id };

            workspace.Add(testContext.LocalWarehouseAccount);
            workspace.Add(testContext.Seller1Account);
            workspace.Add(testContext.Seller2Account);

            testContext.LocalWarehouse = new Warehouse
                {
                    Name = "Local Warehouse",
                    WarehouseTypeId = testContext.WarehouseType.Id
                };
            testContext.BarWarehouse = new Warehouse
                {
                    Name = "Bar Warehouse",
                    WarehouseTypeId = testContext.WarehouseType.Id
                };
            testContext.Seller1Warehouse = new Warehouse
                {
                    WarehouseTypeId = testContext.WarehouseType.Id
                };
            testContext.Seller2Warehouse = new Warehouse
                {
                    WarehouseTypeId = testContext.WarehouseType.Id
                };

            workspace.Add(testContext.LocalWarehouse);
            workspace.Add(testContext.BarWarehouse);
            workspace.Add(testContext.Seller1Warehouse);
            workspace.Add(testContext.Seller2Warehouse);

            testContext.LocalWarehouseEntity = new Entity
                {
                    WarehouseId = testContext.LocalWarehouse.Id,
                    EntityTypeId = testContext.WarehouseEntityType.Id,
                    AccountId = testContext.LocalWarehouseAccount.Id
                };
            testContext.BarWarehouseEntity = new Entity
                {
                    WarehouseId = testContext.BarWarehouse.Id,
                    EntityTypeId = testContext.WarehouseEntityType.Id
                };
            testContext.Seller1WarehouseEntity = new Entity
                {
                    WarehouseId = testContext.Seller1Warehouse.Id,
                    EntityTypeId = testContext.WarehouseEntityType.Id,
                    AccountId = testContext.Seller1Account.Id
                };
            testContext.Seller2WarehouseEntity = new Entity
                {
                    WarehouseId = testContext.Seller2Warehouse.Id,
                    EntityTypeId = testContext.WarehouseEntityType.Id,
                    AccountId = testContext.Seller2Account.Id
                };

            workspace.Add(testContext.LocalWarehouseEntity);
            workspace.Add(testContext.BarWarehouseEntity);
            workspace.Add(testContext.Seller1WarehouseEntity);
            workspace.Add(testContext.Seller2WarehouseEntity);

            testContext.PurchaseAccountTransactionType = new AccountTransactionType
                                                             {
                                                                 SourceAccountTypeId =
                                                                     testContext.SellerWarehouseAccountType.Id,
                                                                 TargetAccountTypeId =
                                                                     testContext.LocalWarehouseAccountType.Id,
                                                                 DefaultTargetAccountId =
                                                                     testContext.LocalWarehouseAccount.Id
                                                             };

            workspace.Add(testContext.PurchaseAccountTransactionType);

            testContext.PurchaseTransactionType = new InventoryTransactionType
                {
                    Name = "PurchaseTransaction",
                    SourceWarehouseTypeId = testContext.WarehouseType.Id,
                    TargetWarehouseTypeId = testContext.WarehouseType.Id,
                    DefaultSourceWarehouseId = testContext.Seller1Warehouse.Id,
                    DefaultTargetWarehouseId = testContext.LocalWarehouse.Id,
                };

            testContext.PurchaseTransactionDocumentType = new InventoryTransactionDocumentType
                {
                    AccountTransactionType = testContext.PurchaseAccountTransactionType,
                    InventoryTransactionType = testContext.PurchaseTransactionType,
                    SourceEntityTypeId = testContext.WarehouseEntityType.Id,
                    TargetEntityTypeId = testContext.WarehouseEntityType.Id,
                    DefaultSourceEntityId = testContext.Seller1WarehouseEntity.Id,
                    DefaultTargetEntityId = testContext.LocalWarehouseEntity.Id
                };

            testContext.BarTransferTransactionType = new InventoryTransactionType
                {
                    Name = "Bar Transfer",
                    SourceWarehouseTypeId = testContext.WarehouseType.Id,
                    TargetWarehouseTypeId = testContext.WarehouseType.Id,
                    DefaultSourceWarehouseId = testContext.LocalWarehouse.Id,
                    DefaultTargetWarehouseId = testContext.BarWarehouse.Id
                };

            testContext.BarTransferTransactionDocumentType = new InventoryTransactionDocumentType
            {
                InventoryTransactionType = testContext.BarTransferTransactionType,
                SourceEntityTypeId = testContext.WarehouseEntityType.Id,
                TargetEntityTypeId = testContext.WarehouseEntityType.Id,
                DefaultSourceEntityId = testContext.LocalWarehouseEntity.Id,
                DefaultTargetEntityId = testContext.BarWarehouseEntity.Id
            };

            workspace.Add(testContext.PurchaseTransactionType);
            workspace.Add(testContext.BarTransferTransactionType);

            testContext.Department = workspace.Single<Department>(x => x.Name == "Restoran");
            testContext.Department.WarehouseId = testContext.LocalWarehouse.Id;

            ApplicationStateSetter.SetCurrentDepartment(testContext.Department.Id);
            WorkPeriodService.StartWorkPeriod("");
            Thread.Sleep(1);
        }

        private static IWorkspace PrepareMenu(string fileName)
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            var lp = new Uri(ass.CodeBase);
            string pth = Path.GetDirectoryName(lp.LocalPath);
            pth = Path.Combine(pth, "..\\..\\..\\Samba.Presentation");
            LocalSettings.AppPath = pth;
            LocalSettings.CurrentLanguage = "tr";
            var dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\tests";
            if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);
            var filePath = string.Format("{0}\\{1}", dataFolder, fileName);
            if (File.Exists(filePath)) File.Delete(filePath);
            WorkspaceFactory.UpdateConnection(filePath);
            var dataCreationService = new DataCreationService();
            dataCreationService.CreateData();
            Thread.Sleep(1);
            var workspace = WorkspaceFactory.Create();
            return workspace;
        }
    }

    internal class WarehouseTestContext
    {
        public MenuItem Iskender { get; set; }
        public MenuItem Doner { get; set; }
        public InventoryItem Tuz { get; set; }
        public InventoryItem ZeytinYagi { get; set; }
        public InventoryItem Pide { get; set; }
        public InventoryItem Yogurt { get; set; }
        public InventoryItem DonerEti { get; set; }
        public InventoryItem Kekik { get; set; }
        public Recipe IskenderRecipe { get; set; }
        public Recipe DonerRecipe { get; set; }

        public AccountType LocalWarehouseAccountType { get; set; }
        public AccountType SellerWarehouseAccountType { get; set; }
        public Account LocalWarehouseAccount { get; set; }
        public Account Seller1Account { get; set; }
        public Account Seller2Account { get; set; }

        public WarehouseType WarehouseType { get; set; }
        public Warehouse LocalWarehouse { get; set; }
        public Warehouse BarWarehouse { get; set; }
        public Warehouse Seller1Warehouse { get; set; }
        public Warehouse Seller2Warehouse { get; set; }

        public EntityType WarehouseEntityType { get; set; }
        public Entity LocalWarehouseEntity { get; set; }
        public Entity BarWarehouseEntity { get; set; }
        public Entity Seller1WarehouseEntity { get; set; }
        public Entity Seller2WarehouseEntity { get; set; }

        public InventoryTransactionType PurchaseTransactionType { get; set; }
        public InventoryTransactionType BarTransferTransactionType { get; set; }

        public Department Department { get; set; }

        public AccountTransactionType PurchaseAccountTransactionType { get; set; }
        public InventoryTransactionDocumentType PurchaseTransactionDocumentType { get; set; }
        public InventoryTransactionDocumentType BarTransferTransactionDocumentType { get; set; }

    }
}
