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
            this.TypeVal = TypeConstants.Unit;
        }
    }
}
