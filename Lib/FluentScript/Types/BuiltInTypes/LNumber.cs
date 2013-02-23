

using ComLib.Lang.Core;

namespace ComLib.Lang.Types
{
    /// <summary>
    /// Used to store a number value.
    /// </summary>
    public class LNumber : LObject
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val"></param>
        public LNumber(double val)
        {
            this.Value = val;
            this.Type = LTypes.Number;
        }


        /// <summary>
        /// The raw type value.
        /// </summary>
        public double Value;


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
            return new LNumber(this.Value);
        }
    }



    /// <summary>
    /// Boolean datatype.
    /// </summary>
    public class LNumberType : LObjectType
    {   
        /// <summary>
        /// Initialize
        /// </summary>
        public LNumberType()
        {
            this.Name = "number";
            this.FullName = "sys.number";
            this.TypeVal = TypeConstants.Number;
            this.IsSystemType = true;
        }
    }
}
