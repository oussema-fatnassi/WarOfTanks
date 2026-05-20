using System;
using WarOfTanks.Enums;
using WarOfTanks.Interfaces;

namespace WarOfTanks.AI.BehaviourTree
{
    public class ConditionNode : IBehaviourNode
    {
        private readonly Func<bool> _condition;

        public ConditionNode(Func<bool> condition)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        public NodeStatus Tick()
        {
            return _condition() ? NodeStatus.Success : NodeStatus.Failure;
        }
    }
}