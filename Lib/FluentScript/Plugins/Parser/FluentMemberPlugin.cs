using System;
using System.Collections.Generic;
using System.Reflection;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Fluent Member plugin allows properties and methods to be accessed without "."
   
    // Supported ways of calling methods:
    // 1. class     property    
    // 2. class     method      
    // 3. instance  property
    // 4. instance  method 
    // 5. prop      class       
    // 6. prop      instance    
    // 7. method    class       
    // 8. method    instance
    
    // NOTE: In the above list "class" designates access to class level/static members
     
    
    // FLUENT CALLS
    // Example 1 : method instance
    activate user 'kreddy'   
     
    // Example 2 : method class arg
    delete file c:\temp.txt
    
    // Example 3 : class method arg
    file exists c:\temp.txt
    
    </doc:example>
    ***************************************************************************/
    /// <summary>
    /// Represents one of the parts of a fluent function/method call.
    /// </summary>
    public enum FluentPart
    {
        /// <summary>
        /// Fluent part not applicable.
        /// </summary>
        None,


        /// <summary>
        /// Class name
        /// </summary>
        Class,


        /// <summary>
        /// Instance of a class ( variable )
        /// </summary>
        Instance,


        /// <summary>
        /// Property
        /// </summary>
        Prop,


        /// <summary>
        /// Method name
        /// </summary>
        Method,


        /// <summary>
        /// Parameter list
        /// </summary>
        Params
    }



    /// <summary>
    /// Combinator for handles method/function calls in a more fluent way.
    /// </summary>
    public class FluentMemberPlugin : ExprPlugin
    {
        private static string[] _tokens = new string[] { "$IdToken" };
        private static Dictionary<string, List<List<FluentPart>>> _matches = new Dictionary<string, List<List<FluentPart>>>();
        private readonly IDictionary<Token, bool> ExpFluentFuncExpEnd = new Dictionary<Token, bool>()
        {
            { Tokens.Comma, true },
            { Tokens.Semicolon, true }
        };
        

        /// <summary>
        /// Set up the possible combinations of fluent method calls.
        /// </summary>
        static FluentMemberPlugin()
        {
            _matches["class"] = new List<List<FluentPart>>();
            _matches["prop"] = new List<List<FluentPart>>();
            _matches["method"] = new List<List<FluentPart>>();
            _matches["instance"] = new List<List<FluentPart>>();

            _matches["class"].Add(new List<FluentPart>() { FluentPart.Class, FluentPart.Prop });
            _matches["class"].Add(new List<FluentPart>() { FluentPart.Class, FluentPart.Method });

            _matches["prop"].Add(new List<FluentPart>() { FluentPart.Prop, FluentPart.Class });
            _matches["prop"].Add(new List<FluentPart>() { FluentPart.Prop, FluentPart.Instance });

            _matches["method"].Add(new List<FluentPart>() { FluentPart.Method, FluentPart.Class });
            _matches["method"].Add(new List<FluentPart>() { FluentPart.Method, FluentPart.Instance });

            _matches["instance"].Add(new List<FluentPart>() { FluentPart.Instance, FluentPart.Prop });
            _matches["instance"].Add(new List<FluentPart>() { FluentPart.Instance, FluentPart.Method });
        }


        /// <summary>
        /// Initialize.
        /// </summary>
        public FluentMemberPlugin() : this( false)
        {
        }


        /// <summary>
        /// Initialize.
        /// </summary>
        public FluentMemberPlugin(bool enableMethodPartMatching)
        {
            this.Precedence = 200;
            this.IsStatement = true;
            this.StartTokens = _tokens;
            this.IsAssignmentSupported = true;
        }


        /// <summary>
        /// This can not handle all idtoken based expressions.
        /// Only expressions with idtoken followed by idtoken.
        /// e.g.
        /// OK:
        ///     - delete file "c:\temp.txt".
        ///     - file delete "c:\temp.txt".
        /// NOT OK:
        ///     - file.delete("c:\temp.txt");
        ///     - delete("c:\temp.txt");
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            if (!(current.Kind == TokenKind.Ident)) return false;

            // 1. activate user                 =>  idtoken idtoken         => user.activate();
            // 2. delete file 'c:\temp.txt'     =>  idtoken idtoken         => file.delete('c:\temp.txt');
            // 3. if file 'c:\temp.txt' exists  =>  idtoken <arg1> idtoken  => if file.exists('c:\temp.txt')
            var next = _tokenIt.Peek(1, false);

            // Following cases are explicit method | member access calls.
            // Case 1: user.
            // Case 2: user(
            if (!(next.Token.Kind == TokenKind.Ident)) return false;

            // Finally check if the next token is not it self a designator of a plugin.
            // This is a smarter check and will prevent fluent plugin from taking over
            // expressions like "day is Monday" where is = "==" and Monday is a replacement.
            if (_parser.Context.Plugins.ContainsExp(next.Token.Text)) return false;
            if (_parser.Context.PluginsMeta.ContainsTok(next.Token, 1)) return false;
            if (_parser.Context.Symbols.IsFunc(current.Text)) return false;

            // Now check consequitive items ( 2 at most )
            // class
            return true;
        }


        /* ****************************************************************************************
         * The following syntax can be supported via this Fluent expression combinator
         * 
         * 1. activate user.         
         * 2. move file "c:\temp.txt".
         * 3. file "c:\temp.txt" exists.
         * 3. run program "msbuild.exe", solution: 'comlib.sln', mode: "debug", style: "rebuild all".
         * 
         * 1. <method> <class> .
         * 2. <method> <class> <arg1> <arg2> .
         * 3. <class>  <arg1>  <method> .
         * 3. <method> <class> <arg1>, <arg1_name> : <arg1_value>, <arg2_name> : <arg2_value> .
         * 
        ***************************************************************************************** */
        /// <summary>
        /// Parses the fluent expression.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var ctx = _parser.Context;
            var helper = new FluentPluginHelper(ctx);
            var token = _tokenIt.NextToken;
            Expr mexp = null;
            Tuple<Type, Expr> matchResult = null;

            // 1. Check if instance variable.
            if (helper.IsInstance(token.Token))
            {
                matchResult = Match("instance", FluentPart.Instance);
                mexp = matchResult.Item2;
            }
            // 2. Check if class 
            else if (helper.IsClass(token.Token))
            {
                matchResult = Match("class", FluentPart.Class);
                mexp = matchResult.Item2;
            }
            // 3. Has to be method or prop
            else
            {
                matchResult = Match("method", FluentPart.Method);
                mexp = matchResult.Item2;
                if(mexp == null)
                {
                    matchResult = Match("prop", FluentPart.Prop);
                    mexp = matchResult.Item2;
                }
            }
            if (mexp != null )
            {
                // TODO: Performance improvement.
                var memExp = mexp as MemberAccessExpr;
                if (helper.IsClassMethod(matchResult.Item1, memExp.MemberName) || helper.IsInstanceMethod(matchResult.Item1, memExp.MemberName))
                {
                    mexp = ParseParams(mexp);    
                }                
            }
            return mexp;
        }


        private Tuple<Type, Expr>  Match(string groupName, FluentPart part)
        {
            var group = _matches[groupName];            
            var ctx = _parser.Context;
            var helper = new FluentPluginHelper(ctx);
            Expr expr = null;
            Type type = null;

            // 1. Go through all the matches in group. e.g. "class" 
            // Note: This is at most 4 possible groups ( class, instance, property, method )
            for (int ndx = 0; ndx < group.Count; ndx++)
            {
                var match = group[ndx];
                Token klass = null, instance = null, method = null, prop = null;
                var token = _tokenIt.NextToken.Token;
            
                // 2. Go through all the matches in each group
                // Note: There are at most 4 matches.
                int lastTokenPeek = 0;
                for (int ndxT = 0; ndxT < match.Count; ndxT++)
                {
                    part = match[ndxT];
                    if (part == FluentPart.Class) klass = token;
                    else if (part == FluentPart.Instance) instance = token;
                    else if (part == FluentPart.Method) method = token;
                    else if (part == FluentPart.Prop) prop = token;

                    lastTokenPeek = ndxT;
                    token = _tokenIt.Peek(lastTokenPeek + 1, false).Token;
                }
                // Check if match.
                type = helper.GetType(klass, instance);
                if (type != null)
                {
                    var result = IsMatch(helper, type, klass, instance, prop, method);

                    // Success
                    if (result.Item1)
                    {
                        expr = result.Item2;
                        _tokenIt.Advance(lastTokenPeek + 1);
                        break;
                    }
                }
            }
            return new Tuple<Type, Expr>(type, expr);
        }


        private Tuple<bool, Expr> IsMatch(FluentPluginHelper helper, Type type, Token klass, Token instance, Token prop, Token method)
        {
            var memberName = string.Empty;
            var rootVar = string.Empty;
            var match = false;
            var nameToken = klass;

            // 1. Class property
            if (klass != null && prop != null)
            {
                rootVar = klass.Text;
                if (helper.IsClassProp(type, prop.Text))
                {
                    memberName = prop.Text;
                    match = true;
                }
            }
            // 2. Class method
            else if (klass != null && method != null)
            {
                rootVar = type.Name;
                if (helper.IsClassMethod(type, method.Text))
                {   
                    memberName = method.Text;
                    match = true;
                }
            }
            // 3. Instance property
            else if (instance != null && prop != null)
            {
                rootVar = instance.Text;
                if (helper.IsInstanceProp(type, prop.Text))
                {
                    memberName = prop.Text;
                    match = true;
                    nameToken = instance;
                }
            }
            // 4. Instance method
            else if (instance != null && method != null)
            {
                rootVar = instance.Text;
                if (helper.IsInstanceMethod(type, method.Text))
                {
                    memberName = method.Text;
                    match = true;
                    nameToken = instance;
                }
            }
            if (!match)
                return new Tuple<bool, Expr>(false, null);

            var varExp = Exprs.Ident(rootVar, null);
            var memExp = Exprs.MemberAccess(varExp, memberName, false, null);
            return new Tuple<bool, Expr>(memberName != null, memExp);
        }


        private Expr ParseParams(Expr exp)
        {
            return _parser.ParseFuncExpression(exp, null);
        }


        private bool IsEnd(Token token = null, bool peek = false, int peekLevel = 1)
        {
            if (token == null) token = _tokenIt.NextToken.Token;
            if (token == Tokens.Semicolon) return true;
            if (token == Tokens.EndToken) return true;
            if (token == Tokens.NewLine) return true;
            return false;
        }
    }



    /// <summary>
    /// Helper class for the fluent plugin
    /// </summary>
    public class FluentPluginHelper
    {
        private Context _ctx;

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="ctx"></param>
        public FluentPluginHelper(Context ctx)
        {
            _ctx = ctx;
        }


        /// <summary>
        /// Get the type based on either class name or instance variable.
        /// </summary>
        /// <param name="klass"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public Type GetType(Token klass, Token instance)
        {
            // Check if match.
            Type type = null;
            if (klass != null)
            {
                // Case 1: Check if class name e..g "File" exists in the registered types.
                if (_ctx.Types.Contains(klass.Text))
                    type = _ctx.Types.Get(klass.Text);

                // Case 2: Class name is "File" but used as "file" in script, in a case insensitive way.
                // This is ok, as long as there is NOT another instance variable with the same name.
                else if(!_ctx.Symbols.Contains(klass.Text))
                {
                    var name = Char.ToUpper(klass.Text[0]) + klass.Text.Substring(1);
                    if (_ctx.Types.Contains(name))
                        type = _ctx.Types.Get(name);
                }
            }
            if (instance != null)
            {
                var sym = _ctx.Symbols.GetSymbol(instance.Text);
                if( sym != null ) 
                    type = _ctx.Types.Get(sym.DataTypeName);
            }
            return type;
        }


        /// <summary>
        /// Returns whether or not the token represents a reference to a Class
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool IsClass(Token token)
        {
            if (_ctx.Types.Contains(token.Text)) return true;
            if (_ctx.Symbols.IsVar(token.Text)) return false;

            // Try converting the first char to uppercase
            var name = Char.ToUpper(token.Text[0]) + token.Text.Substring(1);
            if (_ctx.Types.Contains(name)) return true;

            return false;
        }


        /// <summary>
        /// Returns whether or not the token represents a reference to a class instance
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool IsInstance(Token token)
        {
            var sym = _ctx.Symbols.GetSymbol(token.Text);
            if (sym == null) return false;
            if (_ctx.Types.Contains(sym.DataTypeName)) return true;
            return false;
        }


        /// <summary>
        /// Returns whether or not the token represents an argument.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool IsArg(Token token)
        {
            if (token.IsLiteralAny()) return true;
            if (token.Kind == TokenKind.Ident && _ctx.Symbols.IsVar(token.Text)) return true;

            return false;
        }


        /// <summary>
        /// Whether or not the member name supplied is an class property of the type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public bool IsClassProp(Type type, string propName)
        {
            return IsMember(type, propName, true, MemberTypes.Property);
        }


        /// <summary>
        /// Whether or not the member name supplied is an instance property of the type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public bool IsInstanceProp(Type type, string propName)
        {
            return IsMember(type, propName, false, MemberTypes.Property);
        }


        /// <summary>
        /// Whether or not the member name supplied is an class method of the type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public bool IsClassMethod(Type type, string memberName)
        {
            return IsMember(type, memberName, true, MemberTypes.Method);
        }


        /// <summary>
        /// Whether or not the member name supplied is an instance method of the type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public bool IsInstanceMethod(Type type, string memberName)
        {
            return IsMember(type, memberName, false, MemberTypes.Method);
        }


        /// <summary>
        /// Whether or not the member supplied exists on the type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="memberName"></param>
        /// <param name="isStatic"></param>
        /// <param name="memberType"></param>
        /// <returns></returns>
        public bool IsMember(Type type, string memberName, bool isStatic, MemberTypes memberType)
        {
            BindingFlags staticOrInstance = isStatic ? BindingFlags.Static : BindingFlags.Instance;
            var members = type.GetMember(memberName, BindingFlags.Public | staticOrInstance | BindingFlags.IgnoreCase);
            if (members == null || members.Length == 0)
                return false;
            if (members[0].MemberType == memberType)
                return true;

            return false;
        }
    }
}