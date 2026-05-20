using System.Collections.Generic;
using WarOfTanks.Enums;
using WarOfTanks.Interfaces;

namespace WarOfTanks.AI.BehaviourTree
{
    public class Selector : CompositeNode
    {
        public Selector(List<IBehaviourNode> children) : base(children)
        {
        }

        public override NodeStatus Tick()
        {
            foreach (var child in children)
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
