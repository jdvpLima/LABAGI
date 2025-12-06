using Esri.ArcGISMapsSDK.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class SpawnData
{
    public int id;
    public string status;
    public int cardPreview;
    public double lat;
    public double lon;
    public string expiresAt;
}

[System.Serializable]
public class NearbySpawnsResponse
{
    public List<SpawnData> features;
}

[System.Serializable]
public class CreateSpawnRequest
{
    public int cardId;
    public double lat;
    public double lon;
    public string expiresAt;
}

[System.Serializable]
public class CreateSpawnResponse
{
    public int spawn_id;
}

public class CardSpawnManager : MonoBehaviour
{
    [Header("API Configuration")]
    public string apiBaseUrl = "https://lagabi-group2-backend.onrender.com/api";
    public float updateInterval = 30f; // Seconds between updates

    [Header("Spawn Settings")]
    public int spawnRadiusMeters = 50000;
    public GameObject cardPrefab;
    public Transform cardContainer;

    [Header("Player Location (Porto Default)")]
    public double currentLatitude = 41.1579;
    public double currentLongitude = -8.6291;

    public Dictionary<int, GameObject> activeSpawns = new Dictionary<int, GameObject>();
    public ArcGISMapComponent arcGISMap; // Reference to ArcGIS component

    [SerializeField] private GameObject loadingInformation;

    void Start()
    {
        if (arcGISMap == null)
        {
            arcGISMap = FindFirstObjectByType<ArcGISMapComponent>();
        }

        if (arcGISMap != null)
        {
            // Subscribe to location updates if available
            // arcGISMap.OnLocationUpdated += OnLocationUpdated;
        }

        loadingInformation.SetActive(true);

        UpdateNearbySpawns();
    }

    void OnDestroy()
    {
        if (arcGISMap != null)
        {
            // arcGISMap.OnLocationUpdated -= OnLocationUpdated;
        }
    }

    // Call this when player location changes
    public void OnLocationUpdated(double newLat, double newLon)
    {
        currentLatitude = newLat;
        currentLongitude = newLon;
        UpdateNearbySpawns();
    }

    private IEnumerator UpdateNearbySpawns()
    {
        yield return StartCoroutine(FetchNearbySpawns());
        ToggleLoading();
    }

    private void ToggleLoading()
    {
        if (loadingInformation.activeInHierarchy)
        {
            loadingInformation.SetActive(false);
        }
    }

    private IEnumerator FetchNearbySpawns()
    {
        //yield return new WaitForSeconds(30);
        string url = $"{apiBaseUrl}/spawns/nearby?lat=41.1495&lon=-8.6108&radiusM=50000";

        
        //string url = $"{apiBaseUrl}/spawns/nearby?lat={currentLatitude}&lon={currentLongitude}&radiusM={spawnRadiusMeters}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string response = webRequest.downloadHandler.text;
                Debug.Log($"Raw API Response: {response}");
                if (response.Contains("\"features\":[]"))
                {
                    Debug.LogWarning("No spawns exist in the database at all!");
                }
                else
                    ProcessSpawnsResponse(response);
            }
            else
            {
                Debug.LogError($"Failed to fetch spawns: {webRequest.error}");
            }
        }
    }

    private void ProcessSpawnsResponse(string jsonResponse)
    {
        try
        {
            var response = JsonUtility.FromJson<NearbySpawnsResponse>(jsonResponse);

            if (response?.features != null)
            {
                ProcessValidSpawns(response.features);
            }
            else
            {
                Debug.LogWarning("No features found in response or response is null");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to process spawns: {e.Message}");
        }
    }

    private void CleanupExpiredSpawns(List<SpawnData> currentSpawns)
    {
        List<int> spawnsToRemove = new List<int>();

        foreach (var activeSpawn in activeSpawns)
        {
            bool stillExists = currentSpawns.Exists(s => s.id == activeSpawn.Key);
            if (!stillExists)
            {
                spawnsToRemove.Add(activeSpawn.Key);
            }
        }

        foreach (int spawnId in spawnsToRemove)
        {
            if (activeSpawns.TryGetValue(spawnId, out GameObject cardObject))
            {
                Destroy(cardObject);
                activeSpawns.Remove(spawnId);
            }
        }
    }

    private void ProcessValidSpawns(List<SpawnData> spawns)
    {
        Debug.Log($"Processing {spawns.Count} valid spawns");

        // Remove expired spawns
        CleanupExpiredSpawns(spawns);

        // Create new spawns
        foreach (var spawn in spawns)
        {
            if (spawn.status == "active" && !activeSpawns.ContainsKey(spawn.id))
            {
                Debug.Log($"Creating card {spawn.id} (Spawn ID: {spawn.id})");
                CreateCardInWorld(spawn);
            }
            else
            {
                Debug.Log($"Skipping spawn {spawn.id}, Already exists: {activeSpawns.ContainsKey(spawn.id)}");
            }
        }
    }

    private void CreateCardInWorld(SpawnData spawn)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("Card prefab is not assigned!");
            return;
        }

        // Convert geographic coordinates to Unity world coordinates
        Vector3 worldPosition = ConvertGeoToWorldPosition(spawn.lat, spawn.lon);

        worldPosition.y += 5f;

        GameObject cardObject = Instantiate(cardPrefab, worldPosition, cardPrefab.transform.rotation, cardContainer);

        activeSpawns[spawn.id] = cardObject;

        Debug.Log($"Created card {spawn.id} at position {worldPosition}");
    }

    // ARCGIS
    public Vector3 ConvertGeoToWorldPosition(double lat, double lon)
    {
        if (arcGISMap == null)
        {
            Debug.LogError("ArcGISMap is null!");
            return Vector3.zero;
        }

        var spatialReference = Esri.GameEngine.Geometry.ArcGISSpatialReference.WGS84();
        var geographicPoint = new Esri.GameEngine.Geometry.ArcGISPoint(lon, lat, spatialReference);

        var worldPoint = arcGISMap.GeographicToEngine(geographicPoint);

        // Convert to Unity Vector3 and transform to world space
        Vector3 localPosition = new Vector3(worldPoint.x, (float)worldPoint.y, (float)worldPoint.z);
        Vector3 worldPosition = arcGISMap.transform.TransformPoint(localPosition);

        Debug.Log($"Unity World Position: {worldPosition}");
        return worldPosition;
    }

    // To create new spawns in the world
    public IEnumerator CreateSpawnCoroutine(int cardId, double lat, double lon, DateTime expiresAt)
    {
        CreateSpawnRequest request = new CreateSpawnRequest
        {
            cardId = cardId,
            lat = lat,
            lon = lon,
            expiresAt = expiresAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        string jsonData = JsonUtility.ToJson(request);
        string url = $"{apiBaseUrl}/spawns";

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                CreateSpawnResponse response = JsonUtility.FromJson<CreateSpawnResponse>(webRequest.downloadHandler.text);
                Debug.Log($"Created spawn with ID: {response.spawn_id}");
            }
            else
            {
                Debug.LogError($"Failed to create spawn: {webRequest.error}");
            }
        }
    }
}
