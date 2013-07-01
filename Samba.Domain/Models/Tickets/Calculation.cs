using System;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class Calculation : ValueClass
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public int CalculationTypeId { get; set; }
        public int TicketId { get; set; }
        public int AccountTransactionTypeId { get; set; }
        public int CalculationType { get; set; }
        public bool IncludeTax { get; set; }
        public bool DecreaseAmount { get; set; }
        public bool UsePlainSum { get; set; }
        public decimal Amount { get; set; }
        public decimal CalculationAmount { get; set; }

        public void Update(decimal sum, decimal currentSum, int decimals)
        {
            if (CalculationType == 0)
            {
                CalculationAmount = Amount > 0 ? (sum * Amount) / 100 : 0;
            }
            else if (CalculationType == 1)
            {
                CalculationAmount = Amount > 0 ? (currentSum * Amount) / 100 : 0;
            }
            else if (CalculationType == 3)
            {
                if (Amount == currentSum) Amount = 0;
                else if (currentSum > 0 && DecreaseAmount && Amount > currentSum)
                    Amount = 0;
                else if (currentSum > 0 && !DecreaseAmount && Amount < currentSum)
                    Amount = 0;
                else
                    CalculationAmount = Amount - currentSum;
            }
            else if (CalculationType == 4)
            {
                if (Amount > 0)
                    CalculationAmount = (decimal.Round(currentSum / Amount, MidpointRounding.AwayFromZero) * Amount) - currentSum;
                else // eğer yuvarlama eksi olarak verildiyse hep aşağı yuvarlar
                    CalculationAmount = (Math.Truncate(currentSum / Amount) * Amount) - currentSum;
                if (DecreaseAmount && CalculationAmount > 0) CalculationAmount = 0;
                if (!DecreaseAmount && CalculationAmount < 0) CalculationAmount = 0;
            }
            else CalculationAmount = Amount;

            CalculationAmount = Decimal.Round(CalculationAmount, decimals);
            if (DecreaseAmount && CalculationAmount > 0) CalculationAmount = 0 - CalculationAmount;
        }

        public void UpdateCalculationTransaction(AccountTransactionDocument document, decimal amount, decimal exchangeRate)
        {
            document.UpdateSingletonTransactionAmount(AccountTransactionTypeId, Name, amount, exchangeRate);
            if (amount == 0 && Amount == 0 && document.AccountTransactions.Any(x => x.AccountTransactionTypeId == AccountTransactionTypeId))
            {
                document.AccountTransactions.Remove(
                    document.AccountTransactions.Single(x => x.AccountTransactionTypeId == AccountTransactionTypeId));
            }
        }
    }
}
