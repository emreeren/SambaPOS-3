using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ComLib.Lang.Helpers;

namespace ComLib.Lang
{
    /// <summary>
    /// Array datatype
    /// </summary>
    public class LArray : LBaseType
    {
        private static Dictionary<string, Func<LArray, ArgsFetcher, object>> _methods;
        private static Dictionary<string, string> _methodMap;


        /// <summary>
        /// Initialize
        /// </summary>
        static LArray()
        {
            _methods = new Dictionary<string, Func<LArray, ArgsFetcher, object>>();
            _methods["concat"] = (lst, fetcher) => lst.Concat(fetcher);
            _methods["indexOf"] = (lst, fetcher) => lst.Push(fetcher);
            _methods["join"] = (lst, fetcher) => lst.Join();
            _methods["pop"] = (lst, fetcher) => lst.Pop();
            _methods["push"] = (lst, fetcher) => lst.Push(fetcher);
            _methods["reverse"] = (lst, fetcher) => lst.Reverse();
            _methods["shift"] = (lst, fetcher) => lst.Shift();
            _methods["slice"] = (lst, fetcher) => lst.Slice(0, 2);
            _methods["sort"] = (lst, fetcher) => lst.Push(fetcher);
            _methods["splice"] = (lst, fetcher) => lst.Splice(0, 2);
            _methods["toString"] = (lst, fetcher) => lst.Push(fetcher);
            _methods["unshift"] = (lst, fetcher) => lst.UnShift(fetcher);
            _methods["valueOf"] = (lst, fetcher) => lst.Push(fetcher);

            _methodMap = new Dictionary<string, string>();
            _methodMap["concat"]  = "Concat"; 
            _methodMap["indexof"] = "IndexOf";
            _methodMap["join"]    = "Join";   
            _methodMap["pop"]     = "Pop";    
            _methodMap["push"]    = "Push";   
            _methodMap["reverse"] = "Reverse";
            _methodMap["shift"]   = "Shift";  
            _methodMap["slice"]   = "Slice";  
            _methodMap["sort"]    = "Sort";   
            _methodMap["splice"]  = "Splice";
            _methodMap["tostring"]= "ToString";
            _methodMap["unshift"] = "Unshift";
            _methodMap["valueOf"] = "ValueOf";
            _methodMap["length"]  = "Length";
        }


