using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Samba.Domain.Builders;

namespace Samba.Domain.Tests
{
    [TestFixture]
    class MenuItemBuilderTests
    {
        [Test]
        public void MenuBuilder_BuildMenuItem_NameAssigned()
        {
            var menuItem = MenuItemBuilder.Create("Tost").Build();
            Assert.AreEqual("Tost", menuItem.Name);
        }

        [Test]
        public void MenuBuilder_AddPortion_PortionAdded()
        {
            var menuItem = MenuItemBuilder.Create("Hamburger")
                                      .AddPortion("Adet", 5)
                                      .Build();
            Assert.AreEqual(1, menuItem.Portions.Count);
        }
        
        [Test]
        public void MenuBuilder_AddMultiplePortions_PortionCountCorrect()
        {
            var menuItem = MenuItemBuilder.Create("Hamburger")
                                      .AddPortion("Küçük", 5)
                                      .AddPortion("Büyük", 8)
                                      .Build();
            Assert.AreEqual(2, menuItem.Portions.Count);
        }

        [Test]
        public void MenuItemBuilder_AddGroupCode_GroupCodeAssigned()
        {
            var menuItem = MenuItemBuilder.Create("Hamburger")
                                          .AddPortion("Küçük", 5)
                                          .WithGroupCode("İçecekler")
                                          .Build();
            Assert.AreEqual("İçecekler",menuItem.GroupCode);
        }

        [Test]
        public void MenuItemBuilder_UpdateProductTag_ProductTagAssigned()
        {
            var menuItem = MenuItemBuilder.Create("Tost")
                                          .AddPortion("Küçük", 2)
                                          .WithProductTag("NoSale")
                                          .Build();
            Assert.AreEqual("NoSale",menuItem.Tag);
        }
    }
}
