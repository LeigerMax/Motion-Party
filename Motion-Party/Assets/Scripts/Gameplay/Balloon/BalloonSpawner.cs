using UnityEngine;

public class BalloonSpawner : MonoBehaviour
{
    public GameObject balloonPrefab;
    public float spawnRate = 4.5f; 
    public float xRange = 5f;    // Largeur du spawn

    void Start()
    {
        InvokeRepeating("SpawnBalloon", 1f, spawnRate); 
    }

    void SpawnBalloon()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-xRange, xRange), transform.position.y, 0);
        Instantiate(balloonPrefab, spawnPosition, Quaternion.identity);
    }

    
}