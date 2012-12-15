using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data.Serializer;

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

            var stateTax = new TaxTemplate { Name = "State Tax", Rate = 25, Id = 1 };
            stateTax.TaxTemplateMaps.Add(new TaxTemplateMap());
            stateTax.AccountTransactionType = stateTaxTransactionType;

            var localTax = new TaxTemplate { Name = "Local Tax", Rate = 3, Id = 2 };
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
            var ticket = Ticket.Create(Department.Default, TicketType, ReceivableAccount, 1, null);
            var order = ticket.AddOrder(AccountTransactionType.Default, "Emre", Pizza, null, Pizza.Portions[0], "", null);
            ticket.Recalculate();
            Assert.AreEqual(10, order.GetVisibleValue());
            Assert.AreEqual(10, ticket.GetSum());
        }

        [Test]
        public void CanCalculateTax()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, ReceivableAccount, 1, null);
            var order = ticket.AddOrder(AccountTransactionType.Default, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            ticket.Recalculate();
            Assert.AreEqual(10, order.GetVisibleValue());
            Assert.AreEqual(10, ticket.GetSum());
        }

        [Test]
        public void CanCalculateExcludedTax()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, ReceivableAccount, 1, null);
            ticket.TaxIncluded = false;
            var order = ticket.AddOrder(TicketType.SaleTransactionType, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            ticket.Recalculate();
            Assert.AreEqual(10, order.GetVisibleValue());
            Assert.AreEqual(10, order.GetTotal());
            Assert.AreEqual(12.5, ticket.GetSum());
            Assert.AreEqual(12.5, ticket.TransactionDocument.AccountTransactions.Where(x => x.TargetAccountTypeId == 3).Sum(x => x.Amount));
        }



        [Test]
        public void CanCalculateMultipleTax()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, ReceivableAccount, 1, null);
            var order = ticket.AddOrder(AccountTransactionType.Default, "Emre", Beer, GetTaxTemplates(Beer.Id), Pizza.Portions[0], "", null);
            ticket.Recalculate();
            const decimal expTax = 2.18m;
            const decimal expStTax = 1.95m;
            const decimal expLcTax = 0.23m;
            //Assert.AreEqual(expStTax, order.TaxValues.Single(x => x.TaxTemplateName == "State Tax").TaxAmount);
            //Assert.AreEqual(expLcTax, order.TaxValues.Single(x => x.TaxTemplateName == "Local Tax").TaxAmount);
            //Assert.AreEqual(10 - expTax, order.GetFinalValue());
            Assert.AreEqual(10, order.GetVisibleValue());
            Assert.AreEqual(10, ticket.GetSum());
        }

        [Test]
        public void CanCalculateDoubleMultipleTax()
        {
            const decimal orderQuantiy = 2;
            var ticket = Ticket.Create(Department.Default, TicketType, ReceivableAccount, 1, null);
            var order = ticket.AddOrder(AccountTransactionType.Default, "Emre", Beer, GetTaxTemplates(Beer.Id), Pizza.Portions[0], "", null);
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
            var ticket = Ticket.Create(Department.Default, TicketType, ReceivableAccount, 1, null);
            var order1 = ticket.AddOrder(TicketType.SaleTransactionType, "Emre", Beer, GetTaxTemplates(Beer.Id), Pizza.Portions[0], "", null);
            var order2 = ticket.AddOrder(TicketType.SaleTransactionType, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
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
        public void CanCalculateDiscountTax()
        {
            const decimal orderQuantiy = 1;
            var ticket = Ticket.Create(Department.Default, TicketType, ReceivableAccount, 1, null);
            var order = ticket.AddOrder(TicketType.SaleTransactionType, "Emre", Beer, GetTaxTemplates(Beer.Id), Pizza.Portions[0], "", null);
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
            Assert.AreEqual(9, ticket.TransactionDocument.AccountTransactions.Where(x => x.TargetAccountTypeId == 3).Sum(x => x.Amount));
        }

        [Test]
        public void CanCalculateExcludedTaxWhenDiscountExists()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, ReceivableAccount, 1, null);
            ticket.TaxIncluded = false;
            var order = ticket.AddOrder(TicketType.SaleTransactionType, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            ticket.AddCalculation(new CalculationType { AccountTransactionType = DiscountTransactionType, Amount = 10, DecreaseAmount = true }, 10);
            ticket.Recalculate();
            Assert.AreEqual(10, order.GetVisibleValue());
            Assert.AreEqual(10, order.GetTotal());
            Assert.AreEqual(11.25, ticket.GetSum());
            Assert.AreEqual(11.25, ticket.TransactionDocument.AccountTransactions.Where(x => x.TargetAccountTypeId == 3).Sum(x => x.Amount));
        }

        [Test]
        public void CanCalculateMultipleOrderExcludedTaxWhenDiscountExists()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, ReceivableAccount, 1, null);
            ticket.TaxIncluded = false;
            var order1 = ticket.AddOrder(TicketType.SaleTransactionType, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            var order2 = ticket.AddOrder(TicketType.SaleTransactionType, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            ticket.AddCalculation(new CalculationType { AccountTransactionType = DiscountTransactionType, Amount = 10, DecreaseAmount = true }, 10);
            ticket.Recalculate();
            Assert.AreEqual(10, order1.GetVisibleValue());
            Assert.AreEqual(10, order2.GetTotal());
            Assert.AreEqual(22.50, ticket.GetSum());
            Assert.AreEqual(22.50, ticket.TransactionDocument.AccountTransactions.Where(x => x.TargetAccountTypeId == 3).Sum(x => x.Amount));
        }
    }
}
