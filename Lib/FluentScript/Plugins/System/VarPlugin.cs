using System;
using System.Collections.Generic;

// <lang:using>
using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Types;
using ComLib.Lang.Parsing;
using ComLib.Lang.Helpers;
// </lang:using>

namespace ComLib.Lang.Plugins
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
        /// The grammer for the function declaration
        /// </summary>
        public override string Grammer
        {
            get { return "var <id> ( '=' <expression> )? ( ',' <id> ( '=' <expression> )? )* <statementterminator>"; }
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
                    "var name;",
                    "var name, age;",
                    "var name = 'kishore', age = 33;",
                    "var name = 'kishore', age = getage('kishore');",
                    "var name = 'kishore', age;"
                };
            }
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
            return ParseAssignment(expectVar, true);
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
        public Expr ParseAssignment(bool expectVar, bool expectId = true, Expr varExp = null)
        {
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
                return new MultiAssignExpr(expectVar, declarations);
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
            if (_tokenIt.IsEndOfStmtOrBlock())
                return new MultiAssignExpr(expectVar, declarations);

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
            return new MultiAssignExpr(expectVar, declarations);
        }


        private VariableExpr ParseVariable()
        {
            var nameToken = _tokenIt.NextToken;
            var name = _tokenIt.ExpectId();
            return _parser.ToIdentExpr(name, nameToken) as VariableExpr;
        }

        
        private void AddAssignment(bool expectVar, Expr varExp, Expr valExp, List<AssignExpr> declarations, TokenData token)
        {
            var a = (AssignExpr)_parser.ToAssignExpr(expectVar, varExp, valExp, token);
            declarations.Add(a);
        }


        /// <summary>
        /// Called by the framework after the parse method is called
        /// </summary>
        /// <param name="node">The node returned by this implementations Parse method</param>
        public void OnParseComplete(AstNode node)
        {
            var stmt = node as MultiAssignExpr;
            if (stmt._assignments == null || stmt._assignments.Count == 0)
                return;
            foreach (var assignment in stmt._assignments)
            {
                var exp = assignment.VarExp;
                if (exp.IsNodeType(NodeTypes.SysVariable))
                {
                    var varExp = exp as VariableExpr;
                    var valExp = assignment.ValueExp;
                    var name = varExp.Name;
                    bool registeredTypeVar = false;
                    if(valExp != null && valExp.IsNodeType(NodeTypes.SysNew) )
                    {
                        var newExp = valExp as NewExpr;
                        if (this.Ctx.Types.Contains(newExp.TypeName))
                        {
                            var type = this.Ctx.Types.Get(newExp.TypeName);
                            var ltype = LangTypeHelper.ConvertToLangTypeClass(type);
                            this.Ctx.Symbols.DefineVariable(name, ltype);
                            registeredTypeVar = true;
                        }
                    }
                    if(!registeredTypeVar)
                        this.Ctx.Symbols.DefineVariable(name, LTypes.Object);
                }
            }
        }
    }
}
