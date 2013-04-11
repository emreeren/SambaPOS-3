using System;
using Fluentscript.Lib.Types;

namespace Fluentscript.Lib.Helpers
{
    /// <summary>
    /// Helper class to convert types from one to another type ( only used for internal fluentscript types )
    /// </summary>
    public class ConversionHelper
    {
        /// <summary>
        /// Converts bool to number
        /// </summary>
        /// <param name="spec">The conversion spec</param>
        /// <param name="val">The bool value to convert</param>
        /// <returns></returns>
        public static object Convert_Bool_To_Number(ConvertSpec spec, object obj) 
        {
            var val = (LBool)obj;
            var result = val.Value == true ? 1 : 0;
            return new LNumber(result);
        }


        /// <summary>
        /// Converts number to bool
        /// </summary>
        /// <param name="spec">The conversion spec</param>
        /// <param name="val">The number value to convert</param>
        /// <returns></returns>
        public static object Convert_Number_To_Bool(ConvertSpec spec, object obj)
        {
            var val = (LNumber)obj;
            var result = val.Value > 0 ? true : false;
            return new LBool(result);
        }


        /// <summary>
        /// Converts date to string
        /// </summary>
        /// <param name="spec">The conversion spec</param>
        /// <param name="val">The date value to convert</param>
        /// <returns></returns>
        public static object Convert_Date_To_String(ConvertSpec spec, object obj)
        {
            var val = (LDate)obj;
            return new LString(val.Value.ToString("MM/DD/yyyy hh:mm tt"));
        }


        /// <summary>
        /// Converts date to time
        /// </summary>
        /// <param name="spec">The conversion spec</param>
        /// <param name="val">The date value to convert</param>
        /// <returns></returns>
        public static object Convert_Date_To_Time(ConvertSpec spec, object obj)
        {
            var val = (LDate)obj;
            return new LTime(val.Value.TimeOfDay);
        }


        /// <summary>
        /// Converts time to string
        /// </summary>
        /// <param name="spec">The conversion spec</param>
        /// <param name="val">The time value to convert</param>
        /// <returns></returns>
        public static object Convert_Time_To_String(ConvertSpec spec, object obj)
        {
            var val = (LTime)obj;
            return new LString(val.Value.ToString("hh:mm tt"));
        }


        /// <summary>
        /// Converts time to date
        /// </summary>
        /// <param name="spec">The conversion spec</param>
        /// <param name="val">The time value to convert</param>
        /// <returns></returns>
        public static object Convert_Time_To_Date(ConvertSpec spec, object obj)
        {
            var val = (LTime)obj;
            var t = val.Value;
            var d = DateTime.Today;
            return new LDate(new DateTime(d.Year, d.Month, d.Day, t.Hours, t.Minutes, t.Seconds));
        }


        /// <summary>
        /// Converts string to number
        /// </summary>
        /// <param name="spec">The conversion spec</param>
        /// <param name="val">The string value to convert</param>
        /// <returns></returns>
        public static object Convert_String_To_Number(ConvertSpec spec, object obj)
        {
            var val = (LString)obj;
            var result = (double)Convert.ChangeType(val.Value, typeof(double), null);
            return new LNumber(result);
        }


        /// <summary>
        /// Converts string to date
        /// </summary>
        /// <param name="spec">The conversion spec</param>
        /// <param name="val">The string value to convert</param>
        /// <returns></returns>
        public static object Convert_String_To_Date(ConvertSpec spec, object obj)
        {
            var val = (LString)obj;
            var result = (DateTime)Convert.ChangeType(val.Value, typeof(DateTime), null);
            return new LDate(result);
        }


        /// <summary>
        /// Converts string to number
        /// </summary>
        /// <param name="spec">The conversion spec</param>
        /// <param name="val">The string value to convert</param>
        /// <returns></returns>
        public static object Convert_String_To_Time(ConvertSpec spec, object obj)
        {
            var val = (LString)obj;
            var txt = val.Value.ToLower();
            var result = DateTimeTypeHelper.ParseTime(txt);
            if (!result.Item2)
                return LObjects.Null;
            return new LTime(result.Item1);
        }


        /// <summary>
        /// Converts string to bool.
        /// </summary>
        /// <param name="spec">The conversion spec</param>
        /// <param name="val">The string value to convert</param>
        /// <returns></returns>
        public static object Convert_String_To_Bool(ConvertSpec spec, object obj)
        {
            var val = (LString)obj;
            var s = val.Value.ToLower();
            if (s == "yes" || s == "true" || s == "1" || s == "ok" || s == "on")
                return new LBool(true);
            return new LBool(false);
        }  
    }
}
