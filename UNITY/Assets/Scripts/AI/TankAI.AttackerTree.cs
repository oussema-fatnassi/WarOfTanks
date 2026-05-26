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
        private BehaviourTreeController BuildAttackerTree()
        {
            Selector root = new Selector(new List<IBehaviourNode>
            {
                // Low HP -> return to spawn
                new Sequence(new List<IBehaviourNode>
                {
                    new ConditionNode(() => _blackboard != null && _blackboard.hpRatio < 0.3f),
                    new ActionNode(MoveToSpawn)
                }),

                // Enemy visible -> move into range -> attack
                new Sequence(new List<IBehaviourNode>
                {
                    new ConditionNode(() => 
                        _blackboard != null &&
                        _blackboard.closestEnemy != null &&
                        _blackboard.closestEnemy.target != null),
                    new ActionNode(MoveToFiringRange),
                    new ActionNode(AttackClosestEnemy)
                }),

                // Default -> patrol toward enemy spawn
                new ActionNode(PatrolToEnemySpawn)
            });

            return new BehaviourTreeController(root);
        }
    }
}
