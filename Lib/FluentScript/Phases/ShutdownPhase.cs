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
    public class ShutdownPhase : Phase
    {
        /// <summary>
        /// initializes this phase.
        /// </summary>
        public ShutdownPhase()
        {
            this.Name = "interpreter-shutdown";
        }


        /// <summary>
        /// Executes all the statements in the script.
        /// </summary>
        public override PhaseResult Execute(PhaseContext phaseCtx)
        {
            var result = LangHelper.Execute(() => phaseCtx.Ctx.Plugins.Dispose());
            return new PhaseResult(result);
        }
    }
}
