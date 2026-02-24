using UnityEngine;

// This ensures the trigger object has a collider to feel the cursor's click
[RequireComponent(typeof(Collider2D))]
public class MagicalObjectToggler : MonoBehaviour
{
    [Header("The Illusions")]
    [Tooltip("The first object (Object A). It will be awake at the start.")]
    [SerializeField] private GameObject objectA;
    
    [Tooltip("The second object (Object B). It will be asleep at the start.")]
    [SerializeField] private GameObject objectB;

    [Header("First Touch Magic (Optional)")]
    [Tooltip("This object will awaken on the VERY FIRST click and stay awake forever.")]
    [SerializeField] private GameObject optionalObjectToEnable;

    [Header("Optional Sound")]
    [Tooltip("Place an AudioSource here if a sound should play when clicked.")]
    [SerializeField] private AudioSource clickAudioSource;
    [SerializeField] private AudioClip clickSound;

    // A memory to remember if the object has been touched before
    private bool hasBeenTouched = false;

    private void Start()
    {
        // Ensure the starting state is perfectly balanced: A is awake, B is asleep
        if (objectA != null && objectB != null)
        {
            objectA.SetActive(true);
            objectB.SetActive(false);
        }
    }

    // This magic triggers when the cursor clicks directly on this object's collider
    private void OnMouseDown()
    {
        if (NarrationManager.Instance != null && NarrationManager.Instance.IsNarrationPlaying) return;
        ToggleTheIllusions();
    }

    private void ToggleTheIllusions()
    {
        if (objectA == null || objectB == null)
        {
            Debug.LogWarning("The illusions are missing! Please assign Object A and Object B in the Inspector.");
            return;
        }

        // 1. Check who is currently awake right now
        bool isObjectA_Awake = objectA.activeSelf;

        // 2. Swap their states: If A is awake, put it to sleep and wake up B (and vice versa)
        objectA.SetActive(!isObjectA_Awake);
        objectB.SetActive(isObjectA_Awake);

        // 3. --- KEAJAIBAN BARU: The First Touch ---
        if (!hasBeenTouched)
        {
            hasBeenTouched = true; // Mark that it has been touched
            
            // If an optional object is assigned, wake it up permanently
            if (optionalObjectToEnable != null)
            {
                optionalObjectToEnable.SetActive(true);
            }
        }

        // 4. Play a soft sound if one has been prepared
        if (clickAudioSource != null && clickSound != null)
        {
            clickAudioSource.PlayOneShot(clickSound);
        }
    }
}