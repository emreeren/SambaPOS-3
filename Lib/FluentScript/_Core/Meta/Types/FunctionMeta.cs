using System;
using System.Collections.Generic;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core.Meta.Docs;

namespace Fluentscript.Lib._Core.Meta.Types
{
    /// <summary>
    /// Meta data about function.
    /// </summary>
    public class FunctionMetaData
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public FunctionMetaData()
        {
        }


        /// <summary>
        /// Initailize
        /// </summary>
        /// <param name="name"></param>
        /// <param name="argNames"></param>
        public FunctionMetaData(string name, List<string> argNames)
        {            
            Init(name, argNames);
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="name"></param>
        /// <param name="argNames"></param>
        public void Init(string name, List<string> argNames)
        {
            this.Name = name;
            this.Arguments = new List<ArgAttribute>();
            this.ArgumentNames = new Dictionary<string, string>();
            this.ArgumentsLookup = new Dictionary<string, ArgAttribute>();

            if (argNames != null && argNames.Count > 0)
            {
                for(int ndx = 0; ndx < argNames.Count; ndx++)
                {
                    var argName = argNames[ndx];
                    var arg = new ArgAttribute() { Name = argName };
                    arg.Index = ndx;
                    this.Arguments.Add(arg);
                    this.ArgumentsLookup[argName] = arg;
                    this.ArgumentNames[argName] = argName;
                }
            }
        }


        /// <summary>
        /// Initailizes
        /// </summary>
        /// <param name="name">Name of the arg</param>
        /// <param name="desc">Description of the arg</param>
        /// <param name="alias">Alias for the arg</param>
        /// <param name="type">Datatype of the arg</param>
        /// <param name="required">Whether or not arg is required</param>
        /// <param name="defaultVal">Default value of arg</param>
        /// <param name="examples">Examples of arg</param>
        public void AddArg(string name, string type, bool required, string alias, object defaultVal, string examples, string desc)
        {
            var arg = new ArgAttribute();
            arg.Name = name;
            arg.Desc = desc;
            arg.Type = type;
            arg.Required = required;
            arg.DefaultValue = defaultVal;
            arg.Alias = alias;
            arg.Examples = new List<string>() { examples };
            arg.Index = this.Arguments.Count;
            this.Arguments.Add(arg);
            this.ArgumentsLookup[arg.Name] = arg;
            this.ArgumentsLookup[arg.Alias] = arg;
            this.ArgumentNames[arg.Name] = arg.Name;
            this.ArgumentNames[arg.Alias] = arg.Name;
        }


        /// <summary>
        /// Function declaration
        /// </summary>
        public string Name;


        /// <summary>
        /// The doc tags for this function.
        /// </summary>
        public DocTags Doc;


        /// <summary>
        /// The aliases for the function name.
        /// </summary>
        public List<string> Aliases;


        /// <summary>
        /// Lookup for all the arguments.
        /// </summary>
        public IDictionary<string, ArgAttribute> ArgumentsLookup;


        /// <summary>
        /// Lookup for all the arguments names
        /// </summary>
        public IDictionary<string, string> ArgumentNames;


        /// <summary>
        /// Names of the parameters
        /// </summary>
        public List<ArgAttribute> Arguments;


        /// <summary>
        /// Whether or not this function can be used as a suffix for a single parameter
        /// e.g. 3 hours
        /// </summary>
        public bool IsSuffixable { get; set; }


        /// <summary>
        /// Whether or not this function supports a wild card.
        /// </summary>
        public bool HasWildCard { get; set; }


        /// <summary>
        /// The version of the function. defaulted to 1.0.0.0.
        /// This to enable having multiple versions of a function ( future feature )
        /// </summary>
        public Version Version { get; set; }


        /// <summary>
        /// The return type of the function.
        /// </summary>
        public LType ReturnType { get; set; }


        /// <summary>
        /// Total arguments
        /// </summary>
        public int TotalArgs
        {
            get { return Arguments == null ? 0 : Arguments.Count; }
        }


        public bool HasArguments()
        {
            return this.TotalArgs > 0;
        }


        /// <summary>
        /// Getst the total required arguments.
        /// </summary>
        /// <returns></returns>
        public int GetTotalRequiredArgs()
        {
            if (this.Arguments == null || this.Arguments.Count == 0) return 0;
            int totalRequired = 0;
            foreach (var arg in Arguments)
                if (arg.Required)
                    totalRequired++;
            return totalRequired;
        }
    }
}
