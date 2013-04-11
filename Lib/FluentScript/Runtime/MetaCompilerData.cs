using System;
using System.Collections.Generic;

namespace Fluentscript.Lib.Runtime
{
    public class MetaCompilerData
    {
        private Dictionary<string, bool> _types;
        private Dictionary<int, DayOfWeek> _daysByNum;
        private Dictionary<int, string> _daysToName;

        public void Init()
        {
            _types = new Dictionary<string, bool>();
            _types["number"] = true;
            _types["date"] = true;
            _types["time"] = true;
            _types["day"] = true;
            _types["bool"] = true;
            _types["uri"] = true;
            _daysByNum = new Dictionary<int, DayOfWeek>();
            _daysToName = new Dictionary<int, string>();

            _daysByNum[Convert.ToInt32(DayOfWeek.Monday)] = DayOfWeek.Monday;
            _daysByNum[Convert.ToInt32(DayOfWeek.Tuesday)] = DayOfWeek.Tuesday;
            _daysByNum[Convert.ToInt32(DayOfWeek.Wednesday)] = DayOfWeek.Wednesday;
            _daysByNum[Convert.ToInt32(DayOfWeek.Thursday)] = DayOfWeek.Thursday;
            _daysByNum[Convert.ToInt32(DayOfWeek.Friday)] = DayOfWeek.Friday;
            _daysByNum[Convert.ToInt32(DayOfWeek.Saturday)] = DayOfWeek.Saturday;
            _daysByNum[Convert.ToInt32(DayOfWeek.Sunday)] = DayOfWeek.Sunday;

            _daysToName[Convert.ToInt32(DayOfWeek.Monday)]    = "Monday";
            _daysToName[Convert.ToInt32(DayOfWeek.Tuesday)]   = "Tuesday";
            _daysToName[Convert.ToInt32(DayOfWeek.Wednesday)] = "Wednesday";
            _daysToName[Convert.ToInt32(DayOfWeek.Thursday)]  = "Thursday";
            _daysToName[Convert.ToInt32(DayOfWeek.Friday)]    = "Friday";
            _daysToName[Convert.ToInt32(DayOfWeek.Saturday)]  = "Saturday";
            _daysToName[Convert.ToInt32(DayOfWeek.Sunday)]    = "Sunday";
            _daysToName[7] = "today";
            _daysToName[8] = "yesterday";
            _daysToName[9] = "tomorrow";
        }


        /// <summary>
        /// Lookup the day of the week.
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public DayOfWeek LookupDay(int day)
        {
            return _daysByNum[day];
        }


        /// <summary>
        /// Lookup the day of the week.
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public string LookupDayName(int day)
        {
            if(_daysToName.ContainsKey(day))
                return _daysToName[day];
            return string.Empty;
        }
    }
}
