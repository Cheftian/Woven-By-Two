using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class MusicalBubbleMinigame : MonoBehaviour
{
    [Header("Bubble Settings")]
    [SerializeField] private BubbleBehavior bubblePrefab;
    [SerializeField] private int maxActiveBubbles = 3;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float bubbleLifeTime = 4f;
    [SerializeField] private float bubbleFloatSpeed = 1.5f;
    [SerializeField] private Sprite bubbleSpriteA;
    [SerializeField] private Sprite bubbleSpriteB;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip bubbleSpawnSFX;
    [SerializeField] private AudioClip bubbleMissedPopSFX;
    [SerializeField] private AudioClip[] musicalNotesSFX = new AudioClip[8];

    [Header("Prologue & Camera Settings")]
    [Tooltip("Check this if this minigame is the main menu / prologue puzzle.")]
    [SerializeField] private bool isPrologueMinigame = false;
    
    [Tooltip("The camera script to move down after the prologue.")]
    [SerializeField] private HorizontalCameraMovement cameraScript;
    
    [Tooltip("The exact Y coordinate the camera will glide to for the main game.")]
    [SerializeField] private float targetCameraY = 0f;
    
    [Tooltip("The new left boundary for the main game area.")]
    [SerializeField] private float mainGameLeftBoundary = -20f;
    
    [Tooltip("The new right boundary for the main game area.")]
    [SerializeField] private float mainGameRightBoundary = 20f;
    
    [Tooltip("How long it takes for the camera to pan down to the main game.")]
    [SerializeField] private float cameraTransitionDuration = 3f;

    [Header("Player Transition Settings")]
    [Tooltip("If true, the player will NOT move or fade out when the puzzle is completed.")]
    [SerializeField] private bool dontMovePlayer = false;

    [Tooltip("The player object that will be moved.")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("The SpriteRenderer of the player to create the fading illusion.")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    [Tooltip("The exact Y coordinate the player will be teleported to for the main game (Only used if NOT prologue AND dontMovePlayer is false).")]
    [SerializeField] private float targetPlayerY = -2f;
    
    [Tooltip("How long the player and next object take to fade out and fade in.")]
    [SerializeField] private float fadeDuration = 1f;

    [Header("Next Level Settings")]
    [Tooltip("The next minigame object to activate when this one is solved.")]
    [SerializeField] private GameObject nextMinigameObject;

    private BoxCollider2D spawnArea;
    private AudioSource audioSource;
    
    private int currentActiveBubbles = 0;
    private int currentStreak = 0;
    private bool isMinigameActive = false;

    private void Awake()
    {
        spawnArea = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        isMinigameActive = true;
        currentStreak = 0;
        currentActiveBubbles = 0;
        StartCoroutine(SpawnBubbleRoutine());
    }

    private IEnumerator SpawnBubbleRoutine()
    {
        while (isMinigameActive)
        {
            if (currentActiveBubbles < maxActiveBubbles)
            {
                SpawnSingleBubble();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnSingleBubble()
    {
        Bounds bounds = spawnArea.bounds;
        
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float bottomYPosition = bounds.min.y;
        
        Vector2 spawnPosition = new Vector2(randomX, bottomYPosition);

        BubbleBehavior newBubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);
        newBubble.Initialize(this, bubbleLifeTime, bubbleFloatSpeed, bubbleSpriteA, bubbleSpriteB);
        
        currentActiveBubbles++;

        if (bubbleSpawnSFX != null) audioSource.PlayOneShot(bubbleSpawnSFX);
    }

    public void BubbleClicked(BubbleBehavior bubble)
    {
        currentActiveBubbles--;
        if (!isMinigameActive) return;

        if (currentStreak < musicalNotesSFX.Length && musicalNotesSFX[currentStreak] != null)
        {
            audioSource.PlayOneShot(musicalNotesSFX[currentStreak]);
        }

        currentStreak++;

        if (currentStreak >= 5) 
        {
            StartCoroutine(VictorySequence());
        }
    }

    public void BubbleMissed(BubbleBehavior bubble)
    {
        currentActiveBubbles--;
        if (!isMinigameActive) return;

        if (bubbleMissedPopSFX != null) audioSource.PlayOneShot(bubbleMissedPopSFX);

        currentStreak = 0;
    }

    private IEnumerator VictorySequence()
    {
        isMinigameActive = false; 

        // 1. Sang pemain memudar terlebih dahulu dalam keheningan (HANYA jika diizinkan bergerak)
        if (!dontMovePlayer && playerSpriteRenderer != null)
        {
            yield return StartCoroutine(FadePlayerSprite(1f, 0f));
        }

        // 2. Jika ini adalah Prologue, biarkan kamera turun menyusuri kedalaman menggunakan koordinat Y
        if (isPrologueMinigame && cameraScript != null)
        {
            cameraScript.TransitionToNewArea(targetCameraY, mainGameLeftBoundary, mainGameRightBoundary, cameraTransitionDuration);
            // Menunggu hingga kamera selesai meluncur turun dengan anggun
            yield return new WaitForSeconds(cameraTransitionDuration);
        }

        // 3. Raga sang pemain berpindah tempat HANYA jika bukan prologue DAN diizinkan bergerak
        if (!isPrologueMinigame && !dontMovePlayer && playerTransform != null)
        {
            Vector3 newPlayerPos = playerTransform.position;
            // X tetap di tengah area puzzle baru, sedangkan Y memakai nilai dari Inspector
            newPlayerPos.x = transform.position.x; 
            newPlayerPos.y = targetPlayerY; 
            playerTransform.position = newPlayerPos;
        }

        // 4. Dunia selanjutnya muncul perlahan dari ketiadaan
        if (nextMinigameObject != null)
        {
            yield return StartCoroutine(FadeInNextLevel(nextMinigameObject));
        }

        // 5. Raga sang pemain kembali mewujud di tempatnya (HANYA jika tadi sempat memudar)
        if (!dontMovePlayer && playerSpriteRenderer != null)
        {
            yield return StartCoroutine(FadePlayerSprite(0f, 1f));
        }

        // 6. Dunia yang lama ditidurkan
        gameObject.SetActive(false);
    }

    private IEnumerator FadePlayerSprite(float startAlpha, float endAlpha)
    {
        float timer = 0f;
        Color playerColor = playerSpriteRenderer.color;
        
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            playerColor.a = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            playerSpriteRenderer.color = playerColor;
            yield return null;
        }
        
        playerColor.a = endAlpha;
        playerSpriteRenderer.color = playerColor;
    }

    private IEnumerator FadeInNextLevel(GameObject nextLevel)
    {
        SpriteRenderer[] levelSprites = nextLevel.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer sr in levelSprites)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        nextLevel.SetActive(true);

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);

            foreach (SpriteRenderer sr in levelSprites)
            {
                Color c = sr.color;
                c.a = currentAlpha;
                sr.color = c;
            }
            yield return null;
        }

        foreach (SpriteRenderer sr in levelSprites)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D colliderArea = GetComponent<BoxCollider2D>();
        
        if (colliderArea != null)
        {
            Gizmos.color = new Color(0.2f, 0.6f, 0.9f, 0.7f); 
            Bounds bounds = colliderArea.bounds;
            
            Vector3 bottomLeft = new Vector3(bounds.min.x, bounds.min.y, 0f);
            Vector3 bottomRight = new Vector3(bounds.max.x, bounds.min.y, 0f);
            Gizmos.DrawLine(bottomLeft, bottomRight);
            
            Gizmos.DrawSphere(bottomLeft, 0.1f);
            Gizmos.DrawSphere(bottomRight, 0.1f);
        }
    }
}