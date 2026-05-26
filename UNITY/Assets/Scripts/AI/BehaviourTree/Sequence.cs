using System.Collections.Generic;

namespace WarOfTanks.AI.BehaviourTree
{
    /// <summary>Composite node that succeeds only when all children succeed.</summary>
    public class Sequence : CompositeNode
    {
        /// <summary>Creates a sequence with the initial child list.</summary>
        public Sequence(List<IBehaviourNode> children) : base(children)
        {
        }

        /// <summary>Ticks children until one fails or runs; succeeds when all children succeed.</summary>
        public override NodeStatus Tick()
        {
            foreach (var child in _children)
            {
                var status = child.Tick();

                if (status == NodeStatus.Failure)
                {
                    return NodeStatus.Failure;
                }
                if (status == NodeStatus.Running)
                {
                    return NodeStatus.Running;
                }
            }
            return NodeStatus.Success;
        }
    }
}
