using NUnit.Framework;
using WarOfTanks.StateMachine;

public class StateMachineTests
{
    [Test]
    public void GetCurrentState_ReturnsNull_WhenNoStateHasBeenSet()
    {
        var owner = new DummyOwner();
        var stateMachine = new StateMachine<DummyOwner>(owner);

        var currentState = stateMachine.GetCurrentState();

        Assert.IsNull(currentState);
    }

    [Test]
    public void SetState_SetsCurrentState_AndCallsEnterOnNewState()
    {
        var owner = new DummyOwner();
        var stateMachine = new StateMachine<DummyOwner>(owner);
        var newState = new MockState();

        stateMachine.SetState(newState);

        Assert.AreSame(newState, stateMachine.GetCurrentState());
        Assert.AreEqual(1, newState.enterCount);
        Assert.AreEqual(0, newState.exitCount);
        Assert.AreEqual(0, newState.updateCount);
    }

    [Test]
    public void Update_CallsUpdateOnCurrentState()
    {
        var owner = new DummyOwner();
        var stateMachine = new StateMachine<DummyOwner>(owner);
        var currentState = new MockState();
        stateMachine.SetState(currentState);

        stateMachine.Update();

        Assert.AreEqual(1, currentState.updateCount);
        Assert.AreEqual(1, currentState.enterCount);
        Assert.AreEqual(0, currentState.exitCount);
    }

    [Test]
    public void SetState_ExitsPreviousState_AndEntersNewState()
    {
        var owner = new DummyOwner();
        var stateMachine = new StateMachine<DummyOwner>(owner);
        var previousState = new MockState();
        var nextState = new MockState();
        stateMachine.SetState(previousState);

        stateMachine.SetState(nextState);

        Assert.AreSame(nextState, stateMachine.GetCurrentState());
        Assert.AreEqual(1, previousState.enterCount);
        Assert.AreEqual(1, previousState.exitCount);
        Assert.AreEqual(1, nextState.enterCount);
        Assert.AreEqual(0, nextState.exitCount);
    }

    [Test]
    public void Update_DoesNotThrow_WhenNoCurrentStateExists()
    {
        var owner = new DummyOwner();
        var stateMachine = new StateMachine<DummyOwner>(owner);

        TestDelegate updateWithoutState = () => stateMachine.Update();

        Assert.DoesNotThrow(updateWithoutState);
    }
}
