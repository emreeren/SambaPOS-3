using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using ComLib.Lang;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations
{
    [Export(typeof(IExpressionService))]
    public class ExpressionService : IExpressionService
    {
        private readonly Interpreter _interpreter;

        public ExpressionService()
        {
            _interpreter = new Interpreter();
            _interpreter.Context.Plugins.RegisterAll();
            _interpreter.Context.Types.Register(typeof(IServiceLocator), null);
            _interpreter.Memory.SetValue("locator", ServiceLocator.Current);
            _interpreter.Context.Types.Register(typeof(Ticket), null);
        }

        public string Eval(string expression)
        {
            _interpreter.Execute("result = " + expression);
            return _interpreter.Memory.Get<string>("result");
        }

        public bool EvalTicketCommand(string canExecuteScript, Ticket selectedTicket)
        {
            if (string.IsNullOrEmpty(canExecuteScript)) return true;
            try
            {
                _interpreter.Memory.SetValue("ticket", selectedTicket);
                _interpreter.Execute(canExecuteScript);
                return _interpreter.Memory.Get<bool>("result");
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}
