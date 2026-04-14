using WarOfTanks.StateMachine;

class MockState : IState<DummyOwner>
{
    public int enterCount;
    public int executeCount;
    public int exitCount;

    public void Enter(DummyOwner context)
    {
        enterCount++;
    }

    public void Execute(DummyOwner context)
    {
        executeCount++;
    }

    public void Exit(DummyOwner context)
    {
        exitCount++;
    }
}
