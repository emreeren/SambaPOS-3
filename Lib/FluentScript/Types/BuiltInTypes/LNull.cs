
using ComLib.Lang.Core;

namespace ComLib.Lang.Types
{
    /// <summary>
    /// Used to store a bool value.
    /// </summary>
    public class LNull : LObject
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public LNull()
        {
            this.Type = LTypes.Null;
        }
    }


    
    /// <summary>
    /// Class to represent null
    /// </summary>
    public class LNullType : LObjectType
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public LNullType()
        {
            this.Name = "null";
            this.FullName = "sys.null";
            this.TypeVal = TypeConstants.Null;
        }
    }
}
