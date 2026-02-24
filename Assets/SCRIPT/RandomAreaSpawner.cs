using UnityEngine;
using System.Collections;

// This ensures the script always has a BoxCollider2D to define the area
[RequireComponent(typeof(BoxCollider2D))]
public class GrowingFlowerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("The prefab that will be scattered from the top edge.")]
    [SerializeField] private GameObject prefabToSpawn;
    
    [Tooltip("Total number of items to spawn.")]
    [SerializeField] private int totalObjectsToSpawn = 20;

    [Tooltip("How long to wait before spawning the next flower.")]
    [SerializeField] private float timeBetweenSpawns = 0.2f;

    [Header("Growth Animation Settings")]
    [Tooltip("How deep below the surface the flower starts growing from.")]
    [SerializeField] private float growStartDepth = 2.0f;
    
    [Tooltip("How fast the flower grows upwards to its final position.")]
    [SerializeField] private float growSpeed = 3.0f;

    [Header("Scale Settings")]
    [Tooltip("The minimum size of the spawned object.")]
    [SerializeField] private float minimumScale = 0.5f;
    
    [Tooltip("The maximum size of the spawned object.")]
    [SerializeField] private float maximumScale = 2.0f;

    private BoxCollider2D spawnArea;

    private void Start()
    {
        spawnArea = GetComponent<BoxCollider2D>();
        
        // Start the magical sequence of growing flowers
        StartCoroutine(ScatterAndGrowObjects());
    }

    private IEnumerator ScatterAndGrowObjects()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("The prefab is missing! Please assign it in the inspector.");
            yield break; // Stop the magic if there is no prefab
        }

        Bounds bounds = spawnArea.bounds;
        
        // Find the absolute highest point of the collider to use as a fixed ceiling/ground
        float topYPosition = bounds.max.y;

        for (int i = 0; i < totalObjectsToSpawn; i++)
        {
            // Calculate a random horizontal position within the box
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            
            // This is where the flower WILL end up
            Vector2 targetPosition = new Vector2(randomX, topYPosition);
            
            // This is where the flower STARTS growing from (hidden below)
            Vector2 startingPosition = new Vector2(randomX, topYPosition - growStartDepth);

            // Calculate a random rotation
            float randomZRotation = Random.Range(0f, 360f);
            Quaternion spawnRotation = Quaternion.Euler(0f, 0f, randomZRotation);

            // Bring the object to life at the deep starting position
            GameObject newlySpawnedObject = Instantiate(prefabToSpawn, startingPosition, spawnRotation);

            // Apply a random size to make each unique
            float randomScale = Random.Range(minimumScale, maximumScale);
            newlySpawnedObject.transform.localScale = new Vector3(randomScale, randomScale, 1f);

            // Command this specific flower to start moving upwards
            StartCoroutine(AnimateFlowerGrowth(newlySpawnedObject.transform, targetPosition));

            // Wait a beautifully brief moment before planting the next seed
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    private IEnumerator AnimateFlowerGrowth(Transform flowerTransform, Vector2 targetPosition)
    {
        // Keep moving upwards smoothly as long as the flower exists and hasn't reached the top
        while (flowerTransform != null && Vector2.Distance(flowerTransform.position, targetPosition) > 0.01f)
        {
            // Lerp creates a smooth movement that slows down gracefully as it reaches the destination
            flowerTransform.position = Vector2.Lerp(flowerTransform.position, targetPosition, Time.deltaTime * growSpeed);
            
            // Wait for the next frame
            yield return null; 
        }

        // Snap to the exact final position just to be perfectly aligned
        if (flowerTransform != null)
        {
            flowerTransform.position = targetPosition;
        }
    }
}