using Esri.ArcGISMapsSDK.Components;
using Esri.GameEngine.Geometry;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class CreateSpawnRequestClaim
{
    public int spawn_id;
}

public class GPSLocationService : MonoBehaviour
{
    [SerializeField]
    private char unit = 'K';

    [SerializeField]
    private GameObject playerPrefab;

    public float checkDistance = 0.1f; // 100 meters

    public bool gps_ok = false;

    GPSLoc startLoc = new GPSLoc();
    GPSLoc currLoc = new GPSLoc();

    public GameObject collectButton; // Reference to your UI button
    private CardSpawnManager spawnManager;

    [SerializeField] private TextMeshProUGUI playerLatitude;
    [SerializeField] private TextMeshProUGUI playerLongitude;

    private ArcGISLocationComponent locationComponent;

    public string apiBaseUrl = "https://lagabi-group2-backend.onrender.com/api";

    bool measureDistance = false;

    IEnumerator Start()
    {
        spawnManager = FindFirstObjectByType<CardSpawnManager>();

#if UNITY_ANDROID
        yield return StartCoroutine(RequestLocationPermission());
#endif

        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Location not enabled on device or app does not have permission to access location");
        }
        // Starts the location service.
        Input.location.Start(0.5f, 0.5f);

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location");

            yield break;
        }
        else
        {
            gps_ok = true;

        }

        GameObject player = Instantiate(playerPrefab);

        locationComponent = player.GetComponent<ArcGISLocationComponent>();


        yield return gps_ok;
    }

    void Update()
    {
        if (gps_ok)
        {
            currLoc.lat = Input.location.lastData.latitude;
            currLoc.lon = Input.location.lastData.longitude;

            playerLatitude.text = "Latitude: " + Input.location.lastData.latitude;
            playerLongitude.text = "Longitude: " + Input.location.lastData.longitude;

            locationComponent.Position = new ArcGISPoint(currLoc.lon, currLoc.lat, 50, ArcGISSpatialReference.WGS84());

            CheckSpawnProximity();
        }
        else
        {
            playerLatitude.text = "Player's location is unavailable.";
            playerLongitude.text = "";
        }
    }

    private IEnumerator RequestLocationPermission()
    {
#if UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);

            // Espera ate o utilizador responder
            while (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation))
            {
                yield return null;
            }
        }
#endif
    }


    private void CheckSpawnProximity()
    {
        if (spawnManager == null || !gps_ok)
        {
            collectButton.SetActive(false);
            return;
        }

        bool anySpawnInRange = false;
        GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");


        foreach (GameObject marker in markers)
        {
            if (marker != null)
            {
                SpawnData spawnData = marker.GetComponent<SpawnData>();

                if (spawnData != null &&
                    IsSpawnWithinDistance(spawnData.lat, spawnData.lon, checkDistance))
                {
                    anySpawnInRange = true;
                    break; // Found one, no need to check others
                }
            }
        }

        collectButton.SetActive(anySpawnInRange);
    }

    private SpawnData CheckClosestSpawn()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");
        SpawnData closestSpawn = null;

        double currentDistance = 0;
        double smallestDistance = 1000;

        foreach (GameObject marker in markers)
        {
            if (marker != null)
            {
                SpawnData spawnData = marker.GetComponent<SpawnData>();

                currentDistance = distance(currLoc.lat, currLoc.lon, spawnData.lat, spawnData.lon, 'K');

                if (currentDistance < smallestDistance)
                {
                    smallestDistance = currentDistance;
                    closestSpawn = spawnData;
                }
            }
        }

        return closestSpawn;
    }

    public void ClaimSpawn()
    {
        SpawnData spawn = CheckClosestSpawn();

        if (spawn != null)
        {
            StartCoroutine(ClaimCard(spawn.id));
        }
        else
        {
            Debug.LogWarning("No spawn found to claim!");
        }
    }

    public bool IsSpawnWithinDistance(double spawnLat, double spawnLon, double maxDistanceKm)
    {
        if (!gps_ok) return false;

        double distanceToSpawn = distance(currLoc.lat, currLoc.lon, spawnLat, spawnLon, 'K');
        return distanceToSpawn <= maxDistanceKm;
    }

    public IEnumerator ClaimCard(int spawnId)
    {
        CreateSpawnRequest request = new CreateSpawnRequest
        {
            cardId = spawnId
        };

        string jsonData = JsonUtility.ToJson(request);
        string url = $"{apiBaseUrl}/spawns/{spawnId}/claim";

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
                Debug.Log($"Claimed card in spawn with ID: {response.spawn_id}");
            }
            else
            {
                Debug.LogError($"Failed to claim card in spawn: {webRequest.error}");
            }
        }
    }

    public void StopGPS()
    {
        Input.location.Stop();

    }

    public void StoreCurrentGPS()
    {
        startLoc = new GPSLoc(currLoc.lon, currLoc.lat);
        measureDistance = true;
    }

    private double distance(double lat1, double lon1, double lat2, double lon2, char unit)
    {
        if ((lat1 == lat2) && (lon1 == lon2))
        {
            return 0;
        }
        else
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60 * 1.1515;
            if (unit == 'K')
            {
                dist = dist * 1.609344;
            }
            else if (unit == 'N')
            {
                dist = dist * 0.8684;
            }
            return (dist);
        }
    }

    //  This function converts decimal degrees to radians
    private double deg2rad(double deg)
    {
        return (deg * Math.PI / 180.0);
    }

    //  This function converts radians to decimal degrees
    private double rad2deg(double rad)
    {
        return (rad / Math.PI * 180.0);
    }

}

public class GPSLoc
{
    public double lon;
    public double lat;

    public GPSLoc()
    {
        lon = 0;
        lat = 0;
    }
    public GPSLoc(double lon, double lat)
    {
        this.lon = lon;
        this.lat = lat;
    }

    public string getLocData()
    {
        return "Lat: " + lat + " \nLon: " + lon;
    }
}