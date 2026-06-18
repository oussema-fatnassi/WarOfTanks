using System.Collections.Generic;
using WarOfTanks.AI.BehaviourTree;
using BehaviourTreeController = WarOfTanks.AI.BehaviourTree.BehaviourTree;

namespace WarOfTanks.AI
{
    public partial class TankAI
    {
        /// <summary>
        /// Builds the captor role tree: retreat when low, hold the zone, and signal visible enemies.
        /// </summary>
        /// <returns>The root node for captor role behaviour.</returns>
        private IBehaviourNode BuildCaptorTreeRoot()
        {
            Selector root = new Selector(new List<IBehaviourNode>
            {
                // Low HP -> retreat to spawn
                new Sequence(new List<IBehaviourNode>
                {
                    new ConditionNode(() => _blackboard != null && _blackboard.hpRatio < 0.3f),
                    new ActionNode(MoveToSpawn)
                }),

                // Own team captured zone -> stay/go to zone
                new Sequence(new List<IBehaviourNode>
                {
                    new ConditionNode(() => 
                        _zone != null &&
                        _blackboard != null &&
                        _zone.IsCaptured() &&
                        _zone.controllingTeam == (int)_blackboard.teamId),
                    new ActionNode(MoveToZone)
                }),

                // In zone and enemy visible -> signal commander placeholder
                new Sequence(new List<IBehaviourNode>
                {
                    new ConditionNode(() => IsInZone()),
                    new ConditionNode(() => _blackboard != null && _blackboard.closestEnemy != null),
                    new ActionNode(SignalEnemyVisible)
                }),

                // Default Move to zone
                new ActionNode(MoveToZone)
            });

            return root;
        }
    }
}
