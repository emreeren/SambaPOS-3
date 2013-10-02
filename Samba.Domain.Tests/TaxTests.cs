using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Samba.Domain.Builders;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;

namespace Samba.Domain.Tests
{
    [TestFixture]
    public class TaxTests
    {
        private static MenuItem CreateMenuItem(int id, string name, decimal price)
        {
            return MenuItemBuilder.Create(name).WithId(id).AddPortion("Portion", price).Build();
        }

        [SetUp]
        public void Setup()
        {
            Pizza = CreateMenuItem(1, "Pizza", 10);
            Cola = CreateMenuItem(2, "Cola", 5);
            Beer = CreateMenuItem(3, "Beer", 10);
            Product = CreateMenuItem(4, "Product", 1);

            var saleAccountType = new AccountType { Name = "Sales Accounts", Id = 1 };
            var taxAccountType = new AccountType { Name = "Tax Accounts", Id = 2 };
            var receivableAccountType = new AccountType { Name = "Receivable Accounts", Id = 3 };
            var discountAccountType = new AccountType { Name = "Discount Accounts", Id = 4 };
            var defaultSaleAccount = new Account { AccountTypeId = saleAccountType.Id, Name = "Sales", Id = 1 };
            var receivableAccount = new Account { AccountTypeId = receivableAccountType.Id, Name = "Receivables", Id = 2 };
            var stateTaxAccount = new Account { AccountTypeId = taxAccountType.Id, Name = "State Tax", Id = 3 };
            var localTaxAccount = new Account { AccountTypeId = taxAccountType.Id, Name = "Local Tax", Id = 4 };
            var defaultDiscountAccount = new Account { AccountTypeId = discountAccountType.Id, Name = "Discount", Id = 5 };

            var saleTransactionType = new AccountTransactionType
            {
                Id = 1,
                Name = "Sale Transaction",
                SourceAccountTypeId = saleAccountType.Id,
                TargetAccountTypeId = receivableAccountType.Id,
                DefaultSourceAccountId = defaultSaleAccount.Id,
                DefaultTargetAccountId = receivableAccount.Id
            };

            var localTaxTransactionType = new AccountTransactionType
            {
                Id = 2,
                Name = "Local Tax Transaction",
                SourceAccountTypeId = taxAccountType.Id,
                TargetAccountTypeId = receivableAccountType.Id,
                DefaultSourceAccountId = localTaxAccount.Id,
                DefaultTargetAccountId = receivableAccount.Id
            };

            var stateTaxTransactionType = new AccountTransactionType
            {
                Id = 3,
                Name = "State Tax Transaction",
                SourceAccountTypeId = taxAccountType.Id,
                TargetAccountTypeId = receivableAccountType.Id,
                DefaultSourceAccountId = stateTaxAccount.Id,
                DefaultTargetAccountId = receivableAccount.Id
            };

            DiscountTransactionType = new AccountTransactionType
            {
                Id = 4,
                Name = "Discount Transaction",
                SourceAccountTypeId = receivableAccountType.Id,
                TargetAccountTypeId = discountAccountType.Id,
                DefaultSourceAccountId = receivableAccount.Id,
                DefaultTargetAccountId = defaultDiscountAccount.Id
            };

            var stateTax = TaxTemplateBuilder.Create("State Tax")
                                             .WithRate(25)
                                             .WithRounding(2)
                                             .WithAccountTransactionType(stateTaxTransactionType)
                                             .AddDefaultTaxTemplateMap()
                                             .Build();

            var localTax = TaxTemplateBuilder.Create("Local Tax").WithRate(3)
                                             .AddTaxTemplateMap(new TaxTemplateMap { MenuItemId = Cola.Id })
                                             .AddTaxTemplateMap(new TaxTemplateMap { MenuItemId = Beer.Id })
                                             .WithAccountTransactionType(localTaxTransactionType)
                                             .WithRounding(2)
                                             .Build();


            TaxTemplates = new List<TaxTemplate> { stateTax, localTax };

            TicketType = new TicketType { SaleTransactionType = saleTransactionType, TaxIncluded = true };
        }

        protected AccountTransactionType DiscountTransactionType { get; set; }
        protected TicketType TicketType { get; set; }

        public MenuItem Pizza { get; set; }
        public MenuItem Cola { get; set; }
        public MenuItem Beer { get; set; }
        public MenuItem Product { get; set; }

