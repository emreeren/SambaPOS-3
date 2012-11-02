using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComLib.Lang.Helpers;

namespace ComLib.Lang
{
    /// <summary>
    /// Array type.
    /// </summary>
    public class LDate : LBaseType
    {
        private static Dictionary<string, Func<DateTime, object>> _getMethods;
        private static Dictionary<string, Action<LDate, ArgsFetcher>> _setMethods;
        private static Dictionary<string, Func<LDate, string>> _toMethods;

        /// <summary>
        /// Initialize
        /// </summary>
        static LDate()
        {
            _getMethods = new Dictionary<string, Func<DateTime, object>>();
            _setMethods = new Dictionary<string, Action<LDate, ArgsFetcher>>();
            _toMethods = new Dictionary<string, Func<LDate, string>>();
            //_methods["getTime"                 ] = (date) => date.ToUniversalTime().
            //_methods["getTimezoneOffset"       ] = (date) => date.
            _getMethods["getDate"] = (date) => date.Day;
            _getMethods["getDay"] = (date) => (int)date.DayOfWeek;
            _getMethods["getFullYear"] = (date) => date.Year;
            _getMethods["getHours"] = (date) => date.Hour;
            _getMethods["getMilliseconds"] = (date) => date.Millisecond;
            _getMethods["getMinutes"] = (date) => date.Minute;
            _getMethods["getMonth"] = (date) => date.Month;
            _getMethods["getSeconds"] = (date) => date.Second;
            _getMethods["getUTCDate"] = (date) => date.ToUniversalTime().Day;
            _getMethods["getUTCDay"] = (date) => (int)date.ToUniversalTime().DayOfWeek;
            _getMethods["getUTCFullYear"] = (date) => date.ToUniversalTime().Year;
            _getMethods["getUTCHours"] = (date) => date.ToUniversalTime().Hour;
            _getMethods["getUTCMilliseconds"] = (date) => date.ToUniversalTime().Millisecond;
            _getMethods["getUTCMinutes"] = (date) => date.ToUniversalTime().Minute;
            _getMethods["getUTCMonth"] = (date) => date.ToUniversalTime().Month;
            _getMethods["getUTCSeconds"] = (date) => date.ToUniversalTime().Second;

            _setMethods["setDate"] = (date, fetcher) => date.SetDate(DateTimeKind.Local, fetcher);
            _setMethods["setFullYear"] = (date, fetcher) => date.SetYear(DateTimeKind.Local, fetcher);
            _setMethods["setHours"] = (date, fetcher) => date.SetHours(DateTimeKind.Local, fetcher);
            _setMethods["setMilliseconds"] = (date, fetcher) => date.SetMilliseconds(DateTimeKind.Local, fetcher);
            _setMethods["setMinutes"] = (date, fetcher) => date.SetMinutes(DateTimeKind.Local, fetcher);
            _setMethods["setMonth"] = (date, fetcher) => date.SetMonth(DateTimeKind.Local, fetcher);
            _setMethods["setSeconds"] = (date, fetcher) => date.SetSeconds(DateTimeKind.Local, fetcher);
            //_setMethods["setTime"       ]     = (date, fetcher) =>  date.SetYear(DateTimeKind.Local);
            _setMethods["setUTCDate"] = (date, fetcher) => date.SetDate(DateTimeKind.Utc, fetcher);
            _setMethods["setUTCFullYear"] = (date, fetcher) => date.SetYear(DateTimeKind.Utc, fetcher);
            _setMethods["setUTCHours"] = (date, fetcher) => date.SetHours(DateTimeKind.Utc, fetcher);
            _setMethods["setUTCMilliseconds"] = (date, fetcher) => date.SetMilliseconds(DateTimeKind.Utc, fetcher);
            _setMethods["setUTCMinutes"] = (date, fetcher) => date.SetMinutes(DateTimeKind.Utc, fetcher);
            _setMethods["setUTCMonth"] = (date, fetcher) => date.SetMonth(DateTimeKind.Utc, fetcher);
            _setMethods["setUTCSeconds"] = (date, fetcher) => date.SetSeconds(DateTimeKind.Utc, fetcher);

            _toMethods["toDateString"] = (date) => date.Raw.ToString("ddd MMM dd yyyy");
            _toMethods["toLocaleDateString"] = (date) => date.Raw.ToLocalTime().ToString("ddd MMM dd yyyy");
            _toMethods["toLocaleTimeString"] = (date) => date.Raw.ToLocalTime().ToString("hh mm ss");
            _toMethods["toLocaleString"] = (date) => date.Raw.ToLocalTime().ToString("ddd MMM dd yyyy hh mm ss");
            _toMethods["toString"] = (date) => date.Raw.ToString("ddd MMM dd yyyy hh mm ss");
            _toMethods["toTimeString"] = (date) => date.Raw.ToString("hh mm ss");
            _toMethods["toUTCString"] = (date) => date.Raw.ToUniversalTime().ToString("ddd MMM dd yyyy hh mm ss");
        }


