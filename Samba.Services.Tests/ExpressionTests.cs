using NUnit.Framework;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Tests
{
    [TestFixture]
    public class ExpressionTests
    {
        public IExpressionService ExpressionService { get; set; }

        [SetUp]
        public void Setup()
        {
            ExpressionService = MefBootstrapper.Resolve<IExpressionService>();
        }

        [Test]
        public void CanSumNumbers()
        {
            var result = ExpressionService.Eval("2+2");
            Assert.AreEqual("4", result);
            result = ExpressionService.Eval("2+3");
            Assert.AreEqual("5", result);
            result = ExpressionService.Eval("2*4");
            Assert.AreEqual("8", result);
        }

        [Test]
        public void CanReadTicketId()
        {
            var ticket = new Ticket(1);
            var result = ExpressionService.Eval("result = Ticket.Model.Id", new { Ticket = ticket }, 0);
            Assert.AreEqual(1, result);
        }

        [Test]
        public void CanReadExchangeRate()
        {
            var ticket = new Ticket(1);
            var result = ExpressionService.Eval("result = Ticket.Model.ExchangeRate", new { Ticket = ticket }, 0m);
            Assert.AreEqual(1m, result);
        }

        [Test]
        public void CanUpdateExchangeRate()
        {
            var ticket = new Ticket(1);
            ExpressionService.Eval("Ticket.Model.ExchangeRate=1.15", new { Ticket = ticket }, 0);
            Assert.AreEqual(1.15m, ticket.ExchangeRate);
            var result = ExpressionService.Eval("result = Ticket.Model.ExchangeRate", new { Ticket = ticket }, 0m);
            Assert.AreEqual(1.15m, result);
        }

        [Test]
        public void CanDivideValues()
        {
            var result = ExpressionService.Eval("10 / 2");
            Assert.AreEqual("5", result);
        }

        [Test]
        public void CanDivideOrderPrice()
        {
            var order = new Order { Price = 10, Quantity = 1 };
            ExpressionService.Eval("Order.Price = Order.Price / 2", new { Order = order }, 0);
            Assert.AreEqual(5, order.Price);
        }

        [Test]
        public void CanFormatNumbers()
        {
            var result = ExpressionService.Eval("F('10')");
            const int expected = 10;
            Assert.AreEqual(expected.ToString("#,#0.00"), result);

            result = ExpressionService.Eval("F('1000')");
            const int expected2 = 1000;
            Assert.AreEqual(expected2.ToString("#,#0.00"), result);

            result = ExpressionService.Eval("F(TN('5.000')*2)");
            const int expected3 = 10000;
            Assert.AreEqual(expected3.ToString("#,#0.00"), result);       
            
            result = ExpressionService.Eval("F(TN('11,00')/2)");
            const double expected4 = 5.5;
            Assert.AreEqual(expected4.ToString("#,#0.00"), result);

            result = ExpressionService.Eval("FF('11,0')");
            const double expected5 = 11;
            Assert.AreEqual(expected5.ToString("#,#0.00"), result);
        }
    }
}
