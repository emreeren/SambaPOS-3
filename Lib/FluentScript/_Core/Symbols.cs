using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang
{
    /// <summary>
    /// Constants for various symbol type names
    /// </summary>
    public class SymbolConstants
    {
        /// <summary>
        /// Variable
        /// </summary>
        public const string Var = "var";
        

        /// <summary>
        /// Constant
        /// </summary>
        public const string Const = "const";
        
        
        /// <summary>
        /// Function
        /// </summary>
        public const string Func = "func";
    }



    /// <summary>
    /// Represents a symbol definition in the language.
    /// </summary>
    public class SymbolType
    {
        /// <summary>
        /// Name of the symbol
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Category of the symbo, e.g. "var", "func", "class", "aggregate"
        /// </summary>
        public string Category { get; set; }


        /// <summary>
        /// Datatype of the symbol ( not used for now since fluentscript is a dynamic language.
        /// </summary>
        public Type DataType { get; set; }


        /// <summary>
        /// Name of datatype
        /// </summary>
        public string DataTypeName { get; set; }
    }



    /// <summary>
    /// Symbol type for const.
    /// </summary>
    public class SymbolTypeConst : SymbolType
    {
        /// <summary>
        /// Value for the constant.
        /// </summary>
        public object Value { get; set; }
    }



    /// <summary>
    /// Symbol type for function.
    /// </summary>
    public class SymbolTypeFunc : SymbolType
    {        
        /// <summary>
        /// Initialize with function metadata
        /// </summary>
        /// <param name="meta"></param>
        public SymbolTypeFunc(FunctionMetaData meta)
        {
            Name = meta.Name;
            Meta = meta;
            Category = SymbolConstants.Func;
            DataType = typeof(LFunction);
        }


        /// <summary>
        /// Function metadata
        /// </summary>
        public FunctionMetaData Meta { get; set; }
    }



    /// <summary>
    /// Interface for a symbol table to store known variables, functions, classes etc.
    /// </summary>
    public interface ISymbols
    {
        /// <summary>
        /// Name of the symbol table
        /// </summary>
        string Name { get; }


        /// <summary>
        /// The parent symbol scope
        /// </summary>
        ISymbols ParentScope { get; set; }


        /// <summary>
        /// Whether or not the symbol scope contains the name supplied
        /// </summary>
        /// <param name="name">Name of the symbol</param>
        /// <returns></returns>
        bool Contains(string name);


        /// <summary>
        /// Gets the symbol with the supplied name.
        /// </summary>
        /// <param name="name">Name of the symbol to get.</param>
        /// <returns></returns>
        SymbolType GetSymbol(string name);


        /// <summary>
        /// Generic version of getsymbol method
        /// </summary>
        /// <param name="name">Name of the symbol to get.</param>
        /// <returns></returns>
        T GetSymbol<T>(string name) where T : class;


        /// <summary>
        /// Defines a variable in the current symbol scope.
        /// </summary>
        /// <param name="name">Name of the variable</param>
        void DefineVariable(string name);


        /// <summary>
        /// Defines a constant in the current symbol scope.
        /// </summary>
        /// <param name="name">Name of the constant</param>
        /// <param name="value">The value of the constant</param>
        void DefineConstant(string name, object value);


        /// <summary>
        /// Defines a variable in the current symbol scope.
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="type">Type of the variable</param>
        void DefineVariable(string name, Type type);


        /// <summary>
        /// Defines an alias name for the existing name supplied.
        /// </summary>
        /// <param name="existingName">The existing symbol name to define the alias for</param>
        /// <param name="alias">The alias</param>
        void DefineAlias(string existingName, string alias);
    }    



    /// <summary>
    /// Block scope.
    /// </summary>
    public class SymbolsBase : ISymbols
    {
        /// <summary>
        /// Name for this symbol scope
        /// </summary>
        protected string _name;


        /// <summary>
        /// Parent symbol scope
        /// </summary>
        protected ISymbols _parent;


        /// <summary>
        /// Map of the registered symbols in this scope.
        /// </summary>
        protected IDictionary<string, SymbolType> _symbols;


        /// <summary>
        /// Initialize with name and paretn symbol scope.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        public SymbolsBase(string name, ISymbols parent)
        {
            _name = name;
            _parent = parent;
            _symbols = new Dictionary<string, SymbolType>();

        }


        /// <summary>
        /// Name of the symbol scope.
        /// </summary>
        public string Name { get { return _name; } }


        /// <summary>
        /// Gets the parent symbol scope of this scope.
        /// </summary>
        public virtual ISymbols ParentScope { get { return _parent; } set { _parent = value;} }


        /// <summary>
        /// Define the symbol within this scope.
        /// </summary>
        /// <param name="name">Name of the varaible</param>
        public virtual void DefineVariable(string name)
        {
            Symbols[name] = new SymbolType() { Name = name, Category = SymbolConstants.Var,  DataType = typeof(object) };
        }


        /// <summary>
        /// Define the symbol within this scope.
        /// </summary>
        /// <param name="name">Name of the varaible</param>
        /// <param name="type">The type of the variable</param>
        public virtual void DefineVariable(string name, Type type)
        {
            Symbols[name] = new SymbolType() { Name = name, Category = SymbolConstants.Var, DataTypeName = type.Name, DataType = type };
        }


        /// <summary>
        /// Define a function symbol within this scope.
        /// </summary>
        /// <param name="name">Name of the varaible</param>
        /// <param name="totalNumberOfArgs">The total number of arguments</param>
        /// <param name="argNames">The names of the arguments</param>
        /// <param name="returnType">The return type of the function</param>
        public virtual void DefineFunction(string name, int totalNumberOfArgs, string[] argNames, Type returnType)
        {
            var meta = new FunctionMetaData(name, argNames.ToList());
            meta.ReturnType = returnType;
            Symbols[name] = new SymbolTypeFunc(meta);
        }


        /// <summary>
        /// Define a function symbol within this scope.
        /// </summary>
        /// <param name="func">The function metadata</param>
        public virtual void DefineFunction(FunctionMetaData func)
        {            
            Symbols[func.Name] = new SymbolTypeFunc(func);
        }


        /// <summary>
        /// Defines an alias name for the existing name supplied.
        /// </summary>
        /// <param name="existingName">The existing symbol name to define the alias for</param>
        /// <param name="alias">The alias</param>
        public virtual void DefineAlias(string existingName, string alias)
        {
            var symType = Symbols[existingName];
            Symbols[alias] = symType;
        }


        /// <summary>
        /// Define the symbol within this scope.
        /// </summary>
        /// <param name="name">Name of the constant</param>
        /// <param name="value">The value of constant</param>
        public virtual void DefineConstant(string name, object value)
        {               
            if(value == null) 
            {                
                throw new ArgumentException("Constant value not supplied for : " + name);
            }
            Symbols[name] = new SymbolTypeConst() { Name = name, Category = SymbolConstants.Const, DataType = value.GetType(), Value = value };
        }


        /// <summary>
        /// Get the symbol associated with the supplied name
        /// </summary>
        /// <param name="name">Name of the symbol</param>
        /// <returns></returns>
        public virtual bool Contains(string name)
        {
            return Symbols.ContainsKey(name);
        }


        /// <summary>
        /// Get the symbol associated with the supplied name
        /// </summary>
        /// <param name="name">Name of the symbol</param>
        /// <returns></returns>
        public virtual SymbolType GetSymbol(string name)
        {
            if (Symbols.ContainsKey(name))
                return Symbols[name];

            return null;
        }


        /// <summary>
        /// Get the symbol associated with the supplied name
        /// </summary>
        /// <param name="name">Name of the symbol</param>
        /// <returns></returns>
        public virtual T GetSymbol<T>(string name) where T: class
        {
            object sym = GetSymbol(name);
            if (sym == null)
                return default(T);

            return (T)sym;
        }


        /// <summary>
        /// Gets the symbol table for this symbol scope
        /// </summary>
        /// <returns></returns>
        protected virtual IDictionary<string, SymbolType> Symbols
        {
            get { return _symbols; }
        }
    }



    /// <summary>
    /// Symbol table for global scope.
    /// </summary>
    public class SymbolsGlobal : SymbolsBase
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public SymbolsGlobal() : base("global", null)
        {
        }
    }



    /// <summary>
    /// Symbol table for functions
    /// </summary>
    public class SymbolsNested : SymbolsBase
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="name">The name of the symbol scope</param>
        public SymbolsNested(string name) : base(name, null)
        {
        }


        /// <summary>
        /// Initialize
        /// </summary>
        public SymbolsNested(string name, ISymbols parentScope) : base(name, parentScope)
        {
        }


        /// <summary>
        /// Get the symbol associated with the supplied name
        /// </summary>
        /// <param name="name">Name of the symbol</param>
        /// <returns></returns>
        public override bool Contains(string name)
        {
            if (base.Contains(name))
                return true;
            if (_parent != null && _parent.Contains(name))
                return true;
            return false;
        }


        /// <summary>
        /// Get the symbol associated with the supplied name
        /// </summary>
        /// <param name="name">Name of the symbol</param>
        /// <returns></returns>
        public override SymbolType GetSymbol(string name)
        {
            SymbolType symbol = base.GetSymbol(name);
            if (symbol == null && _parent != null)
                symbol = _parent.GetSymbol(name);

            return symbol;
        }
    }



    /// <summary>
    /// Symbol table for functions
    /// </summary>
    public class SymbolsFunction : SymbolsNested
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="name">The name of the symbol scope</param>
        public SymbolsFunction(string name) : base(name, null)
        {
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="name">The name of the symbol scope</param>
        /// <param name="parentScope">The parent scope</param>
        public SymbolsFunction(string name, ISymbols parentScope) : base(name, parentScope)
        {
        }
    }



    /// <summary>
    /// Used to store local variables.
    /// </summary>
    public class Symbols
    {
        private SymbolsGlobal _global;
        private ISymbols _current;
        

        /// <summary>
        /// Initialize default
        /// </summary>
        public Symbols()
        {
            _global = new SymbolsGlobal();
            _current = _global;
        }


        /// <summary>
        /// The symbol table at the global scope
        /// </summary>
        public SymbolsGlobal Global { get { return _global; } }


        /// <summary>
        /// The symbol table at the current scope
        /// </summary>
        public ISymbols Current { get { return _current; } }


        /// <summary>
        /// Push as function scope
        /// </summary>
        public void Push(ISymbols symbols, bool assignCurrentScopeAsParent)
        {
            if(assignCurrentScopeAsParent)
                symbols.ParentScope = _current;
            _current = symbols;
        }


        /// <summary>
        /// Pop the current
        /// </summary>
        public void Pop()
        {
            if (_current == _global)
                return;

            _current = _current.ParentScope;
        }


        /// <summary>
        /// Get the symbol associated with the supplied name
        /// </summary>
        /// <param name="name">Name of the symbol</param>
        /// <returns></returns>
        public bool Contains(string name)
        {
            return _current.Contains(name);
        }


        /// <summary>
        /// Get the symbol associated with the supplied name
        /// </summary>
        /// <param name="name">Name of the symbol</param>
        /// <returns></returns>
        public SymbolType GetSymbol(string name)
        {
            return _current.GetSymbol(name);
        }


        /// <summary>
        /// Get the symbol associated with the supplied name
        /// </summary>
        /// <param name="name">Name of the symbol</param>
        /// <returns></returns>
        public T GetSymbol<T>(string name) where T: class
        {
            return _current.GetSymbol<T>(name);
        }


        /// <summary>
        /// Define the symbol within this scope.
        /// </summary>
        /// <param name="name">Name of the varaible</param>
        public void DefineVariable(string name)
        {
            _current.DefineVariable(name);
        }


        /// <summary>
        /// Define the symbol within this scope.
        /// </summary>
        /// <param name="name">Name of the constant</param>
        /// <param name="val">Value of the constant</param>
        public void DefineConstant(string name, object val)
        {
            _current.DefineConstant(name, val);
        }


        /// <summary>
        /// Whether or not the name of the symbol supplied is a variable
        /// </summary>
        /// <param name="name">The name of the variable symbol</param>
        /// <returns></returns>
        public bool IsVar(string name)
        {
            return IsType(name, SymbolConstants.Var);
        }


        /// <summary>
        /// Whether or not the name of the symbol supplied is a constant
        /// </summary>
        /// <param name="name">The name of the constant symbol</param>
        /// <returns></returns>
        public bool IsConst(string name)
        {
            return IsType(name, SymbolConstants.Const);
        }


        /// <summary>
        /// Whether or not the name of the symbol supplied is a function
        /// </summary>
        /// <param name="name">The name of the function symbol</param>
        /// <returns></returns>
        public bool IsFunc(string name)
        {
            return IsType(name, SymbolConstants.Func);
        }

        
        /// <summary>
        /// Whether or not the type of the symbol supplied matches the typename 
        /// </summary>
        /// <param name="name">The name of the symbol</param>
        /// <param name="typename">The name of the type</param>
        /// <returns></returns>
        public bool IsType(string name, string typename)
        {
            SymbolType sym = this.GetSymbol(name);
            if (sym == null) return false;
            return sym.Category == typename;
        }
    }
}
