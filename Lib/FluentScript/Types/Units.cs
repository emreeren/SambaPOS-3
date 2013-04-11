
using System.Collections.Generic;

namespace Fluentscript.Lib.Types
{
    /// <summary>
    /// Helper class for calling functions
    /// </summary>
    public class Units
    {
        /// <summary>
        /// Unit to represent the base value and name.
        /// </summary>
        public class UnitGroup
        {
            /// <summary>
            /// Name of the group e.g. length;
            /// </summary>
            public string Name;

            /// <summary>
            /// Name of the basevalue. e.g. "inches" for "length"
            /// </summary>
            public string BaseName;


            /// <summary>
            /// Another name of the basevalue e.g. "inch" for "length"
            /// </summary>
            public string BaseName2;
        }



        /// <summary>
        /// Subgroup eg. "e.g" feet, yards of units length
        /// </summary>
        public class UnitSubGroup
        {
            /// <summary>
            /// Short name. e.g. "ft" for feet.
            /// </summary>
            public string ShortName;


            /// <summary>
            /// Name of the group
            /// </summary>
            public string Name;

            /// <summary>
            /// Name of the group.
            /// </summary>
            public string Group;


            /// <summary>
            /// Conversion value from basevalue.
            /// </summary>
            public double ConversionValue;
        }


        private Dictionary<string, UnitGroup> _groups;
        private Dictionary<string, UnitSubGroup> _subGroups;
        private Dictionary<string, string> _subGroupsToGroupMap;


        /// <summary>
        /// Initialize
        /// </summary>
        public Units()
        {
            _groups = new Dictionary<string, UnitGroup>();
            _subGroups = new Dictionary<string, UnitSubGroup>();
            _subGroupsToGroupMap = new Dictionary<string, string>();
        }


        /// <summary>
        /// Whether or not to enable units
        /// </summary>
        public bool IsEnabled { get; set; }


        /// <summary>
        /// Register all the units.
        /// </summary>
        public void RegisterAll()
        {
            // Length
            RegisterGroup("length", "in", "inches", "inch");
            RegisterUnit("length", "ft", "foot", 12);
            RegisterUnit("length", "ft", "feet", 12);
            RegisterUnit("length", "yd", "yard", 36);
            RegisterUnit("length", "yd", "yards", 36);
            RegisterUnit("length", "mi", "mile", 63360);
            RegisterUnit("length", "mi", "miles", 63360);

            // computer size
            RegisterGroup("computerspace", "B", "bytes", "byte");
            RegisterUnit("computerspace", "kb", "kilobyte", 12);
            RegisterUnit("computerspace", "kb", "kilobytes", 12);
            RegisterUnit("computerspace", "mb", "megabyte", 36);
            RegisterUnit("computerspace", "mb", "megabytes",  36);
            RegisterUnit("computerspace", "gig", "gigabyte", 63360);
            RegisterUnit("computerspace", "gigs", "gigabytes",  63360);

            // Weight
            RegisterGroup("weight", "oz", "ounces"    , "ounce");
            RegisterUnit("weight",  "lb", "pound"     ,  16);
            RegisterUnit("weight",  "lbs","pounds"    , 16);
            RegisterUnit("weight",  "tn", "ton"       ,  32000);
            RegisterUnit("weight",  "tn", "tons"      , 32000);
            RegisterUnit("weight",  "mg", "milligram" , .000352739);
            RegisterUnit("weight",  "mg", "milligrams", .000352739);
            RegisterUnit("weight",  "g",  "gram"      , .0352739);
            RegisterUnit("weight",  "g",  "grams"     , .0352739);
            RegisterUnit("weight",  "kg", "kilogram"  , 35.273962);
            RegisterUnit("weight",  "kg", "kilograms" , 35.273962);
            RegisterUnit("weight",  "t",  "tonne"     , 32000);
            RegisterUnit("weight",  "t",  "tonnes"    , 32000);

            // Volume
            RegisterGroup("volume", "tsp", "teaspoon", "teaspoons");
            RegisterUnit("volume",  "tbsp", "tablespoon", 3);
            RegisterUnit("volume",  "tbsp", "tablespoons", 3);
            RegisterUnit("volume",  "cup", "cup", 48);
            RegisterUnit("volume",  "cup", "cups", 48);
            RegisterUnit("volume", "pt", "pint", 96);
            RegisterUnit("volume", "pt", "pints", 96);
            RegisterUnit("volume", "qt", "quart", 192);
            RegisterUnit("volume", "qt", "quarts", 192);
            RegisterUnit("volume", "gal", "gallon", 768);
            RegisterUnit("volume", "gal", "gallons", 768);
        }


