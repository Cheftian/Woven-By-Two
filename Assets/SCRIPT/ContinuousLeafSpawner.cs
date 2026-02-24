using UnityEngine;
using System.Collections;

// This ensures the script always has a BoxCollider2D to define the area
[RequireComponent(typeof(BoxCollider2D))]
public class ContinuousLeafSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("The leaf prefab to drop from the sky.")]
    [SerializeField] private GameObject leafPrefab;
    
    [Tooltip("How often a new leaf falls (in seconds).")]
    [SerializeField] private float spawnInterval = 0.5f;

    [Header("Scale Settings")]
    [Tooltip("The minimum size of the spawned leaf.")]
    [SerializeField] private float minimumScale = 0.5f;
    
    [Tooltip("The maximum size of the spawned leaf.")]
    [SerializeField] private float maximumScale = 1.2f;

    private BoxCollider2D spawnArea;

    private void Start()
    {
        spawnArea = GetComponent<BoxCollider2D>();
        
        // Start the endless rain of leaves
        StartCoroutine(SpawnLeavesContinuously());
    }

    private IEnumerator SpawnLeavesContinuously()
    {
        // This loop will run forever as long as the object exists
        while (true)
        {
            DropSingleLeaf();
            
            // Wait for a few moments before dropping the next one
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void DropSingleLeaf()
    {
        if (leafPrefab == null) return;

        Bounds bounds = spawnArea.bounds;
        
        // Always spawn at the very top edge
        float topYPosition = bounds.max.y;
        
        // Pick a random horizontal spot between the left and right edges
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        Vector2 spawnPosition = new Vector2(randomX, topYPosition);

        // A slight random rotation to start
        float randomZRotation = Random.Range(0f, 360f);
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, randomZRotation);

        GameObject newlySpawnedLeaf = Instantiate(leafPrefab, spawnPosition, spawnRotation);

        float randomScale = Random.Range(minimumScale, maximumScale);
        newlySpawnedLeaf.transform.localScale = new Vector3(randomScale, randomScale, 1f);
    }

    // --- MAGICAL VISUAL ASSISTANT FOR TIAN ---
    // This will draw a glowing line in the Scene view so Tian can see exactly 
    // where the leaves will spawn from!
    private void OnDrawGizmos()
    {
        BoxCollider2D colliderArea = GetComponent<BoxCollider2D>();
        
        if (colliderArea != null)
        {
            // Give the magic line a soft green color
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.7f); 
            Bounds bounds = colliderArea.bounds;
            
            // Draw the exact top edge where leaves will appear
            Vector3 topLeft = new Vector3(bounds.min.x, bounds.max.y, 0f);
            Vector3 topRight = new Vector3(bounds.max.x, bounds.max.y, 0f);
            Gizmos.DrawLine(topLeft, topRight);
            
            // Draw tiny spheres at the ends of the line to make it crystal clear
            Gizmos.DrawSphere(topLeft, 0.1f);
            Gizmos.DrawSphere(topRight, 0.1f);
        }
    }
}