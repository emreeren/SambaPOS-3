


using ComLib.Lang.Core;

namespace ComLib.Lang.Types
{
    /// <summary>
    /// Used to store a string value.
    /// </summary>
    public class LString : LObject
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val"></param>
        public LString(string val)
        {
            this.Value = val;
            this.Type = LTypes.String;
        }


        /// <summary>
        /// The raw type value.
        /// </summary>
        public string Value;


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
            return new LString(this.Value);
        }
    }



    /// <summary>
    /// Boolean datatype.
    /// </summary>
    public class LStringType : LObjectType
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public LStringType()
        {
            this.Name = "string";
            this.FullName = "sys.string";
            this.TypeVal = TypeConstants.String;
            this.IsSystemType = true;
        }
    }
}
