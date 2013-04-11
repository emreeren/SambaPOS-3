using System;

namespace Fluentscript.Lib._Core
{    
    /// <summary>
    /// Represents a variable in the script
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// Name of the variable.
        /// </summary>
        public string Name { get; set;} 


        /// <summary>
        /// Value of the variable
        /// </summary>
        public object Value;


        /// <summary>
        /// Type of the variable
        /// </summary>
        public Type DataType;


        /// <summary>
        /// Whether or not the variable is initialized.
        /// </summary>
        public bool IsInitialized;
    }



    /// <summary>
    /// Used to store local variables.
    /// </summary>
    public class Memory : Scope
    {        
    }
}
