using System;
using NUnit.Framework;
using Samba.Domain.Models.Entities;

namespace Samba.Domain.Tests
{
    [TestFixture]
    class EntityTest
    {
        [Test]
        public void CanUpdateEntityDateData()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", DateTime.Today.ToShortDateString());
            Assert.AreEqual(DateTime.Today.ToShortDateString(), entity.GetCustomData("Birthday"));
        }

        [Test]
        public void CanUpdateEntityDateDataWithTodayTag()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", "Today");
            Assert.AreEqual(DateTime.Today.ToShortDateString(), entity.GetCustomData("Birthday"));
        }

        [Test]
        public void CanUpdateEntityDateDataWithTodayTagAndInc()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", "Today+2");
            Assert.AreEqual(DateTime.Today.AddDays(2).ToShortDateString(), entity.GetCustomData("Birthday"));
        }

        [Test]
        public void CanUpdateEntityDateDataWithTodayTagAndInc2()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", "Today+30");
            Assert.AreEqual(DateTime.Today.AddDays(30).ToShortDateString(), entity.GetCustomData("Birthday"));
        }

        [Test]
        public void CanHandleEntityDateFunctionError()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", "Today+");
            Assert.AreEqual(DateTime.Today.AddDays(1).ToShortDateString(), entity.GetCustomData("Birthday"));
        }     
        
        [Test]
        public void CanHandleEntityDateFunctionError1()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", "Today + 1");
            Assert.AreEqual(DateTime.Today.AddDays(1).ToShortDateString(), entity.GetCustomData("Birthday"));
        }       
        
        [Test]
        public void CanHandleEntityDateFunctionError2()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", "Today + ");
            Assert.AreEqual(DateTime.Today.AddDays(1).ToShortDateString(), entity.GetCustomData("Birthday"));
        }
        
        [Test]
        public void CanHandleEntityDateFunctionError3()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", "Today + 1 ");
            Assert.AreEqual(DateTime.Today.AddDays(1).ToShortDateString(), entity.GetCustomData("Birthday"));
        }       
        
        [Test]
        public void CanHandleEntityDateFunctionError4()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", "Today + a ");
            Assert.AreEqual("Today + a ", entity.GetCustomData("Birthday"));
        }
        
        [Test]
        public void CanHandleEntityDateFunctionError5()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", "To day + a ");
            Assert.AreEqual("To day + a ", entity.GetCustomData("Birthday"));
        }       
        
        [Test]
        public void CanHandleEntityDateFunctionError6()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", "+A");
            Assert.AreEqual("+A", entity.GetCustomData("Birthday"));
        }      
        
        [Test]
        public void CanHandleEntityDateFunctionError7()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", "+ A ");
            Assert.AreEqual("+ A ", entity.GetCustomData("Birthday"));
        }

        [Test]
        public void CanUpdateEntityDateDataWithTodayTagAndDec()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", "Today-2");
            Assert.AreEqual(DateTime.Today.AddDays(-2).ToShortDateString(), entity.GetCustomData("Birthday"));
        }        
        
        [Test]
        public void CanUpdateEntityDateDataWithDayNumber()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", "+3");
            Assert.AreEqual("3", entity.GetCustomData("Birthday"));
            entity.SetCustomData("Birthday", "+4");
            Assert.AreEqual("7", entity.GetCustomData("Birthday"));
        }

        [Test]
        public void CanUpdateEntityDateDataWithTodayTagAndInc3()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", "Today");
            entity.SetCustomData("Birthday", "+2");
            Assert.AreEqual(DateTime.Today.AddDays(2).ToShortDateString(), entity.GetCustomData("Birthday"));
        }

        [Test]
        public void CanUpdateEntityDateDataWithTodayTagAndDec2()
        {
            var entity = new Entity();
            entity.SetCustomData("Birthday", "Today");
            entity.SetCustomData("Birthday", "-2");
            Assert.AreEqual(DateTime.Today.AddDays(-2).ToShortDateString(), entity.GetCustomData("Birthday"));
        }

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
        public void CanIncNullCustomData()
        {
            var entity = new Entity();
            entity.SetCustomData("Age", null);
            entity.SetCustomData("Age", "+2");
            Assert.AreEqual("2", entity.GetCustomData("Age"));
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
            Assert.AreEqual("+1", entity.GetCustomData("Name"));
        }


    }
}
