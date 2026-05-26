using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Holds all registered tanks split by team. Tanks are never removed — they stay registered
/// even after death so GetAliveTanks() can filter dynamically by tank.IsAlive.
/// </summary>
public class TeamManager
{
    private List<Tank> _teamA;
    private List<Tank> _teamB;

    public TeamManager()
    {
        _teamA = new List<Tank>();
        _teamB = new List<Tank>();
    }

    public void RegisterTank(Tank tank, int teamId)
    {
        if (teamId == 0) { _teamA.Add(tank); }
        else { _teamB.Add(tank); }
    }

    public List<Tank> GetAliveTanks(int teamId)
    {
        List<Tank> aliveTanks = teamId == 0 ? _teamA : _teamB;
        return aliveTanks.Where(tank => tank.IsAlive).ToList();
    }

    public bool IsTeamEliminated(int teamId)
    {
        return GetAliveTanks(teamId).Count == 0;
    }

    public int GetTankCount(int teamId)
    {
        return GetAliveTanks(teamId).Count;
    }
}
