
namespace Fluentscript.Lib._Core
{
    /// <summary>
    /// Type constants.
    /// </summary>
    public class TypeConstants
    {
        /// <summary>
        /// Any type - used to represent all types
        /// </summary>
        public const int Any      = 0;


        /// <summary>
        /// Null type
        /// </summary>
        public const int Null     = 1;


        /// <summary>
        /// Void type
        /// </summary>
        public const int Void     = 2;      


        /// <summary>
        /// Boolean true/false type
        /// </summary>
        public const int Bool     = 3;


        /// <summary>
        /// Number type 1, 1.4
        /// </summary>
        public const int Number   = 4;        


        /// <summary>
        /// String type
        /// </summary>
        public const int String   = 5;


        /// <summary>
        /// Date type 
        /// </summary>
        public const int Date     = 6;


        /// <summary>
        /// Time type
        /// </summary>
        public const int Time     = 7;


        /// <summary>
        /// Array type
        /// </summary>
        public const int Array    = 8;


        /// <summary>
        /// Map type
        /// </summary>
        public const int Map      = 9;
        

        /// <summary>
        /// Function type.
        /// </summary>
        public const int Function = 10;


        /// <summary>
        /// Represents an external class in c# being used
        /// </summary>
        public const int LClass   = 11;


        /// <summary>
        /// A unit of measure.
        /// </summary>
        public const int Unit = 12;


        /// <summary>
        /// Represents a module type.
        /// </summary>
        public const int Module = 13;


        /// <summary>
        /// Day of week type.
        /// </summary>
        public const int DayOfWeek = 14;


        /// <summary>
        /// Table type.
        /// </summary>
        public const int Table = 15;
    }



    /// <summary>
    /// Flags to indicate if conversion from one type to another type is supported.
    /// </summary>
    public class TypeConversionMode
    {
        /// <summary>
        /// Type conversion fully supported
        /// </summary>
        public const int Supported = 0;


        /// <summary>
        /// Type conversion not supported
        /// </summary>
        public const int NotSupported = 1;


        /// <summary>
        /// Represents same type between source/destination. 
        /// </summary>
        public const int SameType = 2;


        /// <summary>
        /// Partially supported. e.g. may require some run-time type checking.
        /// </summary>
        public const int RunTimeCheck = 3;
    }
}
