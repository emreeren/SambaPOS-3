using NUnit.Framework;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure;
using Samba.Infrastructure.Helpers;

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
            var result = ExpressionService.Eval("result = Ticket.Model.Id", (new { Ticket = ticket }).ToDynamic(), 0);
            Assert.AreEqual(1, result);
        }

        [Test]
        public void CanReadExchangeRate()
        {
            var ticket = new Ticket(1);
            var result = ExpressionService.Eval("result = Ticket.Model.ExchangeRate", (new { Ticket = ticket }).ToDynamic(), 0m);
            Assert.AreEqual(1m, result);
        }

        [Test]
        public void CanUpdateExchangeRate()
        {
            var ticket = new Ticket(1);
            ExpressionService.Eval("Ticket.Model.ExchangeRate=1.15", (new { Ticket = ticket }).ToDynamic(), 0);
            Assert.AreEqual(1.15m, ticket.ExchangeRate);
            var result = ExpressionService.Eval("result = Ticket.Model.ExchangeRate", (new { Ticket = ticket }).ToDynamic(), 0m);
            Assert.AreEqual(1.15m, result);
        }

        [Test]
        public void CanTestTicketState()
        {
            var ticket = new Ticket(1);
            ticket.SetStateValue("Status", "New", "");
            var result = ExpressionService.Eval("result = Ticket.IsInState('New')", (new { Ticket = ticket }).ToDynamic(), false);
            Assert.AreEqual(true, result);
            result = ExpressionService.Eval("result = Ticket.InState('Status','New')", (new { Ticket = ticket }).ToDynamic(), false);
            Assert.AreEqual(true, result);
            ticket.SetStateValue("Status", "Paid", "");
            result = ExpressionService.Eval("result = Ticket.InState('Status','New')", (new { Ticket = ticket }).ToDynamic(), false);
            Assert.AreEqual(false, result);
        }

        [Test]
        public void CanSupportFluentSyntax()
        {
            var ticket = new Ticket(1);
            ticket.SetStateValue("Status", "New", "");
            var result = ExpressionService.Eval("result = IsInState Ticket 'New'", (new { Ticket = ticket }).ToDynamic(), false);
            Assert.AreEqual(true, result);
            result = ExpressionService.Eval("result = Ticket IsInState 'New'", (new { Ticket = ticket }).ToDynamic(), false);
            Assert.AreEqual(true, result);
            result = ExpressionService.Eval("result = Ticket InState 'Status','New'", (new { Ticket = ticket }).ToDynamic(), false);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void CanSupporIsKeyword()
        {
            var result = ExpressionService.Eval("result = 1 is 1", null, false);
            Assert.AreEqual(true, result);
            result = ExpressionService.Eval("if 1 is 1 then result = true", null, false);
            Assert.AreEqual(true, result);
            result = ExpressionService.Eval("if 2 is any of (3,4,1,2) then result = true", null, false);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void CanSupportStringMethods()
        {
            var result = ExpressionService.Eval("result = 'EMRE'.substr(0,2)", null, "");
            Assert.AreEqual("EM", result);
            var result1 = ExpressionService.Eval("result = 'EMRE'.length is 4", null, false);
            Assert.AreEqual(true, result1);
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
            ExpressionService.Eval("Order.Price = Order.Price / 2", (new { Order = order }).ToDynamic(), 0);
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

        [Test]
        public void CanGenerateCheckDigit()
        {
            var str = "EMRE";
            var cd = Utility.GenerateCheckDigit(str);
            str = str + cd;
            var valid = Utility.ValidateCheckDigit(str);
            Assert.True(valid);
        }
    }
}
