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
    /// Represents the member access mode
    /// </summary>
    public enum MemberMode
    {
        /// <summary>
        /// External function
        /// </summary>
        FunctionExternal,


        /// <summary>
        /// Internal function
        /// </summary>
        FunctionScript,


        /// <summary>
        /// Instance method on class
        /// </summary>
        CustObjMethodInstance,


        /// <summary>
        /// Static method on class
        /// </summary>
        CustObjMethodStatic
    }



    /// <summary>
    /// Represents the member access
    /// </summary>
    public class MemberAccess
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="mode"></param>
        public MemberAccess(MemberMode mode)
        {
            Mode = mode;
        }


        /// <summary>
        /// The mode of access
        /// </summary>
        public MemberMode Mode;


        /// <summary>
        /// The name of the member.
        /// </summary>
        public string Name;


        /// <summary>
        /// The name of the member being accessed
        /// </summary>
        public string MemberName;


        /// <summary>
        /// Instance of the member
        /// </summary>
        public object Instance;


        /// <summary>
        /// The datatype of the member being accessed.
        /// </summary>
        public Type DataType;


        /// <summary>
        /// The method represetning the member.
        /// </summary>
        public MethodInfo Method;


        /// <summary>
        /// The property representing the member.
        /// </summary>
        public PropertyInfo Property;


        /// <summary>
        /// The full member name.
        /// </summary>
        public string FullMemberName
        {
            get { return Name + "." + MemberName; }
        }


        /// <summary>
        /// Whether or not this represents an external or internal function call.
        /// </summary>
        /// <returns></returns>
        public bool IsInternalExternalFunctionCall()
        {
            return Mode == MemberMode.FunctionExternal || Mode == MemberMode.FunctionScript;
        }
    }


    /// <summary>
    /// Member access expressions for "." property or "." method.
    /// </summary>
    public class MemberAccessExpr : MemberExpr
    {
        private static LDate _dateTypeForMethodCheck = new LDate(null, null);


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="variableExp">The variable expression to use instead of passing in name of variable.</param>
        /// <param name="memberName">Name of member, this could be a property or a method name.</param>
        /// <param name="isAssignment">Whether or not this is part of an assigment</param>
        public MemberAccessExpr(Expr variableExp, string memberName, bool isAssignment) : base(variableExp, memberName)
        {
            this.IsAssignment = isAssignment;
        }


        /// <summary>
        /// Whether or not this member access is part of an assignment.
        /// </summary>
        public bool IsAssignment;


        /// <summary>
        /// Either external function or member name.
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            MemberAccess memberAccess = GetMemberAccess();
            if (IsAssignment)
                return memberAccess;

            // Get property value ( either static or instance )
            if (memberAccess.Property != null)
            {
                return memberAccess.Property.GetValue(memberAccess.Instance, null);
            }
            if (memberAccess.DataType == typeof(LMap))
            {
                // 2. Non-Assignment - Validate property exists.
                if (!((LMap)memberAccess.Instance).HasProperty(MemberName)) 
                    throw this.BuildRunTimeException("Property does not exist : '" + MemberName + "'"); 
                
                return ((LMap)memberAccess.Instance).ExecuteMethod(MemberName, null);
            }           
            return memberAccess;
        }


        private MemberAccess GetMemberAccess()
        {
            Type type = null;
            bool isVariableExp = VariableExp is VariableExpr;
            string variableName = isVariableExp ? ((VariableExpr)VariableExp).Name : string.Empty;
            
            // CASE 1: External function call "user.create"
            if (isVariableExp && IsExternalFunctionCall(variableName))
                return new MemberAccess(MemberMode.FunctionExternal) { Name = variableName, MemberName = MemberName };
            
            // CASE 2. Static method call: "Person.Create" 
            if(isVariableExp )
            { 
                var result = IsMemberStaticAccess(variableName);
                if (result.Success)
                    return GetStaticMemberAccess(result.Item as Type);
            }

            object obj = VariableExp.Evaluate();
            type = obj.GetType();

            // Case 3: LDate, LArray, String,
            bool isCoreType = IsCoreType(obj);
            if ( isCoreType )
            {
                // 1. Map or Array
                if (obj is LMap)
                {
                    // 2. Non-Assignment - Validate property exists.
                    if (!((LMap)obj).HasProperty(MemberName)) 
                        throw this.BuildRunTimeException("Property does not exist : '" + MemberName + "'"); 
                
                    return new MemberAccess(MemberMode.CustObjMethodInstance) { Name = type.Name, DataType = type, Instance = obj, MemberName = MemberName };
                }
                if (obj is LArray)
                {
                    MemberName = LArray.MapMethod(MemberName);
                    return GetInstanceMemberAccess(obj);
                }
                // Case 2a: string.Method
                if (obj is string)
                {
                    return new MemberAccess(MemberMode.CustObjMethodInstance) { Name = type.Name, DataType = type, Instance = obj, MemberName = MemberName };
                }
                // Case 2b: date.Method
                if (obj is DateTime)
                {
                    if (_dateTypeForMethodCheck.HasMethod(MemberName) || _dateTypeForMethodCheck.HasProperty(MemberName))
                        return new MemberAccess(MemberMode.CustObjMethodInstance) { Name = variableName, DataType = typeof(LDate), Instance = obj, MemberName = MemberName };
                    
                    var prop = typeof(DateTime).GetProperty(MemberName);
                    if(prop != null)
                        return new MemberAccess(MemberMode.CustObjMethodInstance) { Name = type.Name, DataType = type, Instance = obj, MemberName = MemberName, Property = prop };

                    return new MemberAccess(MemberMode.CustObjMethodInstance) { Name = type.Name, DataType = type, Instance = obj, MemberName = MemberName };
                }
            }

            // CASE 3: Custom object type
            var member = GetInstanceMemberAccess(obj);
            return member;
        }
    }    
}
