using NUnit.Framework;
using Samba.Domain.Builders;
using Samba.Domain.Models.Tickets;

namespace Samba.Domain.Tests
{
    [TestFixture]
    class TicketMergeTests
    {
        [Test]
        public void CanAddOrders()
        {
            var kola = MenuItemBuilder.Create("Kola").AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do(2)
                                      .Build();
            Assert.AreEqual(2, ticket.Orders.Count);
        }

        [Test]
        public void CanMergeOrders()
        {
            var kola = MenuItemBuilder.Create("Kola").AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do(2)
                                      .Build();
            ticket.MergeOrdersAndUpdateOrderNumbers(1);

            Assert.AreEqual(1, ticket.Orders.Count);
        }

        [Test]
        public void CanMergeMultipleOrders()
        {
            var kola = MenuItemBuilder.Create("Kola").AddPortion("Adet", 5).Build();
            var pizza = MenuItemBuilder.Create("Pizza").AddPortion("Adet", 10).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do(2)
                                      .AddOrder().ForMenuItem(pizza).Do(2)
                                      .Build();
            ticket.MergeOrdersAndUpdateOrderNumbers(1);

            Assert.AreEqual(2, ticket.Orders.Count);
        }

        [Test]
        public void CanUpdateQuantityOfMergedOrders()
        {
            var kola = MenuItemBuilder.Create("Kola").AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do(2)
                                      .Build();
            ticket.MergeOrdersAndUpdateOrderNumbers(1);

            Assert.AreEqual(2, ticket.Orders[0].Quantity);
        }

        [Test]
        public void MergeOrders_DifferentPrice_ShouldSkipMerge()
        {
            var kola = MenuItemBuilder.Create("Kola").AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do(2)
                                      .Build();
            ticket.Orders[0].UpdatePrice(4, "");
            ticket.MergeOrdersAndUpdateOrderNumbers(1);

            Assert.AreEqual(2, ticket.Orders.Count);
            Assert.AreEqual(1, ticket.Orders[0].Quantity);
            Assert.AreEqual(1, ticket.Orders[1].Quantity);
        }

        [Test]
        public void MergeOrders_OrdersTagged_ShouldSkipMerge()
        {
            var kola = MenuItemBuilder.Create("Kola").AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do(2)
                                      .Build();
            ticket.Orders[0].ToggleOrderTag(new OrderTagGroup { Name = "Service" }, new OrderTag { Name = "Pause" }, 0, "");
            ticket.MergeOrdersAndUpdateOrderNumbers(1);
            Assert.AreEqual(2, ticket.Orders.Count);
        }

        [Test]
        public void MergeOrders_DifferentOrderStates_ShouldSkipMerge()
        {
            var kola = MenuItemBuilder.Create("Kola").AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do(2)
                                      .Build();
            ticket.Orders[0].SetStateValue("Status", 1, "New", 1, "", 0);
            ticket.MergeOrdersAndUpdateOrderNumbers(1);
            Assert.AreEqual(2, ticket.Orders.Count);
        }

        [Test]
        public void MergeOrders_DifferentOrderStates_ShouldSkipMerge2()
        {
            var kola = MenuItemBuilder.Create("Kola").AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do(2)
                                      .Build();
            ticket.Orders[0].SetStateValue("Status", 1, "New", 1, "", 0);
            ticket.Orders[1].SetStateValue("Status", 1, "New", 1, "", 0);
            ticket.Orders[0].SetStateValue("State", 1, "2", 1, "", 0);
            ticket.Orders[1].SetStateValue("State", 1, "3", 1, "", 0);
            ticket.MergeOrdersAndUpdateOrderNumbers(1);
            Assert.AreEqual(2, ticket.Orders.Count);
        }

        [Test]
        public void MergeOrders_DifferentOrderStates_ShouldMerge()
        {
            var kola = MenuItemBuilder.Create("Kola").AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do(2)
                                      .Build();
            ticket.Orders[0].SetStateValue("Status", 1, "New", 1, "", 0);
            ticket.Orders[1].SetStateValue("Status", 1, "New", 1, "", 0);
            ticket.Orders[0].SetStateValue("State", 1, "2", 1, "", 0);
            ticket.Orders[1].SetStateValue("State", 1, "2", 1, "", 0);
            ticket.MergeOrdersAndUpdateOrderNumbers(1);
            Assert.AreEqual(1, ticket.Orders.Count);
        }

