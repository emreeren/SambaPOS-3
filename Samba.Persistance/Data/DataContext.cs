using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Automation;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tasks;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data.SQL;
using Samba.Infrastructure.Settings;

namespace Samba.Persistance.Data
{
    public class DataContext : CommonDbContext
    {
        public DataContext(bool disableProxy)
            : base(LocalSettings.AppName)
        {
            if (disableProxy)
                ObjContext().ContextOptions.ProxyCreationEnabled = false;
        }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<MenuItemPortion> MenuItemPortions { get; set; }
        public DbSet<ScreenMenuCategory> ScreenMenuCategories { get; set; }
        public DbSet<ScreenMenuItem> ScreenMenuItems { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
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
        public DbSet<TicketEntity> TicketEntities { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderTag> OrderTags { get; set; }
        public DbSet<OrderTagGroup> OrderTagGroups { get; set; }
        public DbSet<OrderTagMap> OrderTagMaps { get; set; }
        public DbSet<ProductTimer> Productimers { get; set; }
        public DbSet<ProdcutTimerMap> ProductTimerMaps { get; set; }
        public DbSet<ProductTimerValue> ProductTimerValues { get; set; }
        public DbSet<ScreenMenu> ScreenMenus { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<WarehouseType> WarehouseTypes { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeItem> RecipeItems { get; set; }
        public DbSet<InventoryTransactionType> InventoryTransactionTypes { get; set; }
        public DbSet<InventoryTransactionDocumentType> InventoryTransactionDocumentTypes { get; set; }
        public DbSet<InventoryTransactionDocument> InventoryTransactionsDocuments { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<PeriodicConsumption> PeriodicConsumptions { get; set; }
        public DbSet<WarehouseConsumption> WarehouseConsumptions { get; set; }
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
        public DbSet<CalculationType> CalculationTypes { get; set; }
        public DbSet<CalculationSelector> CalculationSelectors { get; set; }
        public DbSet<CalculationSelectorMap> CalculationTypeMaps { get; set; }
        public DbSet<Calculation> Calculations { get; set; }
        public DbSet<ForeignCurrency> ForeignCurrencies { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<PaymentTypeMap> PaymentTypeMaps { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ChangePayment> ChangePayments { get; set; }
        public DbSet<ChangePaymentType> ChangePaymentTypes { get; set; }
        public DbSet<ChangePaymentTypeMap> ChangePaymentTypeMaps { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountType> AccountTypes { get; set; }
        public DbSet<AccountScreen> AccountScreens { get; set; }
        public DbSet<AccountTransaction> AccountTransactions { get; set; }
        public DbSet<AccountTransactionValue> AccountTransactionValues { get; set; }
        public DbSet<AccountTransactionType> AccountTransactionTypes { get; set; }
        public DbSet<AccountTransactionDocumentTypeMap> AccountTransactionDocumentTypeMaps { get; set; }
        public DbSet<AccountTransactionDocument> AccountTransactionDocuments { get; set; }
        public DbSet<AccountTransactionDocumentType> AccountTransactionDocumentTypes { get; set; }
        public DbSet<Entity> Entities { get; set; }
        public DbSet<EntityType> EntityTypes { get; set; }
        public DbSet<EntityCustomField> EntityCustomFields { get; set; }
        public DbSet<EntityScreenItem> EntityScreenItems { get; set; }
        public DbSet<EntityScreen> EntityScreens { get; set; }
        public DbSet<Widget> Widgets { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<EntityStateValue> EntityStateValues { get; set; }
        public DbSet<Script> Scripts { get; set; }
        public DbSet<TaskType> TaskTypes { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<TaskToken> TaskResources { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Script>().Property(x => x.Code).IsMaxLength();
            modelBuilder.Entity<Entity>().Property(x => x.CustomData).IsMaxLength();
            modelBuilder.Entity<Ticket>().Property(x => x.TicketTags).IsMaxLength();
            modelBuilder.Entity<Ticket>().Property(x => x.TicketStates).IsMaxLength();
            modelBuilder.Entity<Ticket>().Property(x => x.Note).IsMaxLength();
            modelBuilder.Entity<Ticket>().Property(x => x.TicketLogs).IsMaxLength();
            modelBuilder.Entity<PrinterTemplate>().Property(x => x.Template).IsMaxLength();
            modelBuilder.Entity<TicketEntity>().Property(x => x.EntityCustomData).IsMaxLength();
            modelBuilder.Entity<ActionContainer>().Property(x => x.ParameterValues).IsMaxLength();
            modelBuilder.Entity<AppAction>().Property(x => x.Parameter).IsMaxLength();
            modelBuilder.Entity<AppRule>().Property(x => x.EventConstraints).IsMaxLength();
            modelBuilder.Entity<AppRule>().Property(x => x.RuleConstraints).IsMaxLength();
            modelBuilder.Entity<Order>().Property(x => x.OrderTags).IsMaxLength();
            modelBuilder.Entity<Order>().Property(x => x.OrderStates).IsMaxLength();
            modelBuilder.Entity<Order>().Property(x => x.Taxes).IsMaxLength();
            modelBuilder.Entity<Printer>().Property(x => x.CustomPrinterData).IsMaxLength();
            modelBuilder.Entity<AccountScreen>().Property(x => x.AutomationCommandMapData).IsMaxLength();
            modelBuilder.Entity<ScreenMenuCategory>().Property(x => x.SubButtonColorDef).IsMaxLength();
            modelBuilder.Entity<Task>().Property(x => x.CustomData).IsMaxLength();

            modelBuilder.Entity<CalculationSelector>().HasMany(x => x.CalculationTypes).WithMany();
            modelBuilder.Entity<AccountTransactionDocumentType>().HasMany(x => x.TransactionTypes).WithMany();

            modelBuilder.Entity<WarehouseConsumption>().HasKey(p => new { p.Id, p.PeriodicConsumptionId });
            modelBuilder.Entity<WarehouseConsumption>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<PeriodicConsumption>().HasMany(p => p.WarehouseConsumptions).WithRequired().HasForeignKey(x => x.PeriodicConsumptionId);

            modelBuilder.Entity<PeriodicConsumptionItem>().HasKey(p => new { p.Id, p.WarehouseConsumptionId, p.PeriodicConsumptionId });
            modelBuilder.Entity<PeriodicConsumptionItem>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<WarehouseConsumption>().HasMany(p => p.PeriodicConsumptionItems).WithRequired().HasForeignKey(x => new { x.PeriodicConsumptionId, x.WarehouseConsumptionId });

            modelBuilder.Entity<CostItem>().HasKey(p => new { p.Id, p.WarehouseConsumptionId, p.PeriodicConsumptionId });
            modelBuilder.Entity<CostItem>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<WarehouseConsumption>().HasMany(p => p.CostItems).WithRequired().HasForeignKey(x => new { x.PeriodicConsumptionId, x.WarehouseConsumptionId });

            modelBuilder.Entity<TaskToken>().HasKey(p => new { p.Id, p.TaskId });
            modelBuilder.Entity<TaskToken>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Task>().HasMany(p => p.TaskTokens).WithRequired().HasForeignKey(x => x.TaskId);

            modelBuilder.Entity<TaskCustomField>().HasKey(p => new { p.Id, p.TaskTypeId });
            modelBuilder.Entity<TaskCustomField>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<TaskType>().HasMany(p => p.TaskCustomFields).WithRequired().HasForeignKey(x => x.TaskTypeId);

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

            modelBuilder.Entity<PaidItem>().HasKey(p => new { p.Id, p.TicketId });
            modelBuilder.Entity<PaidItem>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Ticket>().HasMany(p => p.PaidItems).WithRequired().HasForeignKey(x => x.TicketId);

            modelBuilder.Entity<ChangePayment>().HasKey(p => new { p.Id, p.TicketId });
            modelBuilder.Entity<ChangePayment>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Ticket>().HasMany(p => p.ChangePayments).WithRequired().HasForeignKey(x => x.TicketId);

            modelBuilder.Entity<EntityScreenItem>().HasKey(p => new { p.Id, p.EntityScreenId });
            modelBuilder.Entity<EntityScreenItem>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<EntityScreen>().HasMany(p => p.ScreenItems).WithRequired().HasForeignKey(x => x.EntityScreenId);

            modelBuilder.Entity<Order>().Ignore(p => p.IsSelected);
            modelBuilder.Entity<Order>().HasKey(p => new { p.Id, p.TicketId });
            modelBuilder.Entity<Order>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<Ticket>().HasMany(p => p.Orders).WithRequired().HasForeignKey(x => x.TicketId);

            modelBuilder.Entity<MenuItemPrice>().HasKey(p => new { p.Id, p.MenuItemPortionId });
            modelBuilder.Entity<MenuItemPrice>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<MenuItemPortion>().HasMany(p => p.Prices).WithRequired().HasForeignKey(x => x.MenuItemPortionId);

            modelBuilder.Entity<AccountScreenValue>().HasKey(p => new { p.Id, p.AccountScreenId });
            modelBuilder.Entity<AccountScreenValue>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<AccountScreen>().HasMany(p => p.AccountScreenValues).WithRequired().HasForeignKey(x => x.AccountScreenId);

            modelBuilder.Entity<EntityTypeAssignment>().HasKey(p => new { p.Id, p.TicketTypeId });
            modelBuilder.Entity<EntityTypeAssignment>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<TicketType>().HasMany(p => p.EntityTypeAssignments).WithRequired().HasForeignKey(x => x.TicketTypeId);

            modelBuilder.Entity<MenuAssignment>().HasKey(p => new { p.Id, p.TicketTypeId });
            modelBuilder.Entity<MenuAssignment>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<TicketType>().HasMany(p => p.MenuAssignments).WithRequired().HasForeignKey(x => x.TicketTypeId);

            const int qscale = 3;
            const int scale = 2;
            const int precision = 16;

            //CalculationType
            modelBuilder.Entity<CalculationType>().Property(x => x.Amount).HasPrecision(precision, scale);

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
            modelBuilder.Entity<CostItem>().Property(x => x.Quantity).HasPrecision(precision, qscale);
            modelBuilder.Entity<CostItem>().Property(x => x.CostPrediction).HasPrecision(precision, scale);
            modelBuilder.Entity<CostItem>().Property(x => x.Cost).HasPrecision(precision, scale);

            //PeriodicConsumptionIntem
            modelBuilder.Entity<PeriodicConsumptionItem>().Property(x => x.InStock).HasPrecision(precision, qscale);
            modelBuilder.Entity<PeriodicConsumptionItem>().Property(x => x.Added).HasPrecision(precision, qscale);
            modelBuilder.Entity<PeriodicConsumptionItem>().Property(x => x.Removed).HasPrecision(precision, qscale);
            modelBuilder.Entity<PeriodicConsumptionItem>().Property(x => x.Consumption).HasPrecision(precision, qscale);
            modelBuilder.Entity<PeriodicConsumptionItem>().Property(x => x.PhysicalInventory).HasPrecision(precision, qscale);
            modelBuilder.Entity<PeriodicConsumptionItem>().Property(x => x.Cost).HasPrecision(precision, scale);
            modelBuilder.Entity<PeriodicConsumptionItem>().Property(x => x.UnitMultiplier).HasPrecision(precision, scale);

            //RecipeItem
            modelBuilder.Entity<RecipeItem>().Property(x => x.Quantity).HasPrecision(precision, qscale);

            //TransactionItem
            modelBuilder.Entity<InventoryTransaction>().Property(x => x.Price).HasPrecision(precision, scale);
            modelBuilder.Entity<InventoryTransaction>().Property(x => x.Quantity).HasPrecision(precision, qscale);

            //PaidItem
            modelBuilder.Entity<PaidItem>().Property(x => x.Quantity).HasPrecision(precision, qscale);

            //Order
            modelBuilder.Entity<Order>().Property(x => x.Quantity).HasPrecision(precision, qscale);
            modelBuilder.Entity<Order>().Property(x => x.Price).HasPrecision(precision, scale);

            //Ticket
            modelBuilder.Entity<Ticket>().Property(x => x.RemainingAmount).HasPrecision(precision, scale);
            modelBuilder.Entity<Ticket>().Property(x => x.TotalAmount).HasPrecision(precision, scale);

            //Account Transaction
            modelBuilder.Entity<AccountTransaction>().Property(x => x.Amount).HasPrecision(precision, scale);
            modelBuilder.Entity<AccountTransaction>().Property(x => x.ExchangeRate).HasPrecision(precision, scale);
            modelBuilder.Entity<AccountTransactionValue>().Property(x => x.Debit).HasPrecision(precision, scale);
            modelBuilder.Entity<AccountTransactionValue>().Property(x => x.Credit).HasPrecision(precision, scale);
            modelBuilder.Entity<AccountTransactionValue>().Property(x => x.Exchange).HasPrecision(precision, scale);

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