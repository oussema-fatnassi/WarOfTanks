using System.Collections.Generic;
using WarOfTanks.AI.BehaviourTree;
using BehaviourTreeController = WarOfTanks.AI.BehaviourTree.BehaviourTree;

namespace WarOfTanks.AI
{
    public partial class TankAI
    {
        /// <summary>
        /// Builds the attacker role tree: retreat when low, pursue visible enemies, otherwise patrol enemy spawn.
        /// </summary>
        /// <returns>The root node for attacker role behaviour.</returns>
        private IBehaviourNode BuildAttackerTreeRoot()
        {
            Selector root = new Selector(new List<IBehaviourNode>
            {
                // Low HP -> return to spawn
                new Sequence(new List<IBehaviourNode>
                {
                    new ConditionNode(() => _blackboard != null && _blackboard.hpRatio < 0.3f),
                    new ActionNode(MoveToSpawn)
                }),

                // Enemy visible with line of sight -> chase -> attack
                new Sequence(new List<IBehaviourNode>
                {
                    new ConditionNode(() =>
                        _blackboard != null &&
                        _blackboard.closestEnemy != null &&
                        _blackboard.closestEnemy.target != null &&
                        _blackboard.closestEnemy.isInLineOfSight),
                    new ActionNode(MoveToFiringRange),
                    new ActionNode(AttackClosestEnemy)
                }),

                // Default -> patrol between own spawn and enemy spawn
                new ActionNode(PatrolBetweenSpawns)
            });
            return root;

        }
    }
}
