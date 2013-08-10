using System;
using System.Collections.Generic;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;

namespace Samba.Domain.Builders
{
    public class TicketBuilder
    {
        private TicketType _ticketType;
        private Department _department;
        private decimal _exchangeRate;
        private readonly IList<CalculationType> _calculations;

        public TicketBuilder()
        {
            _calculations = new List<CalculationType>();
            _exchangeRate = 1m;
        }

        public static TicketBuilder Create()
        {
            return new TicketBuilder();
        }

        public TicketBuilder WithTicketType(TicketType ticketType)
        {
            _ticketType = ticketType;
            return this;
        }

        public Ticket Build()
        {
            if (_department == null) throw new ArgumentNullException("Department");
            if (_ticketType == null) throw new ArgumentNullException("TicketType");
            var result = new Ticket
                             {
                                 TicketTypeId = _ticketType.Id,
                                 DepartmentId = _department.Id,
                                 ExchangeRate = _exchangeRate,
                                 TaxIncluded = _ticketType.TaxIncluded,
                                 TransactionDocument = new AccountTransactionDocument()
                             };
            foreach (var calculation in _calculations)
            {
                result.AddCalculation(calculation, calculation.Amount);
            }
            return result;
        }

        public TicketBuilder ForDepartment(Department department)
        {
            _department = department;
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
    }
}