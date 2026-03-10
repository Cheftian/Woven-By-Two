using UnityEngine;
using System.Collections;

// This ensures the script always has a BoxCollider2D to define the area
[RequireComponent(typeof(BoxCollider2D))]
public class GrowingFlowerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("The prefab that will be scattered from the top edge.")]
    [SerializeField] private GameObject prefabToSpawn;
    
    [Tooltip("Total number of items to spawn (if NOT using cursor or character mode).")]
    [SerializeField] private int totalObjectsToSpawn = 20;

    [Tooltip("How long to wait before spawning the next flower.")]
    [SerializeField] private float timeBetweenSpawns = 0.2f;

    [Header("Interactive Magic Settings")]
    [Tooltip("If true, flowers only grow when the cursor touches this area.")]
    [SerializeField] private bool isCursorSpawner = false;

    [Tooltip("If true, flowers will grow exactly where the specified character is standing.")]
    [SerializeField] private bool isCharacterSpawner = false;

    [Tooltip("The tag of the character that makes flowers bloom beneath their feet.")]
    [SerializeField] private string characterTag = "Man";

    [Tooltip("If true, interacting with this spawner triggers the grand ending event.")]
    [SerializeField] private bool isEndingSpawner = false;

    [Header("Ending Movement Settings")]
    [Tooltip("The first character that will flip and walk. The camera will follow this character.")]
    [SerializeField] private Transform characterA;

    [Tooltip("The second character that will walk with character A.")]
    [SerializeField] private Transform characterB;

    [Tooltip("The exact X coordinate where character A will stop.")]
    [SerializeField] private float endingTargetXA = 30f;

    [Tooltip("The exact X coordinate where character B will stop.")]
    [SerializeField] private float endingTargetXB = 32f;

    [Tooltip("How fast the characters walk during the ending.")]
    [SerializeField] private float walkSpeed = 2f;
    
    [Tooltip("Reference to the camera script to follow character A.")]
    [SerializeField] private HorizontalCameraMovement cameraMovementScript;

    [Tooltip("The magical door to notify when the walking sequence is complete.")]
    [SerializeField] private MagicalObjectToggler endDoorTrigger;

    [Header("Narration Settings")]
    [Tooltip("Narration ID to play the very first time a flower is grown by interaction.")]
    [SerializeField] private string firstFlowerNarrationId;
    
    [Tooltip("Narration ID to play when both characters have arrived at their destinations.")]
    [SerializeField] private string endingArrivalNarrationId;

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

    [Header("Destruction Settings")]
    [Tooltip("If true, the flowers will wither and fall shortly after blooming.")]
    [SerializeField] private bool isDestructive = false;

    [Tooltip("How long the flower stays in full bloom before it tragically withers away.")]
    [SerializeField] private float timeBeforeWither = 0.5f;

    [Tooltip("How fast each flower shrinks and falls back into the earth (in seconds).")]
    [SerializeField] private float flowerWitherDuration = 0.5f;

    private BoxCollider2D spawnArea;
    private Camera mainCamera;
    
    // Internal trackers
    private float interactionSpawnTimer = 0f;
    private bool endingHasTriggered = false;
    private bool hasPlayedFirstFlowerNarration = false;

    // Character presence trackers
    private bool isCharacterInside = false;
    private Transform characterTransform;

    private void Start()
    {
        spawnArea = GetComponent<BoxCollider2D>();
        mainCamera = Camera.main;
        
        // Start the magical sequence of growing flowers IF NOT in any interactive mode
        if (!isCursorSpawner && !isCharacterSpawner)
        {
            StartCoroutine(ScatterAndGrowObjects());
        }
    }

    private void Update()
    {
        // Jangan biarkan bunga tumbuh jika semesta sedang membeku karena narasi
        if (NarrationManager.Instance != null && NarrationManager.Instance.IsNarrationPlaying) return;

        bool shouldSpawnThisFrame = false;
        float targetSpawnX = 0f;

        // 1. Cek sihir kursor
        if (isCursorSpawner)
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (spawnArea.OverlapPoint(mousePos))
            {
                shouldSpawnThisFrame = true;
                targetSpawnX = mousePos.x;
            }
        }

        // 2. Cek pijakan langkah sang karakter (Ini akan menimpa posisi kursor jika keduanya aktif)
        if (isCharacterSpawner && isCharacterInside && characterTransform != null)
        {
            shouldSpawnThisFrame = true;
            targetSpawnX = characterTransform.position.x;
        }

        // 3. Tumbuhkan bunga jika ada interaksi
        if (shouldSpawnThisFrame)
        {
            interactionSpawnTimer += Time.deltaTime;

            if (interactionSpawnTimer >= timeBetweenSpawns)
            {
                interactionSpawnTimer = 0f;
                SpawnSingleFlowerAt(targetSpawnX);

                // --- FIRST FLOWER NARRATION ---
                if (!hasPlayedFirstFlowerNarration && !string.IsNullOrEmpty(firstFlowerNarrationId))
                {
                    hasPlayedFirstFlowerNarration = true;
                    if (NarrationManager.Instance != null)
                    {
                        NarrationManager.Instance.PlayNarration(firstFlowerNarrationId);
                    }
                }

                // --- THE GRAND ENDING TRIGGER ---
                if (isEndingSpawner && !endingHasTriggered)
                {
                    endingHasTriggered = true;
                    StartCoroutine(GrandEndingRoutine());
                }
            }
        }
    }

    // --- CHARACTER TRIGGER DETECTION ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCharacterSpawner && collision.CompareTag(characterTag))
        {
            isCharacterInside = true;
            characterTransform = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isCharacterSpawner && collision.CompareTag(characterTag))
        {
            isCharacterInside = false;
            characterTransform = null;
        }
    }

    private void SpawnSingleFlowerAt(float targetX)
    {
        if (prefabToSpawn == null) return;

        Bounds bounds = spawnArea.bounds;
        float topYPosition = bounds.max.y;

        Vector2 targetPosition = new Vector2(targetX, topYPosition);
        Vector2 startingPosition = new Vector2(targetX, topYPosition - growStartDepth);

        float randomZRotation = Random.Range(0f, 360f);
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, randomZRotation);

        GameObject newlySpawnedObject = Instantiate(prefabToSpawn, startingPosition, spawnRotation);

        float randomScale = Random.Range(minimumScale, maximumScale);
        newlySpawnedObject.transform.localScale = new Vector3(randomScale, randomScale, 1f);

        StartCoroutine(AnimateFlowerGrowth(newlySpawnedObject, targetPosition));
    }

    private IEnumerator ScatterAndGrowObjects()
    {
        if (prefabToSpawn == null) yield break; 

        Bounds bounds = spawnArea.bounds;
        float topYPosition = bounds.max.y;

        for (int i = 0; i < totalObjectsToSpawn; i++)
        {
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            SpawnSingleFlowerAt(randomX);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    private IEnumerator AnimateFlowerGrowth(GameObject flower, Vector2 targetPosition)
    {
        Transform flowerTransform = flower.transform;

        while (flowerTransform != null && Vector2.Distance(flowerTransform.position, targetPosition) > 0.01f)
        {
            flowerTransform.position = Vector2.Lerp(flowerTransform.position, targetPosition, Time.deltaTime * growSpeed);
            yield return null; 
        }

        if (flowerTransform != null)
        {
            flowerTransform.position = targetPosition;

            if (isDestructive)
            {
                StartCoroutine(WitherAndFallRoutine(flower));
            }
        }
    }

    private IEnumerator WitherAndFallRoutine(GameObject flower)
    {
        yield return new WaitForSeconds(timeBeforeWither);
        if (flower == null) yield break;

        float elapsed = 0f;
        Vector3 startScale = flower.transform.localScale;
        Vector3 startPos = flower.transform.position;

        while (elapsed < flowerWitherDuration)
        {
            if (flower == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / flowerWitherDuration;

            flower.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            flower.transform.position = startPos - new Vector3(0f, t * 2f, 0f);

            yield return null;
        }

        if (flower != null) Destroy(flower);
    }

    // --- MAGICAL ENDING SEQUENCE ---
    private IEnumerator GrandEndingRoutine()
    {
        Debug.Log("Puncak keajaiban telah tersentuh. Dua jiwa mulai berjalan bersama.");

        if (characterA != null)
        {
            Vector3 scale = characterA.localScale;
            scale.x = -Mathf.Abs(scale.x); 
            characterA.localScale = scale;
            
            if (cameraMovementScript != null)
            {
                cameraMovementScript.StartAutomaticFollow(characterA);
            }
        }

        while (true)
        {
            bool aIsMoving = false;
            bool bIsMoving = false;

            if (characterA != null && Mathf.Abs(characterA.position.x - endingTargetXA) > 0.05f)
            {
                Vector3 pos = characterA.position;
                pos.x = Mathf.MoveTowards(pos.x, endingTargetXA, walkSpeed * Time.deltaTime);
                characterA.position = pos;
                aIsMoving = true;
            }

            if (characterB != null && Mathf.Abs(characterB.position.x - endingTargetXB) > 0.05f)
            {
                Vector3 pos = characterB.position;
                pos.x = Mathf.MoveTowards(pos.x, endingTargetXB, walkSpeed * Time.deltaTime);
                characterB.position = pos;
                bIsMoving = true;
            }

            if (!aIsMoving && !bIsMoving) break;

            yield return null;
        }

        Debug.Log("Mereka telah sampai di ujung perjalanan.");

        if (NarrationManager.Instance != null && !string.IsNullOrEmpty(endingArrivalNarrationId))
        {
            NarrationManager.Instance.PlayNarration(endingArrivalNarrationId);
        }

        // --- THE MAGIC CORD --- Memberi tahu pintu bahwa ia kini boleh ditutup
        if (endDoorTrigger != null)
        {
            endDoorTrigger.AllowDoorToClose();
        }
    }
}