
using System.Collections.Generic;
using ComLib.Lang.Core;


namespace ComLib.Lang.Types
{
    /// <summary>
    /// Used to store a array
    /// </summary>
    public class LMap : LObject
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val"></param>
        public LMap(IDictionary<string, object> val)
        {
            this.Value = val;
            this.Type = LTypes.Map;
        }


        /// <summary>
        /// The raw type value.
        /// </summary>
        public IDictionary<string, object> Value;


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
    /// Datatype for map
    /// </summary>
    public class LMapType : LObjectType
    {        

        /// <summary>
        /// Initialize
        /// </summary>
        public LMapType()
        {
            this.Name = "map";
            this.FullName = "sys.map";
            this.TypeVal = TypeConstants.Map;
            this.IsSystemType = true;
            // Dictionary<string, object>
        }
    }
}
