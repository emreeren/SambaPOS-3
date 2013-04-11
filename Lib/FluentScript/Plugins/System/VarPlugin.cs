using System.Collections.Generic;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Plugins.System
{
    /// <summary>
    /// Plugin for throwing errors from the script.
    /// </summary>
    public class VarPlugin : ExprPlugin, IParserCallbacks
    {
        /// <summary>
        /// Intialize.
        /// </summary>
        public VarPlugin()
        {
            this.ConfigureAsSystemStatement(false, true, "var,$IdToken");
            this.IsAutoMatched = false;
            this.Precedence = 1000;
        }


        /// <summary>
        /// Whether or not this can handle the current token.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public override bool CanHandle(Token current)
        {
            if (current == Tokens.Var) return true;
            var next = _tokenIt.Peek().Token;
            if (next == Tokens.Assignment) return true;

            return false;
        }


        /// <summary>
        /// Parses a assignment statement with declaration.
        /// </summary>
        /// <returns></returns>
        public override Expr Parse()
        {
            bool expectVar = _tokenIt.NextToken.Token == Tokens.Var;
            return ParseAssignment(expectVar, true, null);
        }


        /// <summary>
        /// Parses an assignment statement. 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Expr Parse(object context)
        {
            return ParseAssignment(false, false, context as Expr);
        }


        /// <summary>
        /// 1. var name;
        /// 2. var age = 21;
        /// 3. canDrink = age >= 21;
        /// 4. canVote = CanVote(age);
        /// </summary>
        /// <returns></returns>
        public Expr ParseAssignment(bool expectVar, bool expectId, Expr varExp)
        {
            var initiatorToken = _tokenIt.NextToken;
            var declarations = new List<AssignExpr>();
            if (expectVar) _tokenIt.Expect(Tokens.Var);
            if (expectId)
            {
                varExp = ParseVariable();
            }

            var assignToken = _tokenIt.NextToken;
            // Case 1: var name;
            if (_tokenIt.IsEndOfStmtOrBlock())
            {
                AddAssignment(expectVar, varExp, null, declarations, _tokenIt.NextToken);
                return Exprs.AssignMulti(expectVar, declarations, initiatorToken);
            }

            // Case 2: var name = <expression>
            Expr valueExp = null;
            assignToken = _tokenIt.NextToken;
            if (_tokenIt.NextToken.Token == Tokens.Assignment)
            {
                _tokenIt.Advance();
                valueExp = _parser.ParseExpression(Terminators.ExpVarDeclarationEnd, passNewLine: false);
            }

            AddAssignment(expectVar, varExp, valueExp, declarations, assignToken);
            if (_tokenIt.IsEndOfStmtOrBlock() || _tokenIt.NextToken.Token.Kind == TokenKind.Keyword)
                return Exprs.AssignMulti(expectVar, declarations, initiatorToken);

            // Case 3: Multiple var a,b,c; or var a = 1, b = 2, c = 3;
            _tokenIt.Expect(Tokens.Comma);

            while (true)
            {
                varExp = ParseVariable();
                valueExp = null;
                assignToken = _tokenIt.NextToken;
            
                // , or expression?
                if (_tokenIt.NextToken.Token == Tokens.Assignment)
                {
                    _tokenIt.Advance();
                    valueExp = _parser.ParseExpression(Terminators.ExpVarDeclarationEnd, passNewLine: false);
                }
                AddAssignment(expectVar, varExp, valueExp, declarations, assignToken);

                if (_tokenIt.IsEndOfStmtOrBlock())
                    break;

                _tokenIt.Expect(Tokens.Comma);
            }
            return Exprs.AssignMulti(expectVar, declarations, initiatorToken);
        }


        private VariableExpr ParseVariable()
        {
            var nameToken = _tokenIt.NextToken;
            var name = _tokenIt.ExpectId();
            return Exprs.Ident(name, nameToken) as VariableExpr;
        }

        
        private void AddAssignment(bool expectVar, Expr varExp, Expr valExp, List<AssignExpr> declarations, TokenData token)
        {
            var a = (AssignExpr)Exprs.Assign(expectVar, varExp, valExp, token);
            declarations.Add(a);
        }


        /// <summary>
        /// Called by the framework after the parse method is called
        /// </summary>
        /// <param name="node">The node returned by this implementations Parse method</param>
        public override void OnParseComplete(AstNode node)
        {
            this.ExpParser.OnParseAssignComplete(node as Expr);
        }
    }
}