        /// <summary>
        /// Map method name from an alias to actual method
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string MapMethod(string source)
        {
            return _methodMap[source.ToLower()];
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="val">Value of the string</param>
        public LArray(List<object> val) : this(null, val) { }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="context">Context for the script</param>
        /// <param name="val">Value of the string</param>
        public LArray(Context context, List<object> val)
        {
            _context = context;
            Raw = val;
            _supportsIndexer = true;
        }


        /// <summary>
        /// Raw value
        /// </summary>
        public List<object> Raw;



        /// <summary>
        /// Get string value.
        /// </summary>
        /// <returns></returns>
        public string ToStr()
        {
            return Raw.ToString();
        }


        /// <summary>
        /// Whether or not this type supports the supplied method
        /// </summary>
        /// <param name="methodname"></param>
        /// <returns></returns>
        public override bool HasMethod(string methodname)
        {
            return _methods.ContainsKey(methodname);
        }


        /// <summary>
        /// Whether or not this type supports the supplied property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public override bool HasProperty(string propertyName)
        {
            return string.Compare(propertyName, "length", StringComparison.InvariantCultureIgnoreCase) == 0;
        }


        /// <summary>
        /// Calls the method
        /// </summary>
        /// <param name="methodname"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override object ExecuteMethod(string methodname, object[] args)
        {
            ArgsFetcher fetcher = new ArgsFetcher(args);
            if (methodname == "length")
            {
                return this.Raw.Count;
            }

            object result = _methods[methodname](this, fetcher);
            return result;
        }


        #region Javascript API methods
        /// <summary>
        /// Lenght of the array.
        /// </summary>
        public int Length
        {
            get { return Raw.Count; }
        }


        /// <summary>
        /// Get / set value by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public override object this[int index]
        {
            get
            {
                if (Raw == null || Raw.Count == 0) return null;
                if (index < 0 || index >= Raw.Count) throw new IndexOutOfRangeException("Index : " + index);
                return Raw[index];
            }
            set
            {
                if (Raw == null || Raw.Count == 0) return;
                if (index < 0 || index >= Raw.Count) throw new IndexOutOfRangeException("Index : " + index); 
                base[index] = value;
            }
        }


        /// <summary>
        /// Joins two or more arrays, and returns a copy of the joined arrays
        /// </summary>
        /// <param name="arrays">Array of arrays to add</param>
        /// <returns>A copy of joined array</returns>
        public object Concat(params object[] arrays)
        {
            if (arrays == null || arrays.Length == 0) return Raw;

            for (int ndx = 0; ndx < arrays.Length; ndx++)
            {
                object item = arrays[ndx];
                LArray array = (item is LArray) ? (LArray)item : ConvertToLArray(item);
                Raw.AddRange(array.Raw);
            }
            return this;
        }


        /// <summary>
        /// Joins all elements of an array into a string
        /// </summary>
        /// <param name="separator">The separator to use for joining the elements.</param>
        /// <returns></returns>
        public object Join(string separator = ",")
        {
            if (Raw == null || Raw.Count == 0) return string.Empty;

            var buffer = new StringBuilder();

            buffer.Append(Raw[0].ToString());
            if (Raw.Count > 1)
            {
                for (int ndx = 1; ndx < Raw.Count; ndx++)
                {
                    buffer.Append(separator + Raw[ndx].ToString());
                }
            }
            string result = buffer.ToString();
            return result;
        }


        /// <summary>
        /// Removes the last element of an array, and returns that element
        /// </summary>
        /// <returns>The removed element</returns>
        public object Pop()
        {
            object toRemove = Raw[Raw.Count - 1];
            Raw.RemoveAt(Raw.Count - 1);
            return toRemove;
        }


        /// <summary>
        /// Adds new elements to the end of an array, and returns the new length
        /// </summary>
        /// <param name="elements">The elements to add</param>
        /// <returns>The new length</returns>
        public object Push(params object[] elements)
        {
            if (elements == null || elements.Length == 0) return null;

            // Add
            foreach (object elem in elements)
                Raw.Add(elem);

            return Raw.Count;
        }


        /// <summary>
        /// Reverses the order of the elements in an array
        /// </summary>
        /// <returns></returns>
        public object Reverse()
        {
            int length = Raw.Count;
            if (length == 0 || length == 1) return null;

            // 2 or more.
            int highIndex = length - 1;
            int stopIndex = length / 2;
            if (length % 2 == 0)
                stopIndex--;
            for (int lowIndex = 0; lowIndex <= stopIndex; lowIndex++)
            {
                object tmp = Raw[lowIndex];
                Raw[lowIndex] = Raw[highIndex];
                Raw[highIndex] = tmp;
                highIndex--;
            }
            return this;
        }


        /// <summary>
        /// Removes the first element of an array, and returns that element
        /// </summary>
        /// <returns>The first element</returns>
        public object Shift()
        {
            if (Raw.Count == 0) return null;
            object item = Raw[0];
            Raw.RemoveAt(0);
            return item;
        }


        /// <summary>
        /// Selects a part of an array, and returns the new array
        /// </summary>
        /// <param name="start">The start of the selection</param>
        /// <param name="end">The end of the selection, if not supplied, selects all elements from start to end of the array</param>
        /// <returns>A new array</returns>
        public object Slice(int start, int end)
        {
            List<object> items = Raw.GetRange(start, end);
            return new LArray(null, items);
        }


        /// <summary>
        /// Adds/Removes elements from an array
        /// </summary>
        /// <param name="index">The index position to add/remove</param>
        /// <param name="howmany">How many elements to remove, if 0 no elements are removed</param>
        /// <param name="elements">Optional: The elements to add</param>
        /// <returns></returns>
        public object Splice(int index, int howmany, params object[] elements)
        {
            List<object> removed = null;
            if (howmany > 0)
            {
                removed = new List<object>();
                for (int ndxRemove = index; ndxRemove < (index + howmany); ndxRemove++)
                {
                    removed.Add(Raw[ndxRemove]);
                }
                Raw.RemoveRange(index, howmany);
            }
            if (elements != null && elements.Length > 0 )
            {
                int lastIndex = index;
                for (int ndx = 0; ndx < elements.Length; ndx++)
                {
                    object objToAdd = elements[ndx];
                    Raw.Insert(lastIndex, objToAdd);
                    lastIndex++;
                }
            }
            return new LArray(removed);
        }
        

        /// <summary>
        /// Adds new elements to the beginning of an array, and returns the new length
        /// </summary>
        /// <param name="elements">The elements to add.</param>
        /// <returns>The new length</returns>
        public object UnShift(params object[] elements)
        {
            if (elements == null || elements.Length == 0) return null;
            for (int ndx = 0; ndx < elements.Length; ndx++)
            {
                object val = elements[ndx];
                Raw.Insert(0, val);
            }
            return Raw.Count;
        }


        /// <summary>
        /// Gets the element at the specified position
        /// </summary>
        /// <param name="ndx">The index position to get</param>
        /// <returns>Element at position</returns>
        public object GetByIndex(int ndx)
        {
            if (ndx < 0 || ndx >= Raw.Count)
                throw new IndexOutOfRangeException("Index out of bounds for : " + _varName + ", " + ndx);

            // TO_DO:
            return Raw[ndx];
        }


        /// <summary>
        /// Sets the element at the specified position
        /// </summary>
        /// <param name="ndx">The index position to set</param>
        /// <param name="val">The value to set</param>
        /// <returns>The object being set</returns>
        public object SetByIndex(int ndx, object val)
        {
            Raw[ndx] = val;
            return val;
        }


        /// <summary>
        /// Converts an array into a string and returns the string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Join(",").ToString();
        }
        #endregion


        private LArray ConvertToLArray(object list)
        {
            return list as LArray;
        }   

    }
}
