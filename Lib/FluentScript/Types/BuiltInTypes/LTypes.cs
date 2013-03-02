

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
        public static LObjectType Array = new LArrayType();


        /// <summary>
        /// Single instance of the bool type for resusability
        /// </summary>
        public static LObjectType Bool = new LBoolType();


        /// <summary>
        /// Single instance of the date type for resusability
        /// </summary>
        public static LObjectType Date = new LDateType();

        
        /// <summary>
        /// Single instance of the day of week type for resusability
        /// </summary>
        public static LObjectType DayOfWeek = new LDayOfWeekType();


        /// <summary>
        /// Single instance of the function type for resusability
        /// </summary>
        public static LObjectType Function = new LFunctionType(string.Empty);


        /// <summary>
        /// Single instanceo of the Map type for resusability
        /// </summary>
        public static LObjectType Map = new LMapType();


        /// <summary>
        /// Single instance of the Null type for resusability
        /// </summary>
        public static LObjectType Null = new LNullType();


        /// <summary>
        /// Single instance of the number type for resusability
        /// </summary>
        public static LObjectType Number = new LNumberType();


        /// <summary>
        /// Single instance of the string type for reusability
        /// </summary>
        public static LObjectType String = new LStringType();


        /// <summary>
        /// Signle instance of the time type for reusability
        /// </summary>
        public static LObjectType Time = new LTimeType();


        /// <summary>
        /// Object type.
        /// </summary>
        public static LObjectType Object = new LObjectType();


        /// <summary>
        /// A unit of measure type.
        /// </summary>
        public static LObjectType Unit = new LUnitType();
    }
}
