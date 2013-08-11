using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Samba.Domain.Builders;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;

namespace Samba.Domain.Tests
{
    [TestFixture]
    class TicketBuilderTests
    {

        [Test]
        public void TicketBuilder_CreatesDefaultTicket_TicketTypeIdAssigned()
        {
            var context = TicketBuilderTestContext.GetDefaultContext();
            var ticket = TicketBuilder.Create(context.TicketType, context.Department).Build();
            Assert.AreEqual(context.TicketType.Id, ticket.TicketTypeId);
        }

        [Test]
        public void TicketBuilder_CreatesTaxIncludedTicket_IsTaxIncluded()
        {
            var context = TicketBuilderTestContext.GetDefaultContext().WithTicketTypeTaxIsIncluded();
            var ticket = TicketBuilder.Create(context.TicketType, context.Department).Build();
            Assert.True(ticket.TaxIncluded);
        }

        [Test]
        public void TicketBuilder_CreatesTaxExcludedTicket_IsTaxExcluded()
        {
            var context = TicketBuilderTestContext.GetDefaultContext().WithTicketTypeTaxIsExcluded();
            var ticket = TicketBuilder.Create(context.TicketType, context.Department).Build();
            Assert.False(ticket.TaxIncluded);
        }

        [Test]
        public void TicketBuilder_CreatesDefaultTicket_CreatesAccountTransactionDocument()
        {
            var context = TicketBuilderTestContext.GetDefaultContext();
            var ticket = TicketBuilder.Create(context.TicketType, context.Department).Build();
            Assert.IsNotNull(ticket.TransactionDocument);
        }

        [Test]
        public void TicketBuilder_CreatesDefaultTicket_IsDefaultExchangeRate()
        {
            var context = TicketBuilderTestContext.GetDefaultContext();
            var ticket = TicketBuilder.Create(context.TicketType, context.Department).Build();
            Assert.AreEqual(1, ticket.ExchangeRate);
        }

        [Test]
        public void TicketBuilder_CreatesTicketWithExchangeRate_ExchangeRateAssigned()
        {
            var context = TicketBuilderTestContext.GetDefaultContext();
            var ticket = TicketBuilder.Create(context.TicketType, context.Department)
                                      .WithExchangeRate(1.1m)
                                      .Build();
            Assert.AreEqual(1.1m, ticket.ExchangeRate);
        }

        [Test]
        public void TicketBuilder_CreatesTicketWithDepartment_DepartmentAssigned()
        {
            var context = TicketBuilderTestContext.GetDefaultContext();
            var ticket = TicketBuilder.Create(context.TicketType, context.Department).Build();
            Assert.AreEqual(1, ticket.DepartmentId);
        }

        [Test]
        public void TicketBuilder_CreatesTicketWithCalculations_DiscountAdded()
        {
            var context = TicketBuilderTestContext.GetDefaultContext().With10PercentDiscount();
            var ticket = TicketBuilder.Create(context.TicketType, context.Department)
                                      .WithCalculations(context.Calculations)
                                      .Build();
            Assert.AreEqual(10, ticket.Calculations.Sum(x => x.Amount));
        }
    }

    internal class TicketBuilderTestContext
    {
        public TicketType TicketType { get; set; }
        public Department Department { get; set; }
        public IList<CalculationType> Calculations { get; set; }

        public static TicketBuilderTestContext GetDefaultContext()
        {
            var result = new TicketBuilderTestContext();
            result.TicketType = new TicketType { Id = 1, SaleTransactionType = AccountTransactionType.Default };
            result.Department = new Department { Id = 1 };
            return result;
        }

        public TicketBuilderTestContext WithTicketTypeTaxIsIncluded()
        {
            TicketType.TaxIncluded = true;
            return this;
        }

        public TicketBuilderTestContext WithTicketTypeTaxIsExcluded()
        {
            TicketType.TaxIncluded = false;
            return this;
        }

        public TicketBuilderTestContext With10PercentDiscount()
        {
            Calculations = new List<CalculationType>();
            Calculations.Add(new CalculationType
            {
                CalculationMethod = 0,
                Amount = 10,
                Name = "%10 Discount",
                AccountTransactionType = new AccountTransactionType()
            });
            return this;
        }

    }
}
