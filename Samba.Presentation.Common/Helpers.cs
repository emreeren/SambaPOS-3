using System;
using System.Globalization;

namespace Samba.Presentation.Common
{
    public static class Helpers
    {
        public static string AddTypedValue(string actualValue, string typedValue, string format)
        {
            decimal amnt;
            bool stringMode = false;

            Decimal.TryParse(actualValue, out amnt);
            if (amnt == 0) stringMode = true;
            else
            {
                Decimal.TryParse(typedValue, out amnt);
                if (amnt == 0) stringMode = true;
            }

            string dc = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            if (typedValue == "." || typedValue == ",")
            {
                actualValue += dc;
                return actualValue;
            }

            if (stringMode)
                return actualValue + typedValue;

            string fmt = "0";
            string rfmt = format;

            if (actualValue.Contains(dc))
            {
                int dCount = (actualValue.Length - actualValue.IndexOf(dc));

                fmt = "0.".PadRight(dCount + 2, '0');
                rfmt = format.PadRight(dCount + rfmt.Length, '0');
            }

            string amount = String.IsNullOrEmpty(actualValue) ? "0" :
                Convert.ToDecimal(actualValue).ToString(fmt);
            if (amount.Contains(dc))
                amount = amount.Substring(0, amount.Length - 1);

            amnt = Convert.ToDecimal(amount + typedValue);
            return (amnt).ToString(rfmt);
        }
    }
}
