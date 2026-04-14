using NUnit.Framework;
using WarOfTanks.StateMachine;

public class StateMachineTests
{
    [Test]
    public void Constructor_SetsInitialState_AndCallsEnter()
    {
        var owner = new DummyOwner();
        var initialState = new MockState();

        var stateMachine = new StateMachine<DummyOwner>(owner, initialState);

        Assert.AreSame(initialState, stateMachine.GetCurrentState());
        Assert.AreEqual(1, initialState.enterCount);
        Assert.AreEqual(0, initialState.exitCount);
        Assert.AreEqual(0, initialState.executeCount);
    }

    [Test]
    public void Update_CallsExecuteOnCurrentState()
    {
        var owner = new DummyOwner();
        var currentState = new MockState();
        var stateMachine = new StateMachine<DummyOwner>(owner, currentState);

        stateMachine.Update();

        Assert.AreEqual(1, currentState.executeCount);
        Assert.AreEqual(1, currentState.enterCount);
        Assert.AreEqual(0, currentState.exitCount);
    }

    [Test]
    public void ChangeState_ExitsPreviousState_AndEntersNewState()
    {
        var owner = new DummyOwner();
        var previousState = new MockState();
        var nextState = new MockState();
        var stateMachine = new StateMachine<DummyOwner>(owner, previousState);

        stateMachine.ChangeState(nextState);

        Assert.AreSame(nextState, stateMachine.GetCurrentState());
        Assert.AreEqual(1, previousState.enterCount);
        Assert.AreEqual(1, previousState.exitCount);
        Assert.AreEqual(1, nextState.enterCount);
        Assert.AreEqual(0, nextState.exitCount);
    }

    [Test]
    public void Update_CallsExecuteWithCorrectContext()
    {
        var owner = new DummyOwner();
        var state = new MockState();
        var stateMachine = new StateMachine<DummyOwner>(owner, state);

        stateMachine.Update();
        stateMachine.Update();
        stateMachine.Update();

        Assert.AreEqual(3, state.executeCount);
    }
}
