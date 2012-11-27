using System;

using ComLib.Lang.Core;

namespace ComLib.Lang.Types
{
    /// <summary>
    /// Units class for lenght, weight, temperatur, currency etc.
    /// </summary>
    public class LUnit : LObject
    {
        /// <summary>
        /// Initialize.
        /// </summary>
        public LUnit()
        {
            //this.Type = LTypes.Unit;
        }


        /// <summary>
        /// The value of the unit in base terms e.g. 1 inch is the base for length etc.
        /// </summary>
        public double BaseValue { get; set; }


        /// <summary>
        /// The value of the unit in terms of relative to base. e.g value = 2 feet , base value = 24 ( inches )
        /// </summary>
        public double Value { get; set; }


        /// <summary>
        /// The name of the unit eg. length, temperature, currency
        /// </summary>
        public string Group { get; set; }


        /// <summary>
        /// The type of length e.g. feet, inches, yards, miles
        /// </summary>
        public string SubGroup { get; set; }


        /// <summary>
        /// Add 2 unites together.
        /// </summary>
        /// <param name="u1"></param>
        /// <param name="u2"></param>
        /// <returns></returns>
        public static LUnit operator +(LUnit u1, LUnit u2)
        {
            // Validate
            Validate(u1, u2);

            // Now convert the values to their base value.
            double totalBase = u1.BaseValue + u2.BaseValue;
            var result = new LUnit() { BaseValue = totalBase, Value = totalBase, Group = u1.Group, SubGroup = u1.SubGroup };

            // Set the value to the converted relative value 
            // e.g. if u1.subgroup = feet and u2.subgroup == inches.
            // then multiply result.basevalue by the conversion value.
            // Now convert the units
            return result;
        }


        private static void Validate(LUnit u1, LUnit u2)
        {
            // Validate.
            if (u1 == null || u2 == null)
                throw new ArgumentException("Can not add units when 1 unit is empty");

            // Check for matching groups e.g. length + length or weight + weight.
            if (u1.Group != u2.Group)
                throw new ArgumentException("Can not add units " + u1.Group + " to " + u2.Group);
        }
    }


    /// <summary>
    /// Array type.
    /// </summary>
    public class LUnitType : LObjectType
    {
        /// <summary>
        /// Initialize.
        /// </summary>
        public LUnitType()
        {
            this.Name = "unit";
            this.FullName = "sys.unit";
            this.TypeVal = TypeConstants.Time;
        }


        /// <summary>
        /// Sets up the matrix of possible conversions from one type to another type.
        /// </summary>
        public override void SetupConversionMatrix()
        {
            this.AddConversionTo(TypeConstants.Array, TypeConversionMode.NotSupported);
            this.AddConversionTo(TypeConstants.Bool, TypeConversionMode.NotSupported);
            this.AddConversionTo(TypeConstants.Date, TypeConversionMode.Supported);
            this.AddConversionTo(TypeConstants.Map, TypeConversionMode.NotSupported);
            this.AddConversionTo(TypeConstants.Number, TypeConversionMode.Supported);
            this.AddConversionTo(TypeConstants.Null, TypeConversionMode.Supported);
            this.AddConversionTo(TypeConstants.String, TypeConversionMode.Supported);
            this.AddConversionTo(TypeConstants.Time, TypeConversionMode.SameType);
        }
    }
}
