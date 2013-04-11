using System.Collections.Generic;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Types
{
    /// <summary>
    /// A lookup class for types.
    /// </summary>
    public class LTypesLookup
    {
        private static Dictionary<string, LType> _types = new Dictionary<string, LType>();
        private static Dictionary<string, LType> _sysBasicTypes = new Dictionary<string, LType>();
        private static IDictionary<int, Dictionary<int, int>> _basicConversions = new Dictionary<int, Dictionary<int, int>>();


        /// <summary>
        /// Initialize with defaults
        /// </summary>
        public static void Init()
        {
            Register(LTypes.Array);
            Register(LTypes.Bool);
            Register(LTypes.Date);
            Register(LTypes.Function);
            Register(LTypes.Map);
            Register(LTypes.Null);
            Register(LTypes.Number);
            Register(LTypes.String);
            Register(LTypes.Time);
        }


        /// <summary>
        /// Register the type
        /// </summary>
        /// <param name="type"></param>
        public static void Register(LType type)
        {
            RegisterAlias(type, type.FullName, type.Name);
        }


        /// <summary>
        /// Register the type
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="fullName">The fullname of the type</param>
        /// <param name="name">The short name of the type</param>
        public static void RegisterAlias(LType type, string fullName, string name)
        {
            _types[fullName] = type;
            if (type.IsSystemType)
                _sysBasicTypes[name] = type;
        }


        /// <summary>
        /// Whether or not the type name supplied is a basic system type. e.g. bool, date etc.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsBasicTypeShortName(string name)
        {
            return _sysBasicTypes.ContainsKey(name);
        }

        
        /// <summary>
        /// Check whether or nor the fulltype name supplied is a basic type
        /// </summary>
        /// <param name="fullName">The full name of the type. e.g. sys.string.</param>
        /// <returns></returns>
        public static bool IsBuiltInType(string fullName)
        {
            if (!_types.ContainsKey(fullName))
                return false;
            var type = _types[fullName];
            return type.IsBuiltInType();
        }


        /// <summary>
        /// Gets the type of the fullname supplied.
        /// </summary>
        /// <param name="fullName">The full name of the type. e.g. sys.string.</param>
        /// <returns></returns>
        public static LType GetLType(string fullName)
        {
            if (!_types.ContainsKey(fullName))
                return null;

            return _types[fullName];
        }


        public static void SetupDefaultConversionMatrix()
        {
            // Array
            SetDefaultConversionMatrix(LTypes.Array, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.Array, TypeConstants.Array, TypeConversionMode.SameType);

            // Bool
            AddConversionTo(LTypes.Bool, TypeConstants.Array, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.Bool, TypeConstants.Bool, TypeConversionMode.SameType);
            AddConversionTo(LTypes.Bool, TypeConstants.Date, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.Bool, TypeConstants.Map, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.Bool, TypeConstants.Number, TypeConversionMode.Supported);
            AddConversionTo(LTypes.Bool, TypeConstants.Null, TypeConversionMode.Supported);
            AddConversionTo(LTypes.Bool, TypeConstants.String, TypeConversionMode.Supported);
            AddConversionTo(LTypes.Bool, TypeConstants.Time, TypeConversionMode.NotSupported);

            // Class
            // SetDefaultConversionMatrix(TypeConversionMode.NotSupported);

            // Date
            AddConversionTo(LTypes.Date, TypeConstants.Array, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.Date, TypeConstants.Bool, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.Date, TypeConstants.Date, TypeConversionMode.SameType);
            AddConversionTo(LTypes.Date, TypeConstants.Map, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.Date, TypeConstants.Number, TypeConversionMode.Supported);
            AddConversionTo(LTypes.Date, TypeConstants.Null, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.Date, TypeConstants.String, TypeConversionMode.Supported);
            AddConversionTo(LTypes.Date, TypeConstants.Time, TypeConversionMode.Supported);
            
            // DayOfWeek
            AddConversionTo(LTypes.DayOfWeek, TypeConstants.Array, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.DayOfWeek, TypeConstants.Bool, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.DayOfWeek, TypeConstants.Date, TypeConversionMode.Supported);
            AddConversionTo(LTypes.DayOfWeek, TypeConstants.Map, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.DayOfWeek, TypeConstants.Number, TypeConversionMode.Supported);
            AddConversionTo(LTypes.DayOfWeek, TypeConstants.Null, TypeConversionMode.Supported);
            AddConversionTo(LTypes.DayOfWeek, TypeConstants.String, TypeConversionMode.Supported);
            AddConversionTo(LTypes.DayOfWeek, TypeConstants.Time, TypeConversionMode.SameType);

            // Function
            SetDefaultConversionMatrix(LTypes.Function, TypeConversionMode.NotSupported);

            // Map
            SetDefaultConversionMatrix(LTypes.Map, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.Map, TypeConstants.Map, TypeConversionMode.SameType);

            // Module
            // SetDefaultConversionMatrix(LTypes.Module, TypeConversionMode.NotSupported);

            // Null
            SetDefaultConversionMatrix(LTypes.Null, TypeConversionMode.NotSupported);
            
            // Number
            AddConversionTo(LTypes.Number, TypeConstants.Array, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.Number, TypeConstants.Bool, TypeConversionMode.Supported);
            AddConversionTo(LTypes.Number, TypeConstants.Date, TypeConversionMode.RunTimeCheck);
            AddConversionTo(LTypes.Number, TypeConstants.Map, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.Number, TypeConstants.Number, TypeConversionMode.SameType);
            AddConversionTo(LTypes.Number, TypeConstants.Null, TypeConversionMode.Supported);
            AddConversionTo(LTypes.Number, TypeConstants.String, TypeConversionMode.Supported);
            AddConversionTo(LTypes.Number, TypeConstants.Time, TypeConversionMode.RunTimeCheck);

            // String
            AddConversionTo(LTypes.String, TypeConstants.Array, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.String, TypeConstants.Bool, TypeConversionMode.RunTimeCheck);
            AddConversionTo(LTypes.String, TypeConstants.Date, TypeConversionMode.RunTimeCheck);
            AddConversionTo(LTypes.String, TypeConstants.Map, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.String, TypeConstants.Number, TypeConversionMode.RunTimeCheck);
            AddConversionTo(LTypes.String, TypeConstants.Null, TypeConversionMode.Supported);
            AddConversionTo(LTypes.String, TypeConstants.String, TypeConversionMode.SameType);
            AddConversionTo(LTypes.String, TypeConstants.Time, TypeConversionMode.RunTimeCheck);

            // Time
            AddConversionTo(LTypes.Time, TypeConstants.Array, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.Time, TypeConstants.Bool, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.Time, TypeConstants.Date, TypeConversionMode.Supported);
            AddConversionTo(LTypes.Time, TypeConstants.Map, TypeConversionMode.NotSupported);
            AddConversionTo(LTypes.Time, TypeConstants.Number, TypeConversionMode.Supported);
            AddConversionTo(LTypes.Time, TypeConstants.Null, TypeConversionMode.Supported);
            AddConversionTo(LTypes.Time, TypeConstants.String, TypeConversionMode.Supported);
            AddConversionTo(LTypes.Time, TypeConstants.Time, TypeConversionMode.SameType);

            // Unit
            // AddConversionTo(LTypes.Unit, TypeConstants.Array, TypeConversionMode.NotSupported);
            // AddConversionTo(LTypes.Unit, TypeConstants.Bool, TypeConversionMode.NotSupported);
            // AddConversionTo(LTypes.Unit, TypeConstants.Date, TypeConversionMode.Supported);
            // AddConversionTo(LTypes.Unit, TypeConstants.Map, TypeConversionMode.NotSupported);
            // AddConversionTo(LTypes.Unit, TypeConstants.Number, TypeConversionMode.Supported);
            // AddConversionTo(LTypes.Unit, TypeConstants.Null, TypeConversionMode.Supported);
            // AddConversionTo(LTypes.Unit, TypeConstants.String, TypeConversionMode.Supported);
            // AddConversionTo(LTypes.Unit, TypeConstants.Time, TypeConversionMode.SameType);
        }


        /// <summary>
        /// Adds a conversion flag that indicates if converting from this type to the supplied type is possible
        /// </summary>
        /// <param name="typeVal">See TypeConstants : The type value of the destination basic type to convert to</param>
        /// <param name="mode">See TypeConversionMode: Flag indicating mode of conversion</param>
        /// <returns></returns>
        public static void AddConversionTo(LObjectType type, int typeVal, int mode)
        {
            Dictionary<int, int> conversionMap = null;
            if (_basicConversions.ContainsKey(type.TypeVal))
                conversionMap = _basicConversions[type.TypeVal];
            else
            {
                conversionMap = new Dictionary<int, int>();
                _basicConversions[type.TypeVal] = conversionMap;
            }
            conversionMap[typeVal] = mode;
        }


        /// <summary>
        /// Sets up the matrix of possible conversions to all basic datatypes to the mode supplied.
        /// </summary>
        /// <param name="typeConversionMode">See TypeConversionMode</param>
        public static void SetDefaultConversionMatrix(LObjectType type, int typeConversionMode)
        {
            AddConversionTo(type, TypeConstants.Array, typeConversionMode);
            AddConversionTo(type, TypeConstants.Bool, typeConversionMode);
            AddConversionTo(type, TypeConstants.Date, typeConversionMode);
            AddConversionTo(type, TypeConstants.Map, typeConversionMode);
            AddConversionTo(type, TypeConstants.Number, typeConversionMode);
            AddConversionTo(type, TypeConstants.Null, typeConversionMode);
            AddConversionTo(type, TypeConstants.String, typeConversionMode);
            AddConversionTo(type, TypeConstants.Time, typeConversionMode);
        }
    }
}
