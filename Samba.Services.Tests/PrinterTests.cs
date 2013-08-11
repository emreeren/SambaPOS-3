using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Documents;
using NUnit.Framework;
using Samba.Domain.Builders;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Persistance.Data;
using Samba.Services.Implementations.PrinterModule.PrintJobs;

namespace Samba.Services.Tests
{
    [TestFixture]
    class PrinterTests
    {
        protected IPrinterService PrinterService { get; set; }
        protected Ticket Ticket { get; set; }

        [SetUp]
        public void Setup()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            LocalSettings.CurrencyFormat = "#,#0.00";

            var dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\tests";
            if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);
            var filePath = string.Format("{0}\\{1}", dataFolder, "1.txt");
            if (File.Exists(filePath)) File.Delete(filePath);
            WorkspaceFactory.UpdateConnection(filePath);

            Ticket = PrepareTestTicket();

            MefBootstrapper.ComposeParts();
            PrinterService = MefBootstrapper.Resolve<IPrinterService>();
        }

        [Test]
        public void CanFormatFillTag()
        {
            const string format = "<F>*";
            var result = PrinterService.GetPrintingContent(new Ticket(), format, 20);
            Assert.AreEqual("********************", result);
        }

        [Test]
        public void CanFormatTicket()
        {
            const string format = @"
12345678901234567890
{ORDERS}
[ORDERS]
<J00>{NAME}|{PRICE}";

            var result = PrinterService.GetPrintingContent(Ticket, format, 20);
            const string expectedResult = @"
12345678901234567890
Hamburger       5.00
Pizza          10.00
Coke            2.00";
            Assert.AreEqual(expectedResult.Trim(new[] { '\r', '\n' }), result);
        }

        [Test]
        public void CanGroupByProductGroup()
        {
            const string format = @"
12345678901234567890
{ORDERS}
[ORDERS]
<J00>{NAME}|{PRICE}
[ORDERS GROUP|PRODUCT GROUP]
-{GROUP KEY}";

            var result = PrinterService.GetPrintingContent(Ticket, format, 20);
            const string expectedResult = @"
12345678901234567890
-Drink
Coke            2.00
-Food
Hamburger       5.00
Pizza          10.00";
            Assert.AreEqual(expectedResult.Trim(new[] { '\r', '\n' }), result);
        }

        [Test]
        public void CanGroupByProductTag()
        {
            const string format = @"
12345678901234567890
{ORDERS}
[ORDERS]
<J00>{NAME}|{PRICE}
[ORDERS GROUP|PRODUCT TAG]
-{GROUP KEY}";

            var result = PrinterService.GetPrintingContent(Ticket, format, 20);
            const string expectedResult = @"
12345678901234567890
-
Coke            2.00
-Tag1
Hamburger       5.00
Pizza          10.00";
            Assert.AreEqual(expectedResult.Trim(new[] { '\r', '\n' }), result);
        }

        [Test]
        public void TextPrinterJob_InvalidShareName_ShouldHandleNullError()
        {
            var textpj = new TextPrinterJob(new Printer());
            Assert.Throws<InvalidOperationException>(() => textpj.DoPrint(new FlowDocument(new Paragraph(new Run("text")))));
        }

        private static Ticket PrepareTestTicket()
        {
            var hamburger = new MenuItem("Hamburger") { Id = 1, GroupCode = "Food", Tag = "Tag1" };
            var hportion = hamburger.AddPortion("Small", 5, "");
            var pizza = new MenuItem("Pizza") { Id = 2, GroupCode = "Food", Tag = "Tag1" };
            var pportion = pizza.AddPortion("Small", 10, "");
            var cola = new MenuItem("Coke") { Id = 3, GroupCode = "Drink" };
            var cportion = cola.AddPortion("Small", 2, "");

            using (var w = WorkspaceFactory.Create())
            {
                w.Add(hamburger);
                w.Add(pizza);
                w.Add(cola);
            }

            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default).Build();
            ticket.AddOrder(AccountTransactionType.Default, Department.Default, "Emre", hamburger, null, hportion, "", null);
            ticket.AddOrder(AccountTransactionType.Default, Department.Default, "Emre", pizza, null, pportion, "", null);
            ticket.AddOrder(AccountTransactionType.Default, Department.Default, "Emre", cola, null, cportion, "", null);
            return ticket;
        }
    }
}
