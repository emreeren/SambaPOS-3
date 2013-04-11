using System;
using System.Collections;
using System.Collections.Generic;

namespace Fluentscript.Lib.Types.Javascript
{
    /// <summary>
    /// Array datatype
    /// </summary>
    public class LJSTableMethods : LTypeMethods
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public LJSTableMethods()
        {
            DataType = LTypes.Table;

            // Create the methods associated with this type.
            AddMethod("removeAt", 		"RemoveAt", 	typeof(object),		"Removes a record at the index supplied" );
            AddMethod("removeAll", 		"RemoveAll", 	typeof(object),		"Removes all the records" );
            AddMethod("removeFirst", 	"RemoveFirst", 	typeof(object),		"Removes the first record and returns it" );
            AddMethod("removeLast", 	"RemoveLast", 	typeof(object),		"Removes the last record and returns it" );
            AddMethodRaw("add", 			"Add", 		    typeof(double),		"Adds a record with the fields values that are supplied" );
            AddMethodRaw("push", 			"Push", 		typeof(double),		"Adds a record with the fields values that are supplied" );
            AddMethod("reverse", 		"Reverse", 		typeof(LTable),		"Reverses the order of the elements in an array" );
            AddMethod("toString", 		"ToString", 	typeof(string),		"Converts the table to a string, and returns the result" );
            AddMethod("valueOf", 		"ValueOf", 		typeof(object),		"Returns the primitive value of an array" );
            AddProperty(true, true,     "length",       "Length",           typeof(double),     "Sets or returns the number of elements in an array");
            AddProperty(true, true,     "total",        "Total",            typeof(double), "Sets or returns the number of elements in an array");
            AddProperty(true, true,     "first",        "First",            typeof(double),     "Sets or returns the number of elements in an array");
            AddProperty(true, true,     "last",         "Last",             typeof(double), "Sets or returns the number of elements in an array");

            // Associate the arguments for each declared function.
            //      Method name,    Param name,     Type,       Required   Alias,  Default,    Example         Description
            AddArg("add",           "items",        "params",   true,      "",     null,       "abc",          "The column field values to add to a new row");
            AddArg("push",          "items",        "params",   true,      "",     null,       "abc",          "The column field values to add to a new row");
            AddArg("removeAt",      "index",        "int",      true,      "",     null,       "3",            "The index number of the record to remove");
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


        #region FluentScript API methods
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
        /// Lenght of the array.
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        public int Total(LObject target)
        {
            var list = target.GetValue() as IList;
            return list.Count;
        }


        /// <summary>
        /// Lenght of the array.
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        public object First(LObject target)
        {
            var list = target.GetValue() as IList;
            if(list == null || list.Count == 0 ) 
                return LObjects.Null;

            return list[0];
        }


        /// <summary>
        /// Lenght of the array.
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        public object Last(LObject target)
        {
            var list = target.GetValue() as IList;
            if (list == null || list.Count == 0)
                return LObjects.Null;

            var last = list.Count - 1;
            return list[last];
        }


        /// <summary>
        /// Removes the last element of an array, and returns that element
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <returns>The removed element</returns>
        public object Pop(LObject target)
        {
            return this.RemoveLast(target);
        }


        /// <summary>
        /// Adds new elements to the end of an array, and returns the new length
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <param name="elements">The elements to add</param>
        /// <returns>The new length</returns>
        public int Add(LObject target, params object[] elements)
        {
            if (elements == null || elements.Length == 0) return 0;

            // Add
            var list = target.GetValue() as IList;
            if (list == null)
                return 0;

            var table = target as LTable;
            var map = new LMap(new Dictionary<string, object>());
            var ndx = 0;
            foreach(var field in table.Fields)
            {
                var val = elements[ndx];
                map.Value[field] = val;
                ndx++;
            }
            list.Add(map);
            return list.Count;
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
            foreach (var item in elements)
            {
                if (item == null)
                    list.Add(LObjects.Null);
                else
                    list.Add(item);
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
        /// Selects a part of an array, and returns the new array
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <param name="index">The index to remove</param>
        /// <returns>The object</returns>
        public LObject RemoveAt(LObject target, int index)
        {
            var list = target.GetValue() as IList;
            if (list == null || list.Count == 0)
                return LObjects.Null;

            var result = list[index];
            list.RemoveAt(index);
            return result as LObject;
        }


        /// <summary>
        /// Selects a part of an array, and returns the new array
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <returns>The object</returns>
        public LObject RemoveFirst(LObject target)
        {
            var list = target.GetValue() as IList;
            if (list == null || list.Count == 0)
                return LObjects.Null;

            var result = list[0];
            list.RemoveAt(0);
            return result as LObject;
        }


        /// <summary>
        /// Selects a part of an array, and returns the new array
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <returns>The object</returns>
        public LObject RemoveLast(LObject target)
        {
            var list = target.GetValue() as IList;
            if (list == null || list.Count == 0)
                return LObjects.Null;

            var last = list.Count - 1;
            var result = list[last];
            list.RemoveAt(last);
            return result as LObject;
        }


        /// <summary>
        /// Selects a part of an array, and returns the new array
        /// </summary>
        /// <param name="target">The target list to apply this method on.</param>
        /// <returns>The object</returns>
        public LObject RemoveAll(LObject target)
        {
            var list = target.GetValue() as IList;
            if (list == null || list.Count == 0)
                return LObjects.Null;

            list.Clear();
            return LObjects.Null;
        }
        #endregion
    }
}
