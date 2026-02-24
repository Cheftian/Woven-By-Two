using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomAudioOnAwake : MonoBehaviour
{
    [Header("Magical Sounds")]
    [Tooltip("Put all the audio clips here. You can change the size to hold as many as you want!")]
    [SerializeField] private AudioClip[] randomSounds;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        PlayRandomSound();
    }

    // --- KEAJAIBAN BARU: Fungsi ini kini terbuka (public) ---
    public void PlayRandomSound()
    {
        if (randomSounds != null && randomSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, randomSounds.Length);
            audioSource.clip = randomSounds[randomIndex];
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("There are no magical sounds in the list! Please add some in the Inspector.");
        }
    }
}