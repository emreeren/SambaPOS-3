using System;
using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class TicketTemplate : Entity
    {
        public virtual Numerator TicketNumerator { get; set; }
        public virtual Numerator OrderNumerator { get; set; }

        private IList<TicketTagGroup> _ticketTagGroups;
        public virtual IList<TicketTagGroup> TicketTagGroups
        {
            get { return _ticketTagGroups; }
            set { _ticketTagGroups = value; }
        }

        private IList<CalculationTemplate> _calculationTemplates;
        public virtual IList<CalculationTemplate> CalulationTemplates
        {
            get { return _calculationTemplates; }
            set { _calculationTemplates = value; }
        }

        private IList<PaymentTemplate> _paymentTemplates;
        public virtual IList<PaymentTemplate> PaymentTemplates
        {
            get { return _paymentTemplates; }
            set { _paymentTemplates = value; }
        }

        private IList<OrderTagGroup> _orderTagGroups;
        public virtual IList<OrderTagGroup> OrderTagGroups
        {
            get { return _orderTagGroups; }
            set { _orderTagGroups = value; }
        }

        private IList<ResourceTemplate> _resourceTemplates;
        public virtual IList<ResourceTemplate> ResourceTemplates
        {
            get { return _resourceTemplates; }
            set { _resourceTemplates = value; }
        }

        public virtual AccountTransactionTemplate SaleTransactionTemplate { get; set; }
        
        public TicketTemplate()
        {
            _ticketTagGroups = new List<TicketTagGroup>();
            _calculationTemplates = new List<CalculationTemplate>();
            _paymentTemplates = new List<PaymentTemplate>();
            _orderTagGroups = new List<OrderTagGroup>();
            _resourceTemplates = new List<ResourceTemplate>();
        }
    }
}
