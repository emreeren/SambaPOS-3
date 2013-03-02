using ComLib.Lang.Core;

namespace ComLib.Lang.Types
{
    /// <summary>
    /// Used to store a bool value.
    /// </summary>
    public class LBool : LObject
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val"></param>
        public LBool(bool val)
        {
            this.Value = val;
            this.Type = LTypes.Bool;
        }


        /// <summary>
        /// The raw type value.
        /// </summary>
        public bool Value;


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
            return new LBool(this.Value);
        }
    }



    /// <summary>
    /// Boolean datatype.
    /// </summary>
    public class LBoolType : LObjectType
    {
        /// <summary>
        /// Initialize bool value.
        /// </summary>
        public LBoolType()
        {
            this.Name = "bool";
            this.FullName = "sys.bool";
            this.TypeVal = TypeConstants.Bool;
            this.IsSystemType = true;
        }
    }
}
