using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// HTTP transport layer. Stateless — all game logic stays in GameManager, all HTTP here.
/// </summary>
public static class ApiClient
{
    public static IEnumerator PostMatchResult(MatchResultPayload payload, System.Action<bool> onDone)
    {
        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);
        string url = TankConstants.BACKEND_BASE_URL + "/api/v1/matches";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler   = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + AuthManager.AccessToken);

        yield return request.SendWebRequest();

        if (request.responseCode == 201)
        {
            Debug.Log("[Match] Result saved.");
            onDone?.Invoke(true);
        }
        else
        {
            Debug.LogError($"[Match] Save failed — {request.responseCode}: {request.error}");
            onDone?.Invoke(false);
        }

        request.Dispose();
    }
}
