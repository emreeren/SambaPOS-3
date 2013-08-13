using System;
using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;

namespace Samba.Domain.Builders
{
    public class TicketBuilder : ILinkableToOrderBuilder<TicketBuilder>
    {
        private TicketType _ticketType;
        private Department _department;
        private decimal _exchangeRate;
        private readonly IList<CalculationType> _calculations;
        private readonly IList<Order> _orders;

        public TicketBuilder()
        {
            _orders = new List<Order>();
            _calculations = new List<CalculationType>();
            _exchangeRate = 1m;
        }

        public static TicketBuilder Create(TicketType ticketType, Department department)
        {
            var result = Create();
            result.WithDepartment(department);
            result.WithTicketType(ticketType);
            return result;
        }

        public static TicketBuilder Create()
        {
            return new TicketBuilder();
        }

        public Ticket Build()
        {
            if (_ticketType == null) throw new ArgumentNullException();
            if (_department == null) throw new ArgumentNullException();

            var result = new Ticket
                             {
                                 TicketTypeId = _ticketType.Id,
                                 DepartmentId = _department.Id,
                                 ExchangeRate = _exchangeRate,
                                 TaxIncluded = _ticketType.TaxIncluded,
                                 TransactionDocument = new AccountTransactionDocument()
                             };

            foreach (var order in _orders)
            {
                result.Orders.Add(order);
            }

            foreach (var calculation in _calculations)
            {
                result.AddCalculation(calculation, calculation.Amount);
            }
            return result;
        }

        public TicketBuilder WithDepartment(Department department)
        {
            _department = department;
            return this;
        }

        public TicketBuilder WithTicketType(TicketType ticketType)
        {
            _ticketType = ticketType;
            return this;
        }

        public TicketBuilder WithExchangeRate(decimal exchangeRate)
        {
            _exchangeRate = exchangeRate;
            return this;
        }

        public TicketBuilder WithCalculations(IEnumerable<CalculationType> calculations)
        {
            foreach (var calculationType in calculations)
            {
                AddCalculation(calculationType);
            }
            return this;
        }

        public TicketBuilder AddCalculation(CalculationType calculation)
        {
            if (string.IsNullOrEmpty(calculation.Name)) throw new ArgumentNullException("Calculation Name");
            if (calculation.AccountTransactionType == null) throw new ArgumentNullException("Calculation Transaction Type");
            _calculations.Add(calculation);
            return this;
        }

        public TicketBuilder AddOrder(Order order)
        {
            _orders.Add(order);
            return this;
        }

        public void Link(OrderBuilder orderBuilder)
        {
            orderBuilder.WithDepartment(_department);
            orderBuilder.WithAccountTransactionType(_ticketType.SaleTransactionType);
            AddOrder(orderBuilder.Build());
        }

        public OrderBuilderFor<TicketBuilder> AddOrder()
        {
            return OrderBuilderFor<TicketBuilder>.Create(this);
        }
        
        public OrderBuilderFor<TicketBuilder> AddOrderFor(MenuItem menuItem)
        {
            var result = AddOrder();
            result.ForMenuItem(menuItem);
            return result;
        }
    }
}