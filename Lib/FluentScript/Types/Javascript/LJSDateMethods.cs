#pragma warning disable 1591
using System;

namespace Fluentscript.Lib.Types.Javascript
{
    /// <summary>
    /// Array type.
    /// </summary>
    public class LJSDateMethods : LTypeMethods
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public LJSDateMethods()
        {
            DataType = LTypes.Date;

            AddMethod("getDate", 			"GetDate", 				typeof(double), "Returns the day of the month (from 1-31)");
            AddMethod("getDay", 	 		"GetDay", 				typeof(double), "Returns the day of the week (from 0-6)");
            AddMethod("getFullYear", 		"GetFullYear", 			typeof(double), "Returns the year (four digits)");
            AddMethod("getHours", 			"GetHours", 			typeof(double), "Returns the hour (from 0-23)");
            AddMethod("getMilliseconds", 	"GetMilliseconds", 		typeof(double), "Returns the milliseconds (from 0-999)");
            AddMethod("getMinutes", 		"GetMinutes", 			typeof(double), "Returns the minutes (from 0-59)");
            AddMethod("getMonth", 			"GetMonth", 			typeof(double), "Returns the month (from 0-11)");
            AddMethod("getSeconds", 		"GetSeconds", 		 	typeof(double), "Returns the seconds (from 0-59)");
            AddMethod("getTime", 			"GetTime", 			 	typeof(double), "Returns the number of milliseconds since midnight Jan 1 1970");
            AddMethod("getTimezoneOffset",  "GetTimezoneOffset", 	typeof(double), "Returns the time difference between UTC time and local time in minutes");
            AddMethod("getUTCDate", 		"GetUtcDate", 			typeof(double), "Returns the day of the month according to universal time (from 1-31)");
            AddMethod("getUTCDay", 		    "GetUtcDay",			typeof(double), "Returns the day of the week according to universal time (from 0-6)");
            AddMethod("getUTCFullYear", 	"GetUtcFullYear", 		typeof(double), "Returns the year according to universal time (four digits)");
            AddMethod("getUTCHours", 		"GetUtcHours", 			typeof(double), "Returns the hour according to universal time (from 0-23)");
            AddMethod("getUTCMilliseconds", "GetUtcMilliseconds", 	typeof(double), "Returns the milliseconds according to universal time (from 0-999)");
            AddMethod("getUTCMinutes", 	    "GetUtcMinutes", 		typeof(double), "Returns the minutes according to universal time (from 0-59)");
            AddMethod("getUTCMonth", 		"GetUtcMonth", 			typeof(double), "Returns the month according to universal time (from 0-11)");
            AddMethod("getUTCSeconds", 	    "GetUtcSeconds", 		typeof(double), "Returns the seconds according to universal time (from 0-59)");

            AddMethod("setDate", 			"SetDate", 				null,           "Sets the day of the month of a date object");
            AddMethod("setFullYear", 		"SetFullYear", 			null,           "Sets the year (four digits) of a date object");
            AddMethod("setHours", 			"SetHours", 			null,           "Sets the hour of a date object");
            AddMethod("setMilliseconds", 	"SetMilliseconds", 		null,           "Sets the milliseconds of a date object");
            AddMethod("setMinutes", 		"SetMinutes", 			null,           "Set the minutes of a date object");
            AddMethod("setMonth", 			"SetMonth", 			null,           "Sets the month of a date object");
            AddMethod("setSeconds", 		"SetSeconds", 			null,           "Sets the seconds of a date object");
            AddMethod("setTime", 			"SetTime", 				null,           "Sets a date and time by adding or subtracting a specified number of milliseconds to/from midnight January 1, 1970");
            AddMethod("setUTCDate", 		"SetUtcDate", 			null,           "Sets the day of the month of a date object, according to universal time");
            AddMethod("setUTCFullYear", 	"SetUtcFullYear", 		null,           "Sets the year of a date object, according to universal time (four digits)");
            AddMethod("setUTCHours", 		"SetUtcHours", 			null,           "Sets the hour of a date object, according to universal time");
            AddMethod("setUTCMilliseconds", "SetUtcMilliseconds", 	null,           "Sets the milliseconds of a date object, according to universal time");
            AddMethod("setUTCMinutes", 	    "SetUtcMinutes", 		null,           "Set the minutes of a date object, according to universal time");
            AddMethod("setUTCMonth", 		"SetUtcMonth", 			null,           "Sets the month of a date object, according to universal time");
            AddMethod("setUTCSeconds",  	"SetUtcSeconds", 		null,           "Set the seconds of a date object, according to universal time");

            // NOTE: I could use reflection add add attributes to the methods to expose, but the attributes are C# specific,
            // and I want to minimize the use of C# specific code.
            // Associate the arguments for each declared function.
            //     Method name,     Param name,     Type,       Required   Alias,   Default,    Example         Description
            AddArg("setFullYear",   "year",         "int",      true,       "",     null,       "2011, 2012",   "An integer representing the day of the month");
            AddArg("setFullYear",   "month",        "int",      false,      "",     -1,         "10, 12",       "An integer representing the day of the month");
            AddArg("setFullYear",   "day",          "int",      false,      "",     -1,         "15, 25",       "An integer representing the day of the month");
            AddArg("setMonth",      "month",        "int",      true,       "",     null,       "10, 12",       "An integer representing the day of the month");
            AddArg("setMonth",      "day",          "int",      false,      "",     -1,         "15, 25",       "An integer representing the day of the month");
            AddArg("setDate",       "day",          "int",      true,       "",     null,       "15, 25",       "An integer representing the day of the month");
            AddArg("setHours",      "hour",         "int",      true,       "",     null,       "10, 21",       "An integer representing the day of the month");
            AddArg("setHours",      "min",          "int",      false,      "",     -1,         "10, 21",       "An integer representing the day of the month");
            AddArg("setHours",      "sec",          "int",      false,      "",     -1,         "10, 21",       "An integer representing the day of the month");
            AddArg("setHours",      "millisec",     "int",      false,      "",     -1,         "10, 21",       "An integer representing the day of the month");
            AddArg("setMinutes",    "min",          "int",      true,       "",     null,       "10, 21",       "An integer representing the day of the month");
            AddArg("setMinutes",    "sec",          "int",      false,      "",     -1,         "10, 21",       "An integer representing the day of the month");
            AddArg("setMinutes",    "millisec",     "int",      false,      "",     -1,         "10, 21",       "An integer representing the day of the month");
            AddArg("setSeconds",    "sec",          "int",      true,       "",     null,       "10, 21",       "An integer representing the day of the month");
            AddArg("setSeconds",    "millisec",     "int",      false,      "",     -1,         "10, 21",       "An integer representing the day of the month");
            AddArg("setMilliseconds", "millisec",   "int",      true,       "",     null,       "10, 21",       "An integer representing the day of the month");
            var d = DateTime.Today;
            
            AddProperty(true, true,     "Year",        "GetFullYear",       typeof(double),     "Gets the full year");
            AddProperty(true, true,     "Month",       "GetMonth",          typeof(double),     "Returns the month");
            AddProperty(true, true,     "Day",         "GetDate",           typeof(double),     "Returns the day of the month");
            AddProperty(true, true,     "DayOfWeek",   "GetDay",            typeof(double),     "Returns the day of the week");
            AddProperty(true, true,     "Hours",       "GetHours",          typeof(double),     "Returns the hour (from 0-23)");
            AddProperty(true, true,     "Minutes",     "GetMinutes",        typeof(double),     "Returns the minutes (from 0-59)");
            AddProperty(true, true,     "Seconds",     "GetSeconds",        typeof(double),     "Returns the seconds (from 0-59)");
            AddProperty(true, true,     "Milliseconds","GetMilliseconds",   typeof(double),     "Returns the number of milliseconds since midnight Jan 1 1970");
        }


