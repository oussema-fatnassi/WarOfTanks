namespace WarOfTanks.AI.BehaviourTree
{
    /// <summary>Decorator that flips success and failure results from its child.</summary>
    public class Inverter : DecoratorNode
    {
        /// <summary>Creates an inverter around one child node.</summary>
        public Inverter(IBehaviourNode child) : base(child)
        {
        }

        /// <summary>Ticks the child, flips success/failure, and preserves running.</summary>
        public override NodeStatus Tick()
        {
            var status = _child.Tick();

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
