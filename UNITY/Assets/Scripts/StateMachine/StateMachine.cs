namespace WarOfTanks.StateMachine
{
    /// <summary>
    /// Generic, reusable state machine. T is the context — the object whose behaviour
    /// is being controlled (e.g. Tank, ControlZone). T must be a reference type
    /// because all contexts in this project are MonoBehaviours.
    ///
    /// Usage: instantiate with the context and initial state, then call Update()
    /// every frame from the context's MonoBehaviour.Update().
    /// </summary>
    public class StateMachine<T> where T : class
    {
        // The state currently running. Set immediately by the constructor.
        private IState<T> _currentState;

        // Reference to the object this machine controls. Passed into every state method call.
        private T _context;

        /// <summary>
        /// Creates a new state machine bound to the given context and enters the initial state immediately.
        /// </summary>
        public StateMachine(T context, IState<T> initialState)
        {
            _context = context;
            ChangeState(initialState);
        }

        /// <summary>
        /// Transitions to a new state. Calls Exit() on the current state (if any),
        /// then calls Enter() on the new state. This is the only way to change state.
        /// </summary>
        public void ChangeState(IState<T> newState)
        {
            _currentState?.Exit(_context);
            _currentState = newState;
            _currentState.Enter(_context);
        }

        /// <summary>
        /// Forwards the update tick to the current state via Execute().
        /// Call this from the context's MonoBehaviour.Update() every frame.
        /// </summary>
        public void Update()
        {
            _currentState?.Execute(_context);
        }

        /// <summary>Returns the state currently active in the machine.</summary>
        public IState<T> GetCurrentState()
        {
            return _currentState;
        }
    }
}
