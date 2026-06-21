using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class MatchResultSender : MonoBehaviour
{
    private const string MatchEndpoint = "/api/v1/matches";

    public void Send(MatchResultPayload payload, string baseUrl)
    {
        StartCoroutine(PostResult(payload, baseUrl));
    }

    private IEnumerator PostResult(MatchResultPayload payload, string baseUrl)
    {
        string json = JsonUtility.ToJson(payload);
        string url = baseUrl.TrimEnd('/') + MatchEndpoint;

        using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(AuthToken.AccessToken))
            request.SetRequestHeader("Authorization", "Bearer " + AuthToken.AccessToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("[MatchResultSender] Match result saved (201)");
        }
        else if (request.responseCode == 401)
        {
            Debug.LogWarning("[MatchResultSender] Unauthorized (401) — token expired or missing");
        }
        else
        {
            Debug.LogError($"[MatchResultSender] Failed ({request.responseCode}): {request.error}");
        }
    }
}
