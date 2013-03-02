using System;
using System.Collections.Generic;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.Docs;
using ComLib.Lang.Helpers;
using ComLib.Lang.Types;
// </lang:using>

namespace ComLib.Lang.AST
{
    /// <summary>
    /// Represents a function call statement
    /// </summary>
    public class FunctionExpr : BlockExpr
    {
        public bool ContinueRunning;
        private object _result = null;
        private bool _hasReturnValue;
        private FunctionMetaData _meta;


        /// <summary>
        /// Create new instance.
        /// </summary>
        public FunctionExpr() 
        {
            this.Nodetype = NodeTypes.SysFunction;
            Init(null, null);
        }


        /// <summary>
        /// Create new instance with function name and argument names.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="argumentNames"></param>
        public FunctionExpr(string name, List<string> argumentNames)
        {
            this.Nodetype = NodeTypes.SysFunction;
            Init(name, argumentNames);
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="name">name of function</param>
        /// <param name="argNames">names of the arguments.</param>
        public void Init(string name, List<string> argNames)
        {
            _meta = new FunctionMetaData(name, argNames);
            _meta.Doc = new DocTags();
            _meta.Version = new Version(1, 0, 0, 0);
            ExecutionCount = 0;
        }


        /// <summary>
        /// Name of the function.
        /// </summary>
        public string Name { get { return _meta.Name; } }


        /// <summary>
        /// Gets the metadata.
        /// </summary>
        public FunctionMetaData Meta { get { return _meta; } }


        /// <summary>
        /// The number of times this has been executed.
        /// </summary>
        public long ExecutionCount { get; set; }


        /// <summary>
        /// Total number of times exceptions occurred in this function
        /// </summary>
        public long ErrorCount { get; set; }

        
        /// <summary>
        /// Values passed to the function.
        /// </summary>
        public List<object> ArgumentValues;


        /// <summary>
        /// The caller of the this function.
        /// </summary>
        public Expr Caller;

        /// <summary>
        /// Whether or not this function has arguments.
        /// </summary>
        public bool HasArguments { get { return _meta.Arguments != null && _meta.Arguments.Count > 0; } }


        /// <summary>
        /// Whether or not this function has a return value.
        /// </summary>
        public bool HasReturnValue { get; set; }


        /// <summary>
        /// The return value;
        /// </summary>
        public object ReturnValue { get; set; }


        /// <summary>
        /// set the return value.
        /// </summary>
        public void Return(object val, bool hasReturnValue)
        {
            _result = val;
            _hasReturnValue = hasReturnValue;
            ContinueRunning = false;
        }


        /// <summary>
        /// Execute the statement.
        /// </summary>
        public override object Visit(IAstVisitor visitor)
        {
            return visitor.VisitFunction(this);
        }
    }
}