        [Test]
        public void MergeOrders_SameOrderStates_ShouldMerge()
        {
            var kola = MenuItemBuilder.Create("Kola").AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do(2)
                                      .Build();
            ticket.Orders[0].SetStateValue("Status", 1, "New", 1, "", 0);
            ticket.Orders[1].SetStateValue("Status", 1, "New", 1, "", 0);
            ticket.MergeOrdersAndUpdateOrderNumbers(1);
            Assert.AreEqual(1, ticket.Orders.Count);
        }

        [Test]
        public void MergeOrders_DifferentOrderQuantites_ShouldSkipMerge()
        {
            var kola = MenuItemBuilder.Create("Kola").AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).WithQuantity(2).Do()
                                      .AddOrder().ForMenuItem(kola).WithQuantity(1).Do()
                                      .Build();
            ticket.MergeOrdersAndUpdateOrderNumbers(1);
            Assert.AreEqual(2, ticket.Orders.Count);
        }

        [Test]
        public void MergeOrders_OneOrderLocked_ShouldSkipMerge()
        {
            var kola = MenuItemBuilder.Create("Kola").AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do(2)
                                      .Build();
            ticket.Orders[0].Locked = true;
            ticket.MergeOrdersAndUpdateOrderNumbers(1);
            Assert.AreEqual(2, ticket.Orders.Count);
        }

        [Test]
        public void MergeOrders_OneOrderIsGift_ShouldSkipMerge()
        {
            var kola = MenuItemBuilder.Create("Kola").AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do(2)
                                      .Build();
            ticket.Orders[0].CalculatePrice = false;
            ticket.MergeOrdersAndUpdateOrderNumbers(1);
            Assert.AreEqual(2, ticket.Orders.Count);
        }


        [Test]
        public void MergeOrders_OneOrderPortionDifferent_ShouldSkipMerge()
        {
            var kola = MenuItemBuilder.Create("Kola").AddPortion("Adet", 5).AddPortion("Büyük", 6).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do(2)
                                      .Build();
            ticket.Orders[0].UpdatePortion(kola.Portions[1], "", null);
            ticket.MergeOrdersAndUpdateOrderNumbers(1);
            Assert.AreEqual(2, ticket.Orders.Count);
            Assert.AreEqual(11, ticket.GetSum());
        }

        [Test]
        public void MergeMultipleOrders_OneOrderLocked_ShouldSkipMerge()
        {
            var kola = MenuItemBuilder.Create("Kola").AddPortion("Adet", 5).Build();
            var pizza = MenuItemBuilder.Create("Pizza").AddPortion("Adet", 10).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do(2)
                                      .AddOrder().ForMenuItem(pizza).Do(2)
                                      .Build();
            ticket.Orders[0].Locked = true;
            ticket.MergeOrdersAndUpdateOrderNumbers(1);
            Assert.AreEqual(3, ticket.Orders.Count);
            Assert.AreEqual(30, ticket.GetSum());
        }

        [Test]
        public void MergeMultipleOrders_DifferentOrders_ShouldSkipMerge()
        {
            var kola = MenuItemBuilder.Create("Kola").WithId(1).AddPortion("Adet", 5).Build();
            var pizza = MenuItemBuilder.Create("Pizza").WithId(2).AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .AddOrder().ForMenuItem(pizza).Do()
                                      .Build();
            ticket.MergeOrdersAndUpdateOrderNumbers(1);
            Assert.AreEqual(2, ticket.Orders.Count);
        }


        [Test]
        public void MergeMultipleOrders_DifferentOrderQuantities_ShouldSkipMerge()
        {
            var kola = MenuItemBuilder.Create("Kola").WithId(1).AddPortion("Adet", 5).Build();
            var pizza = MenuItemBuilder.Create("Pizza").WithId(2).AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .AddOrder().ForMenuItem(kola).WithQuantity(2).Do()
                                      .AddOrder().ForMenuItem(pizza).Do()
                                      .AddOrder().ForMenuItem(pizza).Do()
                                      .Build();
            ticket.MergeOrdersAndUpdateOrderNumbers(1);
            Assert.AreEqual(4, ticket.Orders.Count);
        }

