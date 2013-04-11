using System;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser.Integration;

namespace Fluentscript.Lib.Phases
{
    /// <summary>
    /// Executes the code represented as an AST.
    /// </summary>
    public class TranslateToJsPhase : Phase
    {
        /// <summary>
        /// Initialize phase.
        /// </summary>
        public TranslateToJsPhase()
        {
            this.Name = "ast-translation-to-javascript";
        }


        /// <summary>
        /// Executes all the statements in the script.
        /// </summary>
        public override PhaseResult Execute(PhaseContext phaseCtx)
        {
            // 1. Check number of statements.
            var statements = phaseCtx.Nodes;

            var now = DateTime.Now;

            // 2. No statements ? return
            if (statements == null || statements.Count == 0)
                return ToPhaseResult(now, now, true, "There are 0 nodes to execute");

            // 3. Execute the nodes and get the run-result which captures various data            
            var runResult = LangHelper.Execute(() =>
            {
                foreach (var stmt in statements)
                {
                    
                }
            });

            // 4. Simply wrap the run-result ( success, message, start/end times )
            // inside of a phase result. 
            return new PhaseResult(runResult);
        }
    }
}
