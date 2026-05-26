namespace WarOfTanks.AI.BehaviourTree
{
    /// <summary>Defines one executable node in a behaviour tree.</summary>
    public interface IBehaviourNode
    {
        /// <summary>Runs this node once and returns its current execution status.</summary>
        NodeStatus Tick();
    }
}