        [Test]
        public void OrderMergerMerges_CanCompareOrders()
        {
            var kola = MenuItemBuilder.Create("Kola").WithId(1).AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .Build();

            var equals = OrderMerger.CanMergeOrders(ticket.Orders[0], ticket.Orders[1]);
            Assert.True(equals);

        }

        [Test]
        public void OrderMerge_CanCompareOrdersWithDifferentQuantity_ReturnsFalse()
        {
            var kola = MenuItemBuilder.Create("Kola").WithId(1).AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).WithQuantity(2).Do()
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .Build();

            var equals = OrderMerger.CanMergeOrders(ticket.Orders[0], ticket.Orders[1]);
            Assert.False(equals);

        }

        [Test]
        public void OrderMerge_CanCompareOrdersWithDifferentPrice_ReturnsFalse()
        {
            var kola = MenuItemBuilder.Create("Kola").WithId(1).AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).WithPrice(3).Do()
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .Build();

            var equals = OrderMerger.CanMergeOrders(ticket.Orders[0], ticket.Orders[1]);
            Assert.False(equals);

        }

        [Test]
        public void OrderMerge_CanCompareOrdersWithDifferentPortions_ReturnsFalse()
        {
            var kola = MenuItemBuilder.Create("Kola").WithId(1).AddPortion("Adet", 5).AddPortion("Büyük", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .Build();
            ticket.Orders[0].UpdatePortion(kola.Portions[1], "", null);
            var equals = OrderMerger.CanMergeOrders(ticket.Orders[0], ticket.Orders[1]);
            Assert.False(equals);
        }

        [Test]
        public void OrderMerge_CanCompareOrdersWithOneOfThemGift_ReturnsFalse()
        {
            var kola = MenuItemBuilder.Create("Kola").WithId(1).AddPortion("Adet", 5).AddPortion("Büyük", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .Build();
            ticket.Orders[0].CalculatePrice = false;
            var equals = OrderMerger.CanMergeOrders(ticket.Orders[0], ticket.Orders[1]);
            Assert.False(equals);
        }

        [Test]
        public void OrderMerge_CanCompareOrdersWithDifferentMenuItemId_ReturnsFalse()
        {
            var kola = MenuItemBuilder.Create("Kola").WithId(1).AddPortion("Adet", 5).Build();
            var pizza = MenuItemBuilder.Create("Pizza").WithId(2).AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .AddOrder().ForMenuItem(pizza).Do()
                                      .Build();

            var equals = OrderMerger.CanMergeOrders(ticket.Orders[0], ticket.Orders[1]);
            Assert.False(equals);

        }

        [Test]
        public void OrderMerge_CanCompareOrdersWithDifferentOrderTags_ReturnsFalse()
        {
            var kola = MenuItemBuilder.Create("Kola").WithId(1).AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .Build();
            ticket.Orders[0].ToggleOrderTag(new OrderTagGroup { Name = "Service" }, new OrderTag { Name = "Pause" }, 0, "");
            var equals = OrderMerger.CanMergeOrders(ticket.Orders[0], ticket.Orders[1]);
            Assert.False(equals);
        }

        [Test]
        public void OrderMerge_CanCompareOrdersWithSameOrderStates_ReturnsTrue()
        {
            var kola = MenuItemBuilder.Create("Kola").WithId(1).AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .Build();
            ticket.Orders[0].SetStateValue("Status", 1, "New", 1, "", 0);
            ticket.Orders[1].SetStateValue("Status", 1, "New", 1, "", 0);
            var equals = OrderMerger.CanMergeOrders(ticket.Orders[0], ticket.Orders[1]);
            Assert.True(equals);
        }

        [Test]
        public void OrderMerge_CanCompareOrdersWithDifferentOrderStates_ReturnsFalse()
        {
            var kola = MenuItemBuilder.Create("Kola").WithId(1).AddPortion("Adet", 5).Build();
            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .AddOrder().ForMenuItem(kola).Do()
                                      .Build();
            ticket.Orders[0].SetStateValue("Status", 1, "New", 1, "", 0);
            ticket.Orders[1].SetStateValue("Status", 1, "Submitted", 1, "", 0);
            var equals = OrderMerger.CanMergeOrders(ticket.Orders[0], ticket.Orders[1]);
            Assert.False(equals);
        }
    }
}
