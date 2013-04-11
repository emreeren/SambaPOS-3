using System;
using System.Reflection;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.AST.Core
{    

    /// <summary>
    /// Member access expressions for "." property or "." method.
    /// </summary>
    public class MemberExpr : Expr
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="variableExp">The variable expression to use instead of passing in name of variable.</param>
        /// <param name="memberName">Name of member, this could be a property or a method name.</param>
        public MemberExpr(Expr variableExp, string memberName)
        {
            this.VariableExp = variableExp;
            this.MemberName = memberName;
        }

        
        /// <summary>
        /// The variable expression representing the list.
        /// </summary>
        public Expr VariableExp;


        /// <summary>
        /// The name of the member.
        /// </summary>
        public string MemberName;



        /// <summary>
        /// The full name of the member 
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// Returns the fully qualified name of this node.
        /// </summary>
        /// <returns></returns>
        public override string ToQualifiedName()
        {
            string name = VariableExp.ToQualifiedName() + "." + MemberName;
            return name;
        }


        /// <summary>
        /// Whether or not this variable + member name maps to an external function call.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        protected bool IsExternalFunctionCall(string variableName)
        {
            string funcName = variableName + "." + MemberName;
            if (Ctx.ExternalFunctions.Contains(funcName))
                return true;
            return false;
        }


        /// <summary>
        /// Whether or not this member
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected bool IsMemberInstanceAccess(object obj)
        {
            Type type = obj.GetType();

            // 1. Get the member name.
            MemberInfo[] members = type.GetMember(MemberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (members == null || members.Length == 0)
                return false;
            return true;
        }


        /// <summary>
        /// Whether or not this member
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected bool IsMemberStaticAccessObj(object obj)
        {
            Type type = obj.GetType();
            return IsMemberStaticAccess(type);
        }


        /// <summary>
        /// Whether or not this member
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected bool IsMemberStaticAccess(Type type)
        {
            // 1. Get the member name.
            MemberInfo[] members = type.GetMember(MemberName, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            if (members == null || members.Length == 0)
                return false;
            return true;
        }


        /// <summary>
        /// Whether or not the variable name provided and
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        protected BoolMsgObj IsMemberStaticAccess(string variableName)
        {
            Type type = null;
            bool isStatic = false;
            string actualName = null;
            // 2. Static method : "Person.Create" -> static method call on custom object?
            if (Ctx.Types.Contains(variableName))
            {
                type = Ctx.Types.Get(variableName);
                isStatic = IsMemberStaticAccess(type);
                actualName = variableName;
            }
            else if (!Ctx.Memory.Contains(variableName))
            {
                // 3. Static method but with lowercase classname 
                // Only do this check for "user" -> "User" class / static method.
                char first = Char.ToUpper(variableName[0]);
                string name = first + variableName.Substring(1);
                if (Ctx.Types.Contains(name))
                {
                    type = Ctx.Types.Get(name);
                    isStatic = IsMemberStaticAccess(type);
                    actualName = name;
                }
            }
            return new BoolMsgObj(type, isStatic, string.Empty);
        }


        /// <summary>
        /// Gets the instance member as an MemberAccess object.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected MemberAccess GetStaticMemberAccess(Type type)
        {
            return GetMemberAccess(type, null, true);
        }


        /// <summary>
        /// Gets the instance member as an MemberAccess object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected MemberAccess GetInstanceMemberAccess(object obj)
        {
            var type = obj.GetType();
            return GetMemberAccess(type, obj, false);
        }



        private MemberAccess GetMemberAccess(Type type, object obj, bool isStatic)
        {        
            // 1. Get the member name.
            MemberInfo[] members = null;
            members = type.GetMember(MemberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase);            

            MemberInfo result = null;

            // 2. Validate: Property does not exist ?
            if (members == null || members.Length == 0)
                throw BuildRunTimeException("Property does not exist : '" + MemberName + "' ");

            // 3. Get the first member.
            result = members[0];
            string memberNameCaseIgnorant = result.Name;
            MemberMode mode = isStatic ? MemberMode.CustObjMethodStatic : MemberMode.CustObjMethodInstance;
            MemberAccess member = new MemberAccess(mode);
            member.DataType = type;
            member.Instance = obj;
            member.MemberName = MemberName;

            // Property.
            if (result.MemberType == MemberTypes.Property)
            {
                member.Name = type.Name;
                member.Property = type.GetProperty(memberNameCaseIgnorant);
            }
            // Method
            else if (result.MemberType == MemberTypes.Method)
            {
                string name = (VariableExp.IsNodeType(NodeTypes.SysVariable)) ? ((VariableExpr)VariableExp).Name : null;
                member.Name = name;                
                member.Method = type.GetMethod(memberNameCaseIgnorant);
            }
            return member;
        }
    }    
}
