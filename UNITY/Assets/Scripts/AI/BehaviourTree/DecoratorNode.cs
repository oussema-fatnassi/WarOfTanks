using System;

namespace WarOfTanks.AI.BehaviourTree
{
    /// <summary>Base class for nodes that wrap and transform one child node.</summary>
    public abstract class DecoratorNode : IBehaviourNode
    {
        protected readonly IBehaviourNode _child;

        /// <summary>Creates a decorator around one child node.</summary>
        public DecoratorNode(IBehaviourNode child)
        {
            _child = child ?? throw new ArgumentNullException(nameof(child));
        }

        /// <summary>Runs this node once and returns its current execution status.</summary>
        public abstract NodeStatus Tick();
    }
}
