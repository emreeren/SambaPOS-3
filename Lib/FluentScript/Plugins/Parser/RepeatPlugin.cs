using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Helpers;
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
    // Repeat plugin provides convenient ways to execute loops.        
    
    // Case 1: for(it = 1; it <= 10; it++ )
    repeat to 10 
	    print "hi"

	
    // Case 2: for(it = 2; it <= 10; it++ )		
    repeat 2 to 10 
	    print "hi"

	
    // Case 3: for(it = 2; it < 10; it++ )	
    repeat 2 to < 10 
	    print "hi"


    // Case 4: for(it = 2; it < 10; it+= 2 )	
    repeat 2 to < 10 by 2
	    print "hi"
	

    // Case 5: for( ndx = 0; ndx <= 10; ndx++ )
    repeat ndx to 10
	    print "hi"
	

    // Case 6: for( ndx = 0; ndx < 10; ndx++ )
    repeat ndx to < 10
	    print "hi"
    
    
    // Case 7: for( ndx = 1; ndx <= 10; ndx++ )
    repeat ndx = 1 to 10
	    print "hi"
	
	
    // Case 8: for( ndx = 1; ndx < 10; ndx++ )
    repeat ndx = 1 to < 10
	    print "hi"
	

    // Case 9: for( ndx = 1; ndx < 10; ndx+= 2)
    repeat ndx = 1 to < 10 by 2
	    print "hi"
    
    </doc:example>
    ***************************************************************************/

    /// <summary>
    /// Combinator for handling swapping of variable values. swap a and b.
    /// </summary>
    public class RepeatPlugin : ExprBlockPlugin
    {
        private static Dictionary<Token, bool> _terminatorForTo;
        private static Dictionary<Token, bool> _terminatorForBy;

        /// <summary>
        /// Intialize.
        /// </summary>
        public RepeatPlugin()
        {
            this.StartTokens = new string[] { "repeat" };
            this.IsStatement = true;
            this.IsAutoMatched = true;
            this.IsContextFree = false;
            this.Precedence = 50;

            _terminatorForTo = new Dictionary<Token, bool>();
            _terminatorForTo[TokenBuilder.ToIdentifier("to")] = true;
            _terminatorForTo[Tokens.Semicolon] = true;
            _terminatorForTo[Tokens.NewLine] = true;
            _terminatorForTo[Tokens.LeftBrace] = true;

            _terminatorForBy = new Dictionary<Token, bool>();
            _terminatorForBy[TokenBuilder.ToIdentifier("by")] = true;
            _terminatorForBy[Tokens.Semicolon] = true;
            _terminatorForBy[Tokens.NewLine] = true;
            _terminatorForBy[Tokens.LeftBrace] = true;
        }


        /// <summary>
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get
            {
                return "repeat ( ( <ident> = <expression> ) | <literal>)? to [symbol]? <expression> [by <expression>]? ";
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
                    "repeat to 10 { print 'hi' }",	
                    "repeat 2 to 10	{ print 'hi' }",
                    "repeat 2 to < 10 { print 'hi' }",
                    "repeat 2 to < 10 by 1 { print 'hi' }",
                    "repeat ndx to 10 { print 'hi' }",
                    "repeat ndx = 1 to 10 { print 'hi' }",
                    "repeat ndx = 1 to < 10 { print 'hi' }",
                    "repeat ndx = 1 to < 10 by 2 { print 'hi' }"
                };
            }
        }


        /// <summary>
        /// run step 123.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            var startToken = _tokenIt.NextToken;
            var conditionToken = startToken;
            var assignToken = startToken;
            var incToken = startToken;
            var loopToken = startToken;
            var varToken = startToken;

            _tokenIt.ExpectIdText("repeat");
            Expr varnameExpr = null;
            Expr startVal = null;
            Expr endVal = null;
            Expr incVal = null;
            Operator op = Operator.LessThanEqual;
            
            // Case 1: repeat to 10
            if (_tokenIt.NextToken.Token.Text == "to")
            {
                var result = ParseTo();
                startVal = Exprs.Const(new LNumber(1.0), startToken);
                op = result.Item1;
                endVal = result.Item2;
                incVal = result.Item3;
            }
            // Case 2: repeat 1 to 10
            else if (_tokenIt.NextToken.Token.Kind == TokenKind.LiteralNumber)
            {
                var num = _tokenIt.ExpectNumber();
                var result = ParseTo();
                startVal = Exprs.Const(new LNumber(num), startToken);
                op = result.Item1;
                endVal = result.Item2;
                incVal = result.Item3;
            }
            // Case 3: repeat ndx to 10
            else if (_tokenIt.NextToken.Token.Kind == TokenKind.Ident)
            {
                var variableName = _tokenIt.ExpectId();
                varnameExpr = Exprs.Ident(variableName, _tokenIt.LastToken);
                if (_tokenIt.NextToken.Token.Type == TokenTypes.Assignment)
                {
                    _tokenIt.Advance();

                    // Upto "to"
                    startVal = ParseExpr(_terminatorForTo);
                }
                else
                {
                    startVal = Exprs.Const(new LNumber(0), startToken);
                }
                var result = ParseTo();
                op = result.Item1;
                endVal = result.Item2;
                incVal = result.Item3;
            }
            // auto-create variable name.
            if (varnameExpr == null)
            {
                varnameExpr = Exprs.Ident("it", _tokenIt.LastToken);
            }            

            // Now setup the stmts
            var start     = Exprs.Assign(true, varnameExpr, startVal, varToken);
            var condition = Exprs.Compare(varnameExpr, op, endVal, varToken);            
            var incExp    = Exprs.Unary(varnameExpr.ToQualifiedName(), incVal, 0, Operator.PlusEqual, incToken);
            var incStmt   = Exprs.Assign(false, varnameExpr, incExp, assignToken);
            var loopStmt  = Exprs.For(start, condition, incStmt, loopToken);
            ParseBlock(loopStmt as BlockExpr);            
            return loopStmt;
        }


        private Tuple<Operator, Expr, Expr> ParseTo()
        {
            var op = Operator.LessThanEqual;
            _tokenIt.Advance();

            // Case 2: repeat to < 10
            if (_tokenIt.NextToken.Token.Kind == TokenKind.Symbol)
            {
                var opText = _tokenIt.NextToken.Token.Text;
                if (Operators.IsOp(opText))
                {
                    op = Operators.ToOp(opText);
                    _tokenIt.Advance();
                }
            }

            // Parse the end value (e.g. 10, total) 
            // EndTokens: "by", newline, ";", eos
            var currentToken = _tokenIt.NextToken;
            var end = ParseExpr(_terminatorForBy);   
            Expr incVal = null;

            // Check for increment value to 10 by 2
            if (_tokenIt.NextToken.Token.Text == "by")
            {
                _tokenIt.Advance();

                // EndTokens: newline, ";", eos
                incVal = ParseExpr(Terminators.ExpStatementEnd);
            }
            else
            {
                incVal = Exprs.Const(new LNumber(1), _tokenIt.NextToken);
            }
            return new Tuple<Operator, Expr, Expr>(op, end, incVal);
        }


        private Expr ParseExpr(IDictionary<Token, bool> terminators)
        {
            var exp = _parser.ParseExpression(terminators, true, true, true, false, true);
            return exp;
        }
    }
}
