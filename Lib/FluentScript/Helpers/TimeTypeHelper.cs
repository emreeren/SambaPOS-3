using System;
using System.Text.RegularExpressions;

namespace Fluentscript.Lib.Helpers
{
    /// <summary>
    /// Helper class for time types.
    /// </summary>
    public class DateTimeTypeHelper
    {
        /// <summary>
        /// Whether or not a timespan can be created from the number of arguments supplied.
        /// </summary>
        /// <param name="paramCount">The number of parameters</param>
        /// <returns></returns>
        public static bool CanCreateTimeFrom(int paramCount)
        {            
            // 1. 0 args = new TimeSpan()
            // 2. 3 args = new TimeSpan(hours, mins, secs)
            // 3. 4 args = new TimeSpan(days, hours, mins, secs)
            if (paramCount == 0 || paramCount == 3 || paramCount == 4)
                return true;

            return false;
        }


        /// <summary>
        /// Whether or not a timespan can be created from the number of arguments supplied.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TimeSpan CreateTimeFrom(object[] args)
        {
            // Validate
            if (!CanCreateTimeFrom(args.Length)) 
                throw new ArgumentException("Incorrect number of inputs for creating time");

            // Convert object into ints
            int[] timeArgs = LangTypeHelper.ConvertToInts(args);
            int len = args.Length;

            // 1. 0 args = new TimeSpan()
            if (len == 0) return new TimeSpan();

            // 2. 3 args = new TimeSpan(hours, mins, secs)
            if (len == 3) return new TimeSpan(timeArgs[0], timeArgs[1], timeArgs[2]);

            // 3. 4 args = new TimeSpan(days, hours, mins, secs)
            return new TimeSpan(timeArgs[0], timeArgs[1], timeArgs[2], timeArgs[3]);
        }


        /// <summary>
        /// Parse the time using Regular expression.
        /// </summary>
        /// <param name="strTime">Text with time.</param>
        /// <returns>Time parse result.</returns>
        public static Tuple<TimeSpan, bool, string> ParseTime(string strTime)
        {
            strTime = strTime.Trim().ToLower();
            var pattern = @"(?<hours>[0-9]+)((\:)(?<minutes>[0-9]{2}))?((\:)(?<seconds>[0-9]{2}))?\s*(?<ampm>(am|a\.m\.|a\.m|pm|p\.m\.|p\.m))?\s*";
            var match = Regex.Match(strTime, pattern);
            if (!match.Success)
                return new Tuple<TimeSpan, bool, string>(TimeSpan.MinValue, false, "Time : " + strTime + " is not a valid time.");

            var strhours = match.Groups["hours"] != null ? match.Groups["hours"].Value : string.Empty;
            var strminutes = match.Groups["minutes"] != null ? match.Groups["minutes"].Value : string.Empty;
            var strseconds = match.Groups["seconds"] != null ? match.Groups["seconds"].Value : string.Empty;
            var ampm = match.Groups["ampm"] != null ? match.Groups["ampm"].Value : string.Empty;
            var hours = 0;
            var minutes = 0;
            var seconds = 0;
            if (!string.IsNullOrEmpty(strhours) && !Int32.TryParse(strhours, out hours))
            {
                return new Tuple<TimeSpan, bool, string>(TimeSpan.MinValue, false, "Hours are invalid.");
            }
            if (!string.IsNullOrEmpty(strminutes) && !Int32.TryParse(strminutes, out minutes))
            {
                return new Tuple<TimeSpan, bool, string>(TimeSpan.MinValue, false, "Minutes are invalid.");
            }
            if (!string.IsNullOrEmpty(strseconds) && !Int32.TryParse(strseconds, out seconds))
            {
                return new Tuple<TimeSpan, bool, string>(TimeSpan.MinValue, false, "Seconds are invalid.");
            }

            var isAm = false;
            if (string.IsNullOrEmpty(ampm) || ampm == "am" || ampm == "a.m" || ampm == "a.m.")
                isAm = true;
            else if (ampm == "pm" || ampm == "p.m" || ampm == "p.m.")
                isAm = false;
            else
            {
                return new Tuple<TimeSpan, bool, string>(TimeSpan.MinValue, false, "unknown am/pm statement");
            }

            // Add 12 hours for pm specification.
            if (hours != 12 && !isAm)
                hours += 12;

            // Handles 12 12am.
            if (hours == 12 && isAm)
                return new Tuple<TimeSpan, bool, string>(new TimeSpan(0, minutes, seconds), true, string.Empty);

            return new Tuple<TimeSpan, bool, string>(new TimeSpan(hours, minutes, seconds), true, string.Empty);
        }


        public static TimeSpan ParseTimeWithoutColons(string numericPart, bool isAm)
        {
            var time = Convert.ToInt32(numericPart);
            var hours = 0;
            var minutes = 0;
            
            // 1pm || 12am
            if(time <= 12)
            {
                hours = time;
            }
            // 130 - 930    am|pm
            else if (time < 1000)
            {
                hours = Convert.ToInt32(numericPart[0].ToString());
                minutes = Convert.ToInt32(numericPart.Substring(1));
            }
            // 1030 - 1230  am|pm                
            else
            {
                hours = Convert.ToInt32(numericPart.Substring(0, 2));
                minutes = Convert.ToInt32(numericPart.Substring(2));
            }
            if (!isAm && hours < 12)
                hours += 12;
            return new TimeSpan(0, hours, minutes, 0);
        }


        /// <summary>
        /// Can create from the paramelist expressions supplied.
        /// </summary>
        /// <returns></returns>
        public static bool CanCreateDateFrom(int paramCount)
        {
            if (paramCount != 0 && paramCount != 1 && paramCount != 3 && paramCount != 6)
                return false;
            return true;
        }


        /// <summary>
        /// Creates a datetime from the parameters supplied.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DateTime CreateDateFrom(object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return DateTime.Now;
            }

            var result = DateTime.MinValue;

            // Case 1: From string
            if (parameters.Length == 1 && parameters[0] is string)
            {
                result = DateTime.Parse((string)parameters[0]);
                return result;
            }

            // Case 2: From Date
            if (parameters.Length == 1 && parameters[0] is DateTime)
            {
                var d = (DateTime)parameters[0];
                result = new DateTime(d.Ticks);
                return result;
            }

            // Convert all parameters to int            
            var args = new int[parameters.Length];
            for (int ndx = 0; ndx < parameters.Length; ndx++)
            {
                args[ndx] = Convert.ToInt32(parameters[ndx]);
            }

            // Case 3: 3 parameters month, day, year
            if (parameters.Length == 3)
                return new DateTime(args[0], args[1], args[2]);

            // Case 4: 6 parameters
            if (parameters.Length == 6)
                return new DateTime(args[0], args[1], args[2], args[3], args[4], args[5]);

            // TODO: Need to handle this better.
            return DateTime.MinValue;
        }
    }
}
