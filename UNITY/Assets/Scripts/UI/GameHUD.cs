using UnityEngine;
using TMPro;

/// <summary>Polls GameManager each frame to update both team scores and the MM:SS countdown timer.</summary>
public class GameHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreTeamAText;
    [SerializeField] private TextMeshProUGUI _scoreTeamBText;
    [SerializeField] private TextMeshProUGUI _timerText;

    private void Update()
    {
        if (GameManager.Instance == null) return;
        _scoreTeamAText.text = GameManager.Instance.GetScore(0).ToString();
        _scoreTeamBText.text = GameManager.Instance.GetScore(1).ToString();
        int total = Mathf.FloorToInt(GameManager.Instance.GetRemainingTime());
        _timerText.text = $"{total / 60:00}:{total % 60:00}";
    }
}