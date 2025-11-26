using System;
using System.Collections;
using UnityEngine;

public class PortoCardSpawner : MonoBehaviour
{
    [Header("Spawn Manager Reference")]
    public CardSpawnManager spawnManager;

    [Header("Porto Locations")]
    public PortoLocation[] portoLocations = new PortoLocation[]
    {
        new PortoLocation("Clérigos Tower", 41.1457, -8.6145, 1),
        new PortoLocation("Ribeira Square", 41.1409, -8.6114, 2),
        new PortoLocation("Lello Bookstore", 41.1467, -8.6154, 3),
        new PortoLocation("São Bento Station", 41.1456, -8.6096, 4),
        new PortoLocation("Crystal Palace Gardens", 41.1487, -8.6250, 5),
        new PortoLocation("Foz do Douro", 41.1513, -8.6753, 6),
        new PortoLocation("Serralves Museum", 41.1585, -8.6613, 7),
        new PortoLocation("Porto Cathedral", 41.1428, -8.6115, 8),
        new PortoLocation("Dom Luís I Bridge", 41.1389, -8.6098, 9),
        new PortoLocation("Bolhão Market", 41.1497, -8.6073, 10)
    };

    [Header("Spawn Settings")]
    public bool spawnOnStart = true;
    public float delayBetweenSpawns = 2f;
    public int hoursUntilExpiry = 24;

    void Start()
    {
        if (spawnManager == null)
        {
            spawnManager = FindFirstObjectByType<CardSpawnManager>();
        }

        if (spawnOnStart)
        {
            StartCoroutine(SpawnAllPortoCards());
        }
    }

    private IEnumerator SpawnAllPortoCards()
    {
        if (spawnManager == null)
        {
            Debug.LogError("CardSpawnManager not found!");
            yield break;
        }

        Debug.Log($"Starting to spawn {portoLocations.Length} cards around Porto...");

        for (int i = 0; i < portoLocations.Length; i++)
        {
            var location = portoLocations[i];

            // Calculate expiry time
            DateTime expiresAt = DateTime.UtcNow.AddHours(hoursUntilExpiry);

            // Start the coroutine to create the spawn
            StartCoroutine(spawnManager.CreateSpawnCoroutine(
                location.cardId,
                location.latitude,
                location.longitude,
                expiresAt
            ));

            Debug.Log($"Spawning card {location.cardId} at {location.locationName} " +
                     $"(Lat: {location.latitude}, Lon: {location.longitude})");

            // Wait before spawning next card
            yield return new WaitForSeconds(delayBetweenSpawns);
        }

        Debug.Log("Finished spawning all Porto cards!");
    }
}

[System.Serializable]
public class PortoLocation
{
    public string locationName;
    public double latitude;
    public double longitude;
    public int cardId;

    public PortoLocation(string name, double lat, double lon, int id)
    {
        locationName = name;
        latitude = lat;
        longitude = lon;
        cardId = id;
    }
}