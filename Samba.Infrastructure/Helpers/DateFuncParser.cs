using System;
using System.Linq;

namespace Samba.Infrastructure.Helpers
{
    public static class DateFuncParser
    {
        private static readonly char[] Operators = new[] { '+', '-' };

        public static string Parse(string expression, string currentValue)
        {
            var result = expression??"";
            var correctedExpression = result.ToLower().Trim();
            if (correctedExpression.ToLower() == "today") result = DateTime.Today.ToShortDateString();
            else if (correctedExpression.ToLower().StartsWith("today")) result = ParseDateExpression(correctedExpression);
            else if (correctedExpression.IndexOfAny(Operators) > -1) result = ExecuteDateExpression(correctedExpression, currentValue);
            return !string.IsNullOrEmpty(result) ? result : expression;
        }

        private static string ExecuteDateExpression(string expression, string currentValue)
        {
            DateTime currentDate;
            if (!DateTime.TryParse(currentValue, out currentDate) && !string.IsNullOrEmpty(currentValue)) return "";
            if (string.IsNullOrEmpty(currentValue)) currentDate = DateTime.Today;
            var parts = expression.Split(Operators, StringSplitOptions.RemoveEmptyEntries);

            var val = parts[0].Trim();
            if (!IsNumber(val)) return "";
            var quantity = Convert.ToInt32(val);

            return UpdateDate(currentDate, expression, quantity);
        }

        private static string ParseDateExpression(string expression)
        {
            var parts = expression.Split(Operators, StringSplitOptions.RemoveEmptyEntries);
            var quantity = 1;
            if (parts.Length > 1)
            {
                var val = parts[1].Trim();
                if (!IsNumber(val)) return "";
                quantity = Convert.ToInt32(val);
            }

            return UpdateDate(DateTime.Today, expression, quantity);
        }

        private static string UpdateDate(DateTime date, string expression, int quantity)
        {
            return expression.Contains("-")
                       ? date.AddDays(0 - quantity).ToShortDateString()
                       : date.AddDays(quantity).ToShortDateString();
        }

        private static bool IsNumber(string value)
        {
            return value.All(x => "1234567890".Contains(x));
        }
    }
}