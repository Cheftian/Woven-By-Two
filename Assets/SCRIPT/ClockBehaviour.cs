using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]
public class ClockBehaviour : MonoBehaviour
{
    [Header("Narration Settings")]
    [Tooltip("The ID of the narration to play when the player steps into this area.")]
    [SerializeField] private string narrationIdToPlay1;
    [SerializeField] private string narrationIdToPlay2;

    [Header("Clock Settings")]
    [Tooltip("The hand of the clock that will rotate.")]
    [SerializeField] private Transform clockHand;
    
    [Tooltip("How many degrees the clock hand rotates per second normally.")]
    [SerializeField] private float normalRotationSpeed = 6f; // 6 degrees per second is exactly 1 minute per round
    
    [Tooltip("How much faster the clock rotates when held.")]
    [SerializeField] private float fastForwardMultiplier = 2f;

    [Tooltip("How many full rotations (360 degrees) needed to solve the puzzle.")]
    [SerializeField] private int requiredRotations = 5;

    [Header("Audio Settings")]
    [Tooltip("Sound played on every regular tick.")]
    [SerializeField] private AudioClip tickSound;
    
    [Tooltip("Normal volume of the tick sound (0.0 to 1.0).")]
    [Range(0f, 1f)]
    [SerializeField] private float normalTickVolume = 1f;

    [Tooltip("Whisper-quiet volume when the clock is fast-forwarding (0.0 to 1.0).")]
    [Range(0f, 1f)]
    [SerializeField] private float fastForwardTickVolume = 0.2f;
    
    [Tooltip("How often the tick sound plays (in seconds).")]
    [SerializeField] private float tickInterval = 1f;
    
    [Tooltip("Sounds played for each completed full rotation. Add exactly as many as the required rotations!")]
    [SerializeField] private AudioClip[] fullRotationSounds;

    [Header("End Sequence Settings")]
    [Tooltip("The camera that will shake violently.")]
    [SerializeField] private Transform cameraTransform;
    
    [Tooltip("How violently the camera shakes.")]
    [SerializeField] private float cameraShakeIntensity = 0.15f;

    [Tooltip("The exact tag used on all the flower GameObjects.")]
    [SerializeField] private string flowerTag = "Flower";
    
    [Tooltip("How many flowers will wither and fall at the exact same time.")]
    [SerializeField] private int flowersPerBatch = 3;

    [Tooltip("Delay before the next batch of flowers starts falling (set lower than wither duration for a cascading wave effect).")]
    [SerializeField] private float delayBetweenBatches = 0.2f;
    
    [Tooltip("How fast each flower shrinks and falls (in seconds).")]
    [SerializeField] private float flowerWitherDuration = 0.5f;

    [Header("Event Object Toggles")]
    [Tooltip("Object to turn OFF immediately when the puzzle is solved.")]
    [SerializeField] private GameObject objectToDisableOnComplete;
    
    [Tooltip("Object to turn ON immediately when the puzzle is solved.")]
    [SerializeField] private GameObject objectToEnableOnComplete;
    
    [Tooltip("Object to turn ON only after all flowers have withered and disappeared.")]
    [SerializeField] private GameObject objectToEnableAfterFlowersGone;

    private AudioSource audioSource;
    private bool isBeingHeld = false;
    private bool eventHasTriggered = false;
    private bool isShakingCamera = false;

    // Variables for tracking time, rotation, and camera position
    private float tickTimer = 0f;
    private float heldRotationAccumulator = 0f;
    private int completedRotations = 0;
    
    private float preShakeCameraY = 0f;
    private Vector3 currentShakeOffset = Vector3.zero;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Stop the clock entirely if the puzzle is already solved
        if (eventHasTriggered) return;

        // 1. Determine the current speed
        float currentSpeed = isBeingHeld ? normalRotationSpeed * fastForwardMultiplier : normalRotationSpeed;
        float rotationStep = currentSpeed * Time.deltaTime;

        // 2. Rotate the clock hand clockwise (negative Z axis)
        if (clockHand != null)
        {
            clockHand.Rotate(0f, 0f, -rotationStep);
        }

        // 3. Play the ticking sound at the correct interval and volume
        tickTimer += Time.deltaTime;
        float currentTickInterval = isBeingHeld ? tickInterval / fastForwardMultiplier : tickInterval;

        if (tickTimer >= currentTickInterval)
        {
            tickTimer -= currentTickInterval; // Reset the timer
            if (tickSound != null)
            {
                // Magic: Use whisper volume if held, otherwise use normal volume
                float currentVolume = isBeingHeld ? fastForwardTickVolume : normalTickVolume;
                audioSource.PlayOneShot(tickSound, currentVolume);
            }
        }