        public IEnumerable<TaxTemplate> TaxTemplates { get; set; }

        public IList<TaxTemplate> GetTaxTemplates(int menuItemId)
        {
            return TaxTemplates.Where(x => x.TaxTemplateMaps.Any(y => y.MenuItemId == menuItemId || y.MenuItemId == 0)).ToList();
        }

        [Test]
        public void CanApplyTax()
        {
            Assert.AreEqual(25, GetTaxTemplates(Pizza.Id).Sum(x => x.Rate));
            Assert.AreEqual(28, GetTaxTemplates(Cola.Id).Sum(x => x.Rate));
            Assert.AreEqual(28, GetTaxTemplates(Beer.Id).Sum(x => x.Rate));
            Assert.AreEqual(10, Pizza.Portions[0].Price);
            Assert.AreEqual(5, Cola.Portions[0].Price);
        }

        [Test]
        public void OrderBuilder_CreatesTaxExcludedOrder_CalculatesVisibleValue()
        {
            var order = OrderBuilder.Create(AccountTransactionType.Default, Department.Default)
                                    .ForMenuItem(Pizza)
                                    .WithTaxTemplates(GetTaxTemplates(Pizza.Id))
                                    .Build();
            Assert.AreEqual(10, order.GetVisibleValue());
        }

        [Test]
        public void CanCalculateTicket()
        {
            var ticket = TicketBuilder.Create(TicketType, Department.Default)
                .AddOrderFor(Pizza).Do()
                .Build();
            Assert.AreEqual(10, ticket.GetSum());
        }

        [Test]
        public void CanCalculateTax()
        {
            var ticket = TicketBuilder.Create(TicketType, Department.Default)
                .AddOrderFor(Pizza).WithTaxTemplates(GetTaxTemplates(Pizza.Id)).Do()
                .Build();
            Assert.AreEqual(2, ticket.GetTaxTotal());
        }

        [Test]
        public void CanCalculateTaxWhenVoidOrderExists()
        {
            var ticket = TicketBuilder.Create(TicketType, Department.Default)
                .AddOrderFor(Pizza).WithTaxTemplates(GetTaxTemplates(Pizza.Id)).Do()
                .AddOrderFor(Pizza).WithTaxTemplates(GetTaxTemplates(Pizza.Id)).CalculatePrice(false).Do()
                .Build();
            Assert.AreEqual(2, ticket.GetTaxTotal());
        }

        [Test]
        public void CanCalculateExcludedTax()
        {
            var ticket = TicketBuilder.Create(TicketType, Department.Default).TaxExcluded()
                .AddOrderFor(Pizza).WithTaxTemplates(GetTaxTemplates(Pizza.Id)).Do()
                .Build();
            Assert.AreEqual(10 + (10 * .25), ticket.GetSum());
        }

        [Test]
        public void CanCreateExcludedTaxTransaction()
        {
            var ticket = TicketBuilder.Create(TicketType, Department.Default).TaxExcluded()
                .AddOrderFor(Pizza).WithTaxTemplates(GetTaxTemplates(Pizza.Id)).WithQuantity(3).Do()
                .Build();
            Assert.AreEqual((10 + (10 * .25)) * 3, ticket.TransactionDocument.AccountTransactions.Where(x => x.TargetAccountTypeId == 3).Sum(x => x.Amount) - ticket.TransactionDocument.AccountTransactions.Where(x => x.SourceAccountTypeId == 3).Sum(x => x.Amount));
        }

