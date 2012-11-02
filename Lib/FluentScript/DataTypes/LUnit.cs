using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang
{
    /// <summary>
    /// Units class for lenght, weight, temperatur, currency etc.
    /// </summary>
    public class LUnit
    {
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
}
