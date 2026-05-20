using WarOfTanks.Enums;
using WarOfTanks.Interfaces;

namespace WarOfTanks.AI.BehaviourTree
{
    public class Repeater : DecoratorNode
    {
        private readonly int _repeatCount;
        private int _currentCount;

        public Repeater(IBehaviourNode child, int repeatCount) : base(child)
        {
            _repeatCount = repeatCount;
            _currentCount = 0;
        }

        public override NodeStatus Tick()
        {
            if (_repeatCount == 0)
            {
                return NodeStatus.Success;
            }

            var status = child.Tick();

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