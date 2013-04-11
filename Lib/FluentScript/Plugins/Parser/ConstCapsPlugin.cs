using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.Parser
{

    /* *************************************************************************
    <doc:example>	
    // Const caps plugin allows the creation of constants using capital letters
    // Constants are defined if all the letters are uppercase
    
    // Example 1
    MIN_SIZE = 10
    MAX_SIZE = 20
    
    // Example 2 : Multiple declarations with different constants.
    FIXED = true, DUE_DATE = "2012-5-10"
    
    // Example 3 : Using constants with other plugins ( Date, DateNumber )
    STARTS = 3/10/2012
    ENDS   = June 10th 2012
    
    // KNOWN ISSUE: Constants should be limited to numbers, bool, strings,
    // but right now there is a bug where a constant can be assigned a date.
    </doc:example>
    ***************************************************************************/
    // <fs:plugin-autogenerate>
    /// <summary>
    /// Combinator for handling days of the week.
    /// </summary>
    public class ConstCapsPlugin : ExprPlugin, IParserCallbacks
    {
        /// <summary>
        /// Initialize
        /// </summary>
        public ConstCapsPlugin()
        {
            this.StartTokens = new string[] { "$IdToken" };
            this.IsSystemLevel = true;
            this.IsStatement = true;
            this.IsEndOfStatementRequired = true;
            this.Precedence = 1;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "<IDENT> = <expression>";
            }
        }


        /// <summary>
        /// Examples
        /// </summary>
        public override string[] Examples
        {
            get
            {
                return new string[]
                {
                    "MIN_SIZE = 10",
                    "MAXIMUM = 20"
                };
            }
        }
        // </fs:plugin-autogenerate>


        /// <summary>
        /// Whether or not this parser can handle the supplied token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override bool CanHandle(Token token)
        {
            return IsConstMatch(token, 1);
        }



        private bool IsConstMatch(Token token, int peekLevel)
        {
            string toUpper = token.Text.ToUpper();
            if (string.Compare(token.Text, toUpper, StringComparison.CurrentCulture) != 0)
                return false;

            var n = _tokenIt.Peek(peekLevel);
            if (n.Token != Tokens.Assignment)
                return false;

            return true;
        }


        /// <summary>
        /// Parses the day expression.
        /// Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var pairs = new List<KeyValuePair<string, Expr>>();
            while (true)
            {
                // Get the const name and advance to the next token "="
                var constName = _tokenIt.ExpectId();
                _tokenIt.Expect(Tokens.Assignment);
                var exp = _parser.ParseExpression(Terminators.ExpVarDeclarationEnd);

                // Make sure it doesn't exist.                
                EnsureConstant(constName, exp);

                // Store assignment
                pairs.Add(new KeyValuePair<string, Expr>(constName, exp));

                // End of statement.
                if (_tokenIt.IsEndOfStmtOrBlock())
                {
                    // Do not need to expect end of statement since framework(parser)
                    // will handle that since this plugin has flag _supportsTerminator = true
                    break;
                }
                EnsureAdditionalConst();

                // Another const assignment at this point via ","
                _tokenIt.Advance();
            }
            return new ConstStmt(pairs);
        }


        /// <summary>
        /// Called by the framework after the parse method is called
        /// </summary>
        /// <param name="node">The node returned by this implementations Parse method</param>
        public override void OnParseComplete(AstNode node)
        {
            var constStmt = node as ConstStmt;            
            foreach (var pair in constStmt.Assignments)
            {
                var exp = pair.Value;
                if (exp.IsNodeType(NodeTypes.SysConstant))
                {
                    var constExp = pair.Value as ConstantExpr;
                    var constVal = constExp.Value as LObject;
                    _parser.Context.Symbols.DefineConstant(pair.Key, constVal.Type, constVal);
                }
            }
        }


        private void EnsureConstant(string constName, Expr exp)
        {
            var ctx = _parser.Context;
            if (ctx.Symbols.Contains(constName) && ctx.Symbols.IsConst(constName))
                throw _tokenIt.BuildSyntaxException("Can not reassign constant", exp);
            if (exp.IsNodeType(NodeTypes.SysNew))
            {
                var nexp = exp as NewExpr;
                if(nexp.TypeName != "Date" && nexp.TypeName != "Time" )
                    throw _tokenIt.BuildSyntaxException("Const : " + constName + " must have a const value");
            }
            //else if (!(exp.IsNodeType(NodeTypes.SysConstant)))
            //    throw _tokenIt.BuildSyntaxException("Const : " + constName + " must have a const value");                
        }


        private void EnsureAdditionalConst()
        {
            // must be comma if not end of statement
            if (_tokenIt.NextToken.Token != Tokens.Comma)
                throw _tokenIt.BuildSyntaxExpectedException("',' or end of statement");

            // "," at this point.
            // Make sure its another constant name ( in UPPERCASE )
            var peek = _tokenIt.Peek(1);
            if (!IsConstMatch(peek.Token, 2))
                _tokenIt.BuildSyntaxExpectedException("constant");
        }
    }



    /// <summary>
    /// A constant assignment
    /// </summary>
    public class ConstStmt : Expr
    {
        /// <summary>
        /// Initialize with pairs of constant assignment values.
        /// </summary>
        /// <param name="pairs"></param>
        public ConstStmt(List<KeyValuePair<string, Expr>> pairs)
        {
            Assignments = pairs;
        }

        /// <summary>
        /// Constant assignments.
        /// </summary>
        public List<KeyValuePair<string, Expr>> Assignments;


        /// <summary>
        /// Execute by storing the constant value in memory.
        /// </summary>
        public override object DoEvaluate(IAstVisitor visitor)
        {
            foreach (var pair in Assignments)
            {                
                object val = pair.Value.Evaluate(visitor);
                this.Ctx.Memory.SetValue(pair.Key, val);
            }
            return LObjects.Null;
        }
    }
}
