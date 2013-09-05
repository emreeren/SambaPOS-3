using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Samba.Domain.Models.Entities;

namespace Samba.Domain.Tests
{
    [TestFixture]
    class EntityTest
    {
        [Test]
        public void CanUpdateEntityCustomData()
        {
            var entity = new Entity();
            entity.SetCustomData("Age", "12");
            Assert.AreEqual("12", entity.GetCustomData("Age"));
        }

        [Test]
        public void CanSetEntityCustomDataWithIncFunction()
        {
            var entity = new Entity();
            entity.SetCustomData("Age", "+12");
            Assert.AreEqual("12", entity.GetCustomData("Age"));
        }

        [Test]
        public void CanIncAlreadySetCustomData()
        {
            var entity = new Entity();
            entity.SetCustomData("Age", "11");
            entity.SetCustomData("Age", "+1");
            Assert.AreEqual("12", entity.GetCustomData("Age"));
        }

        [Test]
        public void CanDecAlreadySetCustomData()
        {
            var entity = new Entity();
            entity.SetCustomData("Age", "13");
            entity.SetCustomData("Age", "-1");
            Assert.AreEqual("12", entity.GetCustomData("Age"));
        }

        [Test]
        public void CanHandleNonFunction()
        {
            var entity = new Entity();
            entity.SetCustomData("Name", "Ahmet");
            entity.SetCustomData("Name", "+Mehmet");
            Assert.AreEqual("+Mehmet", entity.GetCustomData("Name"));
        }

        [Test]
        public void CanHandleNonNumericTarget()
        {
            var entity = new Entity();
            entity.SetCustomData("Name", "Ahmet");
            entity.SetCustomData("Name", "+1");
            Assert.AreEqual("1", entity.GetCustomData("Name"));
        }


    }
}
