namespace WarOfTanks.StateMachine
{
    /// <summary>
    /// Generic, reusable state machine. T is the owner — the object whose behaviour
    /// is being controlled (e.g. Tank, ControlZone). T must be a reference type
    /// because all owners in this project are MonoBehaviours.
    ///
    /// Usage: instantiate with the owner, call SetState() to enter the first state,
    /// then call Update() every frame from the owner's MonoBehaviour.Update().
    /// </summary>
    public class StateMachine<T> where T : class
    {
        // The state currently running. Null until SetState is called for the first time.
        private IState _currentState;

        // Reference to the object this machine controls. Passed to states via their constructors.
        private T _owner;

        /// <summary>Creates a new state machine bound to the given owner.</summary>
        public StateMachine(T owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// Transitions to a new state. Calls Exit() on the current state (if any),
        /// then calls Enter() on the new state. This is the only way to change state.
        /// </summary>
        public void SetState(IState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();
        }

        /// <summary>
        /// Forwards the update tick to the current state.
        /// Call this from the owner's MonoBehaviour.Update() every frame.
        /// </summary>
        public void Update()
        {
            _currentState?.Update();
        }

        /// <summary>Returns the state currently active in the machine.</summary>
        public IState GetCurrentState()
        {
            return _currentState;
        }
    }
}