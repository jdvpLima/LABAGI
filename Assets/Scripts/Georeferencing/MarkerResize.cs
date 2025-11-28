using System.Collections.Generic;
using UnityEngine;

public class MarkerResize : MonoBehaviour
{
    [Header("Scaling Settings")]
    public float baseScale = 1f;
    public float scaleFactor = 0.1f;
    public float minScale = 0.3f;
    public float maxScale = 3f;
    public float maxDistance = 100f;
    public float updateInterval = 0.1f; // Update 10 times per second for performance

    private Camera mainCamera;
    private Transform playerTransform;
    private GameObject[] markers;
    private float lastUpdateTime;

    void Start()
    {
        mainCamera = Camera.main;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Find all markers initially
        FindAllMarkers();

        Debug.Log($"Found {markers.Length} markers with tag 'Marker'");
    }

    void Update()
    {
        // Update at intervals for better performance
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateAllMarkerScales();
            lastUpdateTime = Time.time;
        }
    }

    private void FindAllMarkers()
    {
        markers = GameObject.FindGameObjectsWithTag("Marker");
    }

    [ContextMenu("Refresh Markers")]
    public void RefreshMarkers()
    {
        FindAllMarkers();
        Debug.Log($"Refreshed markers. Now tracking {markers.Length} markers");
    }

    private void UpdateAllMarkerScales()
    {
        Transform referenceTransform = playerTransform != null ? playerTransform : mainCamera.transform;

        if (referenceTransform == null)
        {
            Debug.LogWarning("No reference transform found for scaling");
            return;
        }

        // Update scale for each marker
        foreach (GameObject marker in markers)
        {
            if (marker != null && marker.activeInHierarchy)
            {
                UpdateMarkerScale(marker, referenceTransform);
            }
        }
    }

    private void UpdateMarkerScale(GameObject marker, Transform referenceTransform)
    {
        float distance = Vector3.Distance(marker.transform.position, referenceTransform.position);

        // Scale based on distance (inverse relationship)
        float scale = baseScale * (1f / (1f + scaleFactor * distance));

        // Clamp the scale
        scale = Mathf.Clamp(scale, minScale, maxScale);

        marker.transform.localScale = Vector3.one * scale;
    }

    public void RegisterMarker(GameObject newMarker)
    {
        if (newMarker.CompareTag("Marker"))
        {
            // Add to our array
            List<GameObject> markerList = new List<GameObject>(markers);
            if (!markerList.Contains(newMarker))
            {
                markerList.Add(newMarker);
                markers = markerList.ToArray();
                Debug.Log($"Registered new marker: {newMarker.name}");
            }
        }
    }

    public void UnregisterMarker(GameObject markerToRemove)
    {
        List<GameObject> markerList = new List<GameObject>(markers);
        if (markerList.Remove(markerToRemove))
        {
            markers = markerList.ToArray();
            Debug.Log($"Unregistered marker: {markerToRemove.name}");
        }
    }

    public void LogMarkerCount()
    {
        Debug.Log($"Currently tracking {markers.Length} markers");

        foreach (GameObject marker in markers)
        {
            if (marker != null)
            {
                Debug.Log($" - {marker.name} at {marker.transform.position}");
            }
        }
    }

    void OnEnable()
    {
        // For now, we'll refresh periodically
        InvokeRepeating("RefreshMarkers", 10f, 10f); // Refresh every 10 seconds
    }

    void OnDisable()
    {
        CancelInvoke("RefreshMarkers");
    }
}
