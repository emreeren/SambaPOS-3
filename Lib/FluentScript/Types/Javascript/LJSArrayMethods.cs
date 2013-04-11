using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Fluentscript.Lib.Helpers;

namespace Fluentscript.Lib.Types.Javascript
{
    /// <summary>
    /// Array datatype
    /// </summary>
    public class LJSArrayMethods : LTypeMethods
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public LJSArrayMethods()
        {
            DataType = LTypes.Array;

            // Create the methods associated with this type.
            AddMethod("concat", 		"Concat", 		typeof(LArray),		"Joins two or more arrays, and returns a copy of the joined arrays" );
            AddMethod("indexOf", 		"IndexOf", 		typeof(double),		"Search the array for an element and returns it's position" );
            AddMethod("join", 			"Join", 		typeof(LArray),		"Joins all elements of an array into a string" );
            AddMethod("lastIndexOf", 	"LastIndexOf",	typeof(double),		"Search the array for an element, starting at the end, and returns it's position" );
            AddMethod("pop", 			"Pop", 			typeof(object),		"Removes the last element of an array, and returns that element" );
            AddMethod("push", 			"Push", 		typeof(double),		"Adds new elements to the end of an array, and returns the new length" );
            AddMethod("reverse", 		"Reverse", 		typeof(LArray),		"Reverses the order of the elements in an array" );
            AddMethod("shift", 		    "Shift", 		typeof(object),		"Removes the first element of an array, and returns that element" );
            AddMethod("slice", 		    "Slice", 		typeof(LArray),		"Selects a part of an array, and returns the new array" );
            AddMethod("sort", 			"Sort", 		typeof(LArray),		"Sorts the elements of an array" );
            AddMethod("splice", 		"Splice", 		typeof(LArray),		"Adds/Removes elements from an array" );
            AddMethod("toString", 		"ToString", 	typeof(string),		"Converts an array to a string, and returns the result" );
            AddMethod("unshift", 		"Unshift", 		typeof(double),		"Adds new elements to the beginning of an array, and returns the new length" );
            AddMethod("valueOf", 		"ValueOf", 		typeof(object),		"Returns the primitive value of an array" );
            AddProperty(true, true,     "length",       "Length",           typeof(double),     "Sets or returns the number of elements in an array");

            // Associate the arguments for each declared function.
            //          Method name,    Param name,    Type,     Required   Alias,  Default,    Example         Description
            AddArg("concat", 		"items",       "list",     true,      "",     null,       "'abc', 'def'", "The arrays to be joined");
            AddArg("indexOf", 		"item",        "object",   true,      "",     null,       "abc",          "The item to search for");
            AddArg("indexOf",       "start",        "int",      false,     "",     0,          "0 | 5",        "Where to start the search. Negative values will start at the given position counting from the end, and search to the end");
            AddArg("join",          "separator",    "string",   false,     "",     ",",        "abc",          "The separator to be used. If omitted, the elements are separated with a comma");
            AddArg("lastIndexOf",   "item",         "object",   true,      "",     null,       "abc",          "The item to search for");
            AddArg("lastIndexOf", 	"start",       "int",      false,     "",     0,          "0 | 4",        "Where to start the search. Negative values will start at the given position counting from the end, and search to the beginning");
            AddArg("push",          "items",        "params",     true,      "",     null,       "abc",          "The items(s) to add to the array");
            AddArg("slice",         "start",        "int",      true,      "",     null,       "0",            "An integer that specifies where to start the selection (The first element has an index of 0). Use negative numbers to select from the end of an array");
            AddArg("slice",         "end",          "int",      false,     "",     null,       "1",            "An integer that specifies where to end the selection. If omitted, all elements from the start position and to the end of the array will be selected. Use negative numbers to select from the end of an array");
            AddArg("sort",          "sortfunction", "function", false,     "",     "",         "",             "The function that defines the sort order");
            AddArg("splice",        "index",        "int",      true,      "",     null,       "1",            "An integer that specifies at what position to add/remove items, Use negative values to specify the position from the end of the array");
            AddArg("splice",        "howmany",      "int",      true,      "",     null,       "2",            "The number of items to be removed. If set to 0, no items will be removed");
            AddArg("splice",        "params",        "list",     false,     "",     null,       "2,3,4",        "The new item(s) to be added to the array");            
            AddArg("unshift", 		"items",        "list",     true,      "",     null,       "'abc', 'def'", "The item(s) to add to the beginning of the array");
        }


