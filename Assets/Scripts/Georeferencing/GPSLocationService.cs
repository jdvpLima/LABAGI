using Esri.GameEngine.Geometry;
using Esri.ArcGISMapsSDK.Components;
using System.Collections;
using UnityEngine;

public class GPSLocationService : MonoBehaviour
{
    public ArcGISRebaseComponent rebaseComponent; // Assign from scene
    private ArcGISLocationComponent markerLocation;

    public GameObject playerMarkerPrefab;
    private GameObject playerMarker;

    async void Start()
    {
        // start location service
        Input.location.Start(1f, 1f);

        // await for location
        while (Input.location.status == LocationServiceStatus.Initializing)
            await System.Threading.Tasks.Task.Delay(200);

        // if there's an issue with getting location
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Could not get location.");
            return;
        }

        // create marker
        playerMarker = Instantiate(playerMarkerPrefab, rebaseComponent.transform);

        // Add location component
        markerLocation = playerMarker.AddComponent<ArcGISLocationComponent>();
    }

    void Update()
    {
        // if location is not working, stop
        if (Input.location.status != LocationServiceStatus.Running) return;

        // get location info
        double lat = Input.location.lastData.latitude;
        double lon = Input.location.lastData.longitude;

        // Create ArcGIS geographic point
        ArcGISPoint point = new ArcGISPoint(
            lon,           // x = longitude
            lat,           // y = latitude
            0,             // z = altitude
            ArcGISSpatialReference.WGS84()
        );

        markerLocation.Position = point;
    }
}
