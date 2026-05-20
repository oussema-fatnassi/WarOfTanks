using System.Collections.Generic;
using WarOfTanks.Enums;
using WarOfTanks.Interfaces;

namespace WarOfTanks.AI.BehaviourTree
{
    public class Sequence : CompositeNode
    {
        public Sequence(List<IBehaviourNode> children) : base(children)
        {
        }

        public override NodeStatus Tick()
        {
            foreach (var child in children)
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