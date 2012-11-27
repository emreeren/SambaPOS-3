using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.Types;
// </lang:using>

namespace ComLib.Lang.Parsing
{
    /// <summary>
    /// Context information for the script.
    /// </summary>
    public class Context
    {
        /// <summary>
        /// Creates new instance with default Functions/Types/Scope.
        /// </summary>
        public Context()
        {
            Types = new RegisteredTypes();
            ExternalFunctions = new ExternalFunctions();
            Functions = new RegisteredFunctions();
            Words = new RegisteredWords();
            Plugins = new RegisteredPlugins();
            Symbols = new Symbols();
            Memory = new Memory();
            Limits = new Limits(this);
            TokenAliases = new Dictionary<string, Token>();
            var stack = new CallStack(Limits.CheckCallStack);
            Callbacks = new Callbacks();
            State = new LangState(stack);
            Units = new Units();
            Methods = new RegisteredMethods();
            Plugins.Init();            
        }


        /// <summary>
        /// Registered custom functions outside of script
        /// </summary>
        public ExternalFunctions ExternalFunctions;


        /// <summary>
        /// All the combinators that extend parsing.
        /// </summary>
        public RegisteredPlugins Plugins;


        /// <summary>
        /// Script functions
        /// </summary>
        public RegisteredFunctions Functions;


        /// <summary>
        /// Registered custom types
        /// </summary>
        public RegisteredTypes Types;


        /// <summary>
        /// Registered custom words that can be used in the script.
        /// </summary>
        public RegisteredWords Words;


        /// <summary>
        /// Map of all the registered methods.
        /// </summary>
        public RegisteredMethods Methods;


        /// <summary>
        /// Units e.g. feet, miles, gigabytes
        /// </summary>
        public Units Units;


        /// <summary>
        /// Token replacements map.
        /// </summary>
        public IDictionary<string, Token> TokenAliases;


        /// <summary>
        /// The symbol table.
        /// </summary>
        public Symbols Symbols;


        /// <summary>
        /// The memory space.
        /// </summary>
        public Memory Memory;


        /// <summary>
        /// Settings.
        /// </summary>
        public LangSettings Settings;


        /// <summary>
        /// Callbacks to external methods to notify them when a specific action completes.
        /// </summary>
        public Callbacks Callbacks;


        /// <summary>
        /// State of the language.
        /// </summary>
        public LangState State;


        /// <summary>
        /// Limits for 
        /// </summary>
        internal Limits Limits;
    }
}
