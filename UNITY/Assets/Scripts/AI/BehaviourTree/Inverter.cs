using WarOfTanks.Enums;
using WarOfTanks.Interfaces;

namespace WarOfTanks.AI.BehaviourTree
{
    public class Inverter : DecoratorNode
    {
        public Inverter(IBehaviourNode child) : base(child)
        {
        }

        public override NodeStatus Tick()
        {
            var status = child.Tick();

            if (status == NodeStatus.Success)
            {
                return NodeStatus.Failure;
            }
            if (status == NodeStatus.Failure)
            {
                return NodeStatus.Success;
            }

            return NodeStatus.Running;
        }
    }
}
