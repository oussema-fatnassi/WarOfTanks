namespace WarOfTanks.StateMachine
{
    /// <summary>
    /// Reusable base class for all states in the project.
    /// Inherit from this instead of implementing IState<T> directly.
    ///
    /// T is the context — the MonoBehaviour being controlled (e.g. Zone, Tank).
    /// Subclasses access the context via the Context property and trigger
    /// transitions via Machine.ChangeState().
    /// </summary>
    public abstract class State<T> : IState<T> where T : class
    {
        /// <summary>The state machine that owns this state. Use to call ChangeState().</summary>
        protected StateMachine<T> Machine { get; }

        /// <summary>The object being controlled. Set automatically before each Enter/Execute/Exit call.</summary>
        protected T Context { get; private set; }

        /// <summary>Stores the machine reference for use in transitions.</summary>
        protected State(StateMachine<T> machine)
        {
            Machine = machine;
        }

        // IState<T> bridge — sets Context then calls the parameterless virtual method.
        // Subclasses never call these directly; they override Enter/Execute/Exit below.
        public void Enter(T context) { Context = context; Enter(); }
        public void Execute(T context) { Context = context; Execute(); }
        public void Exit(T context) { Context = context; Exit(); }

        /// <summary>Called once when the state machine transitions into this state. Override to add enter logic.</summary>
        protected virtual void Enter() { }

        /// <summary>Called once when the state machine transitions out of this state. Override to add cleanup logic.</summary>
        protected virtual void Exit() { }

        /// <summary>Called every frame while this state is active. Override to add per-frame logic and transition checks.</summary>
        protected virtual void Execute() { }
    }
}