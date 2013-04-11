using System.Collections.Generic;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
using Fluentscript.Lib._Core.Meta.Types;

namespace Fluentscript.Lib.Parser
{
/// ------------------------------------------------------------------------------------------------
/// remarks: This file is auto-generated from the FSGrammar specification and should not be modified.
/// summary: This file contains all the AST for expressions at the system level.
///			features like control-flow e..g if, while, for, try, break, continue, return etc.
/// version: 0.9.8.10
/// author:  kishore reddy
/// date:	Wednesday, December 19, 2012
/// ------------------------------------------------------------------------------------------------
	public class ExprParser
	{
		public Parser _parser;

        /// <summary>
        /// Called by the framework after the parse method is called
        /// </summary>
        /// <param name="node">The node returned by this implementations Parse method</param>
        public void OnParseAssignComplete(Expr expr)
        {
            var stmt = expr as AssignMultiExpr;
            if (stmt.Assignments == null || stmt.Assignments.Count == 0)
                return;
            foreach (var assignment in stmt.Assignments)
            {
                var exp = assignment.VarExp;
                if (exp.IsNodeType(NodeTypes.SysVariable))
                {
                    var varExp = exp as VariableExpr;
                    var valExp = assignment.ValueExp;
                    var name = varExp.Name;
                    var registeredTypeVar = false;
                    var ctx = this._parser.Context;
                    if (valExp != null && valExp.IsNodeType(NodeTypes.SysNew))
                    {
                        var newExp = valExp as NewExpr;
                        if (ctx.Types.Contains(newExp.TypeName))
                        {
                            var type = ctx.Types.Get(newExp.TypeName);
                            var ltype = LangTypeHelper.ConvertToLangTypeClass(type);
                            ctx.Symbols.DefineVariable(name, ltype);
                            registeredTypeVar = true;
                        }
                    }
                    if (!registeredTypeVar)
                        ctx.Symbols.DefineVariable(name, LTypes.Object);
                }
            }
        }


        public Expr OnParseNew()
        {
            var tokenIt = this._parser.TokenIt;
            var initiatorToken = tokenIt.NextToken;
            var expr = new NewExpr();
            // <codeNew>

            tokenIt.Expect(Tokens.New);
            var typeName = tokenIt.ExpectId();

            // Keep parsing to capture full name.
            // e.g new App.Core.User()
            while (tokenIt.NextToken.Token == Tokens.Dot)
            {
                tokenIt.Advance();
                var name = tokenIt.ExpectId();
                typeName += "." + name;
                if (tokenIt.IsEndOfStmtOrBlock())
                    break;
            }
            expr.TypeName = typeName;
            expr.ParamListExpressions = new List<Expr>();
            expr.ParamList = new List<object>();
            this._parser.State.FunctionCall++;
            this._parser.ParseParameters(expr, true, false);
            this._parser.State.FunctionCall--;

            // </codeNew>
            this._parser.SetupContext(expr, initiatorToken);
            return expr;
        }


        public void OnParseNewComplete(Expr expr)
        {
        }


        public Expr OnParseBreak()
        {
            var tokenIt = this._parser.TokenIt;
            var initiatorToken = tokenIt.NextToken;
            var expr = new BreakExpr();
            // <codeBreak>
            tokenIt.Advance();
            // </codeBreak>
            this._parser.SetupContext(expr, initiatorToken);
            return expr;
        }


        public void OnParseBreakComplete(Expr expr)
        {
        }


        public Expr OnParseContinue()
        {
            var tokenIt = this._parser.TokenIt;
            var initiatorToken = tokenIt.NextToken;
            var expr = new ContinueExpr();
            // <codeContinue>
            tokenIt.Advance();
            // </codeContinue>
            this._parser.SetupContext(expr, initiatorToken);
            return expr;
        }


        public void OnParseContinueComplete(Expr expr)
        {
        }
		