        #region Javascript API methods
        public int      GetDate              (LDate target) { var date = target.Value; return date.Day;                                                      }      	
        public int      GetDay               (LDate target) { var date = target.Value; return (int)date.DayOfWeek;                                           }
        public int      GetFullYear          (LDate target) { var date = target.Value; return date.Year;                                                     }
        public int      GetHours             (LDate target) { var date = target.Value; return date.Hour;                                                     }
        public int      GetMilliseconds      (LDate target) { var date = target.Value; return date.Millisecond;                                              }
        public int      GetMinutes           (LDate target) { var date = target.Value; return date.Minute;		                                           }
        public int      GetMonth             (LDate target) { var date = target.Value; return date.Month;                                                    }
        public int      GetSeconds           (LDate target) { var date = target.Value; return date.Second;                                                   }
        public int      GetUtcDate           (LDate target) { var date = target.Value; return date.ToUniversalTime().Day;                                    }
        public int      GetUtcDay            (LDate target) { var date = target.Value; return (int)date.ToUniversalTime().DayOfWeek;                         }
        public int      GetUtcFullYear       (LDate target) { var date = target.Value; return date.ToUniversalTime().Year;                                   }
        public int      GetUtcHours          (LDate target) { var date = target.Value; return date.ToUniversalTime().Hour;                                   }
        public int      GetUtcMilliseconds   (LDate target) { var date = target.Value; return date.ToUniversalTime().Millisecond;                            }  
        public int      GetUtcMinutes        (LDate target) { var date = target.Value; return date.ToUniversalTime().Minute;                                 }
        public int      GetUtcMonth          (LDate target) { var date = target.Value; return date.ToUniversalTime().Month;                                  }
        public int      GetUtcSeconds        (LDate target) { var date = target.Value; return date.ToUniversalTime().Second;                                 }
        public string   ToDateString         (LDate target) { var date = target.Value; return date.ToString("ddd MMM dd yyyy");                              }
        public string   ToLocaleDateString   (LDate target) { var date = target.Value; return date.ToLocalTime().ToString("ddd MMM dd yyyy");                }
        public string   ToLocaleTimeString   (LDate target) { var date = target.Value; return date.ToLocalTime().ToString("hh mm ss");                       }
        public string   ToLocaleString       (LDate target) { var date = target.Value; return date.ToLocalTime().ToString("ddd MMM dd yyyy hh mm ss");       }
        public string   ToString             (LDate target) { var date = target.Value; return date.ToString("ddd MMM dd yyyy hh mm ss");                     }
        public string   ToTimeString         (LDate target) { var date = target.Value; return date.ToString("hh mm ss");                                     }
        public string   ToUtcString          (LDate target) { var date = target.Value; return date.ToUniversalTime().ToString("ddd MMM dd yyyy hh mm ss");   }



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
        /// Callback for when these methods are registered with the system.
        /// </summary>
        public override void OnRegistered()
        {
            // Associated the javascript Date type name "Date" to LDate type name : "sys.datetime".
            LTypesLookup.RegisterAlias(this.DataType, "Date", "Date");
        }


