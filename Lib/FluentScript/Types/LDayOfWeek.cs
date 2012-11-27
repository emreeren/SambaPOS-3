using System;

using ComLib.Lang.Core;

namespace ComLib.Lang.Types
{
    /// <summary>
    /// Used to store a timespan value.
    /// </summary>
    public class LDayOfWeek : LObject
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val"></param>
        public LDayOfWeek(DayOfWeek val)
        {
            this.Value = val;
            this.Type = LTypes.DayOfWeek;
        }


        /// <summary>
        /// The raw type value.
        /// </summary>
        public DayOfWeek Value;


        /// <summary>
        /// Gets the value of this object.
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            return this.Value;
        }
    }



    /// <summary>
    /// Array type.
    /// </summary>
    public class LDayOfWeekType : LObjectType
    {
        /// <summary>
        /// Initialize.
        /// </summary>
        public LDayOfWeekType()
        {
            this.Name = "time";
            this.FullName = "sys.time";
            this.TypeVal = TypeConstants.Time;
        }


        /// <summary>
        /// Sets up the matrix of possible conversions from one type to another type.
        /// </summary>
        public override void SetupConversionMatrix()
        {
            this.AddConversionTo(TypeConstants.Array,     TypeConversionMode.NotSupported);
            this.AddConversionTo(TypeConstants.Bool,      TypeConversionMode.NotSupported);
            this.AddConversionTo(TypeConstants.Date,      TypeConversionMode.Supported);
            this.AddConversionTo(TypeConstants.Map,       TypeConversionMode.NotSupported);
            this.AddConversionTo(TypeConstants.Number,    TypeConversionMode.Supported);
            this.AddConversionTo(TypeConstants.Null,      TypeConversionMode.Supported);
            this.AddConversionTo(TypeConstants.String,    TypeConversionMode.Supported);
            this.AddConversionTo(TypeConstants.Time,      TypeConversionMode.SameType);
        }
    }
}
