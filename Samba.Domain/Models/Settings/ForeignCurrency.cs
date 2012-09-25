using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Settings
{
    public class ForeignCurrency : Entity
    {
        public string CurrencySymbol { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal Rounding { get; set; }
    }
}
