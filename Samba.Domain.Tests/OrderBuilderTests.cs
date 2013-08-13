using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Samba.Domain.Builders;
using Samba.Domain.Models.Accounts;
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
                                    .WithDepartment(testContext.Department)
                                    .ForMenuItem(testContext.Hamburger)
                                    .WithAccountTransactionType(testContext.AccountTransactionType)
                                    .Build();
            Assert.AreEqual(testContext.Hamburger.Name, order.MenuItemName);
            Assert.AreEqual(testContext.Hamburger.Id, order.MenuItemId);
        }

        [Test]
        public void OrderBuilder_CreateOrder_DefaultPortionAssigned()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                                    .WithDepartment(testContext.Department)
                                    .ForMenuItem(testContext.Hamburger)
                                    .WithAccountTransactionType(testContext.AccountTransactionType)
                                    .Build();
            Assert.AreEqual(testContext.HamburgerPortion.Name, order.PortionName);
        }

        [Test]
        public void OrderBuilder_CreateOrderWithPortion_SecondPortionAssigned()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                                    .WithDepartment(testContext.Department)
                                    .WithPortion(testContext.BigHamburger)
                                    .ForMenuItem(testContext.Hamburger)
                                    .WithAccountTransactionType(testContext.AccountTransactionType)
                                    .Build();
            Assert.AreEqual(testContext.BigHamburger.Name, order.PortionName);
        }

        [Test]
        public void OrderBuilder_CreateOrder_UserNameAssigned()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                                    .WithDepartment(testContext.Department)
                                    .ForMenuItem(testContext.Hamburger)
                                    .WithUserName("Waiter")
                                    .WithAccountTransactionType(testContext.AccountTransactionType)
                                    .Build();
            Assert.AreEqual("Waiter", order.CreatingUserName);
        }

        [Test]
        public void OrderBuilder_CreateOrderWithPriceTag_SkipsNonExistedPriceTag()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                                    .WithDepartment(testContext.Department)
                                    .ForMenuItem(testContext.Hamburger)
                                    .WithPriceTag("HH")
                                    .WithAccountTransactionType(testContext.AccountTransactionType)
                                    .Build();
            Assert.AreEqual("", order.PriceTag);
            Assert.AreEqual(testContext.HamburgerPortion.Price, order.Price);
        }

        [Test]
        public void OrderBuilder_CreateOrderWithPriceTag_UpdatesPrice()
        {
            var testContext = OrderBuilderTestContext.CreateDefault().WithHappyHourPrice();
            var order = OrderBuilder.Create()
                                    .WithDepartment(testContext.Department)
                                    .ForMenuItem(testContext.Hamburger)
                                    .WithPriceTag(testContext.HappyHourPriceTag)
                                    .WithAccountTransactionType(testContext.AccountTransactionType)
                                    .Build();
            Assert.AreEqual(testContext.HappyHourPriceTag, order.PriceTag);
            Assert.AreEqual(testContext.HamburgerHappyHourPrice, order.Price);
        }

        [Test]
        public void OrderBuilder_CreateOrderWithQuantity_UpdatesQuantity()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                                    .WithDepartment(testContext.Department)
                                    .ForMenuItem(testContext.Hamburger)
                                    .WithQuantity(5)
                                    .WithAccountTransactionType(testContext.AccountTransactionType)
                                    .Build();
            Assert.AreEqual(5, order.Quantity);
        }

        [Test]
        public void OrderBuilder_CreateOrder_IsDefaultQuantity()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                                    .WithDepartment(testContext.Department)
                                    .ForMenuItem(testContext.Hamburger)
                                    .WithAccountTransactionType(testContext.AccountTransactionType)
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
                                    .WithAccountTransactionType(testContext.AccountTransactionType)
                                    .Build();
            Assert.AreEqual(testContext.Department.Id, order.DepartmentId);
        }

        [Test]
        public void OrderBuilder_AddTaxTemplates_TaxTemplatesAdded()
        {
            var testContext = OrderBuilderTestContext.CreateDefault().With18PTaxTemplate();
            var order = OrderBuilder.Create()
                                    .ForMenuItem(testContext.Hamburger)
                                    .WithDepartment(testContext.Department)
                                    .AddTaxTemplate(testContext.TaxTemplate)
                                    .WithAccountTransactionType(testContext.AccountTransactionType)
                                    .Build();
            Assert.True(order.TaxValues.Any());
        }

        [Test]
        public void OrderBuilder_UpdateAccountTransactionType_AccountTransactionTypeAssigned()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                                    .ForMenuItem(testContext.Hamburger)
                                    .WithDepartment(testContext.Department)
                                    .WithAccountTransactionType(testContext.AccountTransactionType)
                                    .Build();
            Assert.AreEqual(testContext.AccountTransactionType.Id, order.AccountTransactionTypeId);
        }

        [Test]
        public void OrderBuilder_AssignProductTimer_ProductTimerAssigned()
        {
            var testContext = OrderBuilderTestContext.CreateDefault().WithProductTimer();
            var order = OrderBuilder.Create()
                                    .ForMenuItem(testContext.Hamburger)
                                    .WithDepartment(testContext.Department)
                                    .WithAccountTransactionType(testContext.AccountTransactionType)
                                    .WithProductTimer(testContext.ProductTimer)
                                    .Build();
            Assert.AreEqual(testContext.ProductTimer.Id, order.ProductTimerValue.ProductTimerId);
        }

        [Test]
        public void OrderBuilder_CreateMenuItem_CreatesMenuItem()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                                    .CreateMenuItem("Tost").AddPortion("Küçük", 1).Do()
                                    .WithDepartment(testContext.Department)
                                    .WithAccountTransactionType(testContext.AccountTransactionType)
                                    .Build();
            Assert.AreEqual("Tost", order.MenuItemName);
        }

        [Test]
        public void OrderBuilder_CreateMenuItem_UpdatesOrderPrice()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var order = OrderBuilder.Create()
                                    .CreateMenuItem("Tost").AddPortion("Küçük", 4.5m).Do()
                                    .WithDepartment(testContext.Department)
                                    .WithAccountTransactionType(testContext.AccountTransactionType)
                                    .Build();
            Assert.AreEqual(4.5m, order.Price);
        }

        [Test]
        public void OrderBuilder_CreateMenuItemWithDoublePortions_PriceCorrect()
        {
            var testContext = OrderBuilderTestContext.CreateDefault();
            var tost = MenuItemBuilder.Create("Tost").AddPortion("Küçük", 4.5m).AddPortion("Büyük", 8).Build();
            var order = OrderBuilder.Create()
                                    .ForMenuItem(tost)
                                    .WithPortion(tost.Portions[1])
                                    .WithDepartment(testContext.Department)
                                    .WithAccountTransactionType(testContext.AccountTransactionType)
                                    .Build();
            Assert.AreEqual(8m, order.Price);
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
            result.AccountTransactionType = new AccountTransactionType { Id = 15 };
            return result;
        }

        public MenuItem Hamburger { get; set; }
        public MenuItemPortion HamburgerPortion { get; set; }
        public MenuItemPortion BigHamburger { get; set; }

        public string HappyHourPriceTag { get; set; }
        public int HamburgerHappyHourPrice { get; set; }

        public Department Department { get; set; }
        public TaxTemplate TaxTemplate { get; set; }
        public AccountTransactionType AccountTransactionType { get; set; }
        public ProductTimer ProductTimer { get; set; }

        public OrderBuilderTestContext WithHappyHourPrice()
        {
            HamburgerPortion.Prices.Add(new MenuItemPrice { PriceTag = HappyHourPriceTag, Price = HamburgerHappyHourPrice });
            return this;
        }

        public OrderBuilderTestContext With18PTaxTemplate()
        {
            TaxTemplate = new TaxTemplate
                             {
                                 Id = 8,
                                 Name = "%18 Tax",
                                 Rate = 18,
                                 AccountTransactionType = AccountTransactionType.Default
                             };
            return this;
        }


        public OrderBuilderTestContext WithProductTimer()
        {
            ProductTimer = new ProductTimer { Id = 12 };
            return this;
        }
    }
}
