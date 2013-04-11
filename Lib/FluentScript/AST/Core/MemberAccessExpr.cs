using System;
using System.Reflection;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.AST.Core
{   
    /// <summary>
    /// Information for an index access operation.
    /// </summary>
    public class IndexAccess
    {
        /// <summary>
        /// Instance of the member
        /// </summary>
        public LObject Instance;


        /// <summary>
        /// The name of the member being accessed
        /// </summary>
        public LObject MemberName;
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
        /// Whether or not the member exists.
        /// </summary>
        public bool MemberMissing;


        /// <summary>
        /// The datatype of the member being accessed.
        /// </summary>
        public Type DataType;


        /// <summary>
        /// The type if the member access is on a built in a fluentscript type.
        /// </summary>
        public LType Type;


        /// <summary>
        /// The method represetning the member.
        /// </summary>
        public MethodInfo Method;


        /// <summary>
        /// The property representing the member.
        /// </summary>
        public PropertyInfo Property;


        /// <summary>
        /// Field info.
        /// </summary>
        public FieldInfo Field;


        /// <summary>
        /// The scope to use.
        /// </summary>
        public ISymbols Scope;


        /// <summary>
        /// An expression, currently used for functions.
        /// </summary>
        public Expr Expr;


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


        /// <summary>
        /// Whether or not this represents a property access on a basic built in type (date, array, etc )
        /// </summary>
        /// <returns></returns>
        public bool IsPropertyAccessOnBuiltInType()
        {
            return this.Type != null && this.Mode == MemberMode.PropertyMember;
        }


        /// <summary>
        /// Whether or not this is a property access on a custom object.
        /// </summary>
        /// <returns></returns>
        public bool IsPropertyAccessOnClass()
        {
            return this.DataType != null && this.Property != null;
        }


        public bool IsFieldAccessOnClass()
        {
            return this.DataType != null && this.Field != null; 
        }


        /// <summary>
        /// Whether or not this is a property access on a custom object.
        /// </summary>
        /// <returns></returns>
        public bool IsModuleAccess()
        {
            return this.Mode == MemberMode.Module;
        }
    }
}
