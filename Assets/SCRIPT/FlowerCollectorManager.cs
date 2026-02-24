using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class FlowerCollectorManager : MonoBehaviour
{
    [Header("Collection Settings")]
    [Tooltip("Target number of flowers needed to trigger the magical event.")]
    [SerializeField] private int targetFlowerCount = 10;

    [Header("Narration Settings")]
    [Tooltip("The ID of the narration to play when the player steps into this area.")]
    [SerializeField] private string narrationIdToPlay;


    [Header("Progressive Sounds")]
    [Tooltip("Sounds that play in order as flowers are collected. Add exactly as many as the target count!")]
    [SerializeField] private AudioClip[] collectionProgressSounds;

    [Header("Cinematic Event Settings")]
    [Tooltip("The character that will move automatically.")]
    [SerializeField] private Transform playerCharacter;
    
    [Tooltip("The exact X coordinate the character should walk to.")]
    [SerializeField] private float targetXPosition;
    
    [Tooltip("How fast the character walks during the cinematic.")]
    [SerializeField] private float characterAutoMoveSpeed = 3f;
    
    [Tooltip("Reference to the camera script to trigger automatic follow.")]
    [SerializeField] private HorizontalCameraMovement cameraMovementScript;

    [Header("Clock Appearance Settings")]
    [Tooltip("The magical clock object to appear.")]
    [SerializeField] private GameObject clockObject;

    [Tooltip("The starting Y position from where the clock will rise or fall.")]
    [SerializeField] private float clockStartY = -10f;

    [Tooltip("The final Y position where the clock will rest.")]
    [SerializeField] private float clockTargetY = 5f;

    [Tooltip("How fast the clock slides into view.")]
    [SerializeField] private float clockRevealSpeed = 3f;

    private int currentFlowerCount = 0;
    private AudioSource audioSource;
    private bool isCinematicPlaying = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Ensure the clock is hidden when the game starts
        if (clockObject != null)
        {
            clockObject.SetActive(false);
        }
    }

    public void CollectFlower()
    {
        // Stop accepting flowers if the magic is already happening
        if (isCinematicPlaying || currentFlowerCount >= targetFlowerCount) return;

        // Sing the specific note for this exact progress step
        if (collectionProgressSounds != null && collectionProgressSounds.Length > currentFlowerCount)
        {
            AudioClip soundToPlay = collectionProgressSounds[currentFlowerCount];
            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }

        currentFlowerCount++;
        Debug.Log("Sebuah bunga telah diterima. Total saat ini: " + currentFlowerCount);

        // Check if the grand melody is complete
        if (currentFlowerCount >= targetFlowerCount)
        {
            TriggerCompletionEvent();
        }
    }

    private void TriggerCompletionEvent()
    {
        Debug.Log("Keajaiban terpenuhi! 10 Bunga telah dikumpulkan. Memulai perjalanan...");
        isCinematicPlaying = true;
        
        // Tell the camera to start looking at the character automatically
        if (cameraMovementScript != null && playerCharacter != null)
        {
            cameraMovementScript.StartAutomaticFollow(playerCharacter);
        }

        // Start moving the character smoothly
        if (playerCharacter != null)
        {
            StartCoroutine(AutoMoveCharacterRoutine());
        }
    }

    private IEnumerator AutoMoveCharacterRoutine()
    {
        if (NarrationManager.Instance != null && !string.IsNullOrEmpty(narrationIdToPlay))
            {
                NarrationManager.Instance.PlayNarration(narrationIdToPlay);
            }
        // Loop until the character is extremely close to the target X position
        while (Mathf.Abs(playerCharacter.position.x - targetXPosition) > 0.05f)
        {
            Vector3 newPos = playerCharacter.position;
            
            // Move gently towards the target X, keeping Y and Z exactly the same
            newPos.x = Mathf.MoveTowards(newPos.x, targetXPosition, characterAutoMoveSpeed * Time.deltaTime);
            playerCharacter.position = newPos;
            
            // Wait for the next frame before continuing the magical journey
            yield return null;
        }

        // Snap perfectly to the target X just to be precise at the end
        Vector3 finalPos = playerCharacter.position;
        finalPos.x = targetXPosition;
        playerCharacter.position = finalPos;

        Debug.Log("Karakter telah sampai di titik takdirnya.");

        // Break the enchantment, let the camera be free again
        if (cameraMovementScript != null)
        {
            cameraMovementScript.StopAutomaticFollow();
        }

        // Begin the new magic: Reveal the clock!
        if (clockObject != null)
        {
            StartCoroutine(RevealClockRoutine());
        }
    }

    private IEnumerator RevealClockRoutine()
    {
        // Prepare the clock at its starting position before waking it up
        Vector3 clockPos = clockObject.transform.position;
        clockPos.y = clockStartY;
        clockObject.transform.position = clockPos;
        
        // Wake the clock up
        clockObject.SetActive(true);
        
        // Slide the clock beautifully to its target Y position
        while (Mathf.Abs(clockObject.transform.position.y - clockTargetY) > 0.01f)
        {
            clockPos = clockObject.transform.position;
            clockPos.y = Mathf.MoveTowards(clockPos.y, clockTargetY, clockRevealSpeed * Time.deltaTime);
            clockObject.transform.position = clockPos;
            
            yield return null;
        }
        
        // Snap exactly to target Y just to be perfectly aligned
        clockPos = clockObject.transform.position;
        clockPos.y = clockTargetY;
        clockObject.transform.position = clockPos;

        Debug.Log("Jam ajaib telah muncul dan waktu mulai berjalan.");
    }
}