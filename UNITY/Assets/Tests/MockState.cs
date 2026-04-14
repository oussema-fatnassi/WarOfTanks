using WarOfTanks.StateMachine;
    
class MockState : IState
{
    public int enterCount;
    public int updateCount;
    public int exitCount;

    public void Enter()
    {
        enterCount++;
    }

    public void Update()
    {
        updateCount++;
    }

    public void Exit()
    {
        exitCount++;
    }
}