namespace WarOfTanks.StateMachine
{
    public class StateMachine<T> where T : class
    {
        private IState _currentState;
        private T _owner;

        public StateMachine(T owner)
        {
            _owner = owner;
        }

        public void SetState(IState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();
        }

        public void Update()
        {
            _currentState?.Update();
        }

        public IState GetCurrentState()
        {
            return _currentState;
        }
    }
}