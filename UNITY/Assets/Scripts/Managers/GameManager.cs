using UnityEngine;
using WarOfTanks.StateMachine;
using WarOfTanks.Zone;

/// <summary>
/// Singleton that owns the full match lifecycle: timer, scoring, team tracking, and state machine.
/// Wires together Zone events and Tank death callbacks at scene start.
/// </summary>
public class GameManager : SingletonBehaviour<GameManager>
{
    [Header("Debug")]
    [SerializeField] private bool _enableLogs = false;

    [Header("Match Settings")]
    [SerializeField] private float _matchDuration = 180f;
    [SerializeField] private int _scoreLimit = 100;

    [Header("References")]
    [SerializeField] private Zone _zone;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameOverScreen _gameOverScreen;
    [SerializeField] private PlayerInputHandler _playerInputHandler;

    private TeamManager _teamManager;
    private ScoreManager _scoreManager;
    private MatchTimer _matchTimer;
    private StateMachine<GameManager> _stateMachine;
    // Guards against a double GameOver trigger if the zone fires a score on the same frame the timer expires.
    private bool _matchEnded;

    protected override void Awake()
    {
        base.Awake();
        ApplyDebugSettings();
        _teamManager = new TeamManager();
        _scoreManager = new ScoreManager(_scoreLimit);
        _matchTimer = new MatchTimer(_matchDuration);
    }
    private void OnDestroy()
    {
        _zone.OnZoneScored -= OnZoneScored;
    }
    private void Start()
    {
        Tank[] tanks = FindObjectsOfType<Tank>();
        foreach (Tank tank in tanks)
        {
            _teamManager.RegisterTank(tank, (int)tank.TeamId);
            tank.OnDied += () => OnTankDestroyed(tank);
        }
        _zone.OnZoneScored += OnZoneScored;
        _stateMachine = new GameStateMachine(this);
        _gameOverScreen.Hide();
        _pausePanel.SetActive(false);
    }
    private void Update()
    {
        _stateMachine.Update();
        _matchTimer.Tick(Time.deltaTime);
        if (!_matchEnded && _matchTimer.IsTimeUp)
        {
            _matchEnded = true;
            _stateMachine.ChangeState(new GameOverState(_stateMachine));
        }
    }
    private void OnValidate()
    {
        ApplyDebugSettings();
    }

    private void ApplyDebugSettings()
    {
        DebugLogger.IsEnabled = _enableLogs;
    }

    #region GameLoop Methods
    public void StartMatch() { _matchTimer.StartTimer(); }
    public void PauseMatch() { _matchTimer.PauseTimer(); }
    /// <summary>
    /// Stops the timer only. Does NOT call ChangeState — callers in Update and OnZoneScored do that.
    /// Keeping ChangeState out of here prevents infinite recursion when GameOverState.Enter() calls this.
    /// </summary>
    public void EndMatch() { _matchTimer.PauseTimer(); }

    private void OnZoneScored(int teamId)
    {
        _scoreManager.AddZoneScore(teamId, 1);
        if (!_matchEnded && _scoreManager.HasTeamWon(teamId))
        {
            _matchEnded = true;
            _stateMachine.ChangeState(new GameOverState(_stateMachine));
        }
    }

    /// <summary>Credits the kill to the opposing team (not the tank that died).</summary>
    private void OnTankDestroyed(Tank tank)
    {
        int killingTeam = (int)tank.TeamId == 0 ? 1 : 0;
        _scoreManager.AddKillScore(killingTeam);
    }
    public int GetWinner() { return _scoreManager.GetLeadingTeam(); }
    public int GetScore(int teamId) { return _scoreManager.GetScore(teamId); }
    public float GetRemainingTime() { return _matchTimer.RemainingTime; }
    public void SetPauseUI(bool active) { _pausePanel.SetActive(active); }
    public void ShowGameOver() { _gameOverScreen.Show(GetWinner(), GetScore(0), GetScore(1)); }
    public void SetInputEnabled(bool enabled) { _playerInputHandler.enabled = enabled; }
    #endregion
}
