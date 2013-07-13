using System;
using System.ComponentModel.Composition;
using System.Linq.Expressions;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Specification;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PrinterModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class ExecutePrintJob : ActionType
    {
        private readonly ITicketService _ticketService;
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;
        private readonly IPrinterService _printerService;

        [ImportingConstructor]
        public ExecutePrintJob(ITicketService ticketService, IApplicationState applicationState, ICacheService cacheService, IPrinterService printerService)
        {
            _ticketService = ticketService;
            _applicationState = applicationState;
            _cacheService = cacheService;
            _printerService = printerService;
        }

        public override void Process(ActionData actionData)
        {
            var ticket = actionData.GetDataValue<Ticket>("Ticket");
            var pjName = actionData.GetAsString("PrintJobName");
            if (!string.IsNullOrEmpty(pjName))
            {
                _ticketService.UpdateTicketNumber(ticket, _applicationState.CurrentTicketType.TicketNumerator);
                var j = _cacheService.GetPrintJobByName(pjName);

                if (j != null)
                {
                    if (ticket != null)
                    {
                        var orderTagName = actionData.GetAsString("OrderTagName");
                        var orderTagValue = actionData.GetAsString("OrderTagValue");
                        var orderStateName = actionData.GetAsString("OrderStateName");
                        var orderState = actionData.GetAsString("OrderState");
                        var orderStateValue = actionData.GetAsString("OrderStateValue");
                        Expression<Func<Order, bool>> expression = ex => true;
                        if (!string.IsNullOrWhiteSpace(orderTagName))
                        {
                            expression = ex => ex.OrderTagExists(y => y.TagName == orderTagName && y.TagValue == orderTagValue);
                        }
                        if (!string.IsNullOrWhiteSpace(orderStateName))
                        {
                            expression = expression.And(ex => ex.IsInState(orderStateName, orderState));
                            if (!string.IsNullOrWhiteSpace(orderStateValue))
                                expression = expression.And(ex => ex.IsInState(orderStateValue));
                        }
                        _printerService.PrintTicket(ticket, j, expression.Compile());
                    }
                    else
                        _printerService.ExecutePrintJob(j);
                }
            }
        }

        protected override object GetDefaultData()
        {
            return
                new
                    {
                        PrintJobName = "",
                        OrderStateName = "",
                        OrderState = "",
                        OrderStateValue = "",
                        OrderTagName = "",
                        OrderTagValue = ""
                    };
        }

        protected override string GetActionName()
        {
            return Resources.ExecutePrintJob;
        }

        protected override string GetActionKey()
        {
            return ActionNames.ExecutePrintJob;
        }
    }
}
