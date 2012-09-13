using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Actions;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
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
        public DbSet<ScreenMenuCategory> ScreenMenuCategories { get; set; }
        public DbSet<ScreenMenuItem> ScreenMenuItems { get; set; }
        public DbSet<TicketTemplate> TicketTemplates { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Terminal> Terminals { get; set; }
        public DbSet<Printer> Printers { get; set; }
        public DbSet<PrintJob> PrintJobs { get; set; }
        public DbSet<ProgramSettingValue> ProgramSettings { get; set; }
        public DbSet<PrinterMap> PrinterMaps { get; set; }
        public DbSet<PrinterTemplate> PrinterTemplates { get; set; }
        public DbSet<Numerator> Numerators { get; set; }
        public DbSet<WorkPeriod> WorkPeriods { get; set; }
        public DbSet<PaidItem> PaidItems { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketResource> TicketResources { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderTag> OrderTags { get; set; }
        public DbSet<OrderTagGroup> OrderTagGroups { get; set; }
        public DbSet<OrderTagMap> OrderTagMaps { get; set; }        
        public DbSet<OrderState> OrderStates { get; set; }
        public DbSet<OrderStateGroup> OrderStateGroups { get; set; }
        public DbSet<OrderStateMap> OrderStateMaps { get; set; }
        public DbSet<OrderTagTemplate> OrderTagTemplates { get; set; }
        public DbSet<OrderTagTemplateValue> OrderTagTemplateValues { get; set; }
        public DbSet<OrderTagValue> OrderTagValues { get; set; }
        public DbSet<ProductTimer> Productimers { get; set; }
        public DbSet<ProdcutTimerMap> ProductTimerMaps { get; set; }
        public DbSet<ProductTimerValue> ProductTimerValues { get; set; }
        public DbSet<ScreenMenu> ScreenMenus { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeItem> RecipeItems { get; set; }
        public DbSet<InventoryTransaction> Transactions { get; set; }
        public DbSet<InventoryTransactionItem> TransactionItems { get; set; }
        public DbSet<PeriodicConsumption> PeriodicConsumptions { get; set; }
        public DbSet<PeriodicConsumptionItem> PeriodicConsumptionItems { get; set; }
        public DbSet<CostItem> CostItems { get; set; }
        public DbSet<TicketTag> TicketTags { get; set; }
        public DbSet<TicketTagGroup> TicketTagGroups { get; set; }
        public DbSet<TicketTagMap> TicketTagMaps { get; set; }
        public DbSet<AppAction> RuleActions { get; set; }
        public DbSet<ActionContainer> ActionContainers { get; set; }
        public DbSet<AppRule> Rules { get; set; }
        public DbSet<Trigger> Triggers { get; set; }
        public DbSet<AutomationCommand> AutomationCommands { get; set; }
        public DbSet<AutomationCommandMap> AutomationCommandMaps { get; set; }
        public DbSet<MenuItemPriceDefinition> MenuItemPriceDefinitions { get; set; }
        public DbSet<MenuItemPrice> MenuItemPrices { get; set; }
        public DbSet<TaxTemplate> TaxTemplates { get; set; }
        public DbSet<CalculationTemplate> CalculationTemplates { get; set; }
        public DbSet<CalculationSelector> CalculationSelectors { get; set; }
        public DbSet<CalculationSelectorMap> CalculationTemplateMaps { get; set; }
        public DbSet<Calculation> Calculations { get; set; }
        public DbSet<PaymentTemplate> PaymentTemplates { get; set; }
        public DbSet<PaymentTemplateMap> PaymentTemplateMaps { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountTemplate> AccountTemplates { get; set; }
        public DbSet<AccountScreen> AccountScreens { get; set; }
        public DbSet<AccountTransaction> AccountTransactions { get; set; }
        public DbSet<AccountTransactionValue> AccountTransactionValues { get; set; }
        public DbSet<AccountTransactionTemplate> AccountTransactionTemplates { get; set; }
        public DbSet<AccountTransactionDocumentTemplateMap> AccountTransactionDocumentTemplateMaps { get; set; }
        public DbSet<AccountTransactionDocument> AccountTransactionDocuments { get; set; }
        public DbSet<AccountTransactionDocumentTemplate> AccountTransactionDocumentTemplates { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<ResourceTemplate> ResourceTemplates { get; set; }
        public DbSet<ResourceCustomField> ResourceCustomFields { get; set; }
        public DbSet<ResourceScreenItem> ResourceScreenItems { get; set; }
        public DbSet<ResourceScreen> ResourceScreens { get; set; }
        public DbSet<Widget> Widgets { get; set; }
        public DbSet<ResourceState> ResourceStates { get; set; }
        public DbSet<ResourceStateValue> ResourceStateValues { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Resource>().Property(x => x.CustomData).IsMaxLength();
            modelBuilder.Entity<Ticket>().Property(x => x.TicketTags).IsMaxLength();
            modelBuilder.Entity<PrinterTemplate>().Property(x => x.Template).IsMaxLength();
            modelBuilder.Entity<TicketResource>().Property(x => x.ResourceCustomData).IsMaxLength();
            
            modelBuilder.Entity<Department>().HasMany(p => p.ResourceScreens).WithMany();
            modelBuilder.Entity<ResourceScreen>().HasMany(p => p.ScreenItems).WithMany();
            modelBuilder.Entity<CalculationSelector>().HasMany(x => x.CalculationTemplates).WithMany();
            modelBuilder.Entity<AccountTransactionDocumentTemplate>().HasMany(x => x.TransactionTemplates).WithMany();

            modelBuilder.Entity<AccountTransaction>().Ignore(p => p.SourceTransactionValue);
            modelBuilder.Entity<AccountTransaction>().Ignore(p => p.TargetTransactionValue);

            modelBuilder.Entity<AccountTransaction>().HasKey(p => new { p.Id, p.AccountTransactionDocumentId });
            modelBuilder.Entity<AccountTransaction>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<AccountTransactionDocument>().HasMany(p => p.AccountTransactions).WithRequired().HasForeignKey(x => x.AccountTransactionDocumentId);

            modelBuilder.Entity<AccountTransactionValue>().HasKey(p => new { p.Id, p.AccountTransactionId, p.AccountTransactionDocumentId });
            modelBuilder.Entity<AccountTransactionValue>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<AccountTransaction>().HasMany(p => p.AccountTransactionValues).WithRequired().HasForeignKey(x => new { x.AccountTransactionId, x.AccountTransactionDocumentId });

            modelBuilder.Entity<Calculation>().HasKey(p => new { p.Id, p.TicketId });
            modelBuilder.Entity<Calculation>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Ticket>().HasMany(p => p.Calculations).WithRequired().HasForeignKey(x => x.TicketId);

            modelBuilder.Entity<Payment>().HasKey(p => new { p.Id, p.TicketId });
            modelBuilder.Entity<Payment>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Ticket>().HasMany(p => p.Payments).WithRequired().HasForeignKey(x => x.TicketId);

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

            //CalculationTemplate
            modelBuilder.Entity<CalculationTemplate>().Property(x => x.Amount).HasPrecision(precision, scale);

            //Calculation
            modelBuilder.Entity<Calculation>().Property(x => x.Amount).HasPrecision(precision, scale);
            modelBuilder.Entity<Calculation>().Property(x => x.CalculationAmount).HasPrecision(precision, scale);

            //Payment
            modelBuilder.Entity<Payment>().Property(x => x.Amount).HasPrecision(precision, scale);

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
            modelBuilder.Entity<CostItem>().Property(x => x.Quantity).HasPrecision(precision, 3);
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
            modelBuilder.Entity<RecipeItem>().Property(x => x.Quantity).HasPrecision(precision, 3);

            //TransactionItem
            modelBuilder.Entity<InventoryTransactionItem>().Property(x => x.Price).HasPrecision(precision, scale);
            modelBuilder.Entity<InventoryTransactionItem>().Property(x => x.Quantity).HasPrecision(precision, 3);

            //WorkPeriod
            modelBuilder.Entity<WorkPeriod>().Property(x => x.CashAmount).HasPrecision(precision, scale);
            modelBuilder.Entity<WorkPeriod>().Property(x => x.CreditCardAmount).HasPrecision(precision, scale);
            modelBuilder.Entity<WorkPeriod>().Property(x => x.TicketAmount).HasPrecision(precision, scale);

            //PaidItem
            modelBuilder.Entity<PaidItem>().Property(x => x.Quantity).HasPrecision(precision, 3);
            modelBuilder.Entity<PaidItem>().Property(x => x.Price).HasPrecision(precision, scale);

            //OrderTagValue
            modelBuilder.Entity<OrderTagValue>().Property(x => x.Price).HasPrecision(precision, scale);
            modelBuilder.Entity<OrderTagValue>().Property(x => x.Quantity).HasPrecision(precision, 3);
            modelBuilder.Entity<OrderTagValue>().Property(x => x.TaxAmount).HasPrecision(precision, scale);

            //Order
            modelBuilder.Entity<Order>().Property(x => x.Quantity).HasPrecision(precision, 3);
            modelBuilder.Entity<Order>().Property(x => x.Price).HasPrecision(precision, scale);
            modelBuilder.Entity<Order>().Property(x => x.TaxRate).HasPrecision(precision, scale);
            modelBuilder.Entity<Order>().Property(x => x.TaxAmount).HasPrecision(precision, scale);

            //Ticket
            modelBuilder.Entity<Ticket>().Property(x => x.RemainingAmount).HasPrecision(precision, scale);
            modelBuilder.Entity<Ticket>().Property(x => x.TotalAmount).HasPrecision(precision, scale);

            //Account Transaction
            modelBuilder.Entity<AccountTransaction>().Property(x => x.Amount).HasPrecision(precision, scale);
            modelBuilder.Entity<AccountTransactionValue>().Property(x => x.Debit).HasPrecision(precision, scale);
            modelBuilder.Entity<AccountTransactionValue>().Property(x => x.Credit).HasPrecision(precision, scale);

            //MenuItem Timer
            modelBuilder.Entity<ProductTimer>().Property(x => x.PriceDuration).HasPrecision(precision, scale);
            modelBuilder.Entity<ProductTimer>().Property(x => x.MinTime).HasPrecision(precision, scale);
            modelBuilder.Entity<ProductTimer>().Property(x => x.TimeRounding).HasPrecision(precision, scale);

            //MenuItem Timer Value
            modelBuilder.Entity<ProductTimerValue>().Property(x => x.PriceDuration).HasPrecision(precision, scale);
            modelBuilder.Entity<ProductTimerValue>().Property(x => x.MinTime).HasPrecision(precision, scale);
            modelBuilder.Entity<ProductTimerValue>().Property(x => x.TimeRounding).HasPrecision(precision, scale);

            modelBuilder.Entity<Numerator>().Property(x => x.LastUpdateTime).IsConcurrencyToken().HasColumnType("timestamp");
            base.OnModelCreating(modelBuilder);
        }
    }
}