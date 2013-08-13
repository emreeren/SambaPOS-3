using System.Collections.Generic;
using System.Linq;
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

        [Test]
        public void TicketBuilder_CreatesOrder_OrderAdded()
        {
            var context = TicketBuilderTestContext.GetDefaultContext();
            var ticket = TicketBuilder.Create(context.TicketType, context.Department)
                                      .AddOrder().ForMenuItem(MenuItemBuilder.Create("Hamburger").AddPortion("Adet", 10).Build()).Do()
                                      .Build();
            Assert.AreEqual(10, ticket.GetSum());
        }

        [Test]
        public void TicketBuilder_CreatesOrderWithMenuItem_OrderAdded()
        {
            var context = TicketBuilderTestContext.GetDefaultContext();
            var ticket = TicketBuilder.Create(context.TicketType, context.Department)
                                      .AddOrder()
                                        .CreateMenuItem("Hamburger").AddPortion("Adet", 10).Do()
                                      .Do()
                                      .Build();
            Assert.AreEqual(10, ticket.GetSum());
        }

        [Test]
        public void TicketBuilder_AddsOrderWithMultipleMenuItems_OrdersAdded1()
        {
            var context = TicketBuilderTestContext.GetDefaultContext();

            var hamburger = MenuItemBuilder.Create("Hamburger").AddPortion("Adet", 10).Build();
            var tost = MenuItemBuilder.Create("Tost").AddPortion("Adet", 4).Build();

            var order1 = OrderBuilder.Create(context.TicketType.SaleTransactionType, context.Department)
                                     .ForMenuItem(hamburger);
            var order2 = OrderBuilder.Create(context.TicketType.SaleTransactionType, context.Department)
                                     .ForMenuItem(tost).WithQuantity(2);

            var ticket = TicketBuilder.Create(context.TicketType, context.Department)
                                      .AddOrder(order1)
                                      .AddOrder(order2)
                                      .Build();

            Assert.AreEqual(10 + (4 * 2), ticket.GetSum());
        }

        [Test]
        public void TicketBuilder_AddsOrderWithMultipleMenuItems_OrdersAdded2()
        {
            var context = TicketBuilderTestContext.GetDefaultContext();

            var hamburger = MenuItemBuilder.Create("Hamburger").AddPortion("Adet", 10).Build();
            var tost = MenuItemBuilder.Create("Tost").AddPortion("Adet", 4).Build();

            var ticket = TicketBuilder.Create(context.TicketType, context.Department)
                                      .AddOrderFor(hamburger).Do()
                                      .AddOrderFor(tost).WithQuantity(2).Do()
                                      .Build();

            Assert.AreEqual(10 + (4 * 2), ticket.GetSum());
        }

        [Test]
        public void TicketBuilder_AddsOrderWithMultipleMenuItems_OrdersAdded3()
        {
            var context = TicketBuilderTestContext.GetDefaultContext();
            var ticket = TicketBuilder.Create(context.TicketType, context.Department)
                                      .AddOrder()
                                        .CreateMenuItem("Hamburger").AddPortion("Adet", 10).Do()
                                      .Do()
                                      .AddOrder()
                                        .CreateMenuItem("Tost").AddPortion("Adet", 4).Do()
                                      .WithQuantity(2)
                                      .Do()
                                      .Build();
            Assert.AreEqual(10 + (4 * 2), ticket.GetSum());
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
