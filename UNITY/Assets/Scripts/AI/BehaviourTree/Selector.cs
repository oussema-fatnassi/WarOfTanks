using System.Collections.Generic;

namespace WarOfTanks.AI.BehaviourTree
{
    /// <summary>Composite node that succeeds when the first child succeeds.</summary>
    public class Selector : CompositeNode
    {
        /// <summary>Creates a selector with the initial child list.</summary>
        public Selector(List<IBehaviourNode> children) : base(children)
        {
        }

        /// <summary>Ticks children until one succeeds or runs; fails when all children fail.</summary>
        public override NodeStatus Tick()
        {
            foreach (var child in _children)
            {
                var status = child.Tick();

                if (status == NodeStatus.Success)
                {
                    return NodeStatus.Success;
                }
                if (status == NodeStatus.Running)
                {
                    return NodeStatus.Running;
                }
            }
            return NodeStatus.Failure;
        }
    }
}
