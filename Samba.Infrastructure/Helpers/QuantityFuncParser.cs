using System;
using System.Linq;

namespace Samba.Infrastructure.Helpers
{
    public static class QuantityFuncParser
    {
        enum Operations
        {
            Set, Add, Subtract
        }

        public static string Parse(string quantityFunc, string currentQuantity)
        {
            if (!IsFunc(quantityFunc)) return quantityFunc;
            int quantity;
            if(!int.TryParse(currentQuantity, out quantity) && !string.IsNullOrEmpty(currentQuantity)) return quantityFunc;
            return Parse(quantityFunc, quantity).ToString();
        }

        private static bool IsFunc(string quantityFunc)
        {
            if (string.IsNullOrEmpty(quantityFunc)) return false;
            if (quantityFunc.Length == 1) return false;
            var operation = quantityFunc[0];
            if ("+-".All(x => x != operation)) return false;
            var value = quantityFunc.Substring(1);
            return value.All(x => ContainsChar("1234567890", x));
        }

        private static bool ContainsChar(string set, char value)
        {
            return set.ToCharArray().Any(x => x == value);
        }

        private static Operations GetFunc(string quantityFunc)
        {
            if (!IsFunc(quantityFunc)) return Operations.Set;
            if (quantityFunc.StartsWith("+"))
            {
                return Operations.Add;
            }
            if (quantityFunc.StartsWith("-"))
            {
                return Operations.Subtract;
            }
            return Operations.Set;
        }

        public static int Parse(string quantityFunc, int currentQuantity)
        {
            if (string.IsNullOrEmpty(quantityFunc)) return 0;
            int value;
            var operation = GetFunc(quantityFunc);
            var trimmed = quantityFunc.Trim('-', '+', ' ');
            Int32.TryParse(trimmed, out value);
            if (operation == Operations.Add) return currentQuantity + value;
            if (operation == Operations.Subtract) return currentQuantity - value;
            return value;
        }
    }
}