        /// <summary>
        /// Get / set value by index.
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <param name="index"></param>
        /// <returns></returns>
        public override object GetByNumericIndex(LObject target, int index)
        {
            if (target == null) return LObjects.Null;
            var list = target.GetValue() as IList;
            if (list == null || list.Count == 0) return LObjects.Null;

            if (index < 0 || index >= list.Count) throw new IndexOutOfRangeException("Index : " + index);
            return list[index];
        }


        /// <summary>
        /// Get / set value by index.
        /// </summary>
        /// <param name="target">The object whose index value is being set.</param>
        /// <param name="index">The index position to set the value</param>
        /// <param name="val">The value to set at the index</param>
        /// <returns></returns>
        public override void SetByNumericIndex(LObject target, int index, LObject val)
        {
            if (target == null) return;
            var list = target.GetValue() as IList;
            if (list == null || list.Count == 0) return;

            if (index < 0 || index >= list.Count) throw new IndexOutOfRangeException("Index : " + index);
            list[index] = val;
        }


        #region Javascript API methods
        /// <summary>
        /// Lenght of the array.
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        public int Length(LObject target)
        {
            var list = target.GetValue() as IList;
            return list.Count;
        }


        /// <summary>
        /// Joins two or more arrays, and returns a copy of the joined arrays
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <param name="arrays">Array of arrays to add</param>
        /// <returns>A copy of joined array</returns>
        public LObject Concat(LObject target, params object[] arrays)
        {
            if (arrays == null || arrays.Length == 0) return target;

            var list = target.GetValue() as IList;
            
            var copy = new List<object>();
            AddRange(copy, list);
            for (var ndx = 0; ndx < arrays.Length; ndx++)
            {
                object item = arrays[ndx];
                IList array = (IList)item;
                AddRange(copy, array);
            }
            return new LArray(copy);
        }


        /// <summary>
        /// Joins two or more arrays, and returns a copy of the joined arrays
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <param name="item">The item to search for</param>
        /// <param name="start">The starting position of  the search.</param>
        /// <returns>A copy of joined array</returns>
        public int IndexOf(LObject target, object item, int start)
        {
            var list = target.GetValue() as IList;
            
            var foundPos = -1;
            var total = list.Count;
            for(var ndx = start; ndx < total; ndx++)
            {
                var itemAt = list[ndx] as LObject;
                if (itemAt != null && itemAt.GetValue() == item)
                {
                    foundPos = ndx;
                    break;
                }
            }
            return foundPos;
        }


        /// <summary>
        /// Joins all elements of an array into a string
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <param name="separator">The separator to use for joining the elements.</param>
        /// <returns></returns>
        public string Join(LObject target, string separator = ",")
        {
            var list = target.GetValue() as IList;

            if (list == null || list.Count == 0) return string.Empty;

            var buffer = new StringBuilder();
            var total = list.Count;
            var lobj = list[0] as LObject;
            if (lobj != null)
                buffer.Append(lobj.GetValue());
            if (total > 1)
            {
                for (int ndx = 1; ndx < list.Count; ndx++)
                {
                    lobj = list[ndx] as LObject;
                    buffer.Append(separator + lobj.GetValue());
                }
            }
            string result = buffer.ToString();
            return result;
        }


        /// <summary>
        /// Joins two or more arrays, and returns a copy of the joined arrays
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <param name="item">The item to search for</param>
        /// <param name="start">The starting position of  the search.</param>
        /// <returns>A copy of joined array</returns>
        public int LastIndexOf(LObject target, object item, int start)
        {
            var list = target.GetValue() as IList;

            var foundPos = -1;
            var total = list.Count;
            for(var ndx = start; ndx < total; ndx++)
            {
                var itemAt = list[ndx] as LObject;
                if (itemAt != null && itemAt.GetValue() == item)
                {
                    foundPos = ndx;
                }
            }
            return foundPos;
        }


        /// <summary>
        /// Removes the last element of an array, and returns that element
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <returns>The removed element</returns>
        public object Pop(LObject target)
        {
            var list = target.GetValue() as IList;
            var index = list.Count - 1;
            object toRemove = list[index];
            list.RemoveAt(index);
            return toRemove;
        }


