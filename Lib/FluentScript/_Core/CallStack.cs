using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST.Interfaces;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib._Core
{
    /// <summary>
    /// The call stack
    /// </summary>
    public class CallStack
    {
        private List<Tuple<string, IParameterExpression>> _stack;
        private int _lastIndex = -1;
        private Action<AstNode, int> _limitCheck;

        
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="limitCheck"></param>
        public CallStack(Action<AstNode, int> limitCheck)
        {
            _stack = new List<Tuple<string, IParameterExpression>>();
            _limitCheck = limitCheck;
        }


        /// <summary>
        /// Push the function expression on the call stack
        /// </summary>
        /// <param name="qualifiedName">Name of function call</param>
        /// <param name="exp">Function Call expression</param>
        public bool Push(string qualifiedName, IParameterExpression exp)
        {
            _stack.Add(new Tuple<string, IParameterExpression>(qualifiedName, exp));
            _lastIndex++;

            if (_limitCheck != null)
            {
                var node = exp as AstNode;
                _limitCheck(node, _lastIndex);
            }
            return true;
        }


        /// <summary>
        /// Pop the function expression from the call stack.
        /// </summary>
        public void Pop()
        {
            int count = _stack.Count;
            if (count == 0) return;
            _stack.RemoveAt(_lastIndex);
            count--;
            _lastIndex = count - 1;
        }


        /// <summary>
        /// Total item in the call stack.
        /// </summary>
        public int Count
        {
            get { return _stack.Count; }
        }


        /// <summary>
        /// Get by index.
        /// </summary>
        /// <param name="ndx"></param>
        /// <returns></returns>
        public Tuple<string, IParameterExpression> this[int ndx]
        {
            get { return _stack[_lastIndex]; }
        }
    }
}
