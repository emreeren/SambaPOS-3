using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComLib.Lang.Parsing.MetaPlugins
{
    public class MetaCompilerData
    {
        private Dictionary<string, bool> _types;
        private Dictionary<int, DayOfWeek> _daysByNum;

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

            _daysByNum[Convert.ToInt32(DayOfWeek.Monday)] = DayOfWeek.Monday;
            _daysByNum[Convert.ToInt32(DayOfWeek.Tuesday)] = DayOfWeek.Tuesday;
            _daysByNum[Convert.ToInt32(DayOfWeek.Wednesday)] = DayOfWeek.Wednesday;
            _daysByNum[Convert.ToInt32(DayOfWeek.Thursday)] = DayOfWeek.Thursday;
            _daysByNum[Convert.ToInt32(DayOfWeek.Friday)] = DayOfWeek.Friday;
            _daysByNum[Convert.ToInt32(DayOfWeek.Saturday)] = DayOfWeek.Saturday;
            _daysByNum[Convert.ToInt32(DayOfWeek.Sunday)] = DayOfWeek.Sunday;
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
    }
}
