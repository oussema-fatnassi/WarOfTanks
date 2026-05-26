using System;

namespace WarOfTanks.AI.BehaviourTree
{
    /// <summary>Leaf node that converts a boolean condition into success or failure.</summary>
    public class ConditionNode : IBehaviourNode
    {
        private readonly Func<bool> _condition;

        /// <summary>Creates a condition node from a boolean predicate.</summary>
        public ConditionNode(Func<bool> condition)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        /// <summary>Returns success when the condition is true; otherwise failure.</summary>
        public NodeStatus Tick()
        {
            return _condition() ? NodeStatus.Success : NodeStatus.Failure;
        }
    }
}
