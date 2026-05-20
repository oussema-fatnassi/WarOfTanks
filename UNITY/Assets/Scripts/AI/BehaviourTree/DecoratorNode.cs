using System;
using WarOfTanks.Enums;
using WarOfTanks.Interfaces;

namespace WarOfTanks.AI.BehaviourTree
{
    public abstract class DecoratorNode : IBehaviourNode
    {
        protected IBehaviourNode child;

        public DecoratorNode(IBehaviourNode child)
        {
            this.child = child ?? throw new ArgumentNullException(nameof(child));
        }

        public abstract NodeStatus Tick();
    }
}