        /// <summary>
        /// Can create from the paramelist expressions supplied.
        /// </summary>
        /// <returns></returns>
        public static bool CanCreateFrom(int paramCount)
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
        public static DateTime CreateFrom(object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return DateTime.Now;
            }

            DateTime result = DateTime.MinValue;
            
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
            int[] args = new int[parameters.Length];            
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
        
        
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="context"></param>
        /// <param name="varName"></param>
        public LDate(Context context, string varName)
        {
            _context = context;
            _varName = varName;
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="context">Context of the script</param>
        /// <param name="varName">Name of the variable.</param>
        /// <param name="date">Date value</param>
        public LDate(Context context, string varName, DateTime date)
        {
            _context = context;
            _varName = varName;
            Raw = date;
        }


        /// <summary>
        /// The raw datetime.
        /// </summary>
        public DateTime Raw;


        /// <summary>
        /// Whether or not this type supports the supplied property
        /// </summary>
        /// <param name="methodname"></param>
        /// <returns></returns>
        public override bool HasProperty(string methodname)
        {
            return false;
        }


        /// <summary>
        /// Whether or not this type supports the supplied method
        /// </summary>
        /// <param name="methodname"></param>
        /// <returns></returns>
        public override bool HasMethod(string methodname)
        {
            return _getMethods.ContainsKey(methodname) || _setMethods.ContainsKey(methodname);
        }


        /// <summary>
        /// Calls the method
        /// </summary>
        /// <param name="methodname"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override object ExecuteMethod(string methodname, object[] args)
        {
            object result = null;
            var fetcher = new ArgsFetcher(args);
            if (_getMethods.ContainsKey(methodname))
            {
                result = _getMethods[methodname](Raw);
                return result;
            }
            else if (_setMethods.ContainsKey(methodname))
            {
                _setMethods[methodname](this, fetcher);
                result = _context.Memory.Get<object>(_varName);
            }
            else if (_toMethods.ContainsKey(methodname))
            {
                result = _toMethods[methodname](this);
            }
            else
                throw new LangException("Method not supported", "Method name : " + methodname + "is not supported for datetime", string.Empty, 0);
            return result;
        }


        #region API methods
        internal int      GetDate              (DateTime date) { return Raw.Day;                                                      }      	
        internal int      GetDay               (DateTime date) { return (int)Raw.DayOfWeek;                                           }
        internal int      GetFullYear          (DateTime date) { return Raw.Year;                                                     }
        internal int      GetHours             (DateTime date) { return Raw.Hour;                                                     }
        internal int      GetMilliseconds      (DateTime date) { return Raw.Millisecond;                                              }
        internal int      GetMinutes           (DateTime date) { return Raw.Minute;		                                            }
        internal int      GetMonth             (DateTime date) { return Raw.Month;                                                    }
        internal int      GetSeconds           (DateTime date) { return Raw.Second;                                                   }
        internal int      GetUTCDate           (DateTime date) { return Raw.ToUniversalTime().Day;                                    }
        internal int      GetUTCDay            (DateTime date) { return (int)Raw.ToUniversalTime().DayOfWeek;                         }
        internal int      GetUTCFullYear       (DateTime date) { return Raw.ToUniversalTime().Year;                                   }
        internal int      GetUTCHours          (DateTime date) { return Raw.ToUniversalTime().Hour;                                   }
        internal int      GetUTCMilliseconds   (DateTime date) { return Raw.ToUniversalTime().Millisecond;                            }  
        internal int      GetUTCMinutes        (DateTime date) { return Raw.ToUniversalTime().Minute;                                 }
        internal int      GetUTCMonth          (DateTime date) { return Raw.ToUniversalTime().Month;                                  }
        internal int      GetUTCSeconds        (DateTime date) { return Raw.ToUniversalTime().Second;                                 }
        internal string   ToDateString         (DateTime date) { return Raw.ToString("ddd MMM dd yyyy");                              }
        internal string   ToLocaleDateString   (DateTime date) { return Raw.ToLocalTime().ToString("ddd MMM dd yyyy");                }
        internal string   ToLocaleTimeString   (DateTime date) { return Raw.ToLocalTime().ToString("hh mm ss");                       }
        internal string   ToLocaleString       (DateTime date) { return Raw.ToLocalTime().ToString("ddd MMM dd yyyy hh mm ss");       }
        internal string   ToString             (DateTime date) { return Raw.ToString("ddd MMM dd yyyy hh mm ss");                     }
        internal string   ToTimeString         (DateTime date) { return Raw.ToString("hh mm ss");                                     }
        internal string   ToUTCString          (DateTime date) { return Raw.ToUniversalTime().ToString("ddd MMM dd yyyy hh mm ss");   }


        private void SetYear(DateTimeKind kind, ArgsFetcher fetcher)
        {
            SetDateTime(kind, 
                        year:  fetcher.Get<int>(0),
                        month: fetcher.Get<int>(1, Raw.Month),
                        day:   fetcher.Get<int>(2, Raw.Day));
        }


        private void SetMonth(DateTimeKind kind, ArgsFetcher fetcher)
        {
            SetDateTime(kind, 
                        month: fetcher.Get<int>(0), 
                        day: fetcher.Get<int>(1, Raw.Day));
        }


        private void SetDate(DateTimeKind kind, ArgsFetcher fetcher)
        {
            SetDateTime(kind, day: fetcher.Get<int>(0));
        }


        private void SetHours(DateTimeKind kind, ArgsFetcher fetcher)
        {
            SetDateTime(kind,
                        hour: fetcher.Get<int>(0),
                        minute: fetcher.Get<int>(1, Raw.Minute),
                        second: fetcher.Get<int>(2, Raw.Second),
                        millisecond: fetcher.Get<int>(3, Raw.Millisecond));
        }


        private void SetMinutes(DateTimeKind kind, ArgsFetcher fetcher)
        {
            SetDateTime(kind,
                        minute: fetcher.Get<int>(0),
                        second: fetcher.Get<int>(1, Raw.Second),
                        millisecond: fetcher.Get<int>(2, Raw.Millisecond));
        }


        private void SetSeconds(DateTimeKind kind, ArgsFetcher fetcher)
        {
            SetDateTime(kind,
                        second: fetcher.Get<int>(0),
                        millisecond: fetcher.Get<int>(1, Raw.Millisecond));
        }


        private void SetMilliseconds(DateTimeKind kind, ArgsFetcher fetcher)
        {
            SetDateTime(kind, millisecond: fetcher.Get<int>(0));
        }


        private void SetDateTime(DateTimeKind kind, int year = -1, int month = -1, int day = -1,
            int hour = -1, int minute = -1, int second = -1, int millisecond = -1)
        {
            DateTime dt = kind == DateTimeKind.Utc ? Raw.ToUniversalTime() : Raw;
            year = year == -1 ? dt.Year : year;
            month = month == -1 ? dt.Month : month;
            day = day == -1 ? dt.Day : day;
            hour = hour == -1 ? dt.Hour : hour;
            minute = minute == -1 ? dt.Minute : minute;
            second = second == -1 ? dt.Second : second;
            millisecond = millisecond == -1 ? dt.Millisecond : millisecond;

            var finalDateTime = new DateTime(year, month, day, hour, minute, second, millisecond, kind);
            _context.Memory.SetValue(_varName, finalDateTime);
        }
        #endregion
    }
}