		public Expr OnParseForEach()
		{
            var tokenIt = this._parser.TokenIt;
            var initiatorToken = tokenIt.NextToken;
            // <codeForEach>

            if (initiatorToken.Token == Tokens.For)
            {
                tokenIt.ExpectMany(Tokens.For, Tokens.LeftParenthesis);
                var ahead = tokenIt.Peek(1);
                if (ahead.Token != Tokens.In)
                    return OnParseFor();
            }

            var varname = tokenIt.ExpectId();
            tokenIt.Expect(Tokens.In);

		    var sourceExpr = _parser.ParseExpression(Terminators.ExpParenthesisNewLineEnd, true, false, true, false, false);
            tokenIt.Expect(Tokens.RightParenthesis);
            var expr = Exprs.ForEach(varname, sourceExpr, initiatorToken) as BlockExpr;
            this.ParseBlock(expr);
            
            // </codeForEach>
			this._parser.SetupContext(expr, initiatorToken);
			return expr;
		}


		public void OnParseForEachComplete(Expr expr)
		{
		}
		

		public Expr OnParseFor()
		{
            var tokenIt = this._parser.TokenIt;
            var initiatorToken = tokenIt.NextToken;
            // <codeFor>

            if(initiatorToken.Token == Tokens.For)
            {
                tokenIt.ExpectMany(Tokens.For, Tokens.LeftParenthesis);
                var ahead = tokenIt.Peek(1);
                if (ahead.Token == Tokens.In) 
                    return OnParseForEach();
            }

            var start = this._parser.ParseStatement();
            var condition = this._parser.ParseExpression(Terminators.ExpSemicolonEnd);
            tokenIt.Advance();
            var name = tokenIt.ExpectId();
            var increment = this._parser.ParseUnary(name, false);
            tokenIt.Expect(Tokens.RightParenthesis);
            var expr = Exprs.For(start, condition, increment, initiatorToken) as BlockExpr;
            this.ParseBlock(expr);

            // </codeFor>
            this._parser.SetupContext(expr, initiatorToken);
			return expr;
		}


		public void OnParseForComplete(Expr expr)
		{
		}
		

		public Expr OnParseFunctionDeclare()
		{
            var tokenIt = this._parser.TokenIt;
			var initiatorToken = tokenIt.NextToken;
			var expr = new FunctionDeclareExpr();
			// <codeFunctionDeclare>

            var token = tokenIt.NextToken;
            var expectToken = true;
            expr.Function = new FunctionExpr();
            expr.Function.Meta = new FunctionMetaData();
            _parser.SetupContext(expr.Function, token);

            if (expectToken) tokenIt.Expect(token.Token);
            else tokenIt.Advance();

            // Function name.
            var name = tokenIt.ExpectId(true, true);
            var aliases = new List<string>();
            var nextToken = tokenIt.NextToken;
            List<string> argNames = null;

            // Option 1: Wild card 
            if (nextToken.Token == Tokens.Multiply)
            {
                expr.Function.Meta.HasWildCard = true;
                nextToken = tokenIt.Advance();
            }
            // Option 2: Aliases
            else if (nextToken.Token == Tokens.Comma)
            {
                // Collect all function aliases
                while (nextToken.Token == Tokens.Comma)
                {
                    tokenIt.Advance();
                    var alias = tokenIt.ExpectId(true, true);
                    aliases.Add(alias);
                    nextToken = tokenIt.NextToken;
                }
                if (aliases.Count > 0)
                    expr.Function.Meta.Aliases = aliases;
            }

            // Get the parameters.
            if (nextToken.Token == Tokens.LeftParenthesis)
            {
                tokenIt.Expect(Tokens.LeftParenthesis);
                argNames = _parser.ParseNames();
                tokenIt.Expect(Tokens.RightParenthesis);
            }
            expr.Function.Meta.Init(name, argNames);

            // Now parser the function block.
            OnParseFunctionDeclareBlock(expr.Function);

            // </codeFunctionDeclare>
			this._parser.SetupContext(expr, initiatorToken);
			return expr;
		}


