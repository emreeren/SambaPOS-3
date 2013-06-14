using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Settings;
using Samba.Persistance;
using Samba.Persistance.Data;

namespace Samba.Services.Tests
{
    [TestFixture]
    class EntityTests
    {
        protected IEntityService EntityService { get; set; }
        protected IEntityDao EntityDao { get; set; }
        protected IWorkspace Workspace { get; set; }
        protected EntityType CustomerEntityType { get; set; }

        [SetUp]
        public void Setup()
        {
            MefBootstrapper.ComposeParts();
            EntityService = MefBootstrapper.Resolve<IEntityService>();
            EntityDao = MefBootstrapper.Resolve<IEntityDao>();
            Workspace = PrepareWorkspace("sd1.txt");
        }

        [Test]
        public void CanSetDefaultValues()
        {
            var customer4 = new Entity { EntityTypeId = 1 };
            customer4.SetDefaultValues("çapulcu emre");
            Assert.AreEqual("Çapulcu Emre", customer4.Name);

            var customer3 = new Entity { EntityTypeId = 1 };
            customer3.SetDefaultValues("Phone:5555555");
            Assert.AreEqual("5555555", customer3.GetCustomData("Phone"));
        }

        [Test]
        public void CanSearchEntities()
        {
            var customer1 = new Entity { Name = "Emre Eren", EntityTypeId = CustomerEntityType.Id };
            customer1.SetCustomData("Phone", "1111111");
            Assert.AreEqual("1111111", customer1.GetCustomData("Phone"));
            Workspace.Add(customer1);

            var customer2 = new Entity { Name = "Hasan Bulut", EntityTypeId = CustomerEntityType.Id };
            customer2.SetCustomData("Phone", "2222222");
            Assert.AreEqual("2222222", customer2.GetCustomData("Phone"));
            Workspace.Add(customer2);

            Workspace.CommitChanges();

            var customers = Workspace.All<Entity>(x => x.EntityTypeId == CustomerEntityType.Id).ToList();
            Assert.AreEqual(2, customers.Count());

            customer2 = customers.Single(x => x.Name == "Hasan Bulut");
            customer2.SetCustomData("Phone", "3333333");
            Workspace.CommitChanges();

            customer2 = Workspace.Single<Entity>(x => x.Name == "Hasan Bulut");
            Assert.AreEqual("3333333", customer2.GetCustomData("Phone"));

            var foundItems = EntityService.SearchEntities(CustomerEntityType, "111", "");
            Assert.AreEqual(1, foundItems.Count);

            var phoneSearch2 = EntityService.SearchEntities(CustomerEntityType, "Phone:111", "");
            Assert.AreEqual(1, phoneSearch2.Count);
            Assert.AreEqual("Emre Eren", phoneSearch2[0].Name);
        }

        private IWorkspace PrepareWorkspace(string fileName)
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            var lp = new Uri(ass.CodeBase);
            string pth = Path.GetDirectoryName(lp.LocalPath);
            pth = Path.Combine(pth, "..\\..\\..\\Samba.Presentation");
            LocalSettings.AppPath = pth;
            LocalSettings.CurrentLanguage = "tr";
            var dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\tests";
            if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);
            var filePath = string.Format("{0}\\{1}", dataFolder, fileName);
            if (File.Exists(filePath)) File.Delete(filePath);
            WorkspaceFactory.UpdateConnection(filePath);
            var workspace = WorkspaceFactory.Create();

            CustomerEntityType = new EntityType { Name = "Customers", EntityName = "Customer" };
            CustomerEntityType.EntityCustomFields.Add(new EntityCustomField { EditingFormat = "(###) ### ####", FieldType = 0, Name = "Phone" });
            workspace.Add(CustomerEntityType);
            workspace.CommitChanges();

            return workspace;
        }
    }
}
