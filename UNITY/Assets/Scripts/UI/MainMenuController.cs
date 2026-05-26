using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Handles the two main menu buttons. Quit is disabled on WebGL builds.</summary>
public class MainMenuController : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("Game");
    }

    public void Quit()
    {
#if !UNITY_WEBGL
        Application.Quit();
#endif
    }
}