using System.Data.Entity;
using Samba.Domain.Foundation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Actions;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tables;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Transactions;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data.SQL;
using Samba.Infrastructure.Settings;

namespace Samba.Persistance.Data
{
    public class SambaContext : CommonDbContext
    {
        public SambaContext(bool disableProxy)
            : base(LocalSettings.AppName)
        {
            if (disableProxy)
                ObjContext().ContextOptions.ProxyCreationEnabled = false;
        }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<MenuItemPortion> MenuItemPortions { get; set; }
        public DbSet<MenuItemProperty> MenuItemProperties { get; set; }
        public DbSet<MenuItemPropertyGroup> MenuItemPropertyGroups { get; set; }
        public DbSet<ScreenMenu> ScreenMenus { get; set; }
        public DbSet<ScreenMenuCategory> ScreenMenuCategories { get; set; }
        public DbSet<ScreenMenuItem> ScreenMenuItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketItem> TicketItems { get; set; }
        public DbSet<TicketItemProperty> TicketItemProperties { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Terminal> Terminals { get; set; }
        public DbSet<Printer> Printers { get; set; }
        public DbSet<ProgramSetting> ProgramSettings { get; set; }
        public DbSet<PrinterMap> PrinterMaps { get; set; }
        public DbSet<PrinterTemplate> PrinterTemplates { get; set; }
        public DbSet<TableScreen> TableScreens { get; set; }
        public DbSet<Numerator> Numerators { get; set; }
        public DbSet<Reason> Reasons { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<WorkPeriod> WorkPeriods { get; set; }
        public DbSet<PaidItem> PaidItems { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<CashTransaction> CashTransactions { get; set; }
        public DbSet<AccountTransaction> AccountTransactions { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeItem> RecipeItems { get; set; }
        public DbSet<InventoryTransaction> Transactions { get; set; }
        public DbSet<InventoryTransactionItem> TransactionItems { get; set; }
        public DbSet<PeriodicConsumption> PeriodicConsumptions { get; set; }
        public DbSet<PeriodicConsumptionItem> PeriodicConsumptionItems { get; set; }
        public DbSet<CostItem> CostItems { get; set; }
        public DbSet<TicketTag> TicketTags { get; set; }
        public DbSet<AppAction> RuleActions { get; set; }
        public DbSet<ActionContainer> ActionContainers { get; set; }
        public DbSet<AppRule> Rules { get; set; }
        public DbSet<Trigger> Triggers { get; set; }
        public DbSet<MenuItemPriceDefinition> MenuItemPriceDefinitions { get; set; }
        public DbSet<MenuItemPrice> MenuItemPrices { get; set; }
        public DbSet<TaxTemplate> TaxTemplates { get; set; }
        public DbSet<ServiceTemplate> ServiceTemplates { get; set; }
        public DbSet<Service> Services { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.ComplexType<Price>();

            modelBuilder.ComplexType<DocumentDate>();

            modelBuilder.Entity<MenuItem>().HasMany(p => p.PropertyGroups).WithMany();
            modelBuilder.Entity<Department>().HasMany(p => p.TicketTagGroups).WithMany();
            modelBuilder.Entity<Department>().HasMany(p => p.ServiceTemplates).WithMany();
            modelBuilder.Entity<TableScreen>().HasMany(p => p.Tables).WithMany();
            modelBuilder.Entity<Terminal>().HasMany(p => p.PrintJobs).WithMany();

            const int scale = 2;
            const int precision = 16;

            modelBuilder.ComplexType<Price>().Property(x => x.Amount).HasPrecision(precision, scale);

            //ServiceTemplate

            modelBuilder.Entity<ServiceTemplate>().Property(x => x.Amount).HasPrecision(precision, scale);

            //Service

            modelBuilder.Entity<Service>().Property(x => x.Amount).HasPrecision(precision, scale);
            modelBuilder.Entity<Service>().Property(x => x.CalculationAmount).HasPrecision(precision, scale);

            //TaxTemplate

            modelBuilder.Entity<TaxTemplate>().Property(x => x.Rate).HasPrecision(precision, scale);

            //MenuItemPrice
            modelBuilder.Entity<MenuItemPrice>().Property(x => x.Price).HasPrecision(precision, scale);

            //Recipe
            modelBuilder.Entity<Recipe>().Property(x => x.FixedCost).HasPrecision(precision, scale);

            //CostItem
            modelBuilder.Entity<CostItem>().Property(x => x.Quantity).HasPrecision(precision, scale);
            modelBuilder.Entity<CostItem>().Property(x => x.CostPrediction).HasPrecision(precision, scale);
            modelBuilder.Entity<CostItem>().Property(x => x.Cost).HasPrecision(precision, scale);

            //PeriodicConsumptionIntem
            modelBuilder.Entity<PeriodicConsumptionItem>().Property(x => x.InStock).HasPrecision(precision, scale);
            modelBuilder.Entity<PeriodicConsumptionItem>().Property(x => x.Purchase).HasPrecision(precision, scale);
            modelBuilder.Entity<PeriodicConsumptionItem>().Property(x => x.Consumption).HasPrecision(precision, scale);
            modelBuilder.Entity<PeriodicConsumptionItem>().Property(x => x.PhysicalInventory).HasPrecision(precision, scale);
            modelBuilder.Entity<PeriodicConsumptionItem>().Property(x => x.Cost).HasPrecision(precision, scale);
            modelBuilder.Entity<PeriodicConsumptionItem>().Property(x => x.UnitMultiplier).HasPrecision(precision, scale);

            //RecipeItem
            modelBuilder.Entity<RecipeItem>().Property(x => x.Quantity).HasPrecision(precision, scale);

            //TransactionItem
            modelBuilder.Entity<InventoryTransactionItem>().Property(x => x.Price).HasPrecision(precision, scale);
            modelBuilder.Entity<InventoryTransactionItem>().Property(x => x.Quantity).HasPrecision(precision, scale);

            //CashTransaction
            modelBuilder.Entity<CashTransaction>().Property(x => x.Amount).HasPrecision(precision, scale);

            //AccountTransaction
            modelBuilder.Entity<AccountTransaction>().Property(x => x.Amount).HasPrecision(precision, scale);

            //WorkPeriod
            modelBuilder.Entity<WorkPeriod>().Property(x => x.CashAmount).HasPrecision(precision, scale);
            modelBuilder.Entity<WorkPeriod>().Property(x => x.CreditCardAmount).HasPrecision(precision, scale);
            modelBuilder.Entity<WorkPeriod>().Property(x => x.TicketAmount).HasPrecision(precision, scale);

            //PaidItem
            modelBuilder.Entity<PaidItem>().Property(x => x.Quantity).HasPrecision(precision, scale);
            modelBuilder.Entity<PaidItem>().Property(x => x.Price).HasPrecision(precision, scale);

            //TicketItemProperty
            modelBuilder.Entity<TicketItemProperty>().Property(x => x.Quantity).HasPrecision(precision, scale);
            modelBuilder.Entity<TicketItemProperty>().Property(x => x.TaxAmount).HasPrecision(precision, scale);

            //TicketItem
            modelBuilder.Entity<TicketItem>().Property(x => x.Quantity).HasPrecision(precision, scale);
            modelBuilder.Entity<TicketItem>().Property(x => x.Price).HasPrecision(precision, scale);
            modelBuilder.Entity<TicketItem>().Property(x => x.TaxRate).HasPrecision(precision, scale);
            modelBuilder.Entity<TicketItem>().Property(x => x.TaxAmount).HasPrecision(precision, scale);

            //Ticket
            modelBuilder.Entity<Ticket>().Property(x => x.RemainingAmount).HasPrecision(precision, scale);
            modelBuilder.Entity<Ticket>().Property(x => x.TotalAmount).HasPrecision(precision, scale);

            //Payment
            modelBuilder.Entity<Payment>().Property(x => x.Amount).HasPrecision(precision, scale);

            //Discount
            modelBuilder.Entity<Discount>().Property(x => x.Amount).HasPrecision(precision, scale);
            modelBuilder.Entity<Discount>().Property(x => x.DiscountAmount).HasPrecision(precision, scale);


            //modelBuilder.Entity<MenuItem>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            ////modelBuilder.Entity<MenuItemPortion>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            ////modelBuilder.Entity<MenuItemProperty>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            //modelBuilder.Entity<MenuItemPropertyGroup>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            //modelBuilder.Entity<ScreenMenu>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            ////modelBuilder.Entity<ScreenMenuCategory>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            ////modelBuilder.Entity<ScreenMenuItem>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            ////modelBuilder.Entity<Payment>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            //modelBuilder.Entity<Ticket>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            ////modelBuilder.Entity<TicketItem>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            ////modelBuilder.Entity<TicketItemProperty>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            //modelBuilder.Entity<Department>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            //modelBuilder.Entity<User>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            //modelBuilder.Entity<UserRole>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            //modelBuilder.Entity<Table>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            //modelBuilder.Entity<Terminal>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            //modelBuilder.Entity<Printer>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            //modelBuilder.Entity<ProgramSetting>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            //modelBuilder.Entity<PrinterMap>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            //modelBuilder.Entity<PrinterTemplate>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            //modelBuilder.Entity<CurrencyContext>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            //modelBuilder.Entity<TableScreen>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();
            ////modelBuilder.Entity<TableScreenItem>().Property(x => x.LastUpdateTime).HasStoreType("timestamp").IsConcurrencyToken();

            modelBuilder.Entity<Numerator>().Property(x => x.LastUpdateTime).IsConcurrencyToken().HasColumnType(
                "timestamp");
            base.OnModelCreating(modelBuilder);
        }
    }
}