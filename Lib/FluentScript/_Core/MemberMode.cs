namespace Fluentscript.Lib._Core
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
        /// Property access/set on object.
        /// </summary>
        PropertyMember,


        /// <summary>
        /// Method access/call on object.
        /// </summary>
        MethodMember,


        /// <summary>
        /// Static method on class
        /// </summary>
        CustObjMethodStatic,


        /// <summary>
        /// A module type.
        /// </summary>
        Module
    }
}
