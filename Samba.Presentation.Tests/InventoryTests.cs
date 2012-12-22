using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Settings;
using Samba.Persistance.Data;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;

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

            var transaction = InventoryTransaction.Create(testContext.PurchaseTransactionType);
            transaction.SetSourceWarehouse(testContext.Seller1Warehouse);
            workspace.Add(transaction);

            transaction.Add(testContext.DonerEti, 16, 10, "KG", 1000);
            transaction.Add(testContext.Pide, 1, 50, "Adet", 2);
            transaction.Add(testContext.Yogurt, 4, 30, "KG", 1000);
            transaction.Add(testContext.ZeytinYagi, 5, 5, "Litre", 100);

            var ticket = Ticket.Create(testContext.Department, TicketType.Default, Account.Null, 1, null);
            workspace.Add(ticket);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);

            var pc = InventoryService.GetCurrentPeriodicConsumption(testContext.LocalWarehouse.Id);
            workspace.Add(pc);

            var iskenderCostItem = pc.CostItems.Single(x => x.MenuItemId == testContext.Iskender.Id);
            Assert.AreEqual(iskenderCostItem.Quantity, 3);

            var etCost = ((16m / 1000m) * 120m);
            var pideCost = ((1m / 2m) * 2m);
            var yogurtCost = ((4m / 1000m) * 50m);
            var zeytinYagiCost = ((5m / 100m) * 1m);
            var iskenderCost = decimal.Round(etCost + pideCost + yogurtCost + zeytinYagiCost, 2);

            Assert.AreEqual(iskenderCost, iskenderCostItem.CostPrediction);
            var etpc = pc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.DonerEti.Id);
            var pidepc = pc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.Pide.Id);
            var yogurtpc = pc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.Yogurt.Id);
            var zeytinYagipc = pc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.ZeytinYagi.Id);

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

            var transaction = InventoryTransaction.Create(testContext.PurchaseTransactionType);
            transaction.SetSourceWarehouse(testContext.Seller1Warehouse);
            workspace.Add(transaction);

            transaction.TransactionItems.Add(new InventoryTransactionItem { InventoryItem = testContext.DonerEti, Multiplier = 1000, Price = 16, Quantity = 10, Unit = "KG" });
            transaction.TransactionItems.Add(new InventoryTransactionItem { InventoryItem = testContext.Pide, Multiplier = 2, Price = 1, Quantity = 50, Unit = "Adet" });
            transaction.TransactionItems.Add(new InventoryTransactionItem { InventoryItem = testContext.Yogurt, Multiplier = 1000, Price = 4, Quantity = 30, Unit = "KG" });

            var transactionTotal = transaction.TransactionItems.Sum(x => x.Price * x.Quantity);
            Assert.AreEqual(transactionTotal, (16 * 10) + (50 * 1) + (30 * 4));

            var ticket = Ticket.Create(testContext.Department, TicketType.Default, Account.Null, 1, null);
            workspace.Add(ticket);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);

            var transaction2 = InventoryTransaction.Create(testContext.PurchaseTransactionType);
            transaction.SetSourceWarehouse(testContext.Seller1Warehouse);
            workspace.Add(transaction2);
            transaction2.TransactionItems.Add(new InventoryTransactionItem { InventoryItem = testContext.DonerEti, Multiplier = 1000, Price = 15, Quantity = 10, Unit = "KG" });

            var pc = InventoryService.GetCurrentPeriodicConsumption(testContext.LocalWarehouse.Id);
            workspace.Add(pc);

            var etpc = pc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.DonerEti.Id);
            Assert.IsNotNull(etpc);
            Assert.AreEqual(0, etpc.InStock);
            Assert.AreEqual(20, etpc.Purchase);
            Assert.AreEqual(0.24m, etpc.Consumption);
            Assert.AreEqual(15.5m, etpc.Cost);

            var yogurtpc = pc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.Yogurt.Id);
            Assert.IsNotNull(yogurtpc);
            Assert.AreEqual(0, yogurtpc.InStock);
            Assert.AreEqual(30, yogurtpc.Purchase);
            Assert.AreEqual(0.1m, yogurtpc.Consumption);

            var pidepc = pc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.Pide.Id);
            Assert.IsNotNull(pidepc);
            Assert.AreEqual(0, pidepc.InStock);
            Assert.AreEqual(50, pidepc.Purchase);
            Assert.AreEqual(2, pidepc.Consumption);

            Assert.AreEqual(pc.CostItems.Count(), 1);

            RestartWorkperiod(workspace);

            transaction = InventoryTransaction.Create(testContext.PurchaseTransactionType);
            transaction.SetSourceWarehouse(testContext.Seller1Warehouse);
            workspace.Add(transaction);
            const int etAlimMiktari = 50;
            var ti = new InventoryTransactionItem { InventoryItem = testContext.DonerEti, Multiplier = 1000, Price = 12, Quantity = etAlimMiktari, Unit = "KG" };
            transaction.TransactionItems.Add(ti);

            ticket = Ticket.Create(testContext.Department, TicketType.Default, Account.Null, 1, null);
            workspace.Add(ticket);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);

            pc = InventoryService.GetCurrentPeriodicConsumption(testContext.LocalWarehouse.Id);
            workspace.Add(pc);
            var etpc2 = pc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.DonerEti.Id);
            Assert.IsNotNull(etpc2);
            Assert.AreEqual(etpc2.InStock, etpc.GetInventoryPrediction());
            Assert.AreEqual(etpc2.Purchase, etAlimMiktari);
            Assert.AreEqual(etpc2.GetInventoryPrediction(), etpc.GetInventoryPrediction() + etAlimMiktari - 0.24m);
            var cost = ((etpc.Cost * etpc.GetInventoryPrediction()) + (ti.Price * ti.Quantity)) /
                       (etpc2.InStock + etpc2.Purchase);
            cost = decimal.Round(cost, 2);
            Assert.AreEqual(etpc2.Cost, cost);

            RestartWorkperiod(workspace);

            transaction = InventoryTransaction.Create(testContext.PurchaseTransactionType);
            transaction.SetSourceWarehouse(testContext.Seller1Warehouse);
            workspace.Add(transaction);
            ti = new InventoryTransactionItem { InventoryItem = testContext.DonerEti, Multiplier = 1000, Price = 10, Quantity = etAlimMiktari, Unit = "KG" };
            transaction.TransactionItems.Add(ti);

            ticket = Ticket.Create(testContext.Department, TicketType.Default, Account.Null, 1, null);
            workspace.Add(ticket);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);

            pc = InventoryService.GetCurrentPeriodicConsumption(testContext.LocalWarehouse.Id);
            workspace.Add(pc);
            var etpc3 = pc.PeriodicConsumptionItems.Single(x => x.InventoryItemId == testContext.DonerEti.Id);
            Assert.IsNotNull(etpc3);
            Assert.AreEqual(etpc3.InStock, etpc2.GetInventoryPrediction());
            Assert.AreEqual(etpc3.Purchase, etAlimMiktari);
            Assert.AreEqual(etpc3.GetInventoryPrediction(), etpc2.GetInventoryPrediction() + etAlimMiktari - 0.24m);
            cost = ((etpc2.Cost * etpc2.GetInventoryPrediction()) + (ti.Price * ti.Quantity)) /
                       (etpc3.InStock + etpc3.Purchase);
            cost = decimal.Round(cost, 2);
            Assert.AreEqual(etpc3.Cost, cost);
        }

        [Test]
        public void CanCreateTransactionType()
        {
            var workspace = PrepareMenu("sd3.txt");
            var testContext = new WarehouseTestContext();
            CreateWarehouseTestContext(testContext, workspace);
            var inventoryTransaction1 = InventoryTransaction.Create(testContext.PurchaseTransactionType);
            inventoryTransaction1.SetSourceWarehouse(testContext.Seller1Warehouse);
            Assert.AreNotEqual(0, testContext.LocalWarehouse.Id);
            Assert.AreEqual(testContext.LocalWarehouse.Id, inventoryTransaction1.TargetWarehouseId);
        }

        [Test]
        public void CanReadInventory()
        {
            var workspace = PrepareMenu("sd4.txt");
            var testContext = new WarehouseTestContext();
            CreateWarehouseTestContext(testContext, workspace);
            var inventoryTransaction1 = InventoryTransaction.Create(testContext.PurchaseTransactionType);
            inventoryTransaction1.SetSourceWarehouse(testContext.Seller1Warehouse);
            inventoryTransaction1.Add(testContext.DonerEti, 16, 10, "KG", 1000);
            inventoryTransaction1.Add(testContext.Pide, 1, 50, "Adet", 2);
            inventoryTransaction1.Add(testContext.Yogurt, 4, 30, "KG", 1000);
            inventoryTransaction1.Add(testContext.ZeytinYagi, 5, 5, "Litre", 100);
            workspace.Add(inventoryTransaction1);

            Assert.AreEqual(4, inventoryTransaction1.TransactionItems.Count);
            Assert.AreEqual(10, InventoryService.GetInventory(testContext.DonerEti, testContext.LocalWarehouse));
            Assert.AreEqual(50, InventoryService.GetInventory(testContext.Pide, testContext.LocalWarehouse));
            Assert.AreEqual(30, InventoryService.GetInventory(testContext.Yogurt, testContext.LocalWarehouse));
            Assert.AreEqual(5, InventoryService.GetInventory(testContext.ZeytinYagi, testContext.LocalWarehouse));

            inventoryTransaction1.Add(testContext.DonerEti, 16, 15, "KG", 1000);
            Assert.AreEqual(25, InventoryService.GetInventory(testContext.DonerEti, testContext.LocalWarehouse));

            var ticket = Ticket.Create(testContext.Department, TicketType.Default, Account.Null, 1, null);
            workspace.Add(ticket);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);

            Assert.AreEqual(25 - ((120m * 3) / 1000m), InventoryService.GetInventory(testContext.DonerEti, testContext.LocalWarehouse));

            RestartWorkperiod(workspace);

            ticket = Ticket.Create(testContext.Department, TicketType.Default, Account.Null, 1, null);
            workspace.Add(ticket);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            ticket.AddOrder(AccountTransactionType.Default, testContext.Department, "Emre", testContext.Iskender, null, testContext.Iskender.Portions[0], "", null);
            Assert.AreEqual(25 - ((120m * 6) / 1000m), InventoryService.GetInventory(testContext.DonerEti, testContext.LocalWarehouse));
            Assert.AreEqual(50 - (2 * 6) / 2, InventoryService.GetInventory(testContext.Pide, testContext.LocalWarehouse));
        }

        [Test]
        public void CanAssignWarehouse()
        {
            var workspace = PrepareMenu("sd5.txt");
            var testContext = new WarehouseTestContext();
            CreateWarehouseTestContext(testContext, workspace);
            var inventoryTransaction1 = InventoryTransaction.Create(testContext.PurchaseTransactionType);
            inventoryTransaction1.SetSourceWarehouse(testContext.Seller1Warehouse);
            inventoryTransaction1.Add(testContext.DonerEti, 16, 10, "KG", 1000);
            inventoryTransaction1.Add(testContext.Pide, 1, 50, "Adet", 2);
            inventoryTransaction1.Add(testContext.Yogurt, 4, 30, "KG", 1000);
            inventoryTransaction1.Add(testContext.ZeytinYagi, 5, 5, "Litre", 100);
            workspace.Add(inventoryTransaction1);

            Assert.AreEqual(testContext.LocalWarehouse.Id, inventoryTransaction1.TargetWarehouseId);
            Assert.AreEqual(testContext.Seller1Warehouse.Id, inventoryTransaction1.SourceWarehouseId);
            Assert.AreEqual(10, InventoryService.GetInventory(testContext.DonerEti, testContext.LocalWarehouse));

            var inventoryTransaction2 = InventoryTransaction.Create(testContext.BarTransferTransactionType);
            inventoryTransaction2.Add(testContext.DonerEti, 16, 5, "KG", 1000);
            workspace.Add(inventoryTransaction2);

            Assert.AreEqual(5, InventoryService.GetInventory(testContext.DonerEti, testContext.BarWarehouse));
            Assert.AreEqual(5, InventoryService.GetInventory(testContext.DonerEti, testContext.LocalWarehouse));
        }

        private void RestartWorkperiod(IWorkspace workspace)
        {
            WorkPeriodService.StopWorkPeriod("");
            Thread.Sleep(1);
            InventoryService.DoWorkPeriodEnd();
            var pc = InventoryService.GetCurrentPeriodicConsumptions();
            foreach (var periodicConsumption in pc)
            {
                InventoryService.SavePeriodicConsumption(periodicConsumption);
                periodicConsumption.PeriodicConsumptionItems.ToList().ForEach(x =>
                {
                    x.PeriodicConsumptionId = periodicConsumption.Id;
                    workspace.Add(x);
                });
            }

            WorkPeriodService.StartWorkPeriod("");
            Thread.Sleep(1);
            InventoryService.DoWorkPeriodStart();
        }

        private static void CreateWarehouseTestContext(WarehouseTestContext testContext, IWorkspace workspace)
        {
            testContext.Iskender = workspace.Single<MenuItem>(x => x.Name == "İskender");
            testContext.Iskender.Portions[0].MenuItemId = testContext.Iskender.Id;

            testContext.DonerEti = new InventoryItem { Name = "Döner Eti", BaseUnit = "GR", GroupCode = "", TransactionUnit = "KG", TransactionUnitMultiplier = 1000 };
            testContext.Yogurt = new InventoryItem { Name = "Yoğurt", BaseUnit = "GR", GroupCode = "", TransactionUnit = "KG", TransactionUnitMultiplier = 1000 };
            testContext.Pide = new InventoryItem { Name = "Pide", BaseUnit = "Yarım", GroupCode = "", TransactionUnit = "Adet", TransactionUnitMultiplier = 2 };
            testContext.ZeytinYagi = new InventoryItem { Name = "Zeytin Yağı", BaseUnit = "Ölçü", GroupCode = "", TransactionUnit = "Litre", TransactionUnitMultiplier = 100 };

            workspace.Add(testContext.DonerEti);
            workspace.Add(testContext.Yogurt);
            workspace.Add(testContext.Pide);
            workspace.Add(testContext.ZeytinYagi);

            testContext.IskenderRecipe = new Recipe { Name = "İskender Reçetesi", Portion = testContext.Iskender.Portions[0] };
            workspace.Add(testContext.IskenderRecipe);

            testContext.IskenderRecipe.RecipeItems.Add(new RecipeItem { InventoryItem = testContext.DonerEti, Quantity = 120 });
            testContext.IskenderRecipe.RecipeItems.Add(new RecipeItem { InventoryItem = testContext.Yogurt, Quantity = 50 });
            testContext.IskenderRecipe.RecipeItems.Add(new RecipeItem { InventoryItem = testContext.Pide, Quantity = 2 });
            testContext.IskenderRecipe.RecipeItems.Add(new RecipeItem { InventoryItem = testContext.ZeytinYagi, Quantity = 1 });

            testContext.LocalWarehouseAccountType = new AccountType { Name = "Local Warehouse Account Type" };
            testContext.SellerWarehouseAccountType = new AccountType { Name = "Seller Warehouse Account Type" };

            workspace.Add(testContext.LocalWarehouseAccountType);
            workspace.Add(testContext.SellerWarehouseAccountType);

            testContext.LocalWarehouseType = new WarehouseType
                {
                    Name = "Local Warehouse",
                    AccountTpeId = testContext.LocalWarehouseAccountType.Id
                };
            testContext.SellerWarehouseType = new WarehouseType
                {
                    Name = "Seller Warehouse",
                    AccountTpeId = testContext.SellerWarehouseAccountType.Id
                };

            workspace.Add(testContext.LocalWarehouseType);
            workspace.Add(testContext.SellerWarehouseType);

            testContext.LocalWarehouseAccount = new Account { AccountTypeId = testContext.LocalWarehouseAccountType.Id };
            testContext.Seller1Account = new Account { AccountTypeId = testContext.SellerWarehouseAccountType.Id };
            testContext.Seller2Account = new Account { AccountTypeId = testContext.SellerWarehouseAccountType.Id };

            workspace.Add(testContext.LocalWarehouseAccount);
            workspace.Add(testContext.Seller1Account);
            workspace.Add(testContext.Seller2Account);

            testContext.LocalWarehouse = new Warehouse
                {
                    WarehouseTypeId = testContext.LocalWarehouseType.Id,
                    AccountId = testContext.LocalWarehouseAccount.Id
                };
            testContext.BarWarehouse = new Warehouse
                {
                    WarehouseTypeId = testContext.LocalWarehouseType.Id
                };
            testContext.Seller1Warehouse = new Warehouse
                {
                    WarehouseTypeId = testContext.SellerWarehouseType.Id,
                    AccountId = testContext.Seller1Account.Id
                };
            testContext.Seller2Warehouse = new Warehouse
                {
                    WarehouseTypeId = testContext.SellerWarehouseType.Id,
                    AccountId = testContext.Seller2Account.Id
                };

            workspace.Add(testContext.LocalWarehouse);
            workspace.Add(testContext.BarWarehouse);
            workspace.Add(testContext.Seller1Warehouse);
            workspace.Add(testContext.Seller2Warehouse);

            testContext.PurchaseTransactionType = new InventoryTransactionType
                {
                    Name = "PurchaseTransaction",
                    SourceWarehouseTypeId = testContext.SellerWarehouseType.Id,
                    TargetWarehouseTypeId = testContext.LocalWarehouseType.Id,
                    DefaultSourceWarehouseId = 0,
                    DefaultTargetWarehouseId = testContext.LocalWarehouse.Id
                };

            testContext.BarTransferTransactionType = new InventoryTransactionType
                {
                    Name = "Bar Transfer",
                    SourceWarehouseTypeId = testContext.LocalWarehouseType.Id,
                    TargetWarehouseTypeId = testContext.LocalWarehouseType.Id,
                    DefaultSourceWarehouseId = testContext.LocalWarehouse.Id,
                    DefaultTargetWarehouseId = testContext.BarWarehouse.Id
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
        public InventoryItem ZeytinYagi { get; set; }
        public InventoryItem Pide { get; set; }
        public InventoryItem Yogurt { get; set; }
        public InventoryItem DonerEti { get; set; }
        public Recipe IskenderRecipe { get; set; }

        public AccountType LocalWarehouseAccountType { get; set; }
        public AccountType SellerWarehouseAccountType { get; set; }
        public WarehouseType LocalWarehouseType { get; set; }
        public WarehouseType SellerWarehouseType { get; set; }
        public Account LocalWarehouseAccount { get; set; }
        public Account Seller1Account { get; set; }
        public Account Seller2Account { get; set; }
        public Warehouse LocalWarehouse { get; set; }
        public Warehouse BarWarehouse { get; set; }
        public Warehouse Seller1Warehouse { get; set; }
        public Warehouse Seller2Warehouse { get; set; }
        public InventoryTransactionType PurchaseTransactionType { get; set; }
        public InventoryTransactionType BarTransferTransactionType { get; set; }

        public Department Department { get; set; }

    }
}
