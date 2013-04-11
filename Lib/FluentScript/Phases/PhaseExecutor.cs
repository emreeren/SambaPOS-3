using System;
using System.Collections.Generic;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib.Parser.Integration;

namespace Fluentscript.Lib.Phases
{
    /// <summary>
    /// Executes the different phases of the interpreter.
    /// </summary>
    public class PhaseExecutor
    {
        /// <summary>
        /// Executes each phase supplied.
        /// </summary>
        /// <param name="script">The script to execute</param>
        /// <param name="phaseCtx">Contextual information passed to all phases.</param>
        /// <param name="ctx">The context of the runtime</param>
        /// <param name="phases">The list of phases.</param>
        /// <returns></returns>
        public PhaseResult Execute(string script, PhaseContext phaseCtx, Context ctx, List<IPhase> phases)
        {
            if (phases == null || phases.Count == 0)
                throw new ArgumentException("No phases supplied to execute");

            // 2. Keep track of last phase result
            PhaseResult lastPhaseResult = null;
            foreach (var phase in phases)
            {
                // 3. Execute the phase and get it's result.
                phase.Ctx = ctx;
                lastPhaseResult = phase.Execute(phaseCtx);
                phase.Result = lastPhaseResult;

                // 4. Stop the phase execution.
                if (!phase.Result.Success)
                {
                    break;
                }
            }
            return lastPhaseResult;
        }
    }
}