        /// <summary>
        /// Parses a block by first pushing symbol scope and then popping after completion.
        /// </summary>
        public void OnParseFunctionDeclareBlock(BlockExpr expr)
        {        
            var fs = expr as FunctionExpr;
            var funcName = fs.Meta.Name;
            
            // 1. Define the function in global symbol scope
            var funcSymbol = new SymbolFunction(fs.Meta);
            funcSymbol.FuncExpr = expr;

            this._parser.Context.Symbols.Define(funcSymbol);

            // 2. Define the aliases.
            if (fs.Meta.Aliases != null && fs.Meta.Aliases.Count > 0)
                foreach (var alias in fs.Meta.Aliases)
                    this._parser.Context.Symbols.DefineAlias(alias, fs.Meta.Name);
            
            // 3. Push the current scope.
            expr.SymScope = this._parser.Context.Symbols.Current;
            this._parser.Context.Symbols.Push(new SymbolsFunction(fs.Meta.Name), true);

            // 4. Register the parameter names in the symbol scope.
            if( fs.Meta.Arguments != null && fs.Meta.Arguments.Count > 0)
                foreach(var arg in fs.Meta.Arguments)
                    this._parser.Context.Symbols.DefineVariable(arg.Name, LTypes.Object);

            this._parser.ParseBlock(expr);
            this._parser.Context.Symbols.Pop();
        }


		public void OnParseFunctionDeclareComplete(Expr expr)
		{
            var function = (expr as FunctionDeclareExpr).Function;

            // 1. Register the function as a symbol
            this._parser.Context.Symbols.DefineFunction(function.Meta, function);

            // 2. Now register the aliases
            if (function.Meta.Aliases != null && function.Meta.Aliases.Count > 0)
            {
                foreach (string alias in function.Meta.Aliases)
                {
                    this._parser.Context.Symbols.DefineAlias(alias, function.Meta.Name);
                }
            }
		}
		

		public Expr OnParseIf()
        {
            var tokenIt = this._parser.TokenIt;
            var initiatorToken = tokenIt.NextToken;
            var expr = new IfExpr();
            // <codeIf>
            tokenIt.Expect(Tokens.If);

            // Parse the if
            this.ParseConditionalBlock(expr);
            tokenIt.AdvancePastNewLines();

            // Handle "else if" and/or else
            if (tokenIt.NextToken.Token == Tokens.Else)
            {
                // tokenIt.NextToken = "else"
                tokenIt.Advance();
                tokenIt.AdvancePastNewLines();

                // What's after else? 
                // 1. "if"      = else if statement
                // 2. "{"       = multi  line else
                // 3. "nothing" = single line else
                // Peek 2nd token for else if.
                var token = tokenIt.NextToken;
                if (tokenIt.NextToken.Token == Tokens.If)
                {
                    expr.Else = OnParseIf() as BlockExpr;
                }
                else // Multi-line or single line else
                {
                    var elseStmt = new BlockExpr();
                    this._parser.ParseBlock(elseStmt);
                    this._parser.SetupContext(elseStmt, token);
                    expr.Else = elseStmt;
                }
            }
            // </codeIf>
			this._parser.SetupContext(expr, initiatorToken);
			return expr;
		}


		public void OnParseIfComplete(Expr expr)
		{
		}
		

		public Expr OnParseLambda()
		{
            var tokenIt = this._parser.TokenIt;
			var initiatorToken = tokenIt.NextToken;
			// <codeLambda>

            // Check for lambda or function declare.
            var next = tokenIt.Peek();
            if (next.Token != Tokens.LeftParenthesis)
            {
                return OnParseFunctionDeclare();
            }

            // This a lambda.
            var expr = new LambdaExpr();
            var funcExp = new FunctionExpr();
            expr.Expr = funcExp;
            expr.Expr.Meta = new FunctionMetaData();
            this._parser.SetupContext(funcExp, initiatorToken);
            var name = "anon_" + initiatorToken.Line + "_" + initiatorToken.LineCharPos;
            tokenIt.Advance();
            tokenIt.Expect(Tokens.LeftParenthesis);
            var argnames = _parser.ParseNames();
            funcExp.Meta.Init(name, argnames);
            tokenIt.Expect(Tokens.RightParenthesis);
            this.OnParseLambdaBlock(funcExp);
            
            // </codeLambda>
			this._parser.SetupContext(expr, initiatorToken);
			return expr;
		}


        public void OnParseLambdaBlock(Expr expr)
        {
            var fs = expr as FunctionExpr;

            // 1. Define the function in global symbol scope
            var funcSymbol = new SymbolFunction(fs.Meta);
            funcSymbol.FuncExpr = expr;

            // 2. Push the current scope.
            expr.SymScope = this._parser.Context.Symbols.Current;
            this._parser.Context.Symbols.Push(new SymbolsFunction(string.Empty), true);

            // 3. Parse the function block
            this._parser.ParseBlock(fs);

            // 4. Pop the symbols scope.
            this._parser.Context.Symbols.Pop();
        }


