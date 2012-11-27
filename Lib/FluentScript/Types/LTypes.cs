

namespace ComLib.Lang.Types
{
    /// <summary>
    /// Class to reference instance of the basic types
    /// </summary>
    public class LTypes
    {
        /// <summary>
        /// Single instance of the array type for resusability
        /// </summary>
        public static LType Array = new LArrayType();


        /// <summary>
        /// Single instance of the bool type for resusability
        /// </summary>
        public static LType Bool = new LBoolType();


        /// <summary>
        /// Single instance of the date type for resusability
        /// </summary>
        public static LType Date = new LDateType();

        
        /// <summary>
        /// Single instance of the day of week type for resusability
        /// </summary>
        public static LType DayOfWeek = new LDayOfWeekType();


        /// <summary>
        /// Single instance of the function type for resusability
        /// </summary>
        public static LType Function = new LFunctionType(string.Empty);


        /// <summary>
        /// Single instanceo of the Map type for resusability
        /// </summary>
        public static LType Map = new LMapType();


        /// <summary>
        /// Single instance of the Null type for resusability
        /// </summary>
        public static LType Null = new LNullType();


        /// <summary>
        /// Single instance of the number type for resusability
        /// </summary>
        public static LType Number = new LNumberType();


        /// <summary>
        /// Single instance of the string type for reusability
        /// </summary>
        public static LType String = new LStringType();


        /// <summary>
        /// Signle instance of the time type for reusability
        /// </summary>
        public static LType Time = new LTimeType();


        /// <summary>
        /// Object type.
        /// </summary>
        public static LType Object = new LObjectType();


        /// <summary>
        /// A unit of measure type.
        /// </summary>
        public static LType Unit = new LUnitType();
    }



    /// <summary>
    /// Holder for "singleton" like object such as empty string, null.
    /// </summary>
    public class LObjects
    {
        /// <summary>
        /// Empty object
        /// </summary>
        public static LNull Null = new LNull();


        /// <summary>
        /// Empty string.
        /// </summary>
        public static LObject EmptyString = new LString("");
    }
}
