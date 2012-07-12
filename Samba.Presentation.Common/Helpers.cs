using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;

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

        private static readonly IList<string> Unpluralizables = new List<string> { "equipment", "information", "rice", "money", "species", "series", "fish", "sheep", "deer" };

        private static readonly IDictionary<string, string> Pluralizations = new Dictionary<string, string>
                                                                                 {
                                                                                     // Start with the rarest cases, and move to the most common
                                                                                     { "person$", "people" },
                                                                                     { "ox$", "oxen" },
                                                                                     { "child$", "children" },
                                                                                     { "foot$", "feet" },
                                                                                     { "tooth$", "teeth" },
                                                                                     { "goose$", "geese" },
                                                                                     // And now the more standard rules.
                                                                                     { "(.*)fe?", "$1ves" },         // ie, wolf, wife
                                                                                     { "(.*)man$", "$1men" },
                                                                                     { "(.+[aeiou]y)$", "$1s" },
                                                                                     { "(.+[^aeiou])y$", "$1ies" },
                                                                                     { "(.+z)$", "$1zes" },
                                                                                     { "([m|l])ouse$", "$1ice" },
                                                                                     { "(.+)(e|i)x$", @"$1ices"},    // ie, Matrix, Index
                                                                                     { "(octop|vir)us$", "$1i"},
                                                                                     { "(.+(s|x|sh|ch))$", @"$1es"},
                                                                                     { "(.+)", @"$1s" }
                                                                                 };

        public static string ToPlural(this string singular)
        {
            if (LocalSettings.CurrentLanguage != "en")
                return String.Format(Resources.List_f, singular);

            if (Unpluralizables.Contains(singular))
                return singular;

            var plural = "";

            foreach (var pluralization in Pluralizations)
            {
                if (Regex.IsMatch(singular, pluralization.Key))
                {
                    plural = Regex.Replace(singular, pluralization.Key, pluralization.Value);
                    break;
                }
            }

            return plural;
        }
    }
}
