using System;
using System.Collections.Generic;
using System.Linq;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core.Meta.Types;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib._Core
{
    /// <summary>
    /// Constants for various symbol type names
    /// </summary>
    public class SymbolCategory
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


        /// <summary>
        /// Module level scope.
        /// </summary>
        public const string Module = "module";


        /// <summary>
        /// Module level scope.
        /// </summary>
        public const string CustomScope1 = "customscope";
    }



    /// <summary>
    /// Represents a symbol definition in the language.
    /// </summary>
    public class Symbol
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
        public LType DataType { get; set; }


        /// <summary>
        /// Name of datatype
        /// </summary>
        public string DataTypeName { get; set; }
    }



    /// <summary>
    /// Symbol type for const.
    /// </summary>
    public class SymbolConstant : Symbol
    {
        /// <summary>
        /// Value for the constant.
        /// </summary>
        public object Value { get; set; }
    }



    /// <summary>
    /// Symbol type for const.
    /// </summary>
    public class SymbolModule : Symbol
    {
        /// <summary>
        /// Value for the constant.
        /// </summary>
        public ISymbols Scope { get; set; }


        /// <summary>
        /// The parent scope.
        /// </summary>
        public ISymbols ParentScope { get; set; }
    }



    /// <summary>
    /// Symbol type for function.
    /// </summary>
    public class SymbolFunction : Symbol
    {        
        /// <summary>
        /// Initialize with function metadata
        /// </summary>
        /// <param name="meta"></param>
        public SymbolFunction(FunctionMetaData meta)
        {
            this.Name = meta.Name;
            this.Meta = meta;
            this.Category = SymbolCategory.Func;
            this.DataType = new LFunctionType();
            this.DataType.Name = meta.Name;
        }


        /// <summary>
        /// Function metadata
        /// </summary>
        public FunctionMetaData Meta { get; set; }


        /// <summary>
        /// The function expression.
        /// </summary>
        public object FuncExpr { get; set; }
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
        /// Whether or not the type of the symbol supplied matches the typename 
        /// </summary>
        /// <param name="name">The name of the symbol</param>
        /// <param name="categoryName">The category of the symbol</param>
        /// <returns></returns>
        bool IsCategory(string name, string typename);


        /// <summary>
        /// Whether or not the symbol name supplied is a function.
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <returns></returns>
        bool IsFunction(string name);
        

        /// <summary>
        /// Gets the symbol with the supplied name.
        /// </summary>
        /// <param name="name">Name of the symbol to get.</param>
        /// <returns></returns>
        Symbol GetSymbol(string name);


        /// <summary>
        /// Gets a list of all the symbol names.
        /// </summary>
        /// <returns></returns>
        List<string> GetSymbolNames();
        
        
        /// <summary>
        /// Generic version of getsymbol method
        /// </summary>
        /// <param name="name">Name of the symbol to get.</param>
        /// <returns></returns>
        T GetSymbol<T>(string name) where T : class;


        /// <summary>
        /// Define the supplied symbol to this instance of symbol table.
        /// </summary>
        /// <param name="symbol"></param>
        void Define(Symbol symbol);


        /// <summary>
        /// Defines a variable in the current symbol scope with type object
        /// </summary>
        /// <param name="name">Name of the variable</param>
        void DefineVariable(string name);
        
        
        /// <summary>
        /// Defines a variable in the current symbol scope.
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="type">Type of the variable</param>
        void DefineVariable(string name, LType type);


        /// <summary>
        /// Create an alias to reference another existing symbol.
        /// </summary>
        /// <param name="alias">The alias</param>
        /// <param name="symbol">The symbol to map the alias to</param>
        void DefineAlias(string alias, string existingName);


        /// <summary>
        /// Defines a constant in the current symbol scope.
        /// </summary>
        /// <param name="name">Name of the constant</param>
        /// <param name="value">The value of the constant</param>
        void DefineConstant(string name, LType type, object value);


        /// <summary>
        /// Define a function symbol within this scope.
        /// </summary>
        /// <param name="func">The function metadata</param>
        /// <param name="functionExpr">The function expression object that can execute the function</param>
        void DefineFunction(FunctionMetaData func, object functionExpr);
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
        protected IDictionary<string, Symbol> _symbols;


        /// <summary>
        /// Initialize with name and paretn symbol scope.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        public SymbolsBase(string name, ISymbols parent)
        {
            _name = name;
            _parent = parent;
            _symbols = new Dictionary<string, Symbol>();

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
        /// Gets the symbol table for this symbol scope
        /// </summary>
        /// <returns></returns>
        public virtual IDictionary<string, Symbol> Symbols
        {
            get { return _symbols; }
        }


        /// <summary>
        /// Gets a list of all the symbol names.
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetSymbolNames()
        {
            return _symbols.Keys.ToList();
        }


        /// <summary>
        /// Get the symbol associated with the supplied name
        /// </summary>
        /// <param name="name">Name of the symbol</param>
        /// <returns></returns>
        public virtual bool Contains(string name)
        {
            return this.Symbols.ContainsKey(name);
        }


        /// <summary>
        /// Whether or not the type of the symbol supplied matches the typename 
        /// </summary>
        /// <param name="name">The name of the symbol</param>
        /// <param name="categoryName">The category of the symbol</param>
        /// <returns></returns>
        public virtual bool IsCategory(string name, string categoryName)
        {
            var sym = this.GetSymbol(name);
            if (sym == null) return false;
            return sym.Category == categoryName;
        }


        /// <summary>
        /// Whether or not the symbol name supplied is a function.
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <returns></returns>
        public virtual bool IsFunction(string name)
        {
            var sym = this.GetSymbol(name);
            if (sym == null) return false;
            return sym.Category == SymbolCategory.Func;
        }


        /// <summary>
        /// Get the symbol associated with the supplied name
        /// </summary>
        /// <param name="name">Name of the symbol</param>
        /// <returns></returns>
        public virtual Symbol GetSymbol(string name)
        {
            if (this.Symbols.ContainsKey(name))
                return Symbols[name];

            return null;
        }


        /// <summary>
        /// Get the symbol associated with the supplied name
        /// </summary>
        /// <param name="name">Name of the symbol</param>
        /// <returns></returns>
        public virtual T GetSymbol<T>(string name) where T : class
        {
            object sym = this.GetSymbol(name);
            if (sym == null)
                return default(T);

            return (T)sym;
        }


        /// <summary>
        /// Define the supplied symbol to this instance of symbol table.
        /// </summary>
        /// <param name="symbol"></param>
        public virtual void Define(Symbol symbol)
        {
            this.Symbols[symbol.Name] = symbol;
        }


        /// <summary>
        /// Define the symbol within this scope with default type of object.
        /// </summary>
        /// <param name="name">Name of the varaible</param>
        public virtual void DefineVariable(string name)
        {
            this.DefineVariable(name, LTypes.Object);
        }
        
        
        /// <summary>
        /// Define the symbol within this scope.
        /// </summary>
        /// <param name="name">Name of the varaible</param>
        /// <param name="type">The type of the variable</param>
        public virtual void DefineVariable(string name, LType type)
        {
            var symbol = new Symbol() { Name = name, Category = SymbolCategory.Var, DataTypeName = type.Name, DataType = type };
            this.Define(symbol);
        }


        /// <summary>
        /// Define the symbol within this scope.
        /// </summary>
        /// <param name="name">Name of the constant</param>
        /// <param name="value">The value of constant</param>
        public virtual void DefineConstant(string name, LType type, object value)
        {
            if (value == null)
                throw new ArgumentException("Constant value not supplied for : " + name);

            var symbol = new SymbolConstant() { Name = name, Category = SymbolCategory.Const, DataType = LTypes.Object, Value = value };
            this.Define(symbol);
        }


        /// <summary>
        /// Create an alias to reference another existing symbol.
        /// </summary>
        /// <param name="alias">The alias</param>
        /// <param name="existing">The symbol to map the alias to</param>
        public virtual void DefineAlias(string alias, string existing)
        {
            var symbol = this.GetSymbol(existing);
            this.Symbols[alias] = symbol;
        }


        /// <summary>
        /// Define a function symbol within this scope.
        /// </summary>
        /// <param name="func">The function metadata</param>
        /// <param name="functionExpr">The function expression object that can execute the function</param>
        public virtual void DefineFunction(FunctionMetaData func, object functionExpr)
        {
            var symbol = new SymbolFunction(func);
            symbol.FuncExpr = functionExpr;
            this.Define(symbol);
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
        public override Symbol GetSymbol(string name)
        {
            Symbol symbol = base.GetSymbol(name);
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
        public SymbolsFunction(string name) : base(name)
        {
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="name">The name of the symbol scope</param>
        /// <param name="parentScope">The parent scope</param>
        public SymbolsFunction(string name, ISymbols parentScope) : base(name)
        {
            this.ParentScope = parentScope;
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
        public Symbol GetSymbol(string name)
        {
            return _current.GetSymbol(name);
        }


        /// <summary>
        /// Gets a list of all the symbol names.
        /// </summary>
        /// <returns></returns>
        public List<string> GetSymbolNames()
        {
            return _current.GetSymbolNames();
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
        /// Whether or not the name of the symbol supplied is a variable
        /// </summary>
        /// <param name="name">The name of the variable symbol</param>
        /// <returns></returns>
        public bool IsVar(string name)
        {
            return IsCategory(name, SymbolCategory.Var);
        }


        /// <summary>
        /// Whether or not the symbol supplied is a module
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsMod(string name)
        {
            return IsCategory(name, SymbolCategory.Module);
        }


        /// <summary>
        /// Whether or not the name of the symbol supplied is a constant
        /// </summary>
        /// <param name="name">The name of the constant symbol</param>
        /// <returns></returns>
        public bool IsConst(string name)
        {
            return IsCategory(name, SymbolCategory.Const);
        }


        /// <summary>
        /// Whether or not the name of the symbol supplied is a function
        /// </summary>
        /// <param name="name">The name of the function symbol</param>
        /// <returns></returns>
        public bool IsFunc(string name)
        {
            return IsCategory(name, SymbolCategory.Func);
        }

        
        /// <summary>
        /// Whether or not the type of the symbol supplied matches the typename 
        /// </summary>
        /// <param name="name">The name of the symbol</param>
        /// <param name="categoryName">The category of the symbol</param>
        /// <returns></returns>
        public bool IsCategory(string name, string categoryName)
        {
            return _current.IsCategory(name, categoryName);
        }


        /// <summary>
        /// Define the symbol on the current scope.
        /// </summary>
        /// <param name="symbol"></param>
        public void Define(Symbol symbol)
        {
            _current.Define(symbol);
        }


        /// <summary>
        /// Define the symbol within current scope with default type of object
        /// </summary>
        /// <param name="name">Name of the varaible</param>
        public void DefineVariable(string name)
        {
            _current.DefineVariable(name, LTypes.Object);
        }
        
        
        /// <summary>
        /// Define the symbol within current scope.
        /// </summary>
        /// <param name="name">Name of the varaible</param>
        /// <param name="type">The type of the variable</param>
        public void DefineVariable(string name, LType type)
        {
            _current.DefineVariable(name, type);
        }


        /// <summary>
        /// Defines an alias name for the existing name supplied.
        /// </summary>
        /// <param name="existingName">The existing symbol name to define the alias for</param>
        /// <param name="alias">The alias</param>
        public void DefineAlias(string alias, string existingName)
        {
            _current.DefineAlias(alias, existingName);
        }


        /// <summary>
        /// Define the symbol within this scope.
        /// </summary>
        /// <param name="name">Name of the constant</param>
        /// <param name="type">The type of the constant</param>
        /// <param name="value">The value of constant</param>
        public void DefineConstant(string name, LType type, object value)
        {
            _current.DefineConstant(name, type, value);
        }


        /// <summary>
        /// Define a function symbol within this scope.
        /// </summary>
        /// <param name="func">The function metadata</param>
        /// <param name="functionExpr">The function expression object that can execute the function</param>
        public virtual void DefineFunction(FunctionMetaData func, object functionExpr)
        {            
            _current.DefineFunction(func, functionExpr);
        }
    }
}
