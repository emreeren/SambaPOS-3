using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        //private Func<string> func;
        //private Action<string> action;

        //public StateMachine(Func<string> func, Action<string> action)
        //{
        //    // TODO: Complete member initialization
        //    this.func = func;
        //    this.action = action;
        //}
        internal class DynamicTriggerBehaviour : TriggerBehaviour
        {
            readonly Func<object[], TState> _destination;

            public DynamicTriggerBehaviour(TTrigger trigger, Func<object[], TState> destination, Func<bool> guard)
                : base(trigger, guard)
            {
                _destination = Enforce.ArgumentNotNull(destination, "destination");
            }

            public override bool ResultsInTransitionFrom(TState source, object[] args, out TState destination)
            {
                destination = _destination(args);
                return true;
            }
        }
    }
}
