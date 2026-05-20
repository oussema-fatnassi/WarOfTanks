using System;
using WarOfTanks.Enums;
using WarOfTanks.Interfaces;

namespace WarOfTanks.AI.BehaviourTree
{
    public class ActionNode : IBehaviourNode
    {
        private readonly Func<NodeStatus> _action;

        public ActionNode(Func<NodeStatus> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public NodeStatus Tick()
        {
            return _action();
        }
    }
}