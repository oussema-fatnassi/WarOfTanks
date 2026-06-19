using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the pause-menu buttons. The panel itself is shown/hidden by GameManager via
/// SetPauseUI(); these methods drive resume / restart / quit-to-menu. Time.timeScale is reset before
/// any scene load (Unity does not reset it automatically).
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public void Resume()
    {
        if (GameManager.Instance != null) GameManager.Instance.RequestResume();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
