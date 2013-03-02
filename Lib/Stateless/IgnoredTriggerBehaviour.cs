﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        public class IgnoredTriggerBehaviour : TriggerBehaviour
        {
            public IgnoredTriggerBehaviour(TTrigger trigger, Func<bool> guard)
                : base(trigger, guard)
            {
            }

            public override bool ResultsInTransitionFrom(TState source, object[] args, out TState destination)
            {
                destination = default(TState);
                return false;
            }
        }
    }
}
