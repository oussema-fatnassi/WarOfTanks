namespace WarOfTanks.AI.BehaviourTree
{
    /// <summary>Decorator that repeats its child a fixed number of times or forever.</summary>
    public class Repeater : DecoratorNode
    {
        private readonly int _repeatCount;
        private int _currentCount;

        /// <summary>Creates a repeater; use a negative repeat count for infinite repetition.</summary>
        public Repeater(IBehaviourNode child, int repeatCount) : base(child)
        {
            _repeatCount = repeatCount;
            _currentCount = 0;
        }

        /// <summary>Ticks the child until the configured repeat count is reached.</summary>
        public override NodeStatus Tick()
        {
            if (_repeatCount == 0)
            {
                return NodeStatus.Success;
            }

            var status = _child.Tick();

            if (status == NodeStatus.Running)
            {
                return NodeStatus.Running;
            }

            if (_repeatCount < 0)
            {
                return NodeStatus.Running;
            }

            _currentCount++;

            if (_currentCount >= _repeatCount)
            {
                _currentCount = 0;
                return NodeStatus.Success;
            }

            return NodeStatus.Running;
        }
    }
}
