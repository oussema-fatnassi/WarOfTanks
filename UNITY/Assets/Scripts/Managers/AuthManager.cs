using UnityEngine;

/// <summary>
/// Stores the JWT access token received from the React frontend via SendMessage.
/// The GameObject in the scene must be named exactly "AuthManager" for JS interop to work.
/// </summary>
public class AuthManager : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private string _devToken;
#endif

    public static string AccessToken { get; private set; }

    private static string StripWhitespace(string s) => string.Concat(s.Split());

    private void Awake()
    {
#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(_devToken))
            AccessToken = StripWhitespace(_devToken);
#endif
    }

    // Called by React: unityInstance.SendMessage('AuthManager', 'SetToken', token)
    public void SetToken(string token)
    {
        AccessToken = StripWhitespace(token);
        Debug.Log("[Auth] Token received from frontend.");
    }
}
