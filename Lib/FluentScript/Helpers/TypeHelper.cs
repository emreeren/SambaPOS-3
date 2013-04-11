using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Helpers
{
    /// <summary>
    /// Helper class for datatypes.
    /// </summary>
    public class LangTypeHelper
    {
        public static LNumber ConverToLangDayOfWeekNumber(LObject obj)
        {
            if (obj.Type == LTypes.Date)
            {
                var day = (int)((LDate)obj).Value.DayOfWeek;
                return new LNumber(day);
            }

            if (obj.Type == LTypes.DayOfWeek)
            {
                var day = (int)((LDayOfWeek)obj).Value;
                return new LNumber(day);
            }
            return (LNumber)obj;
        }


        /// <summary>
        /// Converts from c# datatypes to fluentscript datatypes inside
        /// </summary>
        /// <param name="val"></param>
        public static LObject ConvertToLangValue(object val)
        {
            if (val == null) return LObjects.Null;

            var type = val.GetType();

            if (type == typeof(int))
                return new LNumber(Convert.ToDouble(val));

            if (type == typeof(double))
                return new LNumber((double)val);

            if (type == typeof(decimal))
                return new LNumber(Convert.ToDouble(val));

            if (type == typeof(string))
                return new LString((string)val);

            if (type == typeof(DateTime))
                return new LDate((DateTime)val);

            if (type == typeof(TimeSpan))
                return new LTime((TimeSpan)val);

            if (type == typeof(DayOfWeek))
                return new LDayOfWeek((DayOfWeek)val);

            if (type == typeof(bool))
                return new LBool((bool)val);

            var isGenType = type.IsGenericType;
            if (isGenType)
            {
                var gentype = type.GetGenericTypeDefinition();
                if (type == typeof(List<object>) || gentype == typeof(List<>) || gentype == typeof(IList<>))
                    return new LArray((IList)val);

                if (type == typeof(Dictionary<string, object>))
                    return new LMap((Dictionary<string, object>)val);
            }
            // object
            return LangTypeHelper.ConvertToLangClass(val);
        }


        /// <summary>
        /// Converts from c# datatypes to fluentscript datatypes inside
        /// </summary>
        /// <param name="val"></param>
        public static LObject ConvertToLangValue(LType ltype, object val)
        {
            if (val == null) return LObjects.Null;

            if (ltype == LTypes.Number)
                return new LNumber(Convert.ToDouble(val));

            if (ltype == LTypes.String)
                return new LString(Convert.ToString(val));

            if (ltype == LTypes.Date)
                return new LDate(Convert.ToDateTime(val));

            if (ltype == LTypes.Time)
            {
                var valText = Convert.ToString(val);
                var result = DateTimeTypeHelper.ParseTime(valText);
                var time = result.Item1;
                return new LTime(time);
            }
            if (ltype == LTypes.Bool)
                return new LBool(Convert.ToBoolean(val));

            return LObjects.Null;
        }

        /// <summary>
        /// Converts a Type object from the host language to a fluentscript type.
        /// </summary>
        /// <param name="hostLangType"></param>
        /// <returns></returns>
        public static LType ConvertToLangType(Type hostLangType)
        {
            if (hostLangType == typeof(bool)) return LTypes.Bool;
            if (hostLangType == typeof(DateTime)) return LTypes.Date;
            if (hostLangType == typeof(int)) return LTypes.Number;
            if (hostLangType == typeof(double)) return LTypes.Number;
            if (hostLangType == typeof(decimal)) return LTypes.Number;
            if (hostLangType == typeof(string)) return LTypes.String;
            if (hostLangType == typeof(TimeSpan)) return LTypes.Time;
            if (hostLangType == typeof(Nullable)) return LTypes.Null;
            if (hostLangType == typeof(IList)) return LTypes.Array;
            if (hostLangType == typeof(IDictionary)) return LTypes.Map;

            return LTypes.Object;
        }


        /// <summary>
        /// Converts a Type object from the host language to a fluentscript type.
        /// </summary>
        /// <param name="langTypeName"></param>
        /// <returns></returns>
        public static LType ConvertToLangTypeFromLangTypeName(string langTypeName)
        {
            if (langTypeName == LTypes.Bool.Name) return LTypes.Bool;
            if (langTypeName == LTypes.Date.Name) return LTypes.Date;
            if (langTypeName == LTypes.Time.Name) return LTypes.Time;
            if (langTypeName == LTypes.Number.Name) return LTypes.Number;
            if (langTypeName == LTypes.String.Name) return LTypes.String;
            return LTypes.Object;
        }


        /// <summary>
        /// Converts to host language type to a fluentscript type.
        /// </summary>
        /// <param name="hostLangType"></param>
        /// <returns></returns>
        public static LType ConvertToLangTypeClass(Type hostLangType)
        {
            var type = new LObjectType();
            type.Name = hostLangType.Name;
            type.FullName = hostLangType.FullName;
            type.TypeVal = TypeConstants.LClass;
            return type;
        }


        /// <summary>
        /// Converts to host language type to a fluentscript type.
        /// </summary>
        /// <param name="hostLangType"></param>
        /// <returns></returns>
        public static LObject ConvertToLangClass(object obj)
        {
            var type = obj.GetType();
            var lclassType = new LClassType();
            lclassType.Name = type.Name;
            lclassType.FullName = type.FullName;
            lclassType.DataType = type;
            lclassType.TypeVal = TypeConstants.LClass;
            var lclass = new LClass(obj);
            lclass.Type = lclassType;
            return lclass;
        }


        /// <summary>
        /// Converts to host language type to a fluentscript type.
        /// </summary>
        /// <param name="hostLangType"></param>
        /// <returns></returns>
        public static LObject ConvertToLangUnit(object obj)
        {
            var type = obj.GetType();
            var lclassType = new LClassType();
            lclassType.Name = type.Name;
            lclassType.FullName = type.FullName;
            lclassType.DataType = type;
            lclassType.TypeVal = TypeConstants.Unit;
            var lclass = new LClass(obj);
            lclass.Type = lclassType;
            return lclass;
        }


        /// <summary>
        /// Get the type in the host language that represents the same type in fluentscript.
        /// </summary>
        /// <param name="ltype">The LType in fluentscript.</param>
        /// <returns></returns>
        public static Type ConvertToHostLangType(LType ltype)
        {
            if (ltype == LTypes.Bool) return typeof(bool);
            if (ltype == LTypes.Date) return typeof(DateTime);
            //if (ltype == LTypes.Number) return typeof(int);
            if (ltype == LTypes.Number) return typeof(double);
            if (ltype == LTypes.String) return typeof(string);
            if (ltype == LTypes.Time) return typeof(TimeSpan);
            if (ltype == LTypes.Array) return typeof(IList);
            if (ltype == LTypes.Map) return typeof(IDictionary);
            if (ltype == LTypes.Null) return typeof(Nullable);

            return typeof(object);
        }


        /// <summary>
        /// Converts from c# datatypes to fluentscript datatypes inside
        /// </summary>
        /// <param name="args"></param>
        public static void ConvertToLangTypeValues(List<object> args)
        {
            if (args == null || args.Count == 0)
                return;

            // Convert types from c# types fluentscript compatible types.
            for (int ndx = 0; ndx < args.Count; ndx++)
            {
                var val = args[ndx];

                args[ndx] = ConvertToLangValue(val);
            }
        }


        /// <summary>
        /// Converts from c# datatypes to fluentscript datatypes inside
        /// </summary>
        /// <param name="args"></param>
        public static object[] ConvertToArrayOfHostLangValues(object[] args)
        {
            if (args == null || args.Length == 0)
                return args;

            // Convert types from c# types fluentscript compatible types.
            var convertedItems = new object[args.Length];
            for (int ndx = 0; ndx < args.Length; ndx++)
            {
                var val = args[ndx];
                if (val is LObject)
                    convertedItems[ndx] = ((LObject)val).GetValue();
                else
                    convertedItems[ndx] = val;
            }
            return convertedItems;
        }


        /// <summary>
        /// Converts the source to the target list type by creating a new instance of the list and populating it.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetListType"></param>
        /// <returns></returns>
        public static object ConvertToTypedList(IList<object> source, Type targetListType)
        {
            var t = targetListType; // targetListType.GetType();
            var dt = targetListType.GetGenericTypeDefinition();
            var targetType = dt.MakeGenericType(t.GetGenericArguments()[0]);
            var targetList = Activator.CreateInstance(targetType);
            var l = targetList as System.Collections.IList;
            foreach (var item in source)
            {
                var val = item;
                if (item is LObject)
                {
                    val = ((LObject)item).GetValue();
                }
                l.Add(val);
            }
            return targetList;
        }


        /// <summary>
        /// Converts the source to the target list type by creating a new instance of the list and populating it.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetListType"></param>
        /// <returns></returns>
        public static object ConvertToTypedDictionary(IDictionary<string, object> source, Type targetListType)
        {
            var t = targetListType; // targetListType.GetType();
            var dt = targetListType.GetGenericTypeDefinition();
            var targetType = dt.MakeGenericType(t.GetGenericArguments()[0], t.GetGenericArguments()[1]);
            var targetDict = Activator.CreateInstance(targetType);
            var l = targetDict as System.Collections.IDictionary;
            foreach (var item in source) l.Add(item.Key, item.Value);
            return targetDict;
        }


        /// <summary>
        /// Converts arguments from one type to another type that is required by the method call.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="method">The method for which the parameters need to be converted</param>
        public static object[] ConvertArgs(List<object> args, MethodInfo method)
        {
            var hostLangArgs = new List<object>();
            var parameters = method.GetParameters();
            if (parameters.Length == 0) return hostLangArgs.ToArray();

            // REQUIREMENT: Number of values must match # of parameters in method.
            for (int ndx = 0; ndx < parameters.Length; ndx++)
            {
                var param = parameters[ndx];
                var sourceArg = args[ndx] as LObject;

                // CASE 1: Null
                if (sourceArg == LObjects.Null)
                {
                    var defaultVal = LangTypeHelper.GetDefaultValue(param.ParameterType);
                    hostLangArgs.Add(defaultVal);
                }

                // CASE 2: int, bool, date, time
                else if (sourceArg.Type.IsPrimitiveType())
                {
                    var convertedVal = sourceArg.GetValue();
                    convertedVal = ConvertToCorrectHostLangValue(param.ParameterType, convertedVal);
                    hostLangArgs.Add(convertedVal);
                }
                // CASE 3: LArrayType and generic types.
                else if (sourceArg.Type == LTypes.Array)
                {
                    // Case 1: Array
                    if (param.ParameterType.IsArray)
                    {
                        if (param.ParameterType == typeof(string[]))
                        {
                            var convertedVal = ConvertToHostLangArray(param.ParameterType, (LArray)sourceArg);
                            hostLangArgs.Add(convertedVal);
                        }
                    }
                    else if (param.ParameterType.IsGenericType)
                    {
                        var gentype = param.ParameterType.GetGenericTypeDefinition();

                        // Case 2: Matching types IList<object>
                        if (gentype == typeof(IList<object>))
                        {
                            var convertedVal = sourceArg.GetValue();
                            hostLangArgs.Add(convertedVal);
                        }
                        // Case 3: Non-matching types List<object> to IList<Person>
                        else if (gentype == typeof(List<>) || gentype == typeof(IList<>))
                        {
                            //args[ndx] = ConvertToTypedList((List<object>) sourceArg.GetValue(), param.ParameterType);
                            var convertedArr = ConvertToTypedList((List<object>)sourceArg.GetValue(), param.ParameterType);
                            hostLangArgs.Add(convertedArr);
                        }
                    }
                }
            }
            return hostLangArgs.ToArray();
        }


        /// <summary>
        /// Gets the default value for the supplied type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetDefaultValue(Type type)
        {
            if (type == typeof(int)) return 0;
            if (type == typeof(bool)) return false;
            if (type == typeof(double)) return 0.0;
            if (type == typeof(DateTime)) return DateTime.MinValue;
            if (type == typeof(TimeSpan)) return TimeSpan.MinValue;
            return null;
        }


        /// <summary>
        /// Whether or not the type supplied is a basic type.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsBasicTypeCSharpType(object obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            Type type = obj.GetType();
            if (type == typeof(int)) return true;
            if (type == typeof(long)) return true;
            if (type == typeof(double)) return true;
            if (type == typeof(bool)) return true;
            if (type == typeof(string)) return true;
            if (type == typeof(DateTime)) return true;
            if (type == typeof(TimeSpan)) return true;
            return false;
        }


        /// <summary>
        /// Converts each item in the parameters object array to an integer.
        /// </summary>
        /// <param name="parameters"></param>
        public static int[] ConvertToInts(object[] parameters)
        {
            // Convert all parameters to int            
            int[] args = new int[parameters.Length];
            for (int ndx = 0; ndx < parameters.Length; ndx++)
            {
                args[ndx] = Convert.ToInt32(parameters[ndx]);
            }
            return args;
        }


        private static object ConvertToCorrectHostLangValue(Type type, object val)
        {
            if (type == typeof(int))
                return Convert.ToInt32(val);
            if (type == typeof(long))
                return Convert.ToInt64(val);
            if (type == typeof(float))
                return Convert.ToSingle(val);
            return val;
        }


        private static object ConvertToHostLangArray(Type type, LArray array)
        {
            var length = array.Value.Count;
            var fsarray = array.Value;
            Array items = null;
            var hostType = typeof(int);
            if (type == typeof(string[]))
            {
                items = new string[length];
                hostType = typeof(string);
            }
            else if (type == typeof(bool[]))
            {
                items = new bool[length];
                hostType = typeof(bool);
            }
            else if (type == typeof(DateTime[]))
            {
                items = new DateTime[length];
                hostType = typeof(DateTime);
            }
            else if (type == typeof(TimeSpan[]))
            {
                items = new TimeSpan[length];
                hostType = typeof(TimeSpan);
            }
            else if (type == typeof(double[]))
            {
                items = new double[length];
                hostType = typeof(double);
            }
            else if (type == typeof(float[]))
            {
                items = new float[length];
                hostType = typeof(float);
            }
            else if (type == typeof(long[]))
            {
                items = new long[length];
                hostType = typeof(long);
            }
            else if (type == typeof(int[]))
            {
                items = new int[length];
                hostType = typeof(int);
            }

            for (var ndx = 0; ndx < fsarray.Count; ndx++)
            {
                var val = fsarray[ndx] as LObject;
                var hostval = val.GetValue();

                // This converts double to long as fluentscript only supports double right now.
                var converted = ConvertToCorrectHostLangValue(hostType, hostval);
                items.SetValue(converted, ndx);
            }
            return items;
        }


        /// <summary>
        /// Add 2 unites together.
        /// </summary>
        /// <param name="u1"></param>
        /// <param name="u2"></param>
        /// <returns></returns>
        public static LUnit AddUnits(LUnit u1, LUnit u2)
        {
            // Validate
            LangTypeHelper.ValidateUnits(u1, u2);

            // Now convert the values to their base value.
            double totalBase = u1.BaseValue + u2.BaseValue;
            var unit = new LUnit(totalBase);
            unit.BaseValue = totalBase;
            unit.Group = u1.Group;
            unit.SubGroup = u1.SubGroup;

            // Set the value to the converted relative value 
            // e.g. if u1.subgroup = feet and u2.subgroup == inches.
            // then multiply result.basevalue by the conversion value.
            // Now convert the units
            return unit;
        }


        private static void ValidateUnits(LUnit u1, LUnit u2)
        {
            // Validate.
            if (u1 == null || u2 == null)
                throw new ArgumentException("Can not add units when 1 unit is empty");

            // Check for matching groups e.g. length + length or weight + weight.
            if (u1.Group != u2.Group)
                throw new ArgumentException("Can not add units " + u1.Group + " to " + u2.Group);
        }
    }
}
