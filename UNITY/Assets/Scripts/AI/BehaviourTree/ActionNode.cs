using System;

namespace WarOfTanks.AI.BehaviourTree
{
    /// <summary>Leaf node that executes an action delegate.</summary>
    public class ActionNode : IBehaviourNode
    {
        private readonly Func<NodeStatus> _action;

        /// <summary>Creates an action node from a delegate returning a node status.</summary>
        public ActionNode(Func<NodeStatus> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>Runs the wrapped action and returns its status.</summary>
        public NodeStatus Tick()
        {
            return _action();
        }
    }
}
