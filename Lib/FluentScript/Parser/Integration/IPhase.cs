﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComLib.Lang.Parsing
{
    /// <summary>
    /// A phase of execution after the parsing.
    /// </summary>
    public interface IPhase
    {
        /// <summary>
        /// Name of the phase.
        /// </summary>
        string Name { get; set; }


        /// <summary>
        /// The context of the runtime.
        /// </summary>
        Context Ctx { get; set; }


        /// <summary>
        /// The result of executing this phase.
        /// </summary>
        PhaseResult Result { get; set; }
        
        
        /// <summary>
        /// Core method to implement a phase hook.
        /// </summary>
        /// <param name="phaseContext"></param>
        /// <returns></returns>
        PhaseResult Execute(PhaseContext phaseContext);
    }
}
