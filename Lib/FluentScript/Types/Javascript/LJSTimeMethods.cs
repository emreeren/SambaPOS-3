using System;
using System.Collections.Generic;
using Fluentscript.Lib.Helpers;

namespace Fluentscript.Lib.Types.Javascript
{
    /// <summary>
    /// Array type.
    /// </summary>
    public class LJSTimeMethods : LTypeMethods
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public LJSTimeMethods()
        {
            DataType = LTypes.Time;

            AddMethod("getHours", 			"GetHours", 			typeof(double), "Returns the hour (from 0-23)");
            AddMethod("getMilliseconds", 	"GetMilliseconds", 		typeof(double), "Returns the milliseconds (from 0-999)");
            AddMethod("getMinutes", 		"GetMinutes", 			typeof(double), "Returns the minutes (from 0-59)");
            AddMethod("getSeconds", 		"GetSeconds", 		 	typeof(double), "Returns the seconds (from 0-59)");
            AddMethod("getTime", 			"GetTime", 			 	typeof(double), "Returns the number of milliseconds since midnight Jan 1 1970");
            
            AddMethod("setHours", 			"SetHours", 			null,           "Sets the hour of a date object");
            AddMethod("setMilliseconds", 	"SetMilliseconds", 		null,           "Sets the milliseconds of a date object");
            AddMethod("setMinutes", 		"SetMinutes", 			null,           "Set the minutes of a date object");
            AddMethod("setSeconds", 		"SetSeconds", 			null,           "Sets the seconds of a date object");
            AddMethod("setTime", 			"SetTime", 				null,           "Sets a date and time by adding or subtracting a specified number of milliseconds to/from midnight January 1, 1970");

            AddProperty(true, true,     "Days",        "GetDays",           typeof(double),     "Sets or returns the number of elements in an array");
            AddProperty(true, true,     "Hours",       "GetHours",          typeof(double),     "Returns the hour (from 0-23)");
            AddProperty(true, true,     "Minutes",     "GetMinutes",        typeof(double),     "Returns the minutes (from 0-59)");
            AddProperty(true, true,     "Seconds",     "GetSeconds",        typeof(double),     "Returns the seconds (from 0-59)");
            AddProperty(true, true,     "Milliseconds","GetMilliseconds",   typeof(double),     "Returns the number of milliseconds since midnight Jan 1 1970");
        }


        #region Javascript API methods
        public int      GetDays              (LTime target) { var date = target.Value; return date.Days;                                                     }
        public int      GetHours             (LTime target) { var date = target.Value; return date.Hours;                                                     }
        public int      GetMilliseconds      (LTime target) { var date = target.Value; return date.Milliseconds;                                              }
        public int      GetMinutes           (LTime target) { var date = target.Value; return date.Minutes;		                                           }
        public int      GetSeconds           (LTime target) { var date = target.Value; return date.Seconds;                                                   }
        public string   ToLocaleTimeString   (LTime target) { var date = target.Value; return date.ToString("hh mm ss");                       }
        public string   ToString             (LTime target) { var date = target.Value; return date.ToString("hh mm ss");                     }


        /// <summary>
        /// Callback for when these methods are registered with the system.
        /// </summary>
        public override void OnRegistered()
        {
            // Associated the javascript Date type name "Date" to LDate type name : "sys.datetime".
            LTypesLookup.RegisterAlias(this.DataType, "Time", "Time");
        }


        /// <summary>
        /// Can create from the paramelist expressions supplied.
        /// </summary>
        /// <param name="args">The arguments</param>
        /// <returns></returns>
        public override bool CanCreateFromArgs(object[] args)
        {
            var paramCount = args == null ? 0 : args.Length;
            if (paramCount == 0 || paramCount == 3 || paramCount == 4)
                return true;
            return false;
        }


        /// <summary>
        /// Creates an instance of the type associated with theses methods from the arguments supplied. Repesents a constructor call
        /// </summary>
        /// <param name="args">The arguments used to construct the instance of this type</param>
        /// <returns></returns>
        public override LObject CreateFromArgs(object[] args)
        {
            var parameters = new List<object>();
            if(args != null && args.Length > 0)
            {
                foreach (var arg in args)
                {
                    if(arg is LObject)
                        parameters.Add(((LObject)arg).GetValue());
                }
            }
            args = parameters.ToArray();
            var time = DateTimeTypeHelper.CreateTimeFrom(args);
            return new LTime(time);
        }


        /// <summary>
        /// Sets the hours of the date
        /// </summary>
        /// <param name="time">The LTimeType to set</param>
        /// <param name="hours">The hours to set</param>
        /// <param name="minutes">The minutes to set</param>
        /// <param name="seconds">The seconds to set</param>
        /// <param name="milliseconds">The milliseconds to set</param>
        public void SetHours(LTime time, int hours, int minutes, int seconds, int milliseconds)
        {
            SetDateTime(time, DateTimeKind.Local, -1, -1, -1, hours, minutes, seconds, milliseconds);
        }


        /// <summary>
        /// Sets the minutes on the date
        /// </summary>
        /// <param name="time">The LTimeType to set</param>
        /// <param name="minutes">The minutes to set</param>
        /// <param name="seconds">The seconds to set</param>
        /// <param name="milliseconds">The milliseconds to set</param>
        public void SetMinutes(LTime time, int minutes, int seconds, int milliseconds)
        {
            SetDateTime(time, DateTimeKind.Local, -1, -1, -1, -1, minutes, seconds, milliseconds);
        }


        /// <summary>
        /// Sets the seconds on the date
        /// </summary>
        /// <param name="time">The LTimeType to set</param>
        /// <param name="seconds">The seconds to set</param>
        /// <param name="milliseconds">The milliseconds to set</param>
        public void SetSeconds(LTime time, int seconds, int milliseconds)
        {
            SetDateTime(time, DateTimeKind.Local, -1, -1, -1, -1, -1, seconds, milliseconds);
        }


        /// <summary>
        /// Sets the milliseconds on the date
        /// </summary>
        /// <param name="time">The LTimeType to set</param>
        /// <param name="milliseconds">The milliseconds to set</param>
        public void SetMilliseconds(LTime time, int milliseconds)
        {
            SetDateTime(time, DateTimeKind.Local, -1, -1, -1, -1, -1, -1, milliseconds);
        }


        private static void SetDateTime(LTime target, DateTimeKind kind, int year = -1, int month = -1, int day = -1,
            int hour = -1, int minute = -1, int second = -1, int millisecond = -1)
        {
            var t = target.Value;
            hour = hour == -1 ? t.Hours : hour;
            minute = minute == -1 ? t.Minutes : minute;
            second = second == -1 ? t.Seconds : second;
            millisecond = millisecond == -1 ? t.Milliseconds : millisecond;

            var finaLTimeTime = new TimeSpan(hour, minute, second, millisecond);
            target.Value = finaLTimeTime;
        }
        #endregion
    }
}
