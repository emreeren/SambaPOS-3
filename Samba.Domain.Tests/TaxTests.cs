using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
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
            var result = new MenuItem(name) { Id = id };
            result.Portions.Add(new MenuItemPortion { Price = price });
            return result;
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
            ReceivableAccount = new Account { AccountTypeId = receivableAccountType.Id, Name = "Receivables", Id = 2 };
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
                DefaultTargetAccountId = ReceivableAccount.Id
            };

            var localTaxTransactionType = new AccountTransactionType
            {
                Id = 2,
                Name = "Local Tax Transaction",
                SourceAccountTypeId = taxAccountType.Id,
                TargetAccountTypeId = receivableAccountType.Id,
                DefaultSourceAccountId = localTaxAccount.Id,
                DefaultTargetAccountId = ReceivableAccount.Id
            };

            var stateTaxTransactionType = new AccountTransactionType
            {
                Id = 3,
                Name = "State Tax Transaction",
                SourceAccountTypeId = taxAccountType.Id,
                TargetAccountTypeId = receivableAccountType.Id,
                DefaultSourceAccountId = stateTaxAccount.Id,
                DefaultTargetAccountId = ReceivableAccount.Id
            };

            DiscountTransactionType = new AccountTransactionType
            {
                Id = 4,
                Name = "Discount Transaction",
                SourceAccountTypeId = receivableAccountType.Id,
                TargetAccountTypeId = discountAccountType.Id,
                DefaultSourceAccountId = ReceivableAccount.Id,
                DefaultTargetAccountId = defaultDiscountAccount.Id
            };

            var stateTax = new TaxTemplate { Name = "State Tax", Rate = 25, Id = 1, Rounding = 2 };
            stateTax.TaxTemplateMaps.Add(new TaxTemplateMap());
            stateTax.AccountTransactionType = stateTaxTransactionType;

            var localTax = new TaxTemplate { Name = "Local Tax", Rate = 3, Id = 2, Rounding = 2 };
            localTax.TaxTemplateMaps.Add(new TaxTemplateMap { MenuItemId = Cola.Id });
            localTax.TaxTemplateMaps.Add(new TaxTemplateMap { MenuItemId = Beer.Id });
            localTax.AccountTransactionType = localTaxTransactionType;

            TaxTemplates = new List<TaxTemplate> { stateTax, localTax };

            TicketType = new TicketType { SaleTransactionType = saleTransactionType, TaxIncluded = true };
        }

        protected AccountTransactionType DiscountTransactionType { get; set; }
        protected Account ReceivableAccount { get; set; }
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
        public void CanCalculateTicket()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, 1, null);
            var order = ticket.AddOrder(AccountTransactionType.Default, Department.Default, "Emre", Pizza, null, Pizza.Portions[0], "", null);
            ticket.Recalculate();
            Assert.AreEqual(10, order.GetVisibleValue());
            Assert.AreEqual(10, ticket.GetSum());
        }

        [Test]
        public void CanCalculateTax()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, 1, null);
            var order = ticket.AddOrder(AccountTransactionType.Default, Department.Default, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            ticket.Recalculate();
            Assert.AreEqual(10, order.GetVisibleValue());
            Assert.AreEqual(2, ticket.GetTaxTotal());
            Assert.AreEqual(10, ticket.GetSum());
        }

        [Test]
        public void CanCalculateTaxWhenVoidExists()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, 1, null);
            ticket.AddOrder(AccountTransactionType.Default, Department.Default, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            ticket.Recalculate();
            Assert.AreEqual(2, ticket.GetTaxTotal());
            Assert.AreEqual(10, ticket.GetSum());
            var order2 = ticket.AddOrder(AccountTransactionType.Default, Department.Default, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            ticket.Recalculate();
            Assert.AreEqual(4, ticket.GetTaxTotal());
            Assert.AreEqual(20, ticket.GetSum());
            order2.CalculatePrice = false;
            ticket.Recalculate();
            Assert.AreEqual(2, ticket.GetTaxTotal());
            Assert.AreEqual(10, ticket.GetSum());
            Assert.AreEqual(10, order2.GetVisibleValue());
        }

        [Test]
        public void CanCalculateExcludedTax()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, 1, null);
            ticket.TaxIncluded = false;
            var order = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            ticket.Recalculate();
            Assert.AreEqual(10, order.GetVisibleValue());
            Assert.AreEqual(10, order.GetTotal());
            Assert.AreEqual(12.5, ticket.GetSum());
            Assert.AreEqual(12.5, ticket.TransactionDocument.AccountTransactions.Where(x => x.TargetAccountTypeId == 3).Sum(x => x.Amount) - ticket.TransactionDocument.AccountTransactions.Where(x => x.SourceAccountTypeId == 3).Sum(x => x.Amount));
        }

        [Test]
        public void CanCalculateDoubleMultipleTax()
        {
            const decimal orderQuantiy = 2;
            var ticket = Ticket.Create(Department.Default, TicketType, 1, null);
            var order = ticket.AddOrder(AccountTransactionType.Default, Department.Default, "Emre", Beer, GetTaxTemplates(Beer.Id), Pizza.Portions[0], "", null);
            order.Quantity = orderQuantiy;
            ticket.Recalculate();
            const decimal expTax = 2.18m * orderQuantiy;
            const decimal expStTax = 1.95m;
            const decimal expLcTax = 0.23m;

            Assert.AreEqual(expTax, ticket.GetTaxTotal());
            Assert.AreEqual(orderQuantiy * 10, order.GetVisibleValue());
            Assert.AreEqual(orderQuantiy * 10, ticket.GetSum());
            Assert.AreEqual(expLcTax * orderQuantiy, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 2).Amount);
            Assert.AreEqual(expStTax * orderQuantiy, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 3).Amount);
        }

        [Test]
        public void CanCalculateDoubleOrdersMultipleTax()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, 1, null);
            var order1 = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Beer, GetTaxTemplates(Beer.Id), Pizza.Portions[0], "", null);
            var order2 = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            order2.UpdatePrice(5, "");
            ticket.Recalculate();
            const decimal expTax = 2.18m + 1;
            const decimal expStTax = 1.95m + 1;
            const decimal expLcTax = 0.23m;

            Assert.AreEqual(expTax, ticket.GetTaxTotal());
            Assert.AreEqual(10, order1.GetVisibleValue());
            Assert.AreEqual(5, order2.GetVisibleValue());
            Assert.AreEqual(15, ticket.GetSum());
            Assert.AreEqual(15 - expTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == TicketType.SaleTransactionType.Id).Amount);
            Assert.AreEqual(expLcTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 2).Amount);
            Assert.AreEqual(expStTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 3).Amount);
        }

        [Test]
        public void CanCalculateDoubleOrdersMultipleTaxWithOrderTag()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, 1, null);
            var order1 = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Beer, GetTaxTemplates(Beer.Id), Pizza.Portions[0], "", null);
            var order2 = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            order2.ToggleOrderTag(new OrderTagGroup { AddTagPriceToOrderPrice = true }, new OrderTag { Price = 5 }, 1, "");
            order2.UpdatePrice(5, "");
            ticket.Recalculate();
            const decimal expTax = 2.18m + 2;
            const decimal expStTax = 1.95m + 2;
            const decimal expLcTax = 0.23m;

            Assert.AreEqual(expTax, ticket.GetTaxTotal());
            Assert.AreEqual(10, order1.GetVisibleValue());
            Assert.AreEqual(10, order2.GetVisibleValue());
            Assert.AreEqual(20, ticket.GetSum());
            Assert.AreEqual(20 - expTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == TicketType.SaleTransactionType.Id).Amount);
            Assert.AreEqual(expLcTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 2).Amount);
            Assert.AreEqual(expStTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 3).Amount);
        }

        [Test]
        public void CanCalculateDoubleOrdersMultipleTaxWithMultipleOrderTag()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, 1, null);
            var order1 = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Beer, GetTaxTemplates(Beer.Id), Pizza.Portions[0], "", null);
            var order2 = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            order2.ToggleOrderTag(new OrderTagGroup { Name = "OT1", Id = 1, AddTagPriceToOrderPrice = true }, new OrderTag { Name = "t1", Id = 1, Price = 5 }, 1, "");
            order2.ToggleOrderTag(new OrderTagGroup { Name = "OT2", Id = 2, AddTagPriceToOrderPrice = false }, new OrderTag { Name = "t2", Id = 2, Price = 5 }, 1, "");
            order2.UpdatePrice(5, "");
            ticket.Recalculate();
            const decimal expTax = 2.18m + 3;
            const decimal expStTax = 1.95m + 3;
            const decimal expLcTax = 0.23m;

            Assert.AreEqual(expTax, ticket.GetTaxTotal());
            Assert.AreEqual(10, order1.GetVisibleValue());
            Assert.AreEqual(10, order2.GetVisibleValue());
            Assert.AreEqual(25, ticket.GetSum());
            Assert.AreEqual(25 - expTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == TicketType.SaleTransactionType.Id).Amount);
            Assert.AreEqual(expLcTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 2).Amount);
            Assert.AreEqual(expStTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 3).Amount);
        }

        [Test]
        public void CanCalculateDoubleOrdersMultipleTaxWithMultipleOrderTagOneTaxFree()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, 1, null);
            var order1 = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Beer, GetTaxTemplates(Beer.Id), Pizza.Portions[0], "", null);
            var order2 = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            Assert.AreEqual(10, order2.GetVisibleValue());
            order2.ToggleOrderTag(new OrderTagGroup { Name = "OT1", Id = 1, AddTagPriceToOrderPrice = true }, new OrderTag { Name = "t1", Id = 1, Price = 5 }, 1, "");
            order2.ToggleOrderTag(new OrderTagGroup { Name = "OT2", Id = 2, AddTagPriceToOrderPrice = false }, new OrderTag { Name = "t2", Id = 2, Price = 5 }, 1, "");
            order2.ToggleOrderTag(new OrderTagGroup { Name = "OT3", Id = 3, AddTagPriceToOrderPrice = true, TaxFree = true }, new OrderTag { Name = "t3", Id = 3, Price = 5 }, 1, "");
            order2.UpdatePrice(5, "");
            ticket.Recalculate();
            const decimal expTax = 2.18m + 3;
            const decimal expStTax = 1.95m + 3;
            const decimal expLcTax = 0.23m;

            Assert.AreEqual(expTax, ticket.GetTaxTotal());
            Assert.AreEqual(10, order1.GetVisibleValue());
            Assert.AreEqual(15, order2.GetVisibleValue());
            Assert.AreEqual(30, ticket.GetSum());
            Assert.AreEqual(30 - expTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == TicketType.SaleTransactionType.Id).Amount);
            Assert.AreEqual(expLcTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 2).Amount);
            Assert.AreEqual(expStTax, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 3).Amount);
        }

        [Test]
        public void CanCalculateDiscountTax()
        {
            const decimal orderQuantiy = 1;
            var ticket = Ticket.Create(Department.Default, TicketType, 1, null);
            var order = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Beer, GetTaxTemplates(Beer.Id), Pizza.Portions[0], "", null);
            order.Quantity = orderQuantiy;
            ticket.AddCalculation(new CalculationType { AccountTransactionType = DiscountTransactionType, Amount = 10, DecreaseAmount = true }, 10);
            ticket.Recalculate();
            var expStTax = decimal.Round((9 * 25) / 128m, 2);
            var expLcTax = decimal.Round((9 * 3) / 128m, 2);
            var expTax = expStTax + expLcTax;

            Assert.AreEqual(orderQuantiy * 9, ticket.GetSum());
            Assert.AreEqual(expTax, ticket.GetTaxTotal());
            Assert.AreEqual(expLcTax * orderQuantiy, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 2).Amount);
            Assert.AreEqual(expStTax * orderQuantiy, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 3).Amount);
            Assert.AreEqual(0, ticket.TransactionDocument.AccountTransactions.Sum(x => x.AccountTransactionValues.Sum(y => y.Debit - y.Credit)));
            Assert.AreEqual(9, ticket.TransactionDocument.AccountTransactions.Where(x => x.TargetAccountTypeId == 3).Sum(x => x.Amount) - ticket.TransactionDocument.AccountTransactions.Where(x => x.SourceAccountTypeId == 3).Sum(x => x.Amount));
        }

        [Test]
        public void CanCalculateExcludedTaxWhenDiscountExists()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, 1, null);
            ticket.TaxIncluded = false;
            var order = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            ticket.AddCalculation(new CalculationType { AccountTransactionType = DiscountTransactionType, Amount = 10, DecreaseAmount = true }, 10);
            ticket.Recalculate();
            Assert.AreEqual(10, order.GetVisibleValue());
            Assert.AreEqual(10, order.GetTotal());
            Assert.AreEqual(11.25, ticket.GetSum());
            Assert.AreEqual(11.25, ticket.TransactionDocument.AccountTransactions.Where(x => x.TargetAccountTypeId == 3).Sum(x => x.Amount) - ticket.TransactionDocument.AccountTransactions.Where(x => x.SourceAccountTypeId == 3).Sum(x => x.Amount));
        }

        [Test]
        public void CanCalculateMultipleOrderExcludedTaxWhenDiscountExists()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, 1, null);
            ticket.TaxIncluded = false;
            var order1 = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            var order2 = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            ticket.AddCalculation(new CalculationType { AccountTransactionType = DiscountTransactionType, Amount = 10, DecreaseAmount = true }, 10);
            ticket.Recalculate();
            Assert.AreEqual(10, order1.GetVisibleValue());
            Assert.AreEqual(10, order2.GetTotal());
            Assert.AreEqual(22.50, ticket.GetSum());
            Assert.AreEqual(22.50, ticket.TransactionDocument.AccountTransactions.Where(x => x.TargetAccountTypeId == 3).Sum(x => x.Amount) - ticket.TransactionDocument.AccountTransactions.Where(x => x.SourceAccountTypeId == 3).Sum(x => x.Amount));
        }

        [Test]
        public void CanVoidAll()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, 1, null);
            ticket.TaxIncluded = false;
            var order1 = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            var order2 = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            ticket.AddCalculation(new CalculationType { AccountTransactionType = DiscountTransactionType, Amount = 10, DecreaseAmount = true }, 10);
            ticket.Recalculate();
            Assert.AreEqual(10, order1.GetVisibleValue());
            Assert.AreEqual(10, order2.GetTotal());
            Assert.AreEqual(22.50, ticket.GetSum());
            Assert.AreEqual(22.50, ticket.TransactionDocument.AccountTransactions.Where(x => x.TargetAccountTypeId == 3).Sum(x => x.Amount) - ticket.TransactionDocument.AccountTransactions.Where(x => x.SourceAccountTypeId == 3).Sum(x => x.Amount));
            ticket.RemoveCalculation(ticket.Calculations[0]);
            order1.CalculatePrice = false;
            ticket.Recalculate();
            Assert.AreEqual(12.5, ticket.TransactionDocument.AccountTransactions.Sum(x => x.Amount));
            order2.CalculatePrice = false;
            ticket.Recalculate();
            Assert.AreEqual(0, ticket.TransactionDocument.AccountTransactions.Sum(x => x.Amount));
        }

        [Test]
        public void CanCalculateSampleCase1()
        {
            // http://forum2.sambapos.com/index.php/topic,1481.0.html

            var percent8Tax = new TaxTemplate { Name = "% 8 Tax", Rate = 8, AccountTransactionType = new AccountTransactionType() { Id = 2 }, Rounding = 0 };
            var percent18Tax = new TaxTemplate { Name = "% 18 Tax", Rate = 18, AccountTransactionType = new AccountTransactionType() { Id = 3 }, Rounding = 0 };
            var tax8 = new List<TaxTemplate> { percent8Tax };
            var tax18 = new List<TaxTemplate> { percent18Tax };
            var ticket = Ticket.Create(Department.Default, TicketType, 1, null);
            ticket.TaxIncluded = true;
            AddOrderToTicket(ticket, tax8, 6, 16);
            Assert.AreEqual(96, ticket.GetSum());
            Assert.AreEqual(7.11, ticket.GetTaxTotal());
            AddOrderToTicket(ticket, tax8, 10, 3);
            AddOrderToTicket(ticket, tax8, 10, 3);
            AddOrderToTicket(ticket, tax8, 3, 10);
            AddOrderToTicket(ticket, tax18, 9, 1);
            AddOrderToTicket(ticket, tax8, 7, 1);
            AddOrderToTicket(ticket, tax8, 18, 3);
            AddOrderToTicket(ticket, tax8, 5, 2);
            AddOrderToTicket(ticket, tax8, 5, 2);
            AddOrderToTicket(ticket, tax8, 28, 3);
            AddOrderToTicket(ticket, tax8, 32, 3);
            AddOrderToTicket(ticket, tax8, 10, 2);
            AddOrderToTicket(ticket, tax8, 10, 2);
            AddOrderToTicket(ticket, tax18, 9, 1);
            AddOrderToTicket(ticket, tax18, 105, 2);
            ticket.Recalculate();
            Assert.AreEqual(715, ticket.GetSum());
            Assert.AreEqual(34.78 + 36.07, ticket.GetTaxTotal());
            Assert.AreEqual(36.07, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 2).Amount);
            Assert.AreEqual(34.78, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 3).Amount);
        }

        private void AddOrderToTicket(Ticket ticket, IList<TaxTemplate> tax, decimal price, decimal quantity)
        {
            var order = ticket.AddOrder(TicketType.SaleTransactionType, Department.Default, "Emre", Product, tax, Product.Portions[0], "", null);
            order.Price = price;
            order.Quantity = quantity;
        }
    }
}
