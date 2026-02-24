using UnityEngine;

// This ensures the object always has a voice box
[RequireComponent(typeof(AudioSource))]
public class ImmersiveAudio2D : MonoBehaviour
{
   [Header("Global Fade Settings")]
    [Tooltip("Distance where sounds start to fade from the camera center.")]
    [SerializeField] private float minDistance = 5f;
    
    [Tooltip("Distance where sounds completely fade into silence.")]
    [SerializeField] private float maxDistance = 20f;

    [Tooltip("The highest volume any sound can reach.")]
    [SerializeField] private float maximumGlobalVolume = 1f;

    private Transform cameraTransform;
    private AudioSource[] activeVoices;
    
    // Timer to periodically scan for newly spawned sounds (like the magical bubbles)
    private float searchTimer = 0f;
    private float searchInterval = 0.5f; 

    private void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // Listen to all sounds present at the very beginning
        FindAllVoices();
    }

    private void Update()
    {
        if (cameraTransform == null) return;

        // Periodically search for new voices to keep performance incredibly smooth
        searchTimer += Time.deltaTime;
        if (searchTimer >= searchInterval)
        {
            FindAllVoices();
            searchTimer = 0f;
        }

        // Get the camera's exact position on the 2D plane (ignoring Z depth)
        Vector2 cameraPos2D = new Vector2(cameraTransform.position.x, cameraTransform.position.y);

        // Gently adjust the volume for every voice currently singing in the world
        if (activeVoices != null)
        {
            foreach (AudioSource voice in activeVoices)
            {
                // Skip if a bubble just popped and its voice is destroyed
                if (voice == null) continue; 

                Vector2 voicePos2D = new Vector2(voice.transform.position.x, voice.transform.position.y);
                float distance = Vector2.Distance(cameraPos2D, voicePos2D);

                if (distance <= minDistance)
                {
                    voice.volume = maximumGlobalVolume;
                }
                else if (distance >= maxDistance)
                {
                    voice.volume = 0f;
                }
                else
                {
                    // Calculate the smooth fade into the distance
                    float fadeRange = maxDistance - minDistance;
                    float currentFade = distance - minDistance;
                    
                    // Apply the fading illusion
                    voice.volume = maximumGlobalVolume * (1f - (currentFade / fadeRange));
                }
            }
        }
    }

    private void FindAllVoices()
    {
        // Unity 6 magic to gather all active AudioSources in the scene
        activeVoices = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
    }
}