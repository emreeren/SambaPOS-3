using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var saleAccountType = new AccountType { Name = "Sales Accounts", Id = 1 };
            var taxAccountType = new AccountType { Name = "Sales Accounts", Id = 2 };
            var receivableAccountType = new AccountType { Name = "Receivable Accounts", Id = 3 };
            var defaultSaleAccount = new Account { AccountTypeId = saleAccountType.Id, Name = "Sales", Id = 1 };
            ReceivableAccount = new Account { AccountTypeId = receivableAccountType.Id, Name = "Receivables", Id = 2 };
            var stateTaxAccount = new Account { AccountTypeId = taxAccountType.Id, Name = "State Tax", Id = 3 };
            var localTaxAccount = new Account { AccountTypeId = taxAccountType.Id, Name = "Local Tax", Id = 4 };

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

            var stateTax = new TaxTemplate { Name = "State Tax", Rate = 25, TaxIncluded = true, Id = 1 };
            stateTax.TaxTemplateMaps.Add(new TaxTemplateMap());
            stateTax.AccountTransactionType = stateTaxTransactionType;

            var localTax = new TaxTemplate { Name = "Local Tax", Rate = 3, TaxIncluded = true, Id = 2 };
            localTax.TaxTemplateMaps.Add(new TaxTemplateMap { MenuItemId = Cola.Id });
            localTax.TaxTemplateMaps.Add(new TaxTemplateMap { MenuItemId = Beer.Id });
            localTax.AccountTransactionType = localTaxTransactionType;

            TaxTemplates = new List<TaxTemplate> { stateTax, localTax };

            TicketType = new TicketType { SaleTransactionType = saleTransactionType };
        }

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
        public void CanCalculateTax()
        {
            var ticket = Ticket.Create(Department.Default, TicketType, ReceivableAccount, 1, null);
            var order = ticket.AddOrder(AccountTransactionType.Default, "Emre", Pizza, GetTaxTemplates(Pizza.Id), Pizza.Portions[0], "", null);
            ticket.Recalculate();
            Assert.AreEqual(2, order.GetTotalTax());
            Assert.AreEqual(8, order.GetFinalValue());
            Assert.AreEqual(10, order.GetVisibleValue());
            Assert.AreEqual(10, ticket.GetSum());
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

            Assert.AreEqual(expTax, order.GetTotalTax());
            Assert.AreEqual(expStTax, order.TaxValues.Single(x => x.TaxTemplateName == "State Tax").TaxAmount);
            Assert.AreEqual(expLcTax, order.TaxValues.Single(x => x.TaxTemplateName == "Local Tax").TaxAmount);
            Assert.AreEqual(10 - expTax, order.GetFinalValue());
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

            Assert.AreEqual(expTax, order.GetTotalTax());
            Assert.AreEqual(expStTax, order.TaxValues.Single(x => x.TaxTemplateName == "State Tax").TaxAmount);
            Assert.AreEqual(expLcTax, order.TaxValues.Single(x => x.TaxTemplateName == "Local Tax").TaxAmount);
            Assert.AreEqual(orderQuantiy * 10 - expTax, order.GetFinalValue());
            Assert.AreEqual(orderQuantiy * 10, order.GetVisibleValue());
            Assert.AreEqual(orderQuantiy * 10, ticket.GetSum());
            Assert.AreEqual(expLcTax * orderQuantiy, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 2).Amount);
            Assert.AreEqual(expStTax * orderQuantiy, ticket.TransactionDocument.AccountTransactions.Single(x => x.AccountTransactionTypeId == 3).Amount);
        }
    }
}
