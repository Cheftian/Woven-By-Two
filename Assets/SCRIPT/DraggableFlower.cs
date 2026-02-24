using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(RandomAudioOnAwake))]
public class DraggableFlower : MonoBehaviour
{
    [Header("Drag Settings")]
    [Tooltip("How small the flower becomes when held by the cursor.")]
    [SerializeField] private float dragScaleMultiplier = 0.8f;

    [Tooltip("The Y position where the flower will vanish and respawn.")]
    [SerializeField] private float destroyYThreshold = -10f;

    [Header("Growth Animation Settings")]
    [Tooltip("How deep below the surface the flower starts growing from when respawning.")]
    [SerializeField] private float growStartDepth = 2.0f;
    
    [Tooltip("How fast the flower grows upwards to its final position.")]
    [SerializeField] private float growSpeed = 3.0f;

    [Tooltip("How long the flower waits before growing back.")]
    [SerializeField] private float respawnDelay = 1.0f;

    private Vector3 originalScale;
    private Vector2 homePosition;
    private Rigidbody2D rb;
    private RandomAudioOnAwake randomAudio;
    private Collider2D flowerCollider;
    
    private bool isBeingDragged = false;
    private bool isRespawning = false;
    private Camera mainCamera;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        randomAudio = GetComponent<RandomAudioOnAwake>();
        flowerCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;

        originalScale = transform.localScale;
        
        // Remember the exact spot it was planted in the scene
        homePosition = transform.position; 
        
        rb.isKinematic = true; 
    }

    private void Update()
    {
        // If it's already respawning, do nothing
        if (isRespawning) return;

        // Trigger respawn if it falls far below the screen
        if (transform.position.y < destroyYThreshold)
        {
            StartCoroutine(RespawnSequence());
        }
    }

    private void OnMouseDown()
    {
        if (NarrationManager.Instance != null && NarrationManager.Instance.IsNarrationPlaying) return;
        if (isRespawning) return;

        isBeingDragged = true;
        transform.localScale = originalScale * dragScaleMultiplier;
        randomAudio.PlayRandomSound();
        
        rb.isKinematic = true;
        rb.linearVelocity = Vector2.zero;
    }

    private void OnMouseDrag()
    {
        if (isBeingDragged && !isRespawning)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f; 
            transform.position = mouseWorldPos;
        }
    }

    private void OnMouseUp()
    {
        if (!isBeingDragged || isRespawning) return;

        isBeingDragged = false;
        randomAudio.PlayRandomSound();

        Vector2 dropPosition = transform.position;
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(dropPosition);
        bool collected = false;

        foreach (Collider2D hit in hitColliders)
        {
            FlowerCollectorManager collector = hit.GetComponent<FlowerCollectorManager>();
            if (collector != null)
            {
                collector.CollectFlower();
                collected = true;
                
                // Instead of destroying, we start the regrow magic
                StartCoroutine(RespawnSequence());
                break;
            }
        }

        if (!collected)
        {
            rb.isKinematic = false; 
        }
    }

    private IEnumerator RespawnSequence()
    {
        isRespawning = true;
        rb.isKinematic = true;
        rb.linearVelocity = Vector2.zero;
        
        // 1. Hide the flower completely (including any fade sprites if they exist)
        SpriteRenderer[] allSprites = GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer sr in allSprites)
        {
            sr.enabled = false;
        }
        flowerCollider.enabled = false;

        // Wait a beautifully brief moment in silence
        yield return new WaitForSeconds(respawnDelay);

        // 2. Move to the deep starting position
        Vector2 startingPosition = new Vector2(homePosition.x, homePosition.y - growStartDepth);
        transform.position = startingPosition;
        transform.localScale = originalScale;

        // 3. Show the flower again
        foreach(SpriteRenderer sr in allSprites)
        {
            sr.enabled = true;
        }
        flowerCollider.enabled = true;

        // 4. Smoothly animate the growth back to the original home position
        while (Vector2.Distance(transform.position, homePosition) > 0.01f)
        {
            transform.position = Vector2.Lerp(transform.position, homePosition, Time.deltaTime * growSpeed);
            yield return null; 
        }

        // Snap it perfectly into place
        transform.position = homePosition;
        isRespawning = false;
    }
}