		public void OnParseLambdaComplete(Expr expr)
		{
            if (expr.Nodetype == NodeTypes.SysFunctionDeclare)
                this.OnParseFunctionDeclareComplete(expr);
		}
		

		public Expr OnParseReturn()
		{
            var tokenIt = this._parser.TokenIt;
            var initiatorToken = tokenIt.NextToken;
            var expr = new ReturnExpr();
            // <codeReturn>

            tokenIt.Expect(Tokens.Return);
            if (tokenIt.IsEndOfStmtOrBlock())
                return expr;

            var exp = this._parser.ParseExpression(Terminators.ExpStatementEnd, passNewLine: false);
            expr.Exp = exp;

            // </codeReturn>
			this._parser.SetupContext(expr, initiatorToken);
			return expr;
		}


		public void OnParseReturnComplete(Expr expr)
		{
		}
		

		public Expr OnParseThrow()
		{
            var tokenIt = this._parser.TokenIt;
            var initiatorToken = tokenIt.NextToken;
			var expr = new ThrowExpr();
			// <codeThrow>

            tokenIt.Expect(Tokens.Throw);
            expr.Exp = this._parser.ParseExpression(Terminators.ExpStatementEnd, true, false, true, true, false);
            
            // </codeThrow>
			this._parser.SetupContext(expr, initiatorToken);
			return expr;
		}


		public void OnParseThrowComplete(Expr expr)
		{
		}
		

		public Expr OnParseTryCatch()
		{
            var tokenIt = this._parser.TokenIt;
            var initiatorToken = tokenIt.NextToken;
			var expr = new TryCatchExpr();
			// <codeTryCatch>

            // Try
            tokenIt.Expect(Tokens.Try);
            expr.Statements = new List<Expr>();
            this.ParseBlock(expr);
            tokenIt.AdvancePastNewLines();

            // Catch
            var catchToken = tokenIt.NextToken;
            tokenIt.ExpectMany(Tokens.Catch, Tokens.LeftParenthesis);
            expr.ErrorName = tokenIt.ExpectId();
            tokenIt.Expect(Tokens.RightParenthesis);
            expr.Catch = new BlockExpr();
            this.ParseBlock(expr.Catch);
            this._parser.SetupContext(expr.Catch, catchToken);
            
            // </codeTryCatch>
			this._parser.SetupContext(expr, initiatorToken);
			return expr;
		}


		public void OnParseTryCatchComplete(Expr expr)
		{
		}
		

		public Expr OnParseWhile()
		{
            var tokenIt = this._parser.TokenIt;
            var initiatorToken = tokenIt.NextToken;
            var expr = new WhileExpr();
            // <codeWhile>

            tokenIt.Expect(Tokens.While);
            ParseConditionalBlock(expr);

            // </codeWhile>
            this._parser.SetupContext(expr, initiatorToken);
			return expr;
		}


		public void OnParseWhileComplete(Expr expr)
		{
		}


        /// <summary>
        /// Parses a block by first pushing symbol scope and then popping after completion.
        /// </summary>
        public void ParseBlock(IBlockExpr expr)
        {
            this._parser.Context.Symbols.Push(new SymbolsNested(string.Empty), true);
            expr.SymScope = this._parser.Context.Symbols.Current;
            var withPush = false;

            if (expr is ForEachExpr)
            {
                var foreachExpr = expr as ForEachExpr;
                if (foreachExpr.EnableAutoVariable)
                {
                    var name = foreachExpr.VarName;
                    Exprs.WithPush(name);
                    withPush = true;
                }
            }
            this._parser.ParseBlock(expr);
            this._parser.Context.Symbols.Pop();
            if (withPush)
                Exprs.WithPop();
        }


        /// <summary>
        /// Parses a conditional block by first pushing symbol scope and then popping after completion.
        /// </summary>
        /// <param name="expr"></param>
        public void ParseConditionalBlock(ConditionalBlockExpr expr)
        {
            this._parser.Context.Symbols.Push(new SymbolsNested(string.Empty), true);
            expr.SymScope = this._parser.Context.Symbols.Current;
            this._parser.ParseConditionalStatement(expr);
            this._parser.Context.Symbols.Pop();
        }
	}
}