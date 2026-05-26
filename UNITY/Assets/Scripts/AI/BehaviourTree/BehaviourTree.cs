using System;

namespace WarOfTanks.AI.BehaviourTree
{
    /// <summary>Owns the root behaviour node and ticks the tree.</summary>
    public class BehaviourTree
    {
        private readonly IBehaviourNode _root;

        /// <summary>Creates a behaviour tree with the given root node.</summary>
        public BehaviourTree(IBehaviourNode root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
        }

        /// <summary>Runs the root node once and returns its status.</summary>
        public NodeStatus Tick()
        {
            return _root.Tick();
        }
    }
}
