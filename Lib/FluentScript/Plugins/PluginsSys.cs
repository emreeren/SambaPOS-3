// ------------------------------------------------------------------------------------------------
// summary: This file contains individual parsers as plugins for parsing system level 
//			features like control-flow e..g if, while, for, try, break, continue, return etc.
// version: 0.9.8.10
// author:  kishore reddy
// date:	Wednesday, December 19, 2012
// ------------------------------------------------------------------------------------------------

using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.PluginSupport;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Plugins
{
        // Plugin: 15 - NewExpr
        public class NewPlugin : ExprPlugin
        {
            public NewPlugin()
            {
                this.ConfigureAsSystemStatement(false, false, "new");
                this.Precedence = 1;
            }

            public override Expr Parse()
            {
                return this.ExpParser.OnParseNew();
            }


            public override void OnParseComplete(AstNode node)
            {
                var expr = node as Expr;
                this.ExpParser.OnParseNewComplete(expr);
            }
        }


		// Plugin: 18 - BreakExpr
		public class BreakPlugin : ExprPlugin
		{
			public BreakPlugin()
			{				
				this.ConfigureAsSystemStatement(false, true, "break");
				this.Precedence = 1;
			}

			public override Expr Parse()
			{
				return this.ExpParser.OnParseBreak();
			}


			public override void OnParseComplete(AstNode node)
			{
				var expr = node as Expr;
				this.ExpParser.OnParseBreakComplete(expr);
			}
		}
	
		// Plugin: 19 - ContinueExpr
		public class ContinuePlugin : ExprPlugin
		{
			public ContinuePlugin()
			{				
				this.ConfigureAsSystemStatement(false, true, "continue");
				this.Precedence = 1;
			}

			public override Expr Parse()
			{
				return this.ExpParser.OnParseContinue();
			}


			public override void OnParseComplete(AstNode node)
			{
				var expr = node as Expr;
				this.ExpParser.OnParseContinueComplete(expr);
			}
		}
	
		// Plugin: 20 - ForEachExpr
		public class ForEachPlugin : ExprPlugin
		{
			public ForEachPlugin()
			{				
				this.ConfigureAsSystemStatement(true, false, "for");
				this.Precedence = 1;
			}

			public override Expr Parse()
			{
				return this.ExpParser.OnParseForEach();
			}


			public override void OnParseComplete(AstNode node)
			{
				var expr = node as Expr;
				this.ExpParser.OnParseForEachComplete(expr);
			}
		}
	
		// Plugin: 21 - ForExpr
		public class ForPlugin : ExprPlugin
		{
			public ForPlugin()
			{				
				this.ConfigureAsSystemStatement(true, false, "for");
				this.Precedence = 2;
			}

			public override Expr Parse()
			{
				return this.ExpParser.OnParseFor();
			}


			public override void OnParseComplete(AstNode node)
			{
				var expr = node as Expr;
				this.ExpParser.OnParseForComplete(expr);
			}
		}
	
		// Plugin: 22 - FunctionDeclareExpr
        public class FunctionDeclarePlugin : ExprPlugin, IParserCallbacks
		{
			public FunctionDeclarePlugin()
			{				
				this.ConfigureAsSystemStatement(true, false, "function");
				this.Precedence = 1;
			}

			public override Expr Parse()
			{
				return this.ExpParser.OnParseFunctionDeclare();
			}


			public override void OnParseComplete(AstNode node)
			{
				var expr = node as Expr;
				this.ExpParser.OnParseFunctionDeclareComplete(expr);
			}
		}
	
		// Plugin: 23 - IfExpr
		public class IfPlugin : ExprPlugin
		{
			public IfPlugin()
			{				
				this.ConfigureAsSystemStatement(true, false, "if");
				this.Precedence = 1;
			}

			public override Expr Parse()
			{
				return this.ExpParser.OnParseIf();
			}


			public override void OnParseComplete(AstNode node)
			{
				var expr = node as Expr;
				this.ExpParser.OnParseIfComplete(expr);
			}
		}
	
		// Plugin: 24 - LambdaExpr
		public class LambdaPlugin : ExprPlugin, IParserCallbacks
		{
			public LambdaPlugin()
			{				
				this.ConfigureAsSystemStatement(true, false, "function");
				this.Precedence = 1;
			}

			public override Expr Parse()
			{
				return this.ExpParser.OnParseLambda();
			}


			public override void OnParseComplete(AstNode node)
			{
				var expr = node as Expr;
				this.ExpParser.OnParseLambdaComplete(expr);
			}
		}
	
		// Plugin: 25 - ReturnExpr
		public class ReturnPlugin : ExprPlugin
		{
			public ReturnPlugin()
			{				
				this.ConfigureAsSystemStatement(false, true, "return");
				this.Precedence = 1;
			}

			public override Expr Parse()
			{
				return this.ExpParser.OnParseReturn();
			}


			public override void OnParseComplete(AstNode node)
			{
				var expr = node as Expr;
				this.ExpParser.OnParseReturnComplete(expr);
			}
		}
	
		// Plugin: 26 - ThrowExpr
		public class ThrowPlugin : ExprPlugin
		{
			public ThrowPlugin()
			{				
				this.ConfigureAsSystemStatement(false, true, "throw");
				this.Precedence = 1;
			}

			public override Expr Parse()
			{
				return this.ExpParser.OnParseThrow();
			}


			public override void OnParseComplete(AstNode node)
			{
				var expr = node as Expr;
				this.ExpParser.OnParseThrowComplete(expr);
			}
		}
	
		// Plugin: 27 - TryCatchExpr
		public class TryCatchPlugin : ExprPlugin
		{
			public TryCatchPlugin()
			{				
				this.ConfigureAsSystemStatement(true, false, "try");
				this.Precedence = 1;
			}

			public override Expr Parse()
			{
				return this.ExpParser.OnParseTryCatch();
			}


			public override void OnParseComplete(AstNode node)
			{
				var expr = node as Expr;
				this.ExpParser.OnParseTryCatchComplete(expr);
			}
		}
	
		// Plugin: 28 - WhileExpr
		public class WhilePlugin : ExprPlugin
		{
			public WhilePlugin()
			{				
				this.ConfigureAsSystemStatement(true, false, "while");
				this.Precedence = 1;
			}

			public override Expr Parse()
			{
				return this.ExpParser.OnParseWhile();
			}


			public override void OnParseComplete(AstNode node)
			{
				var expr = node as Expr;
				this.ExpParser.OnParseWhileComplete(expr);
			}
		}
	}

			