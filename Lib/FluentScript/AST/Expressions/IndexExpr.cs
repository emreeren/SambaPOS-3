using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Reflection;


namespace ComLib.Lang
{
    /// <summary>
    /// Member access expressions for "." property or "." method.
    /// </summary>
    public class IndexExpr : Expr
    {
       
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="variableExp">The variable expression to use instead of passing in name of variable.</param>
        /// <param name="indexExp">The expression representing the index value to get</param>
        /// <param name="isAssignment">Whether or not this is part of an assigment</param>
        public IndexExpr(Expr variableExp, Expr indexExp, bool isAssignment)
        {
            InitBoundary(true, "]");
            VariableExp = variableExp;
            IndexExp = indexExp;
            IsAssignment = isAssignment;
        }


        /// <summary>
        /// Expression representing the index
        /// </summary>
        public Expr IndexExp;


        /// <summary>
        /// The variable expression representing the list.
        /// </summary>
        public Expr VariableExp;


        /// <summary>
        /// The object to get the index value from. Used if ObjectName is null or empty.
        /// </summary>
        public object ListObject;


        /// <summary>
        /// Whether or not this member access is part of an assignment.
        /// </summary>
        public bool IsAssignment;


        /// <summary>
        /// Evaluate object[index]
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            object result = null;
            object ndxVal = IndexExp.Evaluate();
            
            // Either get from scope or from exp.
            if (VariableExp is VariableExpr)
                ListObject = Ctx.Memory.Get<object>(((VariableExpr)VariableExp).Name);
            else
                ListObject = VariableExp.Evaluate();

            if(!this.IsAssignment)
            {
                // Is the index value a number ? Indicates that the object is an array.
                if (ndxVal is int || ndxVal is double )
                {
                    result = GetArrayValue(Convert.ToInt32(ndxVal));
                    return result;    
                }
                // If the index is a string. Then object is a map/dictionary.
                else if (ndxVal is string)
                {
                    string memberName = ndxVal as string;
                    // Check if property exists.
                    if (!((LMap)ListObject).HasProperty(memberName))
                        throw this.BuildRunTimeException("Property does not exist : '" + memberName + "'");
                    return ((LMap)ListObject).ExecuteMethod(memberName, null);
                }       
            }
            if (ndxVal is int || ndxVal is double)
            {
                return new Tuple<object, int>(ListObject, Convert.ToInt32(ndxVal));
            }
            return new Tuple<LMap, string>((LMap)ListObject, (string)ndxVal);          
        }


        private object GetArrayValue(int ndx)
        {
            MethodInfo method = null;
            object result = null;           
            // 1. Array
            if (ListObject is Array)
            {
                method = ListObject.GetType().GetMethod("GetValue", new Type[] { typeof(int) });
            }
            // 2. LArray
            else if (ListObject is LArray)
            {
                method = ListObject.GetType().GetMethod("GetByIndex");
            }
            // 3. IList
            else
            {
                method = ListObject.GetType().GetMethod("get_Item");
            }
            // Getting value?                
            try
            {
                result = method.Invoke(ListObject, new object[] { ndx });
            }
            catch (Exception)
            {
                throw BuildRunTimeException("Access of list item at position " + ndx + " is out of range");
            }
            return result;
        }
    }    
}
