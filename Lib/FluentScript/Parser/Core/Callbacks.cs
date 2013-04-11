using System;
using System.Collections.Generic;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser.Core
{
    /// <summary>
    /// Used for callbacks to external methods after execution of expressions/statements.
    /// </summary>
    public class Callbacks
    {
        private IDictionary<string, List<Action<object, AstNode>>> _subscribers = new Dictionary<string, List<Action<object, AstNode>>>();


        /// <summary>
        /// Whether or not there are any subscribers.
        /// </summary>
        public bool HasAny
        {
            get { return _subscribers.Count > 0; }
        }


        /// <summary>
        /// Subscribe to a specific topic.
        /// </summary>
        /// <param name="topic">e.g. expresionCompleted.</param>
        /// <param name="action">The action to call.</param>
        public void Subscribe(string topic, Action<object, AstNode> action)
        {
            List<Action<object, AstNode>> subscribersForTopic = null;
            if (_subscribers.ContainsKey(topic))
            {
                subscribersForTopic = _subscribers[topic];
            }
            else
            {
                subscribersForTopic = new List<Action<object, AstNode>>();
                _subscribers[topic] = subscribersForTopic;
            }
            subscribersForTopic.Add(action);
        }


        /// <summary>
        /// Notify subscribers.
        /// </summary>
        internal void Notify(string topic, object sender, AstNode node)
        {
            if (!_subscribers.ContainsKey(topic)) return;

            var subscribers = _subscribers[topic];
            foreach (var subscriber in subscribers)
                subscriber(sender, node);
        }
    }
}
