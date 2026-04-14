namespace WarOfTanks.StateMachine
{
    /// <summary>
    /// Contract that every state in the project must implement.
    /// The StateMachine calls these three methods to drive the state lifecycle.
    /// </summary>
    public interface IState
    {
        /// <summary>Called once when the state machine transitions into this state.</summary>
        void Enter();

        /// <summary>Called every frame while this state is active.</summary>
        void Update();

        /// <summary>Called once when the state machine transitions out of this state.</summary>
        void Exit();
    }
}