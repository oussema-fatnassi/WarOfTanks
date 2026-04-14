namespace WarOfTanks.StateMachine
{
    /// <summary>
    /// Contract that every state in the project must implement.
    /// T is the context (owner) passed to each method so states can read and modify it.
    /// The StateMachine calls these three methods to drive the state lifecycle.
    /// </summary>
    public interface IState<T> where T : class
    {
        /// <summary>Called once when the state machine transitions into this state.</summary>
        void Enter(T context);

        /// <summary>Called every frame while this state is active.</summary>
        void Execute(T context);

        /// <summary>Called once when the state machine transitions out of this state.</summary>
        void Exit(T context);
    }
}