        /// <summary>
        /// Whether or not the units name supplied is valid.
        /// </summary>
        /// <param name="unitName"></param>
        /// <returns></returns>
        public bool Contains(string unitName)
        {
            if (_subGroupsToGroupMap.ContainsKey(unitName)) 
                return true;
            return false;
        }


        /// <summary>
        /// Register the base value always as 1 using the group, and names.
        /// </summary>
        /// <param name="group">e.g. the group of the units. e.g. length, volume, weight</param>
        /// <param name="abbreviation">abbreviation for the unit e.g "ft" for feet.</param>
        /// <param name="unitsName">The name of the units e.g if group = lenght, unitsName = feet</param>
        /// <param name="alias">Alias for the subgroup</param>
        public void RegisterGroup(string group, string abbreviation, string unitsName, string alias)
        {
            _groups[group] = new UnitGroup() { Name = group, BaseName = unitsName, BaseName2 = alias };
            _subGroupsToGroupMap[unitsName] = group;
            RegisterUnit(group, abbreviation, unitsName, 1);

            if (!string.IsNullOrEmpty(alias))
                RegisterUnit(group, abbreviation, alias, 1);
        }


        /// <summary>Registers the unit.</summary>
        /// <param name="group">e.g. "length"</param>
        /// <param name="abbreviation">"ft"</param>
        /// <param name="unitsName">"feet"</param>
        /// <param name="conversionValue">12 as in how many base units it takes to get 1 of this unit.</param>
        public void RegisterUnit(string group, string abbreviation, string unitsName, double conversionValue)
        {
            UnitSubGroup unit = unit = new UnitSubGroup() 
            {
                ShortName = abbreviation,
                Name = unitsName, 
                Group = group, 
                ConversionValue = conversionValue 
            };
            _subGroups[group + "_" + unitsName] = unit;
            _subGroups[group + "_" + abbreviation] = unit;
            _subGroupsToGroupMap[unitsName] = group;
            _subGroupsToGroupMap[abbreviation] = group;
        }

        
        /// <summary>
        /// Gets the basename "inches", "bytes" for groups "length", "computerspace" respectively.
        /// </summary>
        /// <param name="group"></param>
        public string GetBaseNameFor(string group)
        {
            if (!_groups.ContainsKey(group))
                return string.Empty;

            var g = _groups[group];
            return g.BaseName;
        }

        
        /// <summary>
        /// Gets conversion value for the group/unit combo, e.g 12 for "length, "feet" since 1 feet = 12 "inches"
        /// </summary>
        /// <param name="group">e.g. "length"</param>
        /// <param name="unitName">eg. "feet"</param>
        /// <returns></returns>
        public double ConversionValueFor(string group, string unitName)
        {
            var units = GetUnitsFor(group, unitName);
            if (units == null) return 0;

            return units.ConversionValue;
        }


        /// <summary>
        /// Convert the value from the source unit to the destination unit. eg. 5 feet to yards
        /// </summary>
        /// <param name="value">5</param>
        /// <param name="sourceName">"feet"</param>
        /// <param name="destinationName">"yards"</param>
        public double Convert(double value, string sourceName, string destinationName)
        {
            // Get length for "feet".
            var group = _subGroupsToGroupMap[sourceName];
            var sourceUnits = GetUnitsFor(group, sourceName);
            var destUnits = GetUnitsFor(group, destinationName);

            // e.g. Convert( 5, "feet", "yards" )
            // Convert 5 feet to inches.
            // Convert 60 inches to yards
                        
            // 1. Convert from source to base
            // e.g. 5 feet to 60 inches.
            double sourceBaseUnits = value * sourceUnits.ConversionValue;
            
            // 2. Convert from base to destination
            // e.g. 60 inches to yards
            double destinationUnits = ConvertToRelativeValue(sourceBaseUnits, destinationName, destUnits);
            return destinationUnits;
        }


