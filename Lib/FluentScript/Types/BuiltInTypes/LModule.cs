using System;

using ComLib.Lang.Core;

namespace ComLib.Lang.Types
{
    /// <summary>
    /// Used to store a bool value.
    /// </summary>
    public class LModule : LObject
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val">The type of the external c# class.</param>
        public LModule(LModuleType type)
        {
            this.Value = null;
            this.Type = type;
        }


        /// <summary>
        /// The raw type value.
        /// </summary>
        public object Value;


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
    public class LModuleType : LObjectType
    {
        /// <summary>
        /// Initialize.
        /// </summary>
        public LModuleType(string name, string fullname)
        {
            // To be determined during parsing phase.
            this.Name = name;
            this.FullName = fullname;
            this.TypeVal = TypeConstants.Module;
        }
    }
}