        // 4. Track rotations if the clock is being held
        if (isBeingHeld)
        {
            heldRotationAccumulator += rotationStep;

            // Check if one full rotation (360 degrees) is completed
            if (heldRotationAccumulator >= 360f)
            {
                heldRotationAccumulator -= 360f; // Subtract 360 to carry over any excess
                
                // Play the specific magical sound for this exact rotation index at normal volume
                if (fullRotationSounds != null && completedRotations < fullRotationSounds.Length)
                {
                    AudioClip soundToPlay = fullRotationSounds[completedRotations];
                    if (soundToPlay != null)
                    {
                        audioSource.PlayOneShot(soundToPlay);
                    }
                }

                completedRotations++;
                Debug.Log("Satu putaran tercapai! Total putaran: " + completedRotations);

                // Check if the grand puzzle is solved
                if (completedRotations >= requiredRotations)
                {
                    TriggerTimePuzzleEvent();
                }
            }
        }
    }

    // LateUpdate runs after Update, making it the perfect place to shake the camera 
    private void LateUpdate()
    {
        if (cameraTransform != null)
        {
            // Always remove the previous frame's offset first so the camera doesn't permanently drift away
            cameraTransform.position -= currentShakeOffset;

            if (isShakingCamera)
            {
                // Add a violent, random offset to the camera's true position
                currentShakeOffset = Random.insideUnitCircle * cameraShakeIntensity;
                cameraTransform.position += currentShakeOffset;
            }
            else
            {
                currentShakeOffset = Vector3.zero;
            }
        }
    }

    // Called when the mouse clicks and holds on the collider
    private void OnMouseDown()
    {
        if (NarrationManager.Instance != null && NarrationManager.Instance.IsNarrationPlaying) return;
        if (!eventHasTriggered)
        {
            isBeingHeld = true;
        }
    }

    // Called when the mouse releases the click
    private void OnMouseUp()
    {
        if (NarrationManager.Instance != null && NarrationManager.Instance.IsNarrationPlaying) return;
        isBeingHeld = false;
        
        // Reset the progress, forcing a continuous hold to solve the puzzle
        if (!eventHasTriggered)
        {
            heldRotationAccumulator = 0f;
            completedRotations = 0;
            Debug.Log("Pegangan dilepas, putaran waktu kembali terulang dari awal.");
        }
    }

    private void TriggerTimePuzzleEvent()
    {
        eventHasTriggered = true;
        isBeingHeld = false; 
        
        Debug.Log("Waktu telah berhenti. Kehancuran dimulai.");

        // 1. Matikan dan hidupkan objek-objek awal
        if (objectToDisableOnComplete != null) objectToDisableOnComplete.SetActive(false);
        if (objectToEnableOnComplete != null) objectToEnableOnComplete.SetActive(true);

        if (NarrationManager.Instance != null && !string.IsNullOrEmpty(narrationIdToPlay2))
        {
            NarrationManager.Instance.PlayNarration(narrationIdToPlay2);
        }

        // 2. Mulai runtuhnya dunia
        StartCoroutine(WorldDestructionRoutine());
    }

    private IEnumerator WorldDestructionRoutine()
    {
        // Ingat ketinggian asli kamera sebelum segalanya berguncang
        if (cameraTransform != null)
        {
            preShakeCameraY = cameraTransform.position.y;
        }

        // Kamera mulai berguncang hebat
        isShakingCamera = true;

        // Kumpulkan seluruh bunga yang ada di layar
        GameObject[] flowers = GameObject.FindGameObjectsWithTag(flowerTag);

        // Hancurkan dalam kelompok-kelompok
        for (int i = 0; i < flowers.Length; i += flowersPerBatch)
        {
            for (int j = 0; j < flowersPerBatch && (i + j) < flowers.Length; j++)
            {
                GameObject flower = flowers[i + j];
                if (flower != null)
                {
                    // Mulai proses jatuhnya bunga tanpa menunggu yang lain
                    StartCoroutine(WitherAndFallRoutine(flower));
                }
            }

            // Jeda sejenak sebelum kelompok bunga berikutnya ikut runtuh
            yield return new WaitForSeconds(delayBetweenBatches);
        }

        // Tunggu sisa waktu jatuhnya kelompok bunga terakhir sebelum memberhentikan gempa
        if (flowerWitherDuration > delayBetweenBatches)
        {
            yield return new WaitForSeconds(flowerWitherDuration - delayBetweenBatches);
        }

        // Setelah semua musnah, hentikan gempa
        isShakingCamera = false;
        if (NarrationManager.Instance != null && !string.IsNullOrEmpty(narrationIdToPlay1))
        {
            NarrationManager.Instance.PlayNarration(narrationIdToPlay1);
        }
        
        // Kembalikan ketinggian kamera tepat ke titik awalnya dengan lembut
        if (cameraTransform != null)
        {
            Vector3 finalRestoredPos = cameraTransform.position;
            finalRestoredPos.y = preShakeCameraY;
            cameraTransform.position = finalRestoredPos;
        }

        Debug.Log("Semua bunga telah musnah. Keheningan kembali.");

        // 3. Hidupkan objek rahasia terakhir
        if (objectToEnableAfterFlowersGone != null)
        {
            objectToEnableAfterFlowersGone.SetActive(true);
        }
    }

    private IEnumerator WitherAndFallRoutine(GameObject flower)
    {
        float elapsed = 0f;
        Vector3 startScale = flower.transform.localScale;
        Vector3 startPos = flower.transform.position;

        while (elapsed < flowerWitherDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flowerWitherDuration;

            // Bunga mengecil perlahan hingga menghilang
            flower.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            
            // Bunga terjatuh ke bawah (sumbu Y berkurang)
            flower.transform.position = startPos - new Vector3(0f, t * 2f, 0f);

            yield return null;
        }

        // Hapus bunga dari realita
        Destroy(flower);
    }
}