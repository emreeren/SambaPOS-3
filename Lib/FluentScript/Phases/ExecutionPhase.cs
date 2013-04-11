using System;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser.Integration;
using Fluentscript.Lib.Runtime;

namespace Fluentscript.Lib.Phases
{
    /// <summary>
    /// Executes the code represented as an AST.
    /// </summary>
    public class ExecutionPhase : Phase
    {
        private bool _execAllNodes;

        /// <summary>
        /// initializes this phase.
        /// </summary>
        public ExecutionPhase(bool execAllNodes)
        {
            _execAllNodes = execAllNodes;
            this.Name = "ast-execution";
        }


        /// <summary>
        /// Executes all the statements in the script.
        /// </summary>
        public override PhaseResult Execute(PhaseContext phaseCtx)
        {
            // 1. Check number of statements.
            var statements = _execAllNodes ? phaseCtx.Nodes : phaseCtx.NodesStack[phaseCtx.NodesStack.Count - 1];

            var now = DateTime.Now;

            // 2. No statements ? return
            if (statements == null || statements.Count == 0)
                return ToPhaseResult(now, now, true, "There are 0 nodes to execute");

            // 3. Execute the nodes and get the run-result which captures various data            
            var runResult = LangHelper.Execute(() =>
            {
                var execution = new Execution();
                execution.Ctx = this.Ctx;
                EvalHelper.Ctx = this.Ctx;
                execution.VisitExprs(statements);
            });

            // 4. Simply wrap the run-result ( success, message, start/end times )
            // inside of a phase result. 
            return new PhaseResult(runResult);
        }
    }
}
