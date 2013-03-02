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

    }
}