        /// <summary>
        /// Converts a baseunits value into the relative value. e.g. 60inches, "feet" returns 5
        /// </summary>
        /// <param name="sourceBaseUnits"></param>
        /// <param name="unitsName"></param>
        /// <param name="destUnits"></param>
        /// <returns></returns>
        public double ConvertToRelativeValue(double sourceBaseUnits, string unitsName, UnitSubGroup destUnits)
        {
            if(destUnits == null)
            {
                var group = _subGroupsToGroupMap[unitsName];
                destUnits = GetUnitsFor(group, unitsName);
            }

            // 2. Convert from base to destination
            // e.g. 60 inches to yards
            double destinationUnits = sourceBaseUnits;

            // "feet" to "yards"
            // feet 12 > 36
            if (destUnits.ConversionValue > 1)
                destinationUnits = sourceBaseUnits / destUnits.ConversionValue;

            // "yards" to "feet"
            // 12 < 36
            else if (destUnits.ConversionValue < 1)
                destinationUnits = sourceBaseUnits * destUnits.ConversionValue;

            return destinationUnits;
        }


        /// <summary>
        /// Converts the inputs into a LUnit object e.g. 3, "yards"
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unitsName"></param>
        /// <returns></returns>
        public LUnit ConvertToUnits(double value, string unitsName)
        {
            // Get length for "feet".
            var group = _subGroupsToGroupMap[unitsName];
            var sourceUnits = GetUnitsFor(group, unitsName);

            // e.g. Convert( 5, "feet")
            // Convert 5 feet to inches.

            // 1. Convert from source to base
            // e.g. 5 feet to 60 inches.
            double sourceBaseUnits = value * sourceUnits.ConversionValue;
            var unit = new LUnit(value);
            unit.Group = group;
            unit.SubGroup = unitsName;
            unit.BaseValue = sourceBaseUnits;
            return unit;
        }


        /// <summary>
        /// Convert the value from the source unit to the destination unit.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="sourceName"></param>
        public double ConvertToBaseUnits(double value, string sourceName)
        {
            // Get length for "feet".
            var group = _subGroupsToGroupMap[sourceName];
            var sourceUnits = GetUnitsFor(group, sourceName);

            // e.g. Convert( 5, "feet", "yards" )
            // Convert 5 feet to inches.
            // Convert 60 inches to yards

            // 1. Convert from source to base
            // e.g. 5 feet to 60 inches.
            double sourceBaseUnits = value * sourceUnits.ConversionValue;
            return sourceBaseUnits;
        }


        /// <summary>
        /// Add units together.
        /// </summary>
        /// <param name="valueSource">Value of the source unit. e.g. 5</param>
        /// <param name="source">Name of the source unit. e.g. feet</param>
        /// <param name="valueDest">Value of the destination unit. e.g. 2</param>
        /// <param name="dest">Name of the destination unit. e.g. yards</param>
        /// <returns></returns>
        public double Add(double valueSource, string source, double valueDest, string dest)
        {
            // e.g. 3 feet + 2 yards
            // 1. Get 3 feet in base units ( inches )
            double sourceInBaseUnits = ConvertToBaseUnits(valueSource, source);

            // 2. Get 2 yards in base unites ( inches )
            double destinationInBaseUnits = ConvertToBaseUnits(valueDest, dest);

            // 3. Get total base units.
            double totalBaseUnits = sourceInBaseUnits + destinationInBaseUnits;

            // 4. Get the base name "inches" for source feet.
            // e.g. go from feet -> length -> inches. where "length" is the group.
            var group = _subGroupsToGroupMap[source];

            // 5. Now get the baseunit name "inches" for "length"
            var baseUnitName = _groups[group].BaseName;
            double result = Convert(totalBaseUnits, baseUnitName, source);
            return result;
        }


        private UnitSubGroup GetUnitsFor(string group, string unitName)
        {
            string key = group + "_" + unitName;
            if (!_subGroups.ContainsKey(key))
                return null;

            return _subGroups[key];
        }
    }
}
