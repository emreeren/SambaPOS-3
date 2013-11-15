using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Settings
{
    public class ForeignCurrency : EntityClass
    {
        public string CurrencySymbol { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal Rounding { get; set; }

        private static ForeignCurrency _default;
        public static ForeignCurrency Default
        {
            get { return _default ?? (_default = new ForeignCurrency() { CurrencySymbol = "{0}", ExchangeRate = 1, Id = 0, Name = "Default" }); }
        }
    }
}
