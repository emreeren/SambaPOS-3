using System;
using Samba.Localization.Properties;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public static class HumanFriendlyInteger
    {
        static readonly string[] Ones = new[] { "", Resources.One, Resources.Two, Resources.Three, Resources.Four, Resources.Five, Resources.Six, Resources.Seven, Resources.Eight, Resources.Nine };
        static readonly string[] Teens = new[] { Resources.Ten, Resources.Eleven, Resources.Twelve, Resources.Thirteen, Resources.Fourteen, Resources.Fifteen, Resources.Sixteen, Resources.Seventeen, Resources.Eighteen, Resources.Nineteen };
        static readonly string[] Tens = new[] { Resources.Twenty, Resources.Thirty, Resources.Forty, Resources.Fifty, Resources.Sixty, Resources.Seventy, Resources.Eighty, Resources.Ninety };
        static readonly string[] ThousandsGroups = { "", " " + Resources.Thousand, " " + Resources.Million, " " + Resources.Billion };

        private static string FriendlyInteger(int n, string leftDigits, int thousands)
        {
            if (n == 0)
            {
                return leftDigits;
            }
            string friendlyInt = leftDigits;
            if (friendlyInt.Length > 0)
            {
                friendlyInt += " ";
            }
            if (n < 10)
            {
                friendlyInt += Ones[n];
            }
            else if (n < 20)
            {
                friendlyInt += Teens[n - 10];
            }
            else if (n < 100)
            {
                friendlyInt += FriendlyInteger(n % 10, Tens[n / 10 - 2], 0);
            }
            else if (n < 1000)
            {
                var t = Ones[n / 100] + " " + Resources.Hundred;
                if (n / 100 == 1) t = Resources.OneHundred;
                friendlyInt += FriendlyInteger(n % 100, t, 0);
            }
            else if (n < 10000 && thousands == 0)
            {
                var t = Ones[n / 1000] + " " + Resources.Thousand;
                if (n / 1000 == 1) t = Resources.OneThousand;
                friendlyInt += FriendlyInteger(n % 1000, t, 0);
            }
            else
            {
                friendlyInt += FriendlyInteger(n % 1000, FriendlyInteger(n / 1000, "", thousands + 1), 0);
            }

            return friendlyInt + ThousandsGroups[thousands];
        }

        public static string CurrencyToWritten(decimal d, bool upper = false)
        {
            var result = "";
            var fraction = d - Math.Floor(d);
            var value = d - fraction;
            if (value > 0)
            {
                var start = IntegerToWritten(Convert.ToInt32(value));
                if (upper) start = start.Replace(" ", "").ToUpper();
                result += string.Format("{0} {1} ", start, Resources.Dollar + GetPlural(value));
            }

            if (fraction > 0)
            {
                var end = IntegerToWritten(Convert.ToInt32(fraction * 100));
                if (upper) end = end.Replace(" ", "").ToUpper();
                result += string.Format("{0} {1} ", end, Resources.Cent + GetPlural(fraction));
            }
            return result.Replace("  ", " ").Trim();
        }

        private static string GetPlural(decimal number)
        {
            var suffix = Resources.PluralCurrencySuffix ?? ".";
            return number == 1 ? "" : suffix.Replace(".", "");
        }

        public static string IntegerToWritten(int n)
        {
            if (n == 0)
            {
                return Resources.Zero;
            }
            if (n < 0)
            {
                return Resources.Negative + " " + IntegerToWritten(-n);
            }
            return FriendlyInteger(n, "", 0);
        }
    }
}