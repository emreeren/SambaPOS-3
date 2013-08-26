using System;

namespace Samba.Infrastructure.Helpers
{
    public static class QuantityFuncParser
    {
        enum Operations
        {
            Set, Add, Subtract
        }

        public static int Parse(string quantityFunc, int currentQuantity)
        {
            if (string.IsNullOrEmpty(quantityFunc)) return 0;
            int value;
            var qf = quantityFunc;
            var operation = Operations.Set;
            if (qf.StartsWith("+"))
            {
                operation = Operations.Add;
            }
            if (qf.StartsWith("-"))
            {
                operation = Operations.Subtract;
            }
            var trimmed = quantityFunc.Trim('-', '+', ' ');
            Int32.TryParse(trimmed, out value);
            if (operation == Operations.Add) return currentQuantity + value;
            if (operation == Operations.Subtract) return currentQuantity - value;
            return value;
        }
    }
}