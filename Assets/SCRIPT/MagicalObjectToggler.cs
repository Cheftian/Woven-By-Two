using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class MagicalObjectToggler : MonoBehaviour
{
    [Header("The Illusions")]
    [Tooltip("The first object (Object A - Door Closed).")]
    [SerializeField] private GameObject objectA;
    
    [Tooltip("The second object (Object B - Door Opened).")]
    [SerializeField] private GameObject objectB;

    [SerializeField] private GameObject optionalObjectToEnable;

    [Header("Ending Fade Objects")]
    [Tooltip("Objects that will smoothly appear when the door is closed at the end.")]
    [SerializeField] private GameObject[] objectsToFadeIn;
    
    [Tooltip("Objects that will smoothly disappear when the door is closed at the end.")]
    [SerializeField] private GameObject[] objectsToFadeOut;
    
    [Tooltip("How long the fading transition lasts.")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Flower Destruction Settings")]
    [Tooltip("The exact tag used on all the flower GameObjects.")]
    [SerializeField] private string flowerTag = "Flower";
    
    [Tooltip("How many flowers will wither and fall at the exact same time.")]
    [SerializeField] private int flowersPerBatch = 10;

    [Tooltip("Delay before the next batch of flowers starts falling (1 second).")]
    [SerializeField] private float delayBetweenBatches = 1f;
    
    [Tooltip("How fast each flower shrinks and falls (in seconds).")]
    [SerializeField] private float flowerWitherDuration = 0.5f;

    [Header("Ending Character Movement")]
    [Tooltip("Character A to flip back and walk.")]
    [SerializeField] private Transform characterA;
    
    [Tooltip("Character B to flip back and walk.")]
    [SerializeField] private Transform characterB;
    
    [Tooltip("Final X target for Character A.")]
    [SerializeField] private float finalTargetXA = 40f;
    
    [Tooltip("Final X target for Character B.")]
    [SerializeField] private float finalTargetXB = 42f;
    
    [Tooltip("How fast they walk away.")]
    [SerializeField] private float walkSpeed = 2f;

    [Header("Ending Camera Boundary Expansion")]
    [Tooltip("Reference to the camera script to expand boundaries and follow character A.")]
    [SerializeField] private HorizontalCameraMovement cameraMovementScript;

    [Tooltip("The new left boundary for the final walk.")]
    [SerializeField] private float newLeftBoundary = -50f;

    [Tooltip("The new right boundary for the final walk.")]
    [SerializeField] private float newRightBoundary = 50f;

    [Tooltip("How long the camera takes to gently expand its boundaries (optional smoothness).")]
    [SerializeField] private float boundaryTransitionDuration = 2f;

    [Header("Ending Narration")]
    [Tooltip("Narration ID to play when the door is finally closed.")]
    [SerializeField] private string finalNarrationId;

    [Header("Optional Sound")]
    [Tooltip("Sound when opening.")]
    [SerializeField] private AudioClip openSound;
    
    [Tooltip("Sound when closing finally.")]
    [SerializeField] private AudioClip closeSound;
    
    [SerializeField] private AudioSource clickAudioSource;

    private bool hasBeenOpened = false;
    private bool isEndingReady = false;
    private bool hasBeenClosedFinal = false;

    private void Start()
    {
        // Pastikan pintu tertutup di awal
        if (objectA != null && objectB != null)
        {
            objectA.SetActive(true);
            objectB.SetActive(false);
        }
        
        // Sembunyikan objek-objek yang baru akan muncul di akhir cerita
        foreach (GameObject obj in objectsToFadeIn)
        {
            if (obj != null) 
            {
                SetAlpha(obj, 0f);
                obj.SetActive(false);
            }
        }
    }

    private void OnMouseDown()
    {
        // Jangan mengganggu jika semesta sedang mendengarkan narasi
        if (NarrationManager.Instance != null && NarrationManager.Instance.IsNarrationPlaying) return;

        // Buka pintu untuk pertama dan terakhir kalinya
        if (!hasBeenOpened)
        {
            OpenDoor();
        }
        // Tutup pintu secara permanen HANYA jika adegan taman telah selesai
        else if (isEndingReady && !hasBeenClosedFinal)
        {
            CloseDoorFinal();
        }
    }

    private void OpenDoor()
    {
        if (optionalObjectToEnable != null)
        {
            optionalObjectToEnable.SetActive(true);
        }
        hasBeenOpened = true;
        
        if (objectA != null) objectA.SetActive(false);
        if (objectB != null) objectB.SetActive(true);

        if (clickAudioSource != null && openSound != null)
        {
            clickAudioSource.PlayOneShot(openSound);
        }
    }

    // Dipanggil oleh taman bunga saat dua jiwa itu telah tiba di tujuannya
    public void AllowDoorToClose()
    {
        isEndingReady = true;
        Debug.Log("Pintu kini dapat ditutup kembali untuk mengakhiri segalanya.");
    }

    private void CloseDoorFinal()
    {
        hasBeenClosedFinal = true;

        if (objectA != null) objectA.SetActive(true);
        if (objectB != null) objectB.SetActive(false);

        if (clickAudioSource != null && closeSound != null)
        {
            clickAudioSource.PlayOneShot(closeSound);
        }

        StartCoroutine(FinalEndingSequenceRoutine());
    }

    private IEnumerator FinalEndingSequenceRoutine()
    {
        // 1. Mulai memudarkan dunia lama dan memunculkan dunia baru
        StartCoroutine(FadeObjectsRoutine());

        // 2. Bisikkan narasi penutup
        if (NarrationManager.Instance != null && !string.IsNullOrEmpty(finalNarrationId))
        {
            NarrationManager.Instance.PlayNarration(finalNarrationId);
        }

        // 3. Hancurkan seluruh taman sebelum mereka beranjak
        yield return StartCoroutine(DestroyAllFlowersRoutine());

        // 4. KEAJAIBAN BARU: Flip kembali dengan sempurna
        if (characterA != null)
        {
            Vector3 scale = characterA.localScale;
            scale.x = -scale.x; // Berbalik dari arah terakhirnya
            characterA.localScale = scale;
        }
        if (characterB != null)
        {
            Vector3 scale = characterB.localScale;
            scale.x = -scale.x; // Berbalik dari arah terakhirnya
            characterB.localScale = scale;
        }

        // 5. Lebarkan batas kamera, lalu ikuti langkah Karakter A
        if (cameraMovementScript != null)
        {
            if (Camera.main != null)
            {
                cameraMovementScript.TransitionToNewArea(Camera.main.transform.position.y, newLeftBoundary, newRightBoundary, boundaryTransitionDuration);
            }
            
            if (characterA != null)
            {
                cameraMovementScript.StartAutomaticFollow(characterA);
            }
        }

        // 6. Berjalan perlahan menuju keabadian
        while (true)
        {
            bool aIsMoving = false;
            bool bIsMoving = false;

            if (characterA != null && Mathf.Abs(characterA.position.x - finalTargetXA) > 0.05f)
            {
                Vector3 pos = characterA.position;
                pos.x = Mathf.MoveTowards(pos.x, finalTargetXA, walkSpeed * Time.deltaTime);
                characterA.position = pos;
                aIsMoving = true;
            }

            if (characterB != null && Mathf.Abs(characterB.position.x - finalTargetXB) > 0.05f)
            {
                Vector3 pos = characterB.position;
                pos.x = Mathf.MoveTowards(pos.x, finalTargetXB, walkSpeed * Time.deltaTime);
                characterB.position = pos;
                bIsMoving = true;
            }

            if (!aIsMoving && !bIsMoving) break;

            yield return null;
        }

        Debug.Log("Langkah mereka perlahan menghilang di kejauhan.");
    }

    private IEnumerator DestroyAllFlowersRoutine()
    {
        Debug.Log("Taman mulai layu sebagai tanda perpisahan.");
        
        while (true)
        {
            GameObject[] flowers = GameObject.FindGameObjectsWithTag(flowerTag);

            if (flowers.Length == 0)
            {
                break; 
            }

            int processedInBatch = 0;

            foreach (GameObject flower in flowers)
            {
                if (flower != null && flower.CompareTag(flowerTag))
                {
                    flower.tag = "Untagged"; 
                    StartCoroutine(WitherAndFallRoutine(flower));
                    
                    processedInBatch++;
                    if (processedInBatch >= flowersPerBatch)
                    {
                        break; 
                    }
                }
            }

            yield return new WaitForSeconds(delayBetweenBatches);
        }

        if (flowerWitherDuration > delayBetweenBatches)
        {
            yield return new WaitForSeconds(flowerWitherDuration - delayBetweenBatches);
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

            flower.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            flower.transform.position = startPos - new Vector3(0f, t * 2f, 0f);

            yield return null;
        }

        Destroy(flower);
    }

    private IEnumerator FadeObjectsRoutine()
    {
        foreach (GameObject obj in objectsToFadeIn)
        {
            if (obj != null) obj.SetActive(true);
        }

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            foreach (GameObject obj in objectsToFadeIn)
            {
                if (obj != null) SetAlpha(obj, progress);
            }

            foreach (GameObject obj in objectsToFadeOut)
            {
                if (obj != null) SetAlpha(obj, 1f - progress);
            }

            yield return null;
        }

        foreach (GameObject obj in objectsToFadeOut)
        {
            if (obj != null) obj.SetActive(false);
        }
    }

    private void SetAlpha(GameObject obj, float alpha)
    {
        SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sr in renderers)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}