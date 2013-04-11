using System;

namespace Fluentscript.Lib._Core
{
    /// <summary>
    /// Settings for the interpreter
    /// </summary>
    public class LangSettings
    {
        /// <summary>
        /// Intialize instances with default values.
        /// </summary>
        public LangSettings()
        {
        }


        /// <summary>
        /// Sets the limits to the defaults.
        /// </summary>
        public void DefaultLimits()
        {
            MaxConsequetiveExpressions = 7;
            MaxConsequetiveMemberAccess = 5;
            MaxExceptions = 10;
            MaxFuncParams = 10;
            MaxLoopLimit = 200;
            MaxCallStack = 15;
            MaxStatements = 200;
            MaxScriptLength = 20000;
        }


        /// <summary>
        /// The folder where the javascript based plugins are located.
        /// </summary>
        public string PluginsFolder;


        /// <summary>
        /// The starting char that signifies an interpolated string #{...}
        /// </summary>
        public char InterpolatedStartChar = '#';


        /// <summary>
        /// Whether or not to enable printing via print/println function.
        /// </summary>
        public bool EnablePrinting;


        /// <summary>
        /// Whether or not to enable logging via the log.* methods.
        /// </summary>
        public bool EnableLogging;


        /// <summary>
        /// Whether or not to enable callbacks for external function calls.
        /// </summary>
        public bool EnableFunctionCallCallBacks;


        /// <summary>
        /// The callback to use log functions, default uses the console.writeline.
        /// </summary>
        public Action LogCallback;


        /// <summary>
        /// Limits the maximimum number of characters for a string.
        /// </summary>
        public int MaxStringLength = 1000;


        /// <summary>
        /// Limits the maximum number of consequetive member access.
        /// </summary>
        public int MaxConsequetiveMemberAccess = -1;


        /// <summary>
        /// Limits the maximum number of consequetive expressions
        /// </summary>
        public int MaxConsequetiveExpressions = -1;


        /// <summary>
        /// Maximum number of variables in scope.
        /// </summary>
        public int MaxScopeVariables = -1;


        /// <summary>
        /// Maximum length of all string variables in scope.
        /// </summary>
        public int MaxScopeStringVariablesLength = -1;


        /// <summary>
        /// Limits the maximum number of loops. This is to prevent infinite loops.
        /// </summary>
        public int MaxLoopLimit = -1;


        /// <summary>
        /// Limits the maximum number of statements allowed.
        /// </summary>
        public int MaxStatements = -1;


        /// <summary>
        /// Number of maximum nested statements.
        /// </summary>
        public int MaxStatementsNested = -1;


        /// <summary>
        /// Limits the number of recursive function calls.
        /// </summary>
        public int MaxCallStack = -1;


        /// <summary>
        /// Limits the number of exceptions that can occur.
        /// </summary>
        public int MaxExceptions = -1;


        /// <summary>
        /// Limits the number of function parameters.
        /// </summary>
        public int MaxFuncParams = -1;


        /// <summary>
        /// Limits the number of function calls as parameters.
        /// </summary>
        public int MaxFuncCallNested = -1;


        /// <summary>
        /// Limits the length of the script to run.
        /// </summary>
        public int MaxScriptLength = -1;



        /// <summary>
        /// Whether or not to enable fluent mode for javascript.
        /// This allows several variations on syntax for method calls.
        /// </summary>
        public bool EnableFluentMode;


        /// <summary>
        /// Whether there is a maximum loop limit.
        /// </summary>
        public bool HasMaxLoopLimit
        {
            get { return MaxLoopLimit > -1; }
        }
    }
}
