using System;
using System.Collections;
using System.Collections.Generic;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Types
{
    /// ------------------------------------------------------------------------------------------------
    /// remarks: This file is auto-generated from the FSGrammar specification and should not be modified.
    /// summary: This file contains all the AST for expressions at the system level.
    ///			features like control-flow e..g if, while, for, try, break, continue, return etc.
    /// version: 0.9.8.10
    /// author:  kishore reddy
    /// date:	02/14/13 10:03:37 AM
    /// ------------------------------------------------------------------------------------------------

    /// <summary>Datatype for array</summary>
    public class LArray : LObject
    {
        public LArray(IList val)
        {
            this.Value = val;
            this.Type = LTypes.Array;
        }


        public IList Value;


        public override object GetValue()
        {
            return this.Value;
        }


        public override object Clone()
        {
            return new LArray(this.Value);
        }


    }

    public class LArrayType : LObjectType
    {
        public LArrayType()
        {
            this.Name = "array";
            this.FullName = "sys.array";
            this.TypeVal = TypeConstants.Array;
            this.IsSystemType = true;
        }

    }


    /// <summary>Datatype for bool</summary>
    public class LBool : LObject
    {
        public LBool(bool val)
        {
            this.Value = val;
            this.Type = LTypes.Bool;
        }


        public bool Value;


        public override object GetValue()
        {
            return this.Value;
        }


        public override object Clone()
        {
            return new LBool(this.Value);
        }


    }

    public class LBoolType : LObjectType
    {
        public LBoolType()
        {
            this.Name = "bool";
            this.FullName = "sys.bool";
            this.TypeVal = TypeConstants.Bool;
            this.IsSystemType = true;
        }

    }


    /// <summary>Datatype for class</summary>
    public class LClass : LObject
    {
        public LClass(object val)
        {
            this.Value = val;
            this.Type = LTypes.Class;
        }


        public object Value;


        public override object GetValue()
        {
            return this.Value;
        }


        public override object Clone()
        {
            return new LClass(this.Value);
        }


    }

    public class LClassType : LObjectType
    {
        public LClassType()
        {
            this.Name = "class";
            this.FullName = "ext.class";
            this.TypeVal = TypeConstants.LClass;
            this.IsSystemType = true;
        }

        public Type DataType;

    }


    /// <summary>Datatype for datetime</summary>
    public class LDate : LObject
    {
        public LDate(DateTime val)
        {
            this.Value = val;
            this.Type = LTypes.Date;
        }


        public DateTime Value;


        public override object GetValue()
        {
            return this.Value;
        }


        public override object Clone()
        {
            return new LDate(this.Value);
        }


    }

    public class LDateType : LObjectType
    {
        public LDateType()
        {
            this.Name = "datetime";
            this.FullName = "sys.datetime";
            this.TypeVal = TypeConstants.Date;
            this.IsSystemType = true;
        }

    }


    /// <summary>Datatype for dayofweek</summary>
    public class LDayOfWeek : LObject
    {
        public LDayOfWeek(DayOfWeek val)
        {
            this.Value = val;
            this.Type = LTypes.DayOfWeek;
        }


        public DayOfWeek Value;


        public override object GetValue()
        {
            return this.Value;
        }


        public override object Clone()
        {
            return new LDayOfWeek(this.Value);
        }


    }

    public class LDayOfWeekType : LObjectType
    {
        public LDayOfWeekType()
        {
            this.Name = "dayofweek";
            this.FullName = "sys.dayofweek";
            this.TypeVal = TypeConstants.DayOfWeek;
            this.IsSystemType = true;
        }

    }


    /// <summary>Datatype for function</summary>
    public class LFunction : LObject
    {
        public LFunction(object val)
        {
            this.Value = val;
            this.Type = LTypes.Function;
        }


        public object Value;


        public override object GetValue()
        {
            return this.Value;
        }


        public override object Clone()
        {
            return new LFunction(this.Value);
        }


    }

    public class LFunctionType : LObjectType
    {
        public LFunctionType()
        {
            this.Name = "function";
            this.FullName = "ext.function";
            this.TypeVal = TypeConstants.Function;
            this.IsSystemType = true;
        }

        public LType Parent;

    }


    /// <summary>Datatype for map</summary>
    public class LMap : LObject
    {
        public LMap(IDictionary<string, object> val)
        {
            this.Value = val;
            this.Type = LTypes.Map;
        }


        public IDictionary<string, object> Value;


        public override object GetValue()
        {
            return this.Value;
        }


        public override object Clone()
        {
            return new LMap(this.Value);
        }


    }

    public class LMapType : LObjectType
    {
        public LMapType()
        {
            this.Name = "map";
            this.FullName = "sys.map";
            this.TypeVal = TypeConstants.Map;
            this.IsSystemType = true;
        }

    }


    /// <summary>Datatype for module</summary>
    public class LModule : LObject
    {
        public LModule(object val)
        {
            this.Value = val;
            this.Type = LTypes.Module;
        }


        public object Value;


        public override object GetValue()
        {
            return this.Value;
        }


        public override object Clone()
        {
            return new LModule(this.Value);
        }


    }

    public class LModuleType : LObjectType
    {
        public LModuleType()
        {
            this.Name = "module";
            this.FullName = "ext.module";
            this.TypeVal = TypeConstants.Module;
            this.IsSystemType = true;
        }

    }


    /// <summary>Datatype for null</summary>
    public class LNull : LObject
    {
        public LNull(object val)
        {
            this.Value = val;
            this.Type = LTypes.Null;
        }


        public object Value;


        public override object GetValue()
        {
            return this.Value;
        }


        public override object Clone()
        {
            return new LNull(this.Value);
        }


    }

    public class LNullType : LObjectType
    {
        public LNullType()
        {
            this.Name = "null";
            this.FullName = "sys.null";
            this.TypeVal = TypeConstants.Null;
            this.IsSystemType = true;
        }

    }


    /// <summary>Datatype for number</summary>
    public class LNumber : LObject
    {
        public LNumber(double val)
        {
            this.Value = val;
            this.Type = LTypes.Number;
        }


        public double Value;


        public override object GetValue()
        {
            return this.Value;
        }


        public override object Clone()
        {
            return new LNumber(this.Value);
        }


    }

    public class LNumberType : LObjectType
    {
        public LNumberType()
        {
            this.Name = "number";
            this.FullName = "sys.number";
            this.TypeVal = TypeConstants.Number;
            this.IsSystemType = true;
        }

    }


    /// <summary>Datatype for string</summary>
    public class LString : LObject
    {
        public LString(string val)
        {
            this.Value = val;
            this.Type = LTypes.String;
        }


        public string Value;


        public override object GetValue()
        {
            return this.Value;
        }


        public override object Clone()
        {
            return new LString(this.Value);
        }


    }

    public class LStringType : LObjectType
    {
        public LStringType()
        {
            this.Name = "string";
            this.FullName = "sys.string";
            this.TypeVal = TypeConstants.String;
            this.IsSystemType = true;
        }

    }


    /// <summary>Datatype for table</summary>
    public class LTable : LObject
    {
        public LTable(IList val)
        {
            this.Value = val;
            this.Type = LTypes.Table;
        }


        public IList Value;


        public override object GetValue()
        {
            return this.Value;
        }


        public override object Clone()
        {
            return new LTable(this.Value);
        }


        public List<string> Fields;

    }

    public class LTableType : LObjectType
    {
        public LTableType()
        {
            this.Name = "table";
            this.FullName = "sys.table";
            this.TypeVal = TypeConstants.Table;
            this.IsSystemType = true;
        }

    }


    /// <summary>Datatype for time</summary>
    public class LTime : LObject
    {
        public LTime(TimeSpan val)
        {
            this.Value = val;
            this.Type = LTypes.Time;
        }


        public TimeSpan Value;


        public override object GetValue()
        {
            return this.Value;
        }


        public override object Clone()
        {
            return new LTime(this.Value);
        }


    }

    public class LTimeType : LObjectType
    {
        public LTimeType()
        {
            this.Name = "time";
            this.FullName = "sys.time";
            this.TypeVal = TypeConstants.Time;
            this.IsSystemType = true;
        }

    }


    /// <summary>Datatype for unit</summary>
    public class LUnit : LObject
    {
        public LUnit(double val)
        {
            this.Value = val;
            this.Type = LTypes.Unit;
        }


        public double Value;


        public override object GetValue()
        {
            return this.Value;
        }


        public override object Clone()
        {
            return new LUnit(this.Value);
        }


        public double BaseValue { get; set; }

        public string Group { get; set; }

        public string SubGroup { get; set; }

    }

    public class LUnitType : LObjectType
    {
        public LUnitType()
        {
            this.Name = "unit";
            this.FullName = "sys.unit";
            this.TypeVal = TypeConstants.Unit;
            this.IsSystemType = true;
        }

    }


    public class LTypes
    {


        /// Single instance of the Array type
        public static LObjectType Array = new LArrayType();


        /// Single instance of the Bool type
        public static LObjectType Bool = new LBoolType();


        /// Single instance of the Class type
        public static LObjectType Class = new LClassType();


        /// Single instance of the Date type
        public static LObjectType Date = new LDateType();


        /// Single instance of the DayOfWeek type
        public static LObjectType DayOfWeek = new LDayOfWeekType();


        /// Single instance of the Function type
        public static LObjectType Function = new LFunctionType();


        /// Single instance of the Map type
        public static LObjectType Map = new LMapType();


        /// Single instance of the Module type
        public static LObjectType Module = new LModuleType();


        /// Single instance of the Null type
        public static LObjectType Null = new LNullType();


        /// Single instance of the Number type
        public static LObjectType Number = new LNumberType();


        /// Single instance of the Object type
        public static LObjectType Object = new LObjectType();


        /// Single instance of the String type
        public static LObjectType String = new LStringType();


        /// Single instance of the Table type
        public static LObjectType Table = new LTableType();


        /// Single instance of the Time type
        public static LObjectType Time = new LTimeType();


        /// Single instance of the Unit type
        public static LObjectType Unit = new LUnitType();


    }
}