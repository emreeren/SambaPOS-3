﻿using System;

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
            this.Name = "dayofweek";
            this.FullName = "sys.dayofweek";
            this.TypeVal = TypeConstants.Time;
        }
    }
}
