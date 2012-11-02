using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang
{
    /// <summary>
    /// Datatype for map
    /// </summary>
    public class LMap : LBaseType
    {        

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val">Values of the map</param>
        public LMap(Dictionary<string, object> val) : this(null, val) { }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="context">Context for the script</param>
        /// <param name="val">Values of the map</param>
        public LMap(Context context, Dictionary<string, object> val)
        {
            _context = context;
            Raw = val;
        }


        /// <summary>
        /// Raw value
        /// </summary>
        public Dictionary<string, object> Raw;

        
        /// <summary>
        /// Whether or not this type supports the supplied method
        /// </summary>
        /// <param name="methodname"></param>
        /// <returns></returns>
        public override bool HasMethod(string methodname)
        {
            return Raw.ContainsKey(methodname);
        }


        /// <summary>
        /// Whether or not this type supports the supplied property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public override bool HasProperty(string propertyName)
        {
            if (propertyName == "length")
                return true;

            return Raw.ContainsKey(propertyName);
        }


        /// <summary>
        /// Calls the method
        /// </summary>
        /// <param name="methodname"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override object ExecuteMethod(string methodname, object[] args)
        {
            if (methodname == "length")
            {
                return this.Raw.Count;
            }

            object result = Raw[methodname];
            return result;
        }


        /// <summary>
        /// Get the value of a property.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetValue(string name)
        {
            object result = Raw[name];
            return result;
        }


        /// <summary>
        /// Get the value of a property.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetValueAs<T>(string name)
        {
            object result = Raw[name];
            T returnVal = (T)result;
            return returnVal;
        }


        /// <summary>
        /// Get the value of a property.
        /// </summary>
        /// <param name="name">The name of the property to set</param>
        /// <param name="value">The value to set</param>
        /// <returns></returns>
        public void SetValue(string name, object value)
        {
            Raw[name] = value;
        }


        /// <summary>
        /// Number of items.
        /// </summary>
        public int Length
        {
            get { return Raw.Count; }
        }
    }
}
