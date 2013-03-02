using System;

using ComLib.Lang.Core;

namespace ComLib.Lang.Types
{
    /// <summary>
    /// Used to store a bool value.
    /// </summary>
    public class LDate : LObject
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val"></param>
        public LDate(DateTime val)
        {
            this.Value = val;
            this.Type = LTypes.Date;
        }


        /// <summary>
        /// The raw type value.
        /// </summary>
        public DateTime Value;


        /// <summary>
        /// Gets the value of this object.
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            return this.Value;
        }


        /// <summary>
        /// Clones this value.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new LDate(this.Value);
        }
    }



    /// <summary>
    /// Array type.
    /// </summary>
    public class LDateType : LObjectType
    {
        /// <summary>
        /// Initialize with date.
        /// </summary>
        public LDateType()
        {
            this.Name = "datetime";
            this.FullName = "sys.datetime";
            this.TypeVal = TypeConstants.Date;
            this.IsSystemType = true;
        }
    }
}
