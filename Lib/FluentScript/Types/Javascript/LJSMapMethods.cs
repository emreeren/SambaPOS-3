using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace ComLib.Lang.Types
{
    /// <summary>
    /// Array datatype
    /// </summary>
    public class LJSMapMethods : LTypeMethods
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public LJSMapMethods()
        {
            DataType = LTypes.Map;
            AddProperty(true, true,     "length",       "Length",           typeof(double),     "Sets or returns the number of elements in an array");
        }


        /// <summary>
        /// Whether or not the associted obj of this methods class has the supplied member.
        /// </summary>
        /// <param name="obj">The data obj to check for the member</param>
        /// <param name="memberName">The name of the member to check for.</param>
        /// <returns></returns>
        public override bool HasMember(LObject obj, string memberName)
        {
            return HasProperty(obj, memberName);
        }


        /// <summary>
        /// Whether or not the associted obj of this methods class has the supplied method.
        /// </summary>
        /// <param name="obj">The data obj to check for the method</param>
        /// <param name="methodName">The name of the method to check for.</param>
        /// <returns></returns>
        public override bool HasMethod(LObject obj, string methodName)
        {
            return HasProperty(obj, methodName);
        }


        /// <summary>
        /// Whether or not the associted obj of this methods class has the supplied property.
        /// </summary>
        /// <param name="target">The data obj to check for the property</param>
        /// <param name="propertyName">The name of the property</param>
        /// <returns></returns>
        public override bool HasProperty(LObject target, string propertyName)
        {
            var map = target.GetValue() as IDictionary;
            if (map == null) return false;

            return map.Contains(propertyName);
        }


        /// <summary>
        /// Gets the property value for the specified propertyname.
        /// </summary>
        /// <param name="target">The object containing the property</param>
        /// <param name="propName">The name of the property</param>
        /// <returns></returns>
        public override object GetProperty(LObject target, string propName)
        {
            var map = target.GetValue() as IDictionary;
            return map[propName];
        }


        /// <summary>
        /// Sets the property value for the specified propertyname.
        /// </summary>
        /// <param name="target">The object to set the property value on</param>
        /// <param name="propName">The name of the property</param>
        /// <param name="val">The value to set on the property</param>
        /// <returns></returns>
        public override void SetProperty(LObject target, string propName, object val)
        {
            var map = target.GetValue() as IDictionary;
            map[propName] = val;
        }


        #region Javascript API methods
        /// <summary>
        /// Lenght of the array.
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        public int Length(LObject target)
        {
            var map = target.GetValue() as IDictionary;
            return map.Count;
        }
        #endregion



        #region Helpers
        /// <summary>
        /// Get the value of a property.
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <param name="key">The name of the map key/property to get</param>
        /// <returns></returns>
        public T GetByStringMemberAs<T>(LObject target, string name)
        {
            object result = GetByStringMember(target, name);
            T returnVal = (T)result;
            return returnVal;
        }


        /// <summary>
        /// Get / set value by index.
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <param name="member">The name of the map key/property to get</param>
        /// <returns></returns>
        public override object GetByStringMember(LObject target, string member)
        {
            if (target == null ) return LObjects.Null;
            var map = target.GetValue() as IDictionary;
            if (map == null || map.Count == 0) return LObjects.Null;
            if (string.IsNullOrEmpty(member)) throw new IndexOutOfRangeException("Property does not exist : " + member);
            return map[member];
        }


        /// <summary>
        /// Get / set value by index.
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <param name="member">The name of the map key/property to set</param>
        /// <param name="value">The vlaue to set</param>
        /// <returns></returns>
        public override void SetByStringMember(LObject obj, string member, LObject val)
        {
            if (obj == null) return;
            var map = obj.GetValue() as IDictionary;
            if (map == null) return;
            if (string.IsNullOrEmpty(member)) throw new IndexOutOfRangeException("Property does not exist : " + member);
            map[member] = val;
        }
        #endregion
    }
}
