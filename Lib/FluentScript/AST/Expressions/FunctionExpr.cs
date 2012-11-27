using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private bool _continueRunning;
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


        private long _errorCount;
        /// <summary>
        /// Total number of times exceptions occurred in this function
        /// </summary>
        public long ErrorCount { get { return _errorCount; } }

        
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
        public bool HasReturnValue { get { return _hasReturnValue; } }


        /// <summary>
        /// The return value;
        /// </summary>
        public object ReturnValue { get { return _result; } }

                
        /// <summary>
        /// Evaluate the function
        /// </summary>
        /// <returns></returns>
        public override object DoEvaluate()
        {
            InitializeCall();
            try
            {
                foreach (var statement in _statements)
                {
                    statement.Evaluate();
                    if (!_continueRunning) break;
                }
            }
            catch (Exception ex)
            {
                _errorCount++;
                throw ex;
            }
            return LObjects.Null;
        }


        /// <summary>
        /// set the return value.
        /// </summary>
        public void Return(object val, bool hasReturnValue)
        {
            _result = val;
            _hasReturnValue = hasReturnValue;
            _continueRunning = false;
        }


        private void PushParametersInScope()
        {
            if (this.ArgumentValues == null || this.ArgumentValues.Count == 0) return;
            if (this.Meta.Arguments == null || this.Meta.Arguments.Count == 0) return;
            //if (this.ArgumentValues.Count > this.Meta.Arguments.Count)
            //    throw new ArgumentException("Invalid function call, more arguments passed than arguments in function: line " + Caller.Ref.Line + ", pos: " + Caller.Ref.CharPos);

            // Check if there is an parameter named "arguments"
            var hasParameterNamedArguments = false;
            if (this.Meta.Arguments != null && this.Meta.Arguments.Count > 0)
                if (this.Meta.ArgumentsLookup.ContainsKey("arguments"))
                    hasParameterNamedArguments = true;
                        
            // Add function arguments to scope.
            for (int ndx = 0; ndx < this.Meta.Arguments.Count; ndx++)
            {
                var val = this.ArgumentValues[ndx] as LObject;
                if(val.Type.IsPrimitiveType())
                {
                    var copied = val.Clone();
                    this.ArgumentValues[ndx] = copied;
                }
                Ctx.Memory.SetValue(this.Meta.Arguments[ndx].Name, this.ArgumentValues[ndx]);
            }

            // Finally add the arguments.
            // NOTE: Any extra arguments will be part of the implicit "arguments" array.
            if(!hasParameterNamedArguments)
            {
                var argArray = new LArray(this.ArgumentValues);
                Ctx.Memory.SetValue("arguments", argArray);
            }
        }


        private void InitializeCall()
        {
            // Keep track of total times this function was executed.
            // Keep tract of total times this function caused an error
            if (ExecutionCount == long.MaxValue)
                ExecutionCount = 0;
            else
                ExecutionCount++;

            if (_errorCount == long.MaxValue)
                _errorCount = 0;

            _continueRunning = true;
            _result = null;
            _hasReturnValue = false;

            PushParametersInScope();
        }
    }
}
