using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Reflection;

using ComLib.Lang.Helpers;

namespace ComLib.Lang
{        
    /// <summary>
    /// Function call expression data.
    /// </summary>
    public class FunctionCallExpr : Expr, IParameterExpression
    {
        /// <summary>
        /// Function call expression
        /// </summary>
        public FunctionCallExpr()
        {
            InitBoundary(true, ")");
            ParamList = new List<object>();
            ParamListExpressions = new List<Expr>();
        } 


        /// <summary>
        /// Expression represnting the name of the function call.
        /// </summary>
        public Expr NameExp;


        /// <summary>
        /// List of expressions.
        /// </summary>
        public List<Expr> ParamListExpressions { get; set; }


        /// <summary>
        /// List of arguments.
        /// </summary>
        public List<object> ParamList { get; set; }


        /// <summary>
        /// Arguments to the function.
        /// </summary>
        public IDictionary ParamMap;


        /// <summary>
        /// Whether or not this is a method call or a member access.
        /// </summary>
        public bool IsScopeVariable { get { return _isScopeVariable; } set { _isScopeVariable = value; } }


        /// <summary>
        /// Evauate and run the function
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            object result = null;
            bool isNameEmpty = string.IsNullOrEmpty(_name);
            
            // CASE 1: Exp is variable -> internal/external script. "getuser()".            
            if (NameExp is VariableExpr)
            {
                return CallInternalOrExternalFunction();
            }

            // At this point, is a method call on an object.
            object member = this.NameExp.Evaluate();
            result = member;
            bool isMemberAccessType = member is MemberAccess;
            if (!isMemberAccessType) return result;

            string callStackName = this.NameExp.ToQualifiedName();
            MemberAccess maccess = member as MemberAccess;
            if (!IsMemberCall(maccess)) return result;

            Ctx.State.Stack.Push(callStackName, this);
            // CASE 3: object "." method call from script is a external/internal function e.g log.error -> external c# callback.
            if (maccess.IsInternalExternalFunctionCall())
            {
                result = FunctionHelper.FunctionCall(Ctx, maccess.FullMemberName as string, this);
            }
            // CASE 4: string method call or date method call
            else if (maccess.DataType == typeof(string) || maccess.DataType == typeof(LDate))
            {
                result = FunctionHelper.MemberCall(Ctx, maccess.Instance.GetType(), maccess.Instance, maccess.Name, maccess.MemberName, null, this.ParamListExpressions, this.ParamList);
            }
            // CASE 5: Member call via "." : either static or instance method call. e.g. Person.Create() or instance1.FullName() e.g.
            else if (maccess.Mode == MemberMode.CustObjMethodStatic || maccess.Mode == MemberMode.CustObjMethodInstance)
            {
                result = FunctionHelper.MemberCall(Ctx, maccess.DataType, maccess.Instance, maccess.Name, maccess.MemberName, maccess.Method, this.ParamListExpressions, this.ParamList);
            }
            // Pop the function name off the call stack.
            Ctx.State.Stack.Pop();
            return result;
        }


        private bool IsMemberCall(MemberAccess maccess)
        {
            if (maccess.IsInternalExternalFunctionCall()
                || maccess.DataType == typeof(string) || maccess.DataType == typeof(LDate)
                || maccess.Mode == MemberMode.CustObjMethodInstance || maccess.Mode == MemberMode.CustObjMethodStatic
              )
                return true;
            return false;
        }


        private object CallInternalOrExternalFunction()
        {
            string name = NameExp.ToQualifiedName();
            if (!FunctionHelper.IsInternalOrExternalFunction(Ctx, name, null))
                throw BuildRunTimeException("Function does not exist : '" + name + "'");

            Ctx.State.Stack.Push(name, this);
            var result = FunctionHelper.FunctionCall(Ctx, name, this);
            Ctx.State.Stack.Pop();
            return result;
        }


        private bool _isScopeVariable;
        private string _name;
        private string _member;
        /// <summary>
        /// Get the name of the function.
        /// </summary>
        public string Name
        {
            get
            {
                if (_name != null)
                    return _name;

                if (NameExp is VariableExpr)
                    return ((VariableExpr)NameExp).Name;

                object name = NameExp.Evaluate();
                if (name is string) return (string)name;
                if (name is MemberAccess) return ((MemberAccess)name).FullMemberName;
                return string.Empty;
            }
            set
            {
                _name = value;
                if (_name.Contains("."))
                {
                    int ndxDot = _name.IndexOf(".");
                    _member = _name.Substring(ndxDot + 1); 
                    _name = _name.Substring(0, ndxDot);                    
                }
                _isScopeVariable = true;
            }
        }


        /// <summary>
        /// Returns the fully qualified name of this node.
        /// </summary>
        /// <returns></returns>
        public override string ToQualifiedName()
        {
            if (_name != null) return _name;
            if (NameExp != null)
                return NameExp.ToQualifiedName();
            return string.Empty;
        }
    }
}
