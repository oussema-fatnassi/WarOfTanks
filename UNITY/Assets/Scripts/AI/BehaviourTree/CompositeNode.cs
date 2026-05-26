using System.Collections.Generic;

namespace WarOfTanks.AI.BehaviourTree
{
    /// <summary>Base class for nodes that execute multiple child nodes.</summary>
    public abstract class CompositeNode : IBehaviourNode
    {
        protected readonly List<IBehaviourNode> _children;

        /// <summary>Creates a composite node with the initial child list.</summary>
        public CompositeNode(List<IBehaviourNode> children)
        {
            _children = children;
        }

        /// <summary>Adds a child node to this composite.</summary>
        public void AddChild(IBehaviourNode child)
        {
            _children.Add(child);
        }

        /// <summary>Runs this node once and returns its current execution status.</summary>
        public abstract NodeStatus Tick();
    }
}
