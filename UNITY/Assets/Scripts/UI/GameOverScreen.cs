using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the end-of-match UI panel. Driven by GameManager.ShowGameOver().
/// PlayAgain and GoToMainMenu must reset Time.timeScale before loading — Unity does not reset it on scene load.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _winnerText;
    [SerializeField] private TextMeshProUGUI _scoreTeamAText;
    [SerializeField] private TextMeshProUGUI _scoreTeamBText;

    public void Show(int winner, int scoreA, int scoreB)
    {
        _panel.SetActive(true);
        _winnerText.text = winner == 0 ? "Player Team Wins!" : winner == 1 ? "Enemy Team Wins!" : "Draw!";
        _scoreTeamAText.text = "Player: " + scoreA;
        _scoreTeamBText.text = "Enemy: " + scoreB;
    }

    /// <summary>Hides the panel at match start in case it was left active in the Inspector.</summary>
    public void Hide()
    {
        _panel.SetActive(false);
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}