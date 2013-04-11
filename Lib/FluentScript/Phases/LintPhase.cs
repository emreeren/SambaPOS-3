using System;
using Fluentscript.Lib.Parser;
using Fluentscript.Lib.Parser.Integration;

namespace Fluentscript.Lib.Phases
{
    /// <summary>
    /// Does a semantic check to validate the AST.
    /// </summary>
    public class LintPhase : Phase
    {
        private bool _execAllNodes;

        /// <summary>
        /// initializes this phase.
        /// </summary>
        public LintPhase(bool execAllNodes)
        {
            _execAllNodes = execAllNodes;
            this.Name = "ast-semantic-check";
        }


        /// <summary>
        /// Validates all the statements in the script.
        /// </summary>
        public override PhaseResult Execute(PhaseContext phaseCtx)
        {
            // 1. Check number of statements.
            var statements = _execAllNodes ? phaseCtx.Nodes : phaseCtx.NodesStack[phaseCtx.NodesStack.Count - 1];

            var now = DateTime.Now;

            // 2. No statements ? return
            if (statements == null || statements.Count == 0)
                return ToPhaseResult(now, now, true, "There are 0 nodes to lint");

            var semacts = new SemActs();
            var result = semacts.Validate(statements);

            // 4. Simply wrap the run-result ( success, message, start/end times )
            // inside of a phase result. 
            return new PhaseResult(result);
        }
    }
}
