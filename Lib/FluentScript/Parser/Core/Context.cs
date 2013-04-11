using System.Collections.Generic;
using Fluentscript.Lib.Parser.Integration;
using Fluentscript.Lib.Parser.MetaPlugins;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser.Core
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
            this.Types = new RegisteredTypes();
            this.ExternalFunctions = new ExternalFunctions();
            this.Words = new RegisteredWords();
            this.Plugins = new RegisteredPlugins();
            this.PluginsMeta = new MetaPluginContainer();
            this.Symbols = new Symbols();
            this.Memory = new Memory();
            this.Limits = new Limits(this);
            this.TokenAliases = new Dictionary<string, Token>();
            var stack = new CallStack(Limits.CheckCallStack);
            this.Callbacks = new Callbacks();
            this.State = new LangState(stack);
            this.Units = new Units();
            this.Methods = new RegisteredMethods();
            this.Directives = new RegisteredDirectives();

            //this.Bindings = new Bindings();
            this.Plugins.Init();            
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
        /// All the combinators that extend parsing.
        /// </summary>
        public MetaPluginContainer PluginsMeta;


        /// <summary>
        /// Script functions
        /// </summary>
        //public RegisteredFunctions Functions;


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
        /// Preprocessor directives.
        /// </summary>
        public RegisteredDirectives Directives;


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
