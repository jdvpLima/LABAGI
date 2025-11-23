using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AuthApiClient : MonoBehaviour
{
    [SerializeField] private string baseUrl = "https://lagabi-group2-backend.onrender.com/api";

    public IEnumerator StartGoogleDeviceFlow(
        Action<DeviceStartResponseDTO> onSuccess,
        Action<string> onError)
    {
        var url = $"{baseUrl}/auth/google/device/start";

        using var req = UnityWebRequest.PostWwwForm(url, "");
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(req.error);
            yield break;
        }

        var json = req.downloadHandler.text;
        DeviceStartResponseDTO resp;
        try
        {
            resp = JsonUtility.FromJson<DeviceStartResponseDTO>(json);
        }
        catch (Exception ex)
        {
            onError?.Invoke("Invalid JSON: " + ex.Message);
            yield break;
        }

        onSuccess?.Invoke(resp);
    }

    public IEnumerator PollGoogleDeviceFlow(
        string loginRequestId,
        Action<DevicePollResponseDTO> onSuccess,
        Action<string> onError)
    {
        var url = $"{baseUrl}/auth/google/device/poll?requestId={loginRequestId}";

        using var req = UnityWebRequest.Get(url);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(req.error);
            yield break;
        }

        var json = req.downloadHandler.text;
        DevicePollResponseDTO resp;
        try
        {
            resp = JsonUtility.FromJson<DevicePollResponseDTO>(json);
        }
        catch (Exception ex)
        {
            onError?.Invoke("Invalid JSON: " + ex.Message);
            yield break;
        }

        onSuccess?.Invoke(resp);
    }

    public IEnumerator GetUserById(long userId, Action<MeResponseDTO> onSuccess, Action<string> onError)
    {
        var url = $"{baseUrl}/User/GetById?userId={userId}";
        Debug.Log("[AuthApiClient] GET " + url);

        using var req = UnityWebRequest.Get(url);

        yield return req.SendWebRequest();
        Debug.Log("[AuthApiClient] Response code: " + req.responseCode);

        if (req.result != UnityWebRequest.Result.Success)
        {
            // Se vier 204 (NoContent) ? user não existe
            var extra = req.downloadHandler != null ? req.downloadHandler.text : "";
            onError?.Invoke(req.error + " | " + extra);
            yield break;
        }

        var json = req.downloadHandler.text;
        Debug.Log("[AuthApiClient] Body: " + json);

        MeResponseDTO resp;
        try
        {
            resp = JsonUtility.FromJson<MeResponseDTO>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[AuthApiClient] Invalid JSON: " + ex.Message);
            onError?.Invoke("Invalid JSON: " + ex.Message);
            yield break;
        }

        onSuccess?.Invoke(resp);
    }
    public IEnumerator GetMe(string sessionToken,
    Action<MeResponseDTO> onSuccess,
    Action onUnauthorized,
    Action<string> onError)
    {
        var url = $"{baseUrl}/Me";
        Debug.Log("[AuthApiClient] GET " + url);

        using var req = UnityWebRequest.Get(url);

        if (!string.IsNullOrEmpty(sessionToken))
            req.SetRequestHeader("Authorization", "Bearer " + sessionToken);

        yield return req.SendWebRequest();

        if (req.responseCode == 401 || req.responseCode == 403)
        {
            onUnauthorized?.Invoke();
            yield break;
        }

        if (req.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(req.error + " | " + req.downloadHandler.text);
            yield break;
        }

        var json = req.downloadHandler.text;
        var resp = JsonUtility.FromJson<MeResponseDTO>(json);
        onSuccess?.Invoke(resp);
    }
}
