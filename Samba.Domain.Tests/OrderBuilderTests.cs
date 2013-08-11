using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Samba.Domain.Builders;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;

namespace Samba.Domain.Tests
{
    [TestFixture]
    class OrderBuilderTests
    {
        [Test]
        public void OrderBuilder_CreateOrder_MenuItemAssigned()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                .ForMenuItem(testContext.Hamburger)
                .Build();
            Assert.AreEqual(testContext.Hamburger.Name, order.MenuItemName);
            Assert.AreEqual(testContext.Hamburger.Id, order.MenuItemId);
        }

        [Test]
        public void OrderBuilder_CreateOrder_DefaultPortionAssigned()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                .ForMenuItem(testContext.Hamburger)
                .Build();
            Assert.AreEqual(testContext.HamburgerPortion.Name, order.PortionName);
        }

        [Test]
        public void OrderBuilder_CreateOrderWithPortion_SecondPortionAssigned()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                .WithPortion(testContext.BigHamburger)
                .ForMenuItem(testContext.Hamburger)
                .Build();
            Assert.AreEqual(testContext.BigHamburger.Name, order.PortionName);
        }

        [Test]
        public void OrderBuilder_CreateOrder_UserNameAssigned()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                .ForMenuItem(testContext.Hamburger)
                .WithUserName("Waiter")
                .Build();
            Assert.AreEqual("Waiter", order.CreatingUserName);
        }

        [Test]
        public void OrderBuilder_CreateOrderWithPriceTag_SkipsNonExistedPriceTag()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                .ForMenuItem(testContext.Hamburger)
                .WithPriceTag("HH")
                .Build();
            Assert.AreEqual("", order.PriceTag);
            Assert.AreEqual(testContext.HamburgerPortion.Price, order.Price);
        }

        [Test]
        public void OrderBuilder_CreateOrderWithPriceTag_UpdatesPrice()
        {
            var testContext = OrderBuilderTestContext.CreateDefault().WithHappyHourPrice();
            var order = OrderBuilder.Create()
                .ForMenuItem(testContext.Hamburger)
                .WithPriceTag(testContext.HappyHourPriceTag)
                .Build();
            Assert.AreEqual(testContext.HappyHourPriceTag, order.PriceTag);
            Assert.AreEqual(testContext.HamburgerHappyHourPrice, order.Price);
        }

        [Test]
        public void OrderBuilder_CreateOrderWithQuantity_UpdatesQuantity()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                .ForMenuItem(testContext.Hamburger)
                .WithQuantity(5)
                .Build();
            Assert.AreEqual(5, order.Quantity);
        }

        [Test]
        public void OrderBuilder_CreateOrder_IsDefaultQuantity()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                .ForMenuItem(testContext.Hamburger)
                .Build();
            Assert.AreEqual(1, order.Quantity);
        }

        [Test]
        public void OrderBuilder_CreateOrderWithDepartment_DepartmentAndWarehouseAssigned()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                                    .ForMenuItem(testContext.Hamburger)
                                    .WithDepartment(testContext.Department)
                                    .Build();
            Assert.AreEqual(testContext.Department.Id,order.DepartmentId);
        }
    }

    internal class OrderBuilderTestContext
    {
        public static OrderBuilderTestContext CreateDefault()
        {
            var result = new OrderBuilderTestContext();
            result.Hamburger = new MenuItem { Name = "Hamburger", Id = 5 };
            result.HamburgerPortion = new MenuItemPortion { MenuItemId = 5, Id = 2, Name = "Standard", Price = 5 };
            result.Hamburger.Portions.Add(result.HamburgerPortion);
            result.BigHamburger = new MenuItemPortion { MenuItemId = 5, Id = 4, Name = "Big", Price = 8 };
            result.Hamburger.Portions.Add(result.BigHamburger);
            result.HappyHourPriceTag = "HH";
            result.HamburgerHappyHourPrice = 5;
            result.Department = new Department { Id = 5, TicketTypeId = 3, WarehouseId = 8 };
            return result;
        }

        public MenuItem Hamburger { get; set; }
        public MenuItemPortion HamburgerPortion { get; set; }
        public MenuItemPortion BigHamburger { get; set; }

        public string HappyHourPriceTag { get; set; }
        public int HamburgerHappyHourPrice { get; set; }

        public Department Department { get; set; }

        public OrderBuilderTestContext WithHappyHourPrice()
        {
            HamburgerPortion.Prices.Add(new MenuItemPrice { PriceTag = HappyHourPriceTag, Price = HamburgerHappyHourPrice });
            return this;
        }

    }
}
