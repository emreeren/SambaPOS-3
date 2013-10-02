using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Samba.Infrastructure.Helpers
{
    public static class Utility
    {
        public static string AddTypedValue(string actualValue, string typedValue, string format)
        {
            decimal amnt;
            bool stringMode = false;

            Decimal.TryParse(actualValue, out amnt);
            if (actualValue.EndsWith("-") || amnt == 0) stringMode = true;
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
                Convert.ToDouble(actualValue).ToString(fmt);
            if (amount.Contains(dc))
                amount = amount.Substring(0, amount.Length - 1);

            var dbl = Convert.ToDouble(amount + typedValue);
            return (dbl).ToString(rfmt);
        }

        public static bool IsValidFile(string fileName)
        {
            fileName = fileName.Trim();
            if (fileName == "." || !fileName.Contains(".")) return false;
            var result = false;
            try
            {
                new FileInfo(fileName);
                result = true;
            }
            catch (ArgumentException)
            {
            }
            catch (PathTooLongException)
            {
            }
            catch (NotSupportedException)
            {
            }
            return result;
        }

        public static bool IsNumericType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    }
                    return false;
            }
            return false;
        }

        public static int GenerateCheckDigit(string idWithoutCheckdigit)
        {
            const string validChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVYWXZ_";
            idWithoutCheckdigit = idWithoutCheckdigit.Trim().ToUpper();

            var sum = 0;

            for (var i = 0; i < idWithoutCheckdigit.Length; i++)
            {
                var ch = idWithoutCheckdigit[idWithoutCheckdigit.Length - i - 1];
                if (validChars.IndexOf(ch) == -1)
                    throw new Exception(ch + " is an invalid character");
                var digit = ch - 48;
                int weight;
                if (i % 2 == 0)
                {
                    weight = (2 * digit) - digit / 5 * 9;
                }
                else
                {
                    weight = digit;
                }
                sum += weight;
            }
            sum = Math.Abs(sum) + 10;
            return (10 - (sum % 10)) % 10;
        }

        public static bool ValidateCheckDigit(string id)
        {
            if (id.Length < 2) return false;
            var cd = Convert.ToInt32(id.Last().ToString());
            return cd == GenerateCheckDigit(id.Remove(id.Length - 1));
        }

        public static string RandomString(int length, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "length cannot be less than zero.");
            if (String.IsNullOrEmpty(allowedChars)) throw new ArgumentException("allowedChars may not be empty.");

            const int byteSize = 0x100;
            var allowedCharSet = new HashSet<char>(allowedChars).ToArray();
            if (byteSize < allowedCharSet.Length) throw new ArgumentException(String.Format("allowedChars may contain no more than {0} characters.", byteSize));

            using (var rng = new RNGCryptoServiceProvider())
            {
                var result = new StringBuilder();
                var buf = new byte[128];
                while (result.Length < length)
                {
                    rng.GetBytes(buf);
                    for (var i = 0; i < buf.Length && result.Length < length; ++i)
                    {
                        var outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
                        if (outOfRangeStart <= buf[i]) continue;
                        result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
                    }
                }
                return result.ToString();
            }
        }

        public static string GetDateBasedUniqueString()
        {
            var date = DateTime.Now;
            return string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:000}", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Millisecond);

        }
    }
}
