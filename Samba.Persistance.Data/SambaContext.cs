using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using Samba.Domain.Models;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Actions;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Locations;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
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
        public DbSet<OrderTag> OrderTags { get; set; }
        public DbSet<OrderTagGroup> OrderTagGroups { get; set; }
        public DbSet<OrderTagTemplate> OrderTagTemplates { get; set; }
        public DbSet<OrderTagTemplateValue> OrderTagTemplateValues { get; set; }
        public DbSet<OrderTagMap> OrderTagMaps { get; set; }
        public DbSet<ScreenMenu> ScreenMenus { get; set; }
        public DbSet<ScreenMenuCategory> ScreenMenuCategories { get; set; }
        public DbSet<ScreenMenuItem> ScreenMenuItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<TicketTemplate> TicketTemplates { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderTagValue> OrderTagValues { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Terminal> Terminals { get; set; }
        public DbSet<Printer> Printers { get; set; }
        public DbSet<ProgramSettingValue> ProgramSettings { get; set; }
        public DbSet<PrinterMap> PrinterMaps { get; set; }
        public DbSet<PrinterTemplate> PrinterTemplates { get; set; }
        public DbSet<LocationScreen> LocationScreens { get; set; }
        public DbSet<Numerator> Numerators { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<WorkPeriod> WorkPeriods { get; set; }
        public DbSet<PaidItem> PaidItems { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountTemplate> AccountTemplates { get; set; }
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
            modelBuilder.Entity<TicketTemplate>().HasMany(p => p.TicketTagGroups).WithMany();
            modelBuilder.Entity<TicketTemplate>().HasMany(p => p.ServiceTemplates).WithMany();
            modelBuilder.Entity<TicketTemplate>().HasMany(p => p.OrderTagGroups).WithMany();

            modelBuilder.Entity<Account>().Property(x => x.CustomData).IsMaxLength();

            modelBuilder.Entity<Department>().HasMany(p => p.LocationScreens).WithMany();

            modelBuilder.Entity<LocationScreen>().HasMany(p => p.Locations).WithMany();
            modelBuilder.Entity<Terminal>().HasMany(p => p.PrintJobs).WithMany();

            modelBuilder.Entity<TicketTagValue>().HasKey(p => new { p.Id, p.TicketId });
            modelBuilder.Entity<TicketTagValue>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Ticket>().HasMany(p => p.Tags).WithRequired().HasForeignKey(x => x.TicketId);

            modelBuilder.Entity<Service>().HasKey(p => new { p.Id, p.TicketId });
            modelBuilder.Entity<Service>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Ticket>().HasMany(p => p.Services).WithRequired().HasForeignKey(x => x.TicketId);

            modelBuilder.Entity<Order>().HasKey(p => new { p.Id, p.TicketId });
            modelBuilder.Entity<Order>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Ticket>().HasMany(p => p.Orders).WithRequired().HasForeignKey(x => x.TicketId);

            modelBuilder.Entity<OrderTagValue>().HasKey(p => new { p.Id, p.OrderId, p.TicketId });
            modelBuilder.Entity<OrderTagValue>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Order>().HasMany(p => p.OrderTagValues).WithRequired().HasForeignKey(x => new { x.OrderId, x.TicketId });

            modelBuilder.Entity<MenuItemPrice>().HasKey(p => new { p.Id, p.MenuItemPortionId });
            modelBuilder.Entity<MenuItemPrice>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<MenuItemPortion>().HasMany(p => p.Prices).WithRequired().HasForeignKey(x => x.MenuItemPortionId);

            const int scale = 2;
            const int precision = 16;

            //ServiceTemplate
            modelBuilder.Entity<ServiceTemplate>().Property(x => x.Amount).HasPrecision(precision, scale);

            //Service
            modelBuilder.Entity<Service>().Property(x => x.Amount).HasPrecision(precision, scale);
            modelBuilder.Entity<Service>().Property(x => x.CalculationAmount).HasPrecision(precision, scale);

            //TaxTemplate
            modelBuilder.Entity<TaxTemplate>().Property(x => x.Rate).HasPrecision(precision, scale);

            //MenuItemPrice
            modelBuilder.Entity<MenuItemPrice>().Property(x => x.Price).HasPrecision(precision, scale);

            //MenuItemPortion
            modelBuilder.Entity<MenuItemPortion>().Ignore(p => p.Price);

            //MenuItemProperty
            modelBuilder.Entity<OrderTag>().Property(x => x.Price).HasPrecision(precision, scale);

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

            //OrderTagValue
            modelBuilder.Entity<OrderTagValue>().Property(x => x.Price).HasPrecision(precision, scale);
            modelBuilder.Entity<OrderTagValue>().Property(x => x.Quantity).HasPrecision(precision, scale);
            modelBuilder.Entity<OrderTagValue>().Property(x => x.TaxAmount).HasPrecision(precision, scale);

            //Order
            modelBuilder.Entity<Order>().Property(x => x.Quantity).HasPrecision(precision, scale);
            modelBuilder.Entity<Order>().Property(x => x.Price).HasPrecision(precision, scale);
            modelBuilder.Entity<Order>().Property(x => x.TaxRate).HasPrecision(precision, scale);
            modelBuilder.Entity<Order>().Property(x => x.TaxAmount).HasPrecision(precision, scale);

            //Ticket
            modelBuilder.Entity<Ticket>().Property(x => x.RemainingAmount).HasPrecision(precision, scale);
            modelBuilder.Entity<Ticket>().Property(x => x.TotalAmount).HasPrecision(precision, scale);

            //Payment
            modelBuilder.Entity<Payment>().Property(x => x.Amount).HasPrecision(precision, scale);

            //Discount
            modelBuilder.Entity<Discount>().Property(x => x.Amount).HasPrecision(precision, scale);
            modelBuilder.Entity<Discount>().Property(x => x.DiscountAmount).HasPrecision(precision, scale);

            modelBuilder.Entity<Numerator>().Property(x => x.LastUpdateTime).IsConcurrencyToken().HasColumnType(
                "timestamp");
            base.OnModelCreating(modelBuilder);
        }
    }
}