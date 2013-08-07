using System;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class VoidTicket : ActionType
    {
        private readonly ICacheService _cacheService;
        private readonly ITicketDao _ticketDao;
        private readonly ITicketService _ticketService;
        private readonly IApplicationStateSetter _applicationStateSetter;
       

        [ImportingConstructor]
        public VoidTicket(ITicketService ticketService, ICacheService cacheService, ITicketDao ticketDao, IApplicationStateSetter applicationStateSetter)
        {
            _ticketService = ticketService;
            _cacheService = cacheService;
            _applicationStateSetter = applicationStateSetter;
            _ticketDao = ticketDao;
        }

        public override void Process(ActionData actionData)
        {
            var ticketId = actionData.GetDataValueAsInt("TicketId");
            Ticket ticket;

            if (ticketId != null && ticketId != 0)
            { ticket = _ticketService.OpenTicket(ticketId); }
            else
            { ticket = actionData.GetDataValue<Ticket>("Ticket"); }

            var orders = Helper.GetOrders(actionData, ticket);
            var voidTicket = new Ticket();
            string voidTicketTypeName = actionData.GetAsString("TicketType");

            var isAlreadyVoided = (from t in _ticketDao.GetAllTickets() where t.VoidsTicketId == ticket.Id
                                   select t).FirstOrDefault();

            if (ticket != null && ticket.VoidsTicketId == 0 && isAlreadyVoided == null)
            {
                if (voidTicketTypeName == null || voidTicketTypeName == string.Empty)
                { voidTicketTypeName = string.Format("{0} {1}", Resources.Void,
                    _cacheService.GetTicketTypeById(ticket.TicketTypeId).Name); }
                var voidTicketType = (from type in _cacheService.GetTicketTypes()
                                      where type.Name == voidTicketTypeName
                                      select type).FirstOrDefault();

                if (voidTicketType == null || !voidTicketType.IsVoidType)
                { return; }

                voidTicket.VoidsTicketId = ticket.Id;
                voidTicket.TicketTypeId = voidTicketType.Id;
                voidTicket.TaxIncluded = voidTicketType.TaxIncluded;
                voidTicket.Date = DateTime.Now;
                voidTicket.DepartmentId = ticket.DepartmentId;

                _applicationStateSetter.SetCurrentDepartment(voidTicket.DepartmentId);
                _applicationStateSetter.SetCurrentTicketType(voidTicketType);

                foreach (TicketEntity entity in ticket.TicketEntities)
                { _ticketService.UpdateEntity(voidTicket, _cacheService.GetEntityById(entity.EntityId)); }

                string note = string.Empty;
                if (ticket.Note != null && ticket.Note != string.Empty)
                { note = string.Format("{0}\n", ticket.Note); }

                voidTicket.Note = string.Format("{2}Voids Ticket #{0}\nDescription: {1}", ticket.TicketNumber,
                    actionData.GetAsString("VoidDescription"), note); // TODO:  localisation-string
                
                voidTicket.TransactionDocument = new AccountTransactionDocument();
        
                foreach (Order order in orders)
                {
                    
                    var voidOrder = _ticketService.AddOrder(voidTicket, order.MenuItemId, order.Quantity,
                        order.PortionName);
                    

                    voidOrder.DecreaseInventory = true;
                    voidOrder.IncreaseInventory = false;
                    
                    voidOrder.PublishEvent(EventTopicNames.OrderAdded);
                }
                //_ticketService.ChangeOrdersAccountTransactionTypeId(voidTicket, voidTicket.Orders,
                //    voidTicketType.SaleTransactionType.Id);
                //_ticketService.RecalculateTicket(voidTicket);
                foreach (Payment payment in ticket.Payments)
                {
                    PaymentType payTpl = _cacheService.GetPaymentTypeByName(string.Format("{0} {1}", Resources.Void,
                        _cacheService.GetPaymentTypeById(payment.PaymentTypeId).Name));
                    if (payTpl != null)
                    { _ticketService.AddPayment(voidTicket, payTpl, payTpl.Account, payment.Amount, payment.Amount); }
                }

                _ticketService.CloseTicket(voidTicket);
            }
        }

        protected override string GetActionKey()
        { return ActionNames.VoidTicket; }

        protected override string GetActionName()
        { return string.Format("{0} {1}", Resources.Void, Resources.Ticket); }

        protected override object GetDefaultData()
        {
            return new { TicketId = 0, VoidDescription = "", TicketType = "" }; // NOTE: possibility to add TicketTag-Support
        }
    }
}
