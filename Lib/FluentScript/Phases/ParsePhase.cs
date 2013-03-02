﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ComLib.Lang.Core;
using ComLib.Lang.AST;
using ComLib.Lang.Parsing;
using ComLib.Lang.Helpers;

namespace ComLib.Lang.Phases
{
    /// <summary>
    /// Executes the code represented as an AST.
    /// </summary>
    public class ParsePhase : Phase
    {
        private Parser _parser;

        /// <summary>
        /// initializes this phase.
        /// </summary>
        public ParsePhase(Parser parser)
        {
            _parser = parser;
            this.Name = "ast-parsing";
        }


        /// <summary>
        /// Executes all the statements in the script.
        /// </summary>
        public override PhaseResult Execute(PhaseContext phaseCtx)
        {
            var script = phaseCtx.ScriptText;
            var memory = phaseCtx.Ctx.Memory;

            var runResult = LangHelper.Execute( () => 
            {
                this.Ctx.Limits.CheckScriptLength(script);
                _parser.Parse(script, memory);

                if (phaseCtx.Nodes == null)
                    phaseCtx.Nodes = new List<Expr>();

                if (phaseCtx.NodesStack == null)
                    phaseCtx.NodesStack = new List<List<Expr>>();

                // 1. Keep track of all the statements.
                phaseCtx.Nodes.AddRange(_parser.Statements);

                // 2. Keep track of the each individual set of statements ( from multiple scripts )
                phaseCtx.NodesStack.Add(_parser.Statements);
            });
            return new PhaseResult(runResult);
        }
    }
}