        /// <summary>
        /// Can create from the paramelist expressions supplied.
        /// </summary>
        /// <param name="args">The arguments</param>
        /// <returns></returns>
        public override bool CanCreateFromArgs(object[] args)
        {
            var paramCount = args == null ? 0 : args.Length;
            if (paramCount == 0 || paramCount == 1 || paramCount == 3 || paramCount == 6)
                return true;
            return false;
        }


        /// <summary>
        /// Creates an instance of the type associated with theses methods from the arguments supplied. Repesents a constructor call
        /// </summary>
        /// <param name="parameters">The arguments used to construct the instance of this type</param>
        /// <returns></returns>
        public override LObject CreateFromArgs(object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return new LDate(DateTime.Now);
            }

            // Case 1: From string
            if (parameters.Length == 1 && parameters[0] is LString)
            {
                var result = DateTime.Parse(((LString)parameters[0]).Value);
                return new LDate(result);
            }

            // Case 2: From Date
            if (parameters.Length == 1 && parameters[0] is LDate)
            {
                var d = (LDate)parameters[0];
               var  result = new DateTime(d.Value.Ticks);
                return new LDate(result);
            }

            // Convert all parameters to int            
            var args = new int[parameters.Length];
            for (int ndx = 0; ndx < parameters.Length; ndx++)
            {
                var arg = parameters[ndx];
                var num = arg is LNumber ? ((LNumber) arg).Value : arg;
                args[ndx] = Convert.ToInt32(num);
            }