        [Test]
        public void CanCalculateDoubleMultipleTax()
        {
            const decimal orderQuantiy = 2;
            var ticket = TicketBuilder.Create(TicketType, Department.Default)
                .AddOrderFor(Beer).WithTaxTemplates(GetTaxTemplates(Beer.Id)).WithQuantity(2).Do().Build();

            const decimal expTax = 2.18m * orderQuantiy;
            const decimal expStTax = 1.95m;
            const decimal expLcTax = 0.23m;

            Assert.AreEqual(expTax, ticket.GetTaxTotal());
            Assert.AreEqual(expLcTax * orderQuantiy, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 2).Amount);
            Assert.AreEqual(expStTax * orderQuantiy, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 3).Amount);
        }

        [Test]
        public void CanCalculateDoubleOrdersMultipleTax()
        {
            var ticket = TicketBuilder.Create(TicketType, Department.Default)
                .AddOrderFor(Beer).WithTaxTemplates(GetTaxTemplates(Beer.Id)).Do()
                .AddOrderFor(Pizza).WithTaxTemplates(GetTaxTemplates(Pizza.Id)).WithPrice(5).Do()
                .Build();

            const decimal expTax = 2.18m + 1;
            const decimal expStTax = 1.95m + 1;
            const decimal expLcTax = 0.23m;

            Assert.AreEqual(expTax, ticket.GetTaxTotal());
            Assert.AreEqual(ticket.GetSum() - expTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == TicketType.SaleTransactionType.Id).Amount);
            Assert.AreEqual(expLcTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 2).Amount);
            Assert.AreEqual(expStTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 3).Amount);
        }

        [Test]
        public void CanCalculateDoubleOrdersMultipleTaxWithOrderTag()
        {
            var ticket = TicketBuilder.Create(TicketType, Department.Default)
                .AddOrderFor(Beer).WithTaxTemplates(GetTaxTemplates(Beer.Id)).Do()
                .AddOrderFor(Pizza).WithTaxTemplates(GetTaxTemplates(Pizza.Id)).WithPrice(5)
                    .ToggleOrderTag(new OrderTagGroup { AddTagPriceToOrderPrice = true }, new OrderTag { Price = 5 }).Do()
                .Build();

            const decimal expTax = 2.18m + 2;
            const decimal expStTax = 1.95m + 2;
            const decimal expLcTax = 0.23m;

            Assert.AreEqual(expTax, ticket.GetTaxTotal());
            Assert.AreEqual(ticket.GetSum() - expTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == TicketType.SaleTransactionType.Id).Amount);
            Assert.AreEqual(expLcTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 2).Amount);
            Assert.AreEqual(expStTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 3).Amount);
        }

        [Test]
        public void CanCalculateDoubleOrdersMultipleTaxWithMultipleOrderTag()
        {
            var ticket = TicketBuilder.Create(TicketType, Department.Default)
                .AddOrderFor(Beer).WithTaxTemplates(GetTaxTemplates(Beer.Id)).Do()
                .AddOrderFor(Pizza).WithTaxTemplates(GetTaxTemplates(Pizza.Id))
                    .ToggleOrderTag(new OrderTagGroup { Name = "OT1", Id = 1, AddTagPriceToOrderPrice = true }, new OrderTag { Name = "t1", Id = 1, Price = 5 })
                    .ToggleOrderTag(new OrderTagGroup { Name = "OT2", Id = 2, AddTagPriceToOrderPrice = false }, new OrderTag { Name = "t2", Id = 2, Price = 5 })
                    .WithPrice(5).Do()
                .Build();

            const decimal expTax = 2.18m + 3;
            const decimal expStTax = 1.95m + 3;
            const decimal expLcTax = 0.23m;

            Assert.AreEqual(expTax, ticket.GetTaxTotal());
            Assert.AreEqual(ticket.GetSum() - expTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == TicketType.SaleTransactionType.Id).Amount);
            Assert.AreEqual(expLcTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 2).Amount);
            Assert.AreEqual(expStTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 3).Amount);
        }

        [Test]
        public void CanCalculateDoubleOrdersMultipleTaxWithMultipleOrderTagOneTaxFree()
        {
            var ticket = TicketBuilder.Create(TicketType, Department.Default)
                .AddOrderFor(Beer).WithTaxTemplates(GetTaxTemplates(Beer.Id)).Do()
                .AddOrderFor(Pizza).WithTaxTemplates(GetTaxTemplates(Pizza.Id))
                    .ToggleOrderTag(new OrderTagGroup { Name = "OT1", Id = 1, AddTagPriceToOrderPrice = true }, new OrderTag { Name = "t1", Id = 1, Price = 5 })
                    .ToggleOrderTag(new OrderTagGroup { Name = "OT2", Id = 2, AddTagPriceToOrderPrice = false }, new OrderTag { Name = "t2", Id = 2, Price = 5 })
                    .ToggleOrderTag(new OrderTagGroup { Name = "OT3", Id = 3, AddTagPriceToOrderPrice = true, TaxFree = true }, new OrderTag { Name = "t3", Id = 3, Price = 5 })
                    .WithPrice(5).Do()
                .Build();

            const decimal expTax = 2.18m + 3;
            const decimal expStTax = 1.95m + 3;
            const decimal expLcTax = 0.23m;

            Assert.AreEqual(expTax, ticket.GetTaxTotal());
            Assert.AreEqual(ticket.GetSum() - expTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == TicketType.SaleTransactionType.Id).Amount);
            Assert.AreEqual(expLcTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 2).Amount);
            Assert.AreEqual(expStTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 3).Amount);
        }

        [Test]
        public void CanCalculateDiscountTax()
        {
            var ticket = TicketBuilder.Create(TicketType, Department.Default)
                .AddOrderFor(Beer).WithTaxTemplates(GetTaxTemplates(Beer.Id)).Do()
                .AddCalculation(new CalculationType { Name = "Discount", AccountTransactionType = DiscountTransactionType, Amount = 10, DecreaseAmount = true }).Build();

            var expStTax = decimal.Round((9 * 25) / 128m, 2);
            var expLcTax = decimal.Round((9 * 3) / 128m, 2);
            var expTax = expStTax + expLcTax;

            Assert.AreEqual(9, ticket.GetSum());
            Assert.AreEqual(expTax, ticket.GetTaxTotal());
            Assert.AreEqual(expLcTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 2).Amount);
            Assert.AreEqual(expStTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 3).Amount);
            Assert.AreEqual(0, ticket.TransactionDocument.AccountTransactions.Sum(x => x.AccountTransactionValues.Sum(y => y.Debit - y.Credit)));
            Assert.AreEqual(9, ticket.TransactionDocument.AccountTransactions.Where(x => x.TargetAccountTypeId == 3).Sum(x => x.Amount) - ticket.TransactionDocument.AccountTransactions.Where(x => x.SourceAccountTypeId == 3).Sum(x => x.Amount));
        }

        [Test]
        public void CanCalculateExcludedTaxWhenDiscountExists()
        {
            var ticket = TicketBuilder.Create(TicketType, Department.Default)
                .TaxExcluded()
                .AddOrderFor(Pizza).WithTaxTemplates(GetTaxTemplates(Pizza.Id)).Do()
                .AddCalculation(new CalculationType { Name = "Discount", AccountTransactionType = DiscountTransactionType, Amount = 10, DecreaseAmount = true })
                .Build();
            Assert.AreEqual(11.25, ticket.GetSum());
            Assert.AreEqual(11.25, ticket.TransactionDocument.AccountTransactions.Where(x => x.TargetAccountTypeId == 3).Sum(x => x.Amount) - ticket.TransactionDocument.AccountTransactions.Where(x => x.SourceAccountTypeId == 3).Sum(x => x.Amount));
        }

        [Test]
        public void CanCalculateMultipleOrderExcludedTaxWhenDiscountExists()
        {
            var calculationType = CalculationTypeBuilder.Create("Discount")
                .WithAccountTransactionType(DiscountTransactionType)
                .WithAmount(10)
                .DecreaseAmount()
                .Build();

            var ticket = TicketBuilder.Create(TicketType, Department.Default)
                .TaxExcluded()
                .AddOrderFor(Pizza).WithTaxTemplates(GetTaxTemplates(Pizza.Id)).Do(2)
                .AddCalculation(calculationType)
                .Build();

            Assert.AreEqual(22.50, ticket.GetSum());
            Assert.AreEqual(22.50, ticket.TransactionDocument.AccountTransactions.Where(x => x.TargetAccountTypeId == 3).Sum(x => x.Amount) - ticket.TransactionDocument.AccountTransactions.Where(x => x.SourceAccountTypeId == 3).Sum(x => x.Amount));
        }

        [Test]
        public void CanVoidAll()
        {
            var ticket = TicketBuilder.Create(TicketType, Department.Default).TaxExcluded()
                .AddOrderFor(Pizza).WithTaxTemplates(GetTaxTemplates(Pizza.Id)).CalculatePrice(false).Do(2)
                .Build();
            Assert.AreEqual(0, ticket.GetTaxTotal());
            Assert.AreEqual(0, ticket.TransactionDocument.AccountTransactions.Sum(x => x.Amount));
        }

        [Test]
        public void CanCalculateSampleCase2()
        {
            var tax5 = TaxTemplateBuilder.Create("%5 Tax").WithRate(5)
                                         .CreateAccountTransactionType().WithId(3).Do()
                                         .WithRounding(0)
                                         .Build();
            var ticket = TicketBuilder.Create(TicketType, Department.Default).TaxExcluded()
                    .AddOrderFor(Product).WithTaxTemplate(tax5).WithPrice(8.95m).WithQuantity(1).Do()
                .Build();

            Assert.AreEqual(9.40m, ticket.GetSum());
        }

        [Test]
        public void CanCalculateSampleCase1()
        {
            // http://forum2.sambapos.com/index.php/topic,1481.0.html

            var tax8 = TaxTemplateBuilder.Create("%8 Tax").WithRate(8)
                    .CreateAccountTransactionType().WithId(2).Do()
                .WithRounding(0)
                .Build();

            var tax18 = TaxTemplateBuilder.Create("%18 Tax").WithRate(18)
                    .CreateAccountTransactionType().WithId(3).Do()
                .WithRounding(0)
                .Build();

            var ticket = TicketBuilder.Create(TicketType, Department.Default).TaxIncluded()
                .AddOrderFor(Product).WithTaxTemplate(tax8).WithPrice(6).WithQuantity(16).Do()
                .AddOrderFor(Product).WithTaxTemplate(tax8).WithPrice(10).WithQuantity(3).Do()
                .AddOrderFor(Product).WithTaxTemplate(tax8).WithPrice(10).WithQuantity(3).Do()
                .AddOrderFor(Product).WithTaxTemplate(tax8).WithPrice(3).WithQuantity(10).Do()
                .AddOrderFor(Product).WithTaxTemplate(tax18).WithPrice(9).WithQuantity(1).Do()
                .AddOrderFor(Product).WithTaxTemplate(tax8).WithPrice(7).WithQuantity(1).Do()
                .AddOrderFor(Product).WithTaxTemplate(tax8).WithPrice(18).WithQuantity(3).Do()
                .AddOrderFor(Product).WithTaxTemplate(tax8).WithPrice(5).WithQuantity(2).Do()
                .AddOrderFor(Product).WithTaxTemplate(tax8).WithPrice(5).WithQuantity(2).Do()
                .AddOrderFor(Product).WithTaxTemplate(tax8).WithPrice(28).WithQuantity(3).Do()
                .AddOrderFor(Product).WithTaxTemplate(tax8).WithPrice(32).WithQuantity(3).Do()
                .AddOrderFor(Product).WithTaxTemplate(tax8).WithPrice(10).WithQuantity(2).Do()
                .AddOrderFor(Product).WithTaxTemplate(tax8).WithPrice(10).WithQuantity(2).Do()
                .AddOrderFor(Product).WithTaxTemplate(tax18).WithPrice(9).WithQuantity(1).Do()
                .AddOrderFor(Product).WithTaxTemplate(tax18).WithPrice(105).WithQuantity(2).Do()
                .Build();

            Assert.AreEqual(715, ticket.GetSum());
            Assert.AreEqual(34.78 + 36.07, ticket.GetTaxTotal());
            Assert.AreEqual(36.07, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 2).Amount);
            Assert.AreEqual(34.78, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 3).Amount);
        }
    }

    public class CalculationTypeBuilder
    {
        private string _name;
        private AccountTransactionType _accountTransactionType;
        private decimal _amount;
        private bool _decreaseAmount;

        private CalculationTypeBuilder(string name)
        {
            _name = name;
        }

        public static CalculationTypeBuilder Create(string discount)
        {
            return new CalculationTypeBuilder(discount);
        }

        public CalculationTypeBuilder WithAccountTransactionType(AccountTransactionType accountTransactionType)
        {
            _accountTransactionType = accountTransactionType;
            return this;
        }

        public CalculationTypeBuilder WithAmount(decimal amount)
        {
            _amount = amount;
            return this;
        }

        public CalculationTypeBuilder DecreaseAmount()
        {
            _decreaseAmount = true;
            return this;
        }

        public CalculationType Build()
        {
            var result = new CalculationType
                             {
                                 Name = _name,
                                 AccountTransactionType = _accountTransactionType,
                                 Amount = _amount,
                                 DecreaseAmount = _decreaseAmount
                             };
            return result;
        }
    }
}
