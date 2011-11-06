using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;

namespace Samba.Domain.Tests
{
    [TestClass]
    public class TicketTest
    {
        [TestMethod]
        public void CanAddTicketDiscounts()
        {
            var mi = new MenuItem("Elma");
            mi.AddPortion("Test", 10, "TL");
            var mi2 = new MenuItem("Armut");
            mi2.AddPortion("Test", 15, "TL");

            var ticket = new Ticket();
            ticket.AddTicketItem(0, mi, "Test");
            ticket.AddTicketItem(0, mi2, "Test");

            Assert.IsTrue(ticket.GetSum() == 25);

            ticket.AddTicketDiscount(DiscountType.Percent, 10, 0);
            Assert.IsTrue(ticket.GetSum() == 22.5m);

            ticket.AddTicketDiscount(DiscountType.Percent, 20, 0);

            Assert.IsTrue(ticket.GetSum() == 20.0m);

            ticket.AddTicketDiscount(DiscountType.Amount, 10, 0);
            Assert.IsTrue(ticket.GetSum() == 10);

            Assert.IsTrue(ticket.GetDiscountAndRoundingTotal() == 15);

            ticket.AddTicketDiscount(DiscountType.Amount, 5, 0);

            Assert.IsTrue(ticket.GetSum() == 15);
            Assert.IsTrue(ticket.GetDiscountAndRoundingTotal() == 10);

            ticket.AddTicketDiscount(DiscountType.Percent, 0, 0);
            ticket.AddTicketDiscount(DiscountType.Amount, 0, 0);

            Assert.IsTrue(ticket.GetSum() == 25);
            Assert.IsTrue(ticket.GetDiscountAndRoundingTotal() == 0);
            Assert.IsTrue(ticket.Discounts.Count == 0);

            ticket.AddTicketDiscount(DiscountType.Percent, 50, 0);
            Assert.IsTrue(ticket.GetSum() == 12.5m);
            Assert.IsTrue(ticket.Discounts.Count == 1);

            ticket.AddTicketDiscount(DiscountType.Percent, 0, 0);
            Assert.IsTrue(ticket.Discounts.Count == 0);
            Assert.IsTrue(ticket.GetSum() == 25);

            ticket.TicketItems[0].Gifted = true;
            Assert.IsTrue(ticket.GetSum() == 15);

            ticket.AddTicketDiscount(DiscountType.Percent, 10, 0);
            Assert.IsTrue(ticket.GetSum() == 13.5m);
            Assert.IsTrue(ticket.GetDiscountAndRoundingTotal() == 1.5m);

            ticket.TicketItems[0].Voided = true;
            Assert.IsTrue(ticket.GetSum() == 13.5m);

            ticket.AddTicketDiscount(DiscountType.Percent, 10, 0);
            Assert.AreEqual(13.5m, ticket.GetSum());
            Assert.AreEqual(1.5m, ticket.GetDiscountAndRoundingTotal());

            ticket.AddTicketDiscount(DiscountType.Amount, 0.5m, 0);
            Assert.AreEqual(13m, ticket.GetSum());

            ticket.Discounts.Clear();
            Assert.AreEqual(15m, ticket.GetSum());
            var t = new TaxTemplate { Rate = 10 };

            var mix = new MenuItem("TestItem2") { TaxTemplate = t };
            mix.AddPortion("Adet", 10, "TL");
            ticket.AddTicketItem(0, mix, "Adet");

            Assert.AreEqual(26m, ticket.GetSum());

            ticket.AddTicketDiscount(DiscountType.Percent, 10, 0);
            Assert.AreEqual(23.4m, ticket.GetSum());
            ticket.AddTicketDiscount(DiscountType.Amount, 0.4m, 0);
            Assert.AreEqual(23m, ticket.GetSum());
        }
    }
}