            // Case 3: 3 parameters month, day, year
            if (parameters.Length == 3)
                return new LDate(new DateTime(args[0], args[1], args[2]));

            // Case 4: 6 parameters
            if (parameters.Length == 6)
                return new LDate(new DateTime(args[0], args[1], args[2], args[3], args[4], args[5]));

            // TODO: Need to handle this better.
            return new LDate(DateTime.MinValue);
        }

        
        /// <summary>
        /// Sets the full year on the date.
        /// </summary>
        /// <param name="date">The LDateType to set</param>
        /// <param name="year">The year to set</param>
        /// <param name="month">The month to set</param>
        /// <param name="day">The day of the month to set</param>
        public void SetFullYear(LDate date, int year, int month, int day)
        {
            SetDateTime(date, DateTimeKind.Local, year, month, day);
        }


        /// <summary>
        /// Sets the month on the date.
        /// </summary>
        /// <param name="date">The LDateType to set</param>
        /// <param name="month">The month to set</param>
        /// <param name="day">The day of the month to set</param>
        public void SetMonth(LDate date, int month, int day)
        {
            SetDateTime(date, DateTimeKind.Local, -1, month, day);
        }


        /// <summary>
        /// Sets the day of the month on the date
        /// </summary>
        /// <param name="date">The LDateType to set</param>
        /// <param name="day">The day of the month to set</param>
        public void SetDate(LDate date, int day)
        {
            SetDateTime(date, DateTimeKind.Local, -1, -1, day);
        }


        /// <summary>
        /// Sets the hours of the date
        /// </summary>
        /// <param name="date">The LDateType to set</param>
        /// <param name="hours">The hours to set</param>
        /// <param name="minutes">The minutes to set</param>
        /// <param name="seconds">The seconds to set</param>
        /// <param name="milliseconds">The milliseconds to set</param>
        public void SetHours(LDate date, int hours, int minutes, int seconds, int milliseconds)
        {
            SetDateTime(date, DateTimeKind.Local, -1, -1, -1, hours, minutes, seconds, milliseconds);
        }


        /// <summary>
        /// Sets the minutes on the date
        /// </summary>
        /// <param name="date">The LDateType to set</param>
        /// <param name="minutes">The minutes to set</param>
        /// <param name="seconds">The seconds to set</param>
        /// <param name="milliseconds">The milliseconds to set</param>
        public void SetMinutes(LDate date, int minutes, int seconds, int milliseconds)
        {
            SetDateTime(date, DateTimeKind.Local, -1, -1, -1, -1, minutes, seconds, milliseconds);
        }


        /// <summary>
        /// Sets the seconds on the date
        /// </summary>
        /// <param name="date">The LDateType to set</param>
        /// <param name="seconds">The seconds to set</param>
        /// <param name="milliseconds">The milliseconds to set</param>
        public void SetSeconds(LDate date, int seconds, int milliseconds)
        {
            SetDateTime(date, DateTimeKind.Local, -1, -1, -1, -1, -1, seconds, milliseconds);
        }


        /// <summary>
        /// Sets the milliseconds on the date
        /// </summary>
        /// <param name="date">The LDateType to set</param>
        /// <param name="milliseconds">The milliseconds to set</param>
        public void SetMilliseconds(LDate date, int milliseconds)
        {
            SetDateTime(date, DateTimeKind.Local, -1, -1, -1, -1, -1, -1, milliseconds);
        }


        /// <summary>
        /// Sets the full year on the date.
        /// </summary>
        /// <param name="date">The LDateType to set</param>
        /// <param name="year">The year to set</param>
        /// <param name="month">The month to set</param>
        /// <param name="day">The day of the month to set</param>
        public void SetUtcFullYear(LDate date, int year, int month, int day)
        {
            SetDateTime(date, DateTimeKind.Utc, year, month, day);
        }


        /// <summary>
        /// Sets the month on the date.
        /// </summary>
        /// <param name="date">The LDateType to set</param>
        /// <param name="month">The month to set</param>
        /// <param name="day">The day of the month to set</param>
        public void SetUtcMonth(LDate date, int month, int day)
        {
            SetDateTime(date, DateTimeKind.Utc, -1, month, day);
        }


        /// <summary>
        /// Sets the day of the month on the date.
        /// </summary>
        /// <param name="date">The LDateType to set</param>
        /// <param name="day">The day of the month to set</param>
        public void SetUtcDate(LDate date, int day)
        {
            SetDateTime(date, DateTimeKind.Utc, -1, -1, day);
        }


        /// <summary>
        /// Sets the hours on the date.
        /// </summary>
        /// <param name="date">The LDateType to set</param>
        /// <param name="hours">The hours to set</param>
        /// <param name="minutes">The minutes to set</param>
        /// <param name="seconds">The seconds to set</param>
        /// <param name="milliseconds">The milliseconds to set</param>
        public void SetUtcHours(LDate date, int hours, int minutes, int seconds, int milliseconds)
        {
            SetDateTime(date, DateTimeKind.Utc, -1, -1, -1, hours, minutes, seconds, milliseconds);
        }


        /// <summary>
        /// Sets the minutes on the date.
        /// </summary>
        /// <param name="date">The LDateType to set</param>
        /// <param name="minutes">The minutes to set</param>
        /// <param name="seconds">The seconds to set</param>
        /// <param name="milliseconds">The milliseconds to set</param>
        public void SetUtcMinutes(LDate date, int minutes, int seconds, int milliseconds)
        {
            SetDateTime(date, DateTimeKind.Utc, -1, -1, -1, -1, minutes, seconds, milliseconds);
        }


        /// <summary>
        /// Sets the seconds on the date.
        /// </summary>
        /// <param name="date">The LDateType to set</param>
        /// <param name="seconds">The seconds to set</param>
        /// <param name="milliseconds">The milliseconds to set</param>
        public void SetUtcSeconds(LDate date, int seconds, int milliseconds)
        {
            SetDateTime(date, DateTimeKind.Utc, -1, -1, -1, -1, -1, seconds, milliseconds);
        }


        /// <summary>
        /// Sets the milliseconds on the date.
        /// </summary>
        /// <param name="date">The LDateType to set</param>
        /// <param name="milliseconds">The milliseconds to set</param>
        public void SetUtcMilliseconds(LDate date, int milliseconds)
        {
            SetDateTime(date, DateTimeKind.Utc, -1, -1, -1, -1, -1, -1, milliseconds);
        }


        private static void SetDateTime(LDate date, DateTimeKind kind, int year = -1, int month = -1, int day = -1,
            int hour = -1, int minute = -1, int second = -1, int millisecond = -1)
        {
            var target = date.Value;
            DateTime dt = kind == DateTimeKind.Utc ? target.ToUniversalTime() : target;
            year = year == -1 ? dt.Year : year;
            month = month == -1 ? dt.Month : month;
            day = day == -1 ? dt.Day : day;
            hour = hour == -1 ? dt.Hour : hour;
            minute = minute == -1 ? dt.Minute : minute;
            second = second == -1 ? dt.Second : second;
            millisecond = millisecond == -1 ? dt.Millisecond : millisecond;

            var finalDateTime = new DateTime(year, month, day, hour, minute, second, millisecond, kind);
            date.Value = finalDateTime;
        }
        #endregion
    }
}