        /// <summary>
        /// Adds new elements to the end of an array, and returns the new length
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <param name="elements">The elements to add</param>
        /// <returns>The new length</returns>
        public int Push(LObject target, params object[] elements)
        {
            if (elements == null || elements.Length == 0) return 0;

            // Add
            var list = target.GetValue() as IList;
            if (list == null)
                return 0;

            foreach (object elem in elements)
            {
                if(list.GetType().IsGenericType)
                {
                    var gt = list.GetType().GetGenericArguments()[0];
                    if(gt != null && gt.FullName.StartsWith("ComLib.Lang"))
                    {
                        var langVal = LangTypeHelper.ConvertToLangValue(elem);
                        list.Add(langVal);
                    }
                    else
                    {
                        list.Add(elem);
                    }
                }
                else
                {
                    var langType = LangTypeHelper.ConvertToLangValue(elem);
                    list.Add(langType);
                }
                
            }

            return list.Count;
        }


        /// <summary>
        /// Reverses the order of the elements in an array
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <returns></returns>
        public LObject Reverse(LObject target)
        {
            var list = target.GetValue() as IList;
            int length = list.Count;
            if (length == 0 || length == 1) return null;

            // 2 or more.
            int highIndex = length - 1;
            int stopIndex = length / 2;
            if (length % 2 == 0)
                stopIndex--;
            for (int lowIndex = 0; lowIndex <= stopIndex; lowIndex++)
            {
                object tmp = list[lowIndex];
                list[lowIndex] = list[highIndex];
                list[highIndex] = tmp;
                highIndex--;
            }
            return target;
        }


        /// <summary>
        /// Removes the first element of an array, and returns that element
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <returns>The first element</returns>
        public object Shift(LObject target)
        {
            var list = target.GetValue() as IList;
            if (list.Count == 0) return null;
            object item = list[0];
            list.RemoveAt(0);
            return item;
        }


        /// <summary>
        /// Selects a part of an array, and returns the new array
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <param name="start">The start of the selection</param>
        /// <param name="end">The end of the selection, if not supplied, selects all elements from start to end of the array</param>
        /// <returns>A new array</returns>
        public LObject Slice(LObject target, int start, int end)
        {
            var list = target.GetValue() as IList;
            var items = new List<object>();
            if (end == -1)
                end = list.Count;
            for (var ndx = start; ndx < end; ndx++)
                items.Add(list[ndx]);
            return new LArray(items);
        }


        /// <summary>
        /// Adds/Removes elements from an array
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <param name="index">The index position to add/remove</param>
        /// <param name="howmany">How many elements to remove, if 0 no elements are removed</param>
        /// <param name="elements">Optional: The elements to add</param>
        /// <returns></returns>
        public LObject Splice(LObject target, int index, int howmany, params object[] elements)
        {
            var list = target.GetValue() as IList;
            List<object> removed = null;
            if (howmany > 0)
            {
                removed = new List<object>();
                for (int ndxRemove = index; ndxRemove < (index + howmany); ndxRemove++)
                {
                    removed.Add(list[ndxRemove]);
                }
                RemoveRange(list, index, howmany);
            }
            if (elements != null && elements.Length > 0 )
            {
                var lastIndex = index;
                for (var ndx = 0; ndx < elements.Length; ndx++)
                {
                    object objToAdd = elements[ndx];
                    list.Insert(lastIndex, objToAdd);
                    lastIndex++;
                }
            }
            return new LArray(removed);
        }
        

        /// <summary>
        /// Adds new elements to the beginning of an array, and returns the new length
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <param name="elements">The elements to add.</param>
        /// <returns>The new length</returns>
        public int UnShift(LObject target, params object[] elements)
        {
            var list = target.GetValue() as IList;
            if (list == null) return 0;
            if (elements == null || elements.Length == 0) return list.Count;
            for (var ndx = 0; ndx < elements.Length; ndx++)
            {
                object val = elements[ndx];
                list.Insert(0, val);
            }
            return list.Count;
        }
        #endregion



        #region Helpers
        /// <summary>
        /// Adds all the items from the send array into the first array(target)
        /// </summary>
        /// <param name="target"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        protected object AddRange(IList target, IList array)
        {
            if (array == null) return target;
            if (array.Count == 0) return target;
            foreach (var item in array)
                target.Add(item);

            return target;
        }


        /// <summary>
        /// Removes items from the array starting at the position supplied and removes the number of items supplied
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <param name="start">The starting index position in the list to remove items from.</param>
        /// <param name="howmany">How many items to remove</param>
        /// <returns></returns>
        protected object RemoveRange(IList target, int start, int howmany)
        {
            if (target == null) return LObjects.Null;
            if (target.Count == 0) return LObjects.Null;
            var totalRemoved = 0;
            while (totalRemoved < howmany)
            {
                target.RemoveAt(start);
                totalRemoved++;
            }

            return target;
        }
        #endregion
    }
}
