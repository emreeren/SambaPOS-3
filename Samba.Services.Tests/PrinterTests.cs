using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Documents;
using NUnit.Framework;
using Samba.Domain.Builders;
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
            var hamburger = MenuItemBuilder.Create("Hamburger").WithId(1).WithGroupCode("Food").WithProductTag("Tag1").AddPortion("Small", 5).Build();
            var pizza = MenuItemBuilder.Create("Pizza").WithId(2).WithGroupCode("Food").WithProductTag("Tag1").AddPortion("Small", 10).Build();
            var cola = MenuItemBuilder.Create("Coke").WithId(3).WithGroupCode("Drink").AddPortion("Small", 2).Build();

            using (var w = WorkspaceFactory.Create())
            {
                w.Add(hamburger);
                w.Add(pizza);
                w.Add(cola);
            }

            var ticket = TicketBuilder.Create(TicketType.Default, Department.Default)
                                       .AddOrderFor(hamburger).Do()
                                       .AddOrderFor(pizza).Do()
                                       .AddOrderFor(cola).Do()
                                       .Build();

            return ticket;
        }
    }
}
