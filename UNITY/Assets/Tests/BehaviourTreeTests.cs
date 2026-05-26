using System.Collections.Generic;
using NUnit.Framework;
using WarOfTanks.AI.BehaviourTree;

public class BehaviourTreeTests
{
    [Test]
    public void BehaviourTree_Tick_ReturnsRootStatus()
    {
        var tree = new BehaviourTree(new TestNode(NodeStatus.Success));

        Assert.AreEqual(NodeStatus.Success, tree.Tick());
    }

    [Test]
    public void ActionNode_Tick_ExecutesActionAndReturnsStatus()
    {
        var callCount = 0;
        var node = new ActionNode(() =>
        {
            callCount++;
            return NodeStatus.Running;
        });

        var status = node.Tick();

        Assert.AreEqual(NodeStatus.Running, status);
        Assert.AreEqual(1, callCount);
    }

    [Test]
    public void ConditionNode_Tick_ReturnsSuccessWhenConditionIsTrue()
    {
        var node = new ConditionNode(() => true);

        Assert.AreEqual(NodeStatus.Success, node.Tick());
    }

    [Test]
    public void ConditionNode_Tick_ReturnsFailureWhenConditionIsFalse()
    {
        var node = new ConditionNode(() => false);

        Assert.AreEqual(NodeStatus.Failure, node.Tick());
    }

    [Test]
    public void Selector_Tick_ReturnsSuccessOnFirstSuccessfulChild()
    {
        var skippedChild = new TestNode(NodeStatus.Success);
        var selector = new Selector(new List<IBehaviourNode>
        {
            new TestNode(NodeStatus.Failure),
            new TestNode(NodeStatus.Success),
            skippedChild,
        });

        var status = selector.Tick();

        Assert.AreEqual(NodeStatus.Success, status);
        Assert.AreEqual(0, skippedChild.TickCount);
    }

    [Test]
    public void Selector_Tick_ReturnsRunningWhenChildIsRunning()
    {
        var skippedChild = new TestNode(NodeStatus.Success);
        var selector = new Selector(new List<IBehaviourNode>
        {
            new TestNode(NodeStatus.Failure),
            new TestNode(NodeStatus.Running),
            skippedChild,
        });

        var status = selector.Tick();

        Assert.AreEqual(NodeStatus.Running, status);
        Assert.AreEqual(0, skippedChild.TickCount);
    }

    [Test]
    public void Selector_Tick_ReturnsFailureWhenAllChildrenFail()
    {
        var selector = new Selector(new List<IBehaviourNode>
        {
            new TestNode(NodeStatus.Failure),
            new TestNode(NodeStatus.Failure),
        });

        Assert.AreEqual(NodeStatus.Failure, selector.Tick());
    }

    [Test]
    public void Sequence_Tick_ReturnsFailureOnFirstFailedChild()
    {
        var skippedChild = new TestNode(NodeStatus.Success);
        var sequence = new Sequence(new List<IBehaviourNode>
        {
            new TestNode(NodeStatus.Success),
            new TestNode(NodeStatus.Failure),
            skippedChild,
        });

        var status = sequence.Tick();

        Assert.AreEqual(NodeStatus.Failure, status);
        Assert.AreEqual(0, skippedChild.TickCount);
    }

    [Test]
    public void Sequence_Tick_ReturnsRunningWhenChildIsRunning()
    {
        var skippedChild = new TestNode(NodeStatus.Success);
        var sequence = new Sequence(new List<IBehaviourNode>
        {
            new TestNode(NodeStatus.Success),
            new TestNode(NodeStatus.Running),
            skippedChild,
        });

        var status = sequence.Tick();

        Assert.AreEqual(NodeStatus.Running, status);
        Assert.AreEqual(0, skippedChild.TickCount);
    }

    [Test]
    public void Sequence_Tick_ReturnsSuccessWhenAllChildrenSucceed()
    {
        var sequence = new Sequence(new List<IBehaviourNode>
        {
            new TestNode(NodeStatus.Success),
            new TestNode(NodeStatus.Success),
        });

        Assert.AreEqual(NodeStatus.Success, sequence.Tick());
    }

    [Test]
    public void Inverter_Tick_FlipsSuccessAndFailure()
    {
        Assert.AreEqual(NodeStatus.Failure, new Inverter(new TestNode(NodeStatus.Success)).Tick());
        Assert.AreEqual(NodeStatus.Success, new Inverter(new TestNode(NodeStatus.Failure)).Tick());
    }

    [Test]
    public void Inverter_Tick_KeepsRunningStatus()
    {
        var inverter = new Inverter(new TestNode(NodeStatus.Running));

        Assert.AreEqual(NodeStatus.Running, inverter.Tick());
    }

    [Test]
    public void Repeater_Tick_ReturnsSuccessAfterRepeatCountCompletes()
    {
        var child = new TestNode(NodeStatus.Success);
        var repeater = new Repeater(child, 3);

        Assert.AreEqual(NodeStatus.Running, repeater.Tick());
        Assert.AreEqual(NodeStatus.Running, repeater.Tick());
        Assert.AreEqual(NodeStatus.Success, repeater.Tick());
        Assert.AreEqual(3, child.TickCount);
    }

    [Test]
    public void Repeater_Tick_ReturnsRunningWhileChildIsRunning()
    {
        var child = new TestNode(NodeStatus.Running);
        var repeater = new Repeater(child, 2);

        Assert.AreEqual(NodeStatus.Running, repeater.Tick());
        Assert.AreEqual(NodeStatus.Running, repeater.Tick());
        Assert.AreEqual(2, child.TickCount);
    }

    [Test]
    public void Repeater_Tick_WithInfiniteRepeatCountNeverCompletes()
    {
        var child = new TestNode(NodeStatus.Success);
        var repeater = new Repeater(child, -1);

        Assert.AreEqual(NodeStatus.Running, repeater.Tick());
        Assert.AreEqual(NodeStatus.Running, repeater.Tick());
        Assert.AreEqual(2, child.TickCount);
    }

    private class TestNode : IBehaviourNode
    {
        private readonly NodeStatus _status;

        public int TickCount { get; private set; }

        public TestNode(NodeStatus status)
        {
            _status = status;
        }

        public NodeStatus Tick()
        {
            TickCount++;
            return _status;
        }
    }
}
