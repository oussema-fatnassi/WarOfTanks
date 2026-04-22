using System.Collections.Generic;
using UnityEngine;

public class CommandDispatcher
{
    public void IssueMoveCommand(List<ISelectable> tanks, Vector3 worldPosition)
    {
        for (int i = 0; i < tanks.Count; i++)
        {
            var (receiver, components) = GetTankInterfaces(tanks[i]);
            receiver.SetCommand(new MoveCommand(components, worldPosition + GetFormationOffset(i, tanks.Count)));
        }
    }

    public void IssueAttackCommand(List<ISelectable> tanks, ISelectable target)
    {
        foreach (ISelectable tank in tanks)
        {
            var (receiver, components) = GetTankInterfaces(tank);
            receiver.SetCommand(new AttackCommand(components, target));
        }
    }

    public void IssueAttackZoneCommand(List<ISelectable> tanks, Vector3 worldPosition)
    {
        foreach (ISelectable tank in tanks)
        {
            var (receiver, components) = GetTankInterfaces(tank);
            MoveCommand moveCommand = receiver.CurrentCommand as MoveCommand;
            Vector3? moveDest = moveCommand != null ? moveCommand.Destination : (Vector3?)null;
            receiver.SetCommand(new AttackZoneCommand(components, worldPosition, moveDest));
        }
    }

    public void IssueStopCommand(List<ISelectable> tanks)
    {
        foreach (ISelectable tank in tanks)
        {
            var (receiver, components) = GetTankInterfaces(tank);
            receiver.SetCommand(new StopCommand(components));
        }
    }

    private Vector3 GetFormationOffset(int index, int totalUnits)
    {
        if (totalUnits <= 1) return Vector3.zero;
        float angle = (2f * Mathf.PI / totalUnits) * index;
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 1.5f;

    }
    private (ICommandReceiver receiver, ITankComponents components) GetTankInterfaces(ISelectable tank)
    {
        return (tank as ICommandReceiver, tank as ITankComponents);
    }
}
