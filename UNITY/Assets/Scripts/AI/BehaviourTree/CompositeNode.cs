using System.Collections.Generic;
using WarOfTanks.Enums;
using WarOfTanks.Interfaces;

namespace WarOfTanks.AI.BehaviourTree
{
    public abstract class CompositeNode : IBehaviourNode
    {
        protected List<IBehaviourNode> children;

        public CompositeNode(List<IBehaviourNode> children)
        {
            this.children = children;
        }

        public void AddChild(IBehaviourNode child)
        {
            children.Add(child);
        }

        public abstract NodeStatus Tick();
    }
}