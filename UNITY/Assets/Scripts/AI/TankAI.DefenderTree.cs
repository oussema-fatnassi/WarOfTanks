using System.Collections.Generic;
using UnityEngine;
using WarOfTanks.AI.BehaviourTree;
using BehaviourTreeController = WarOfTanks.AI.BehaviourTree.BehaviourTree;

namespace WarOfTanks.AI
{
    public partial class TankAI
    {
        /// <summary>
        /// Builds the defender role tree: retreat when low, intercept threats, guard the zone, otherwise patrol.
        /// </summary>
        /// <returns>The root node for defender role behaviour.</returns>
        private IBehaviourNode BuildDefenderTreeRoot()
        {
            Selector root = new Selector(new List<IBehaviourNode>
            {
                // Low HP -> retreat to spawn
                new Sequence(new List<IBehaviourNode>
                {
                    new ConditionNode(() => _blackboard != null && _blackboard.hpRatio < 0.3f),
                    new ActionNode(MoveToSpawn)
                }),

                // Enemy visible near zone -> intercept and attack
                new Sequence(new List<IBehaviourNode>
                {
                    new ConditionNode(() =>
                        _blackboard != null &&
                        _blackboard.closestEnemy != null &&
                        _blackboard.closestEnemy.target != null &&
                        _zone != null &&
                        Vector2.Distance(_blackboard.closestEnemy.target.transform.position, _zone.transform.position) <= 5f),
                    new ActionNode(MoveToIntercept),
                    new ActionNode(AttackClosestEnemy)
                }),

                // Own team is capturing or controlling the zone -> guard perimeter
                new Sequence(new List<IBehaviourNode>
                {
                    new ConditionNode(() =>
                        _blackboard != null &&
                        _zone != null &&
                        _zone.controllingTeam == (int)_blackboard.teamId &&
                        (_zone.IsCaptured() || _zone.captureProgress > 0f)),
                    new ActionNode(MoveToZonePerimeter)
                }),

                // Default -> patrol between zone and spawn
                new ActionNode(PatrolZoneToSpawn)
            });

            return root;
        }
    }
}
