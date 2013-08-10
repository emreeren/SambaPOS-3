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
    class RefundTicket : ActionType
    {
        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly ICacheService _cacheService;
        private readonly ITicketDao _ticketDao;
        private readonly ITicketService _ticketService;

        [ImportingConstructor]
        public RefundTicket(ITicketService ticketService, ICacheService cacheService, ITicketDao ticketDao,
            IApplicationStateSetter applicationStateSetter, IApplicationState applicationState)
        {
            _ticketService = ticketService;
            _cacheService = cacheService;
            _applicationStateSetter = applicationStateSetter;
            _applicationState = applicationState;
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
            var refundTicket = new Ticket();

            string refundPrefix = actionData.GetAsString("RefundPrefix");

            string refundTicketTypeName = actionData.GetAsString("TicketType");

            var isAlreadyRefunded = (from t in _ticketDao.GetAllTickets() where t.RefundsTicketId == ticket.Id
                                     select t).FirstOrDefault();

            if (ticket != null && ticket.RefundsTicketId == 0 && isAlreadyRefunded == null)
            {
                if (refundTicketTypeName == null || refundTicketTypeName == string.Empty)
                { refundTicketTypeName = string.Format("{0} {1}", refundPrefix,
                    _cacheService.GetTicketTypeById(ticket.TicketTypeId).Name); }
                var refundTicketType = _cacheService.GetTicketTypeByName(refundTicketTypeName);
                    
                //(from type in _cacheService.GetTicketTypes()
                //                    where type.Name == refundTicketTypeName
                //                    select type).FirstOrDefault();

                if (refundTicketType == null || !refundTicketType.IsRefundType)
                { 
                    throw new ArgumentOutOfRangeException("RefundTicketType",
                        string.Format("The refund-tickettype {0} wasn't found.", refundTicketTypeName));
                }

                refundTicket.RefundsTicketId = ticket.Id;
                refundTicket.TicketTypeId = refundTicketType.Id;                
                refundTicket.TaxIncluded = refundTicketType.TaxIncluded;
                refundTicket.Date = DateTime.Now;
                refundTicket.DepartmentId = ticket.DepartmentId;

                TicketTag refundTicketTag = null;
                TicketTagGroup refundTicketTagGroup = null;

                bool ticketTagSetted = true;

                if (actionData.GetAsString("TicketTagGroup") != string.Empty &&
                    actionData.GetAsString("TicketTag") != string.Empty)
                {
                    refundTicketTagGroup = _cacheService.GetTicketTagGroupByName(actionData.GetAsString("TicketTagGroup"));

                    if (refundTicketTagGroup == null)
                    {
                        refundTicketTag = (from ticketTag in refundTicketTagGroup.TicketTags
                                           where ticketTag.Name == actionData.GetAsString("TicketTag")
                                           select ticketTag).FirstOrDefault();
                        if (refundTicketTag == null)
                        { refundTicketTag = new TicketTag() { Name = actionData.GetAsString("TicketTag") }; }

                        if (refundTicketTag != null)
                        { _ticketService.UpdateTag(refundTicket, refundTicketTagGroup, refundTicketTag); }
                        else
                        { ticketTagSetted = false; }
                    }
                    else
                    { ticketTagSetted = false; }
                }
                else
                { ticketTagSetted = false; }
               
                if (ticketTagSetted && ticket.TicketTags != null && ticket.TicketTags != string.Empty)
                { refundTicket.TicketTags = ticket.TicketTags; }
               

                _applicationStateSetter.SetCurrentDepartment(refundTicket.DepartmentId);
                _applicationStateSetter.SetCurrentTicketType(refundTicketType);
                

                foreach (TicketEntity entity in ticket.TicketEntities)
                { _ticketService.UpdateEntity(refundTicket, _cacheService.GetEntityById(entity.EntityId)); }

                string note = string.Empty;
                if (ticket.Note != null && ticket.Note != string.Empty)
                { note = string.Format("{0}\n", ticket.Note); }

                refundTicket.Note = string.Format(Resources.RefundNote, note, refundPrefix, ticket.TicketNumber,
                    actionData.GetAsString("RefundDescription")); // TODO:  localisation-string
                
                refundTicket.TransactionDocument = new AccountTransactionDocument();

                foreach (Order order in orders)
                {
                    var voidOrder = _ticketService.AddOrder(refundTicket, order.MenuItemId, order.Quantity,
                        order.PortionName);
                   
                    voidOrder.DecreaseInventory = true;
                    voidOrder.IncreaseInventory = false;
                    
                    voidOrder.PublishEvent(EventTopicNames.OrderAdded);
                }

                foreach (Payment payment in ticket.Payments)
                {
                    PaymentType payTpl = null;
                    string paymentNameStr = string.Format("{0} {1}", Resources.Refund,
                        _cacheService.GetPaymentTypeById(payment.PaymentTypeId).Name);
                    try
                    { payTpl = _cacheService.GetPaymentTypeByName(paymentNameStr); }
                    catch (Exception)
                    {
                        throw new ArgumentOutOfRangeException("PamentNameString",
                            string.Format("The paymentType {0} wasn't found.", paymentNameStr));
                    }
                    if (payTpl != null)
                    { _ticketService.AddPayment(refundTicket, payTpl, payTpl.Account, payment.Amount, payment.Amount); }
                }

                _ticketService.CloseTicket(refundTicket);
            }
        }

        protected override string GetActionKey()
        { return ActionNames.RefundTicket; }

        protected override string GetActionName()
        { return string.Format("{0} {1}", Resources.Refund, Resources.Ticket); }

        protected override object GetDefaultData()
        { return new { TicketId = 0, RefundPrefix = Resources.Refund, RefundDescription = "", TicketType = "", TicketTagGroup = "", TicketTag = "" }; }
    }
}