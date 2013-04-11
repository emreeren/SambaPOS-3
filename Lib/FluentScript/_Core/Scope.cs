using System;
using System.Collections.Generic;
using Fluentscript.Lib.Types;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib._Core
{
    /// <summary>
    /// Block scope.
    /// </summary>
    public class Block : Dictionary<string, object>
    {
        private int _totalStringLength;


        /// <summary>
        /// Get the variable value associated with name from the scope
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Name of the varibale to get</param>
        /// <returns></returns>
        public T Get<T>(string name)
        {            
            Variable variable = this[name] as Variable;

            //fix
            if (variable != null && variable.Value is LObject)
            {
                return (T)Convert.ChangeType(((LObject)variable.Value).GetValue(), typeof(T), null);
            }

            // Convert to correct type if basic type.
            if (typeof(T) == typeof(object))
            {
                return (T)variable.Value;
            }            

            return (T)Convert.ChangeType(variable.Value, typeof(T), null);
        }


        /// <summary>
        /// Sets a value into the current scope.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="val">The value of the variable.</param>
        /// <param name="declare">Whether or not this variable is being declared and initialized.
        /// This is used in cases where a variable is declared with the same name as a variable
        /// in an outer scope. In this case, we should not search for the variable name.</param>
        public void SetValue(string name, object val, bool declare = false)
        {
            // Assert failure
            var lval = (LObject) val;

            Variable variable = null;
            bool add = false;
            int addVal = 0;
            if (!this.ContainsKey(name))
            {
                variable = new Variable() { Name = name, Value = val };                
                add = true;
            }
            else
            {
                variable = this[name] as Variable;
                variable.Value = val;
                if (lval.Type == LTypes.String)
                    addVal = ((LString) lval).Value.Length;
            }
            variable.IsInitialized = val != LObjects.Null && val != null;
            variable.DataType = val != null ? val.GetType() : typeof(LNullType);
            this[name] = variable;

            // Case 1: Adding new string value.
            bool isString = (lval.Type == LTypes.String);
            if (add && isString)
            {
                _totalStringLength += ((LString)lval).Value.Length;
            }
            // Case 2: Changed datatype
            if (!add)
            {
                _totalStringLength -= addVal;
                if (isString)
                    _totalStringLength += ((LString)lval).Value.Length;
            }
        }


        /// <summary>
        /// Get the size of the total length of all string variables in this block.
        /// </summary>
        public int TotalStringSize
        {
            get { return _totalStringLength; }
        }
    }



    /// <summary>
    /// Used to store local variables.
    /// </summary>
    public class Scope 
    {        
        private List<Block> _stack;
        private int _currentStackIndex = 0;
        private int _total = 0;
        private int _totalStringLength = 0;


        /// <summary>
        /// Initialize
        /// </summary>
        public Scope()
        {
            _stack = new List<Block>();
            _stack.Add(new Block());
        }


        /// <summary>
        /// Whether or not the scope contains the supplied variable name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            int stackIndex = Find(name);
            return stackIndex > -1;
        }


        /// <summary>
        /// Iterate through all the variables in the current block
        /// </summary>
        /// <param name="callback"></param>
        public void Each(Action<KeyValuePair<string, object>> callback)
        {
            var block = _stack[_currentStackIndex];
            ForEach(block, pair => callback(pair));
        }


        /// <summary>
        /// Get the variable from the current scope.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object this[string name]
        {
            get
            {
                return Get<object>(name);
            }
        }


        /// <summary>
        /// Get the variable value associated with name from the scope
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Name of the varibale to get</param>
        /// <returns></returns>
        public T Get<T>(string name)
        {
            int stackIndex = Find(name);
            // Not found?
            if (stackIndex == -1) throw new KeyNotFoundException("variable : " + name + " was not found");

            return _stack[stackIndex].Get<T>(name);
        }


        /// <summary>
        /// Get the variable value associated with name from the scope
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Name of the varibale to get</param>
        /// <returns></returns>
        public T GetAs<T>(string name) where T: class
        {
            int stackIndex = Find(name);
            // Not found?
            if (stackIndex == -1) throw new KeyNotFoundException("variable : " + name + " was not found");

            var obj = _stack[stackIndex].Get<object>(name);
            return (T)obj;
        }


        /// <summary>
        /// Sets a value into the current scope.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="val">The value of the variable.</param>
        /// <param name="declare">Whether or not this variable is being declared and initialized.
        /// This is used in cases where a variable is declared with the same name as a variable
        /// in an outer scope. In this case, we should not search for the variable name.</param>
        public void SetValue(string name, object val, bool declare = false)
        {
            int stackIndex = _currentStackIndex;

            if (!declare)
            {
                stackIndex = Find(name);
                if (stackIndex == -1)
                    stackIndex = _currentStackIndex;
            }
            int oldTotalStringLength = _stack[stackIndex].TotalStringSize;
            _stack[stackIndex].SetValue(name, val, declare);
            int newTotalStringLength = _stack[stackIndex].TotalStringSize;

            // LIMITS: Increment total vars and string length;
            _total += 1;
            _totalStringLength += (-oldTotalStringLength + newTotalStringLength);             
        }


        /// <summary>
        /// Finds the index of the scope where the variable provided resides.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int Find(string name)
        {
            int stackIndex = -1;
            for (var ndx = _stack.Count - 1; ndx >= 0; ndx--)
            {
                if (_stack[ndx].ContainsKey(name))
                {
                    stackIndex = ndx;
                    break;
                }
            }
            return stackIndex;
        }


        /// <summary>
        /// Push another scope into the script.
        /// </summary>
        public void Push()
        {
            _stack.Add(new Block());
            _currentStackIndex++;
        }


        /// <summary>
        /// Remove the current scope from the script.
        /// </summary>
        public void Pop()
        {
            if (_stack.Count == 1) return;

            var frameIndex = _stack.Count - 1;
            var frame = _stack[frameIndex];
            
            // LIMITS: Decrement total vars and string length;
            _total -= frame.Count;
            _totalStringLength -= frame.TotalStringSize;

            _stack.RemoveAt(frameIndex);
            _currentStackIndex--;
        }


        /// <summary>
        /// Remove variable from the current stack index
        /// </summary>
        /// <param name="name"></param>
        internal void Remove(string name)
        {
            if (!_stack[_currentStackIndex].ContainsKey(name))
                return;

            int oldTotalStringLength = _stack[_currentStackIndex].TotalStringSize;
             object val = _stack[_currentStackIndex][name];
            _stack[_currentStackIndex].Remove(name);
            int newTotalStringLength = _stack[_currentStackIndex].TotalStringSize;           

            // LIMITS: Decrement total vars and string length;
            _total -= 1;
            _totalStringLength += (-oldTotalStringLength + newTotalStringLength);
        }


        /// <summary>
        /// Get the total number of items in scope.
        /// </summary>
        public int Total
        {
            get
            {
                return _total;
            }
        }


        /// <summary>
        /// Total lenght of all string variables.
        /// </summary>
        public int TotalStringLength
        {
            get { return _totalStringLength; }
        }


        /// <summary>
        /// Clear the scope.
        /// </summary>
        public void Clear()
        {            
            _currentStackIndex = 0;
            _total = 0;
            _totalStringLength = 0;
            _stack = new List<Block>();
            _stack.Add(new Block());
        }


        /// <summary>
        /// Execute action on each item in enumeration.
        /// </summary>
        /// <typeparam name="T">Type of item to use with the method.</typeparam>
        /// <param name="items">List of items.</param>
        /// <param name="action">Action to call for each item.</param>
        public static void ForEach<T>(IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
                action(item);
        }
    }
}
