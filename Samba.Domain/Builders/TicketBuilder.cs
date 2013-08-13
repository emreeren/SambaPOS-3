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
        private readonly IList<OrderData> _orders;
        private bool _taxIncluded;

        private TicketBuilder(TicketType ticketType, Department department)
        {
            _orders = new List<OrderData>();
            _calculations = new List<CalculationType>();
            _exchangeRate = 1m;
            _department = department;
            _ticketType = ticketType;
            _taxIncluded = _ticketType.TaxIncluded;
        }

        public static TicketBuilder Create(TicketType ticketType, Department department)
        {
            return  new TicketBuilder(ticketType,department);
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
                                 TaxIncluded = _taxIncluded,
                                 TransactionDocument = new AccountTransactionDocument()
                             };

            foreach (var orderData in _orders)
            {
                result.AddOrder(orderData.Order, orderData.TaxTemplates, orderData.TransactionType, orderData.UserName);
            }

            foreach (var calculation in _calculations)
            {
                result.AddCalculation(calculation, calculation.Amount);
            }

            result.Recalculate();

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
            _taxIncluded = _ticketType.TaxIncluded;
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

        public TicketBuilder AddOrder(OrderBuilder orderBuilder)
        {
            orderBuilder.WithDepartment(_department);
            orderBuilder.WithAccountTransactionType(_ticketType.SaleTransactionType);
            _orders.Add(new OrderData(orderBuilder));
            return this;
        }

        public void Link(OrderBuilder orderBuilder)
        {
            AddOrder(orderBuilder);
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

        public TicketBuilder TaxExcluded()
        {
            _taxIncluded = false;
            return this;
        }

        public TicketBuilder TaxIncluded()
        {
            _taxIncluded = true;
            return this;
        }
    }

    internal class OrderData
    {
        public OrderData(OrderBuilder orderBuilder)
        {
            Order = orderBuilder.Build();
            TaxTemplates = orderBuilder.GetTaxTemplates();
            TransactionType = orderBuilder.GetTransactionType();
            UserName = orderBuilder.GetUserName();
        }

        public Order Order { get; set; }
        public IEnumerable<TaxTemplate> TaxTemplates { get; set; }
        public AccountTransactionType TransactionType { get; set; }
        public string UserName { get; set; }
    }
}