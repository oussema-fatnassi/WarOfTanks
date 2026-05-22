using System.Collections.Generic;

/// <summary>
/// Tracks zone scores and kill stats for both teams.
/// Only zone score counts toward HasTeamWon — kills are stats only.
/// </summary>
public class ScoreManager
{
    private int _scoreLimit;
    private Dictionary<int, int> _zoneScoresMap;
    private Dictionary<int, int> _killScoresMap;

    public int ScoreLimit => _scoreLimit;

    public ScoreManager(int scoreLimit)
    {
        _scoreLimit = scoreLimit;

        _zoneScoresMap = new Dictionary<int, int>();
        _zoneScoresMap[0] = 0;
        _zoneScoresMap[1] = 0;

        _killScoresMap = new Dictionary<int, int>();
        _killScoresMap[0] = 0;
        _killScoresMap[1] = 0;
    }

    public void AddZoneScore(int teamId, int amount)
    {
        _zoneScoresMap[teamId] += amount;
    }

    public void AddKillScore(int teamId)
    {
        _killScoresMap[teamId] += 1;
    }

    public int GetScore(int teamId)
    {
        return _zoneScoresMap[teamId];
    }

    public bool HasTeamWon(int teamId)
    {
        return GetScore(teamId) >= _scoreLimit;
    }

    /// <summary>Returns 0 (player) or 1 (enemy). Returns -1 if scores are equal (draw).</summary>
    public int GetLeadingTeam()
    {
        if (_zoneScoresMap[0] > _zoneScoresMap[1])
        {
            return 0;
        }
        else if (_zoneScoresMap[0] < _zoneScoresMap[1])
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }

    public void GetKillingScores(out int team0Kills, out int team1Kills)
    {
        team0Kills = _killScoresMap[0];
        team1Kills = _killScoresMap[1];
    }
}
