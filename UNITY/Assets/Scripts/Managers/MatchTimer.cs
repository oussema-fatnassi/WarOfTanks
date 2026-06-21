using System;

/// <summary>
/// Pure C# countdown timer. Driven externally by GameManager.Update() via Tick(deltaTime).
/// Time.deltaTime is 0 when timeScale is 0, so this automatically pauses with the game.
/// </summary>
public class MatchTimer
{
    private float _duration;
    private float _elapsed;
    private bool _running = false;

    public bool IsTimeUp => _elapsed >= _duration;
    // Clamped to 0 — never returns a negative value.
    public float RemainingTime => Math.Max(_duration - _elapsed,0);
    public float Elapsed => _elapsed;
    
    public MatchTimer(float duration)
    {
        _duration = duration;
        _elapsed = 0;
    }

    public void Tick(float deltaTime)
    {
        if (!_running) return;
        _elapsed += deltaTime;
    }

    public void StartTimer() { _running = true; }
    public void PauseTimer() { _running = false; }
    public void ResetTimer() { _elapsed = 0; _running = false; }
}
