using System;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib.Parser.Integration;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Phases
{
    /// <summary>
    /// Base class for other phases.
    /// </summary>
    public class Phase : IPhase
    {

        /// <summary>
        /// Name of the phase.
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// The result of excution of this phase.
        /// </summary>
        public PhaseResult Result { get; set; }


        /// <summary>
        /// The context of the runtime
        /// </summary>
        public Context Ctx { get; set; }


        /// <summary>
        /// Executes the phase
        /// </summary>
        /// <param name="phaseContext"></param>
        /// <returns></returns>
        public virtual PhaseResult Execute(PhaseContext phaseContext)
        {
            return ToEmptyPhaseResult(false, string.Empty);
        }


        /// <summary>
        /// Empty phase result, 0 time duration.
        /// </summary>
        /// <param name="success"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public PhaseResult ToEmptyPhaseResult(bool success, string message)
        {
            var now = DateTime.Now;
            var runResult = new RunResult(now, now, success, message);
            return new PhaseResult(runResult);
        }


        /// <summary>
        /// Builds up a phase result from start, end, success/message fields.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="success"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public PhaseResult ToPhaseResult(DateTime start, DateTime end, bool success, string message)
        {
            var runResult = new RunResult(start, end, success, message);
            return new PhaseResult(runResult);        
        }
    }
}
