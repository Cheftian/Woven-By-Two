using UnityEngine;
using System;

[System.Serializable]
public class SoundData
{
    [Tooltip("The specific name to call this audio (e.g., 'MainTheme' or 'WindSFX').")]
    public string soundName;
    
    [Tooltip("The audio file to be played.")]
    public AudioClip clip;
    
    [Tooltip("The volume level for this specific audio.")]
    [Range(0f, 1f)] 
    public float volume = 1f;

    [Tooltip("Check this if the audio should play continuously (loop). Uncheck to play only once.")]
    public bool loop = true;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("AudioSource used specifically for playing Background Music (BGM).")]
    [SerializeField] private AudioSource bgmSource;
    
    [Tooltip("AudioSource used specifically for playing Sound Effects (SFX).")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Melody Library")]
    [Tooltip("List of all background music available in the game.")]
    [SerializeField] private SoundData[] bgmLibrary;
    
    [Tooltip("List of all sound effects available in the game.")]
    [SerializeField] private SoundData[] sfxLibrary;

    private void Awake()
    {
        // Ensure only one AudioManager exists in the entire game
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            // Prevent the audio from stopping when changing scenes
            DontDestroyOnLoad(this.gameObject); 
        }
    }

    public void PlayBGM(string name)
    {
        SoundData s = Array.Find(bgmLibrary, sound => sound.soundName == name);
        if (s == null)
        {
            Debug.LogWarning("BGM with name " + name + " is not found in the library.");
            return;
        }

        // If the exact same song is already playing, do not restart it
        if (bgmSource.clip == s.clip && bgmSource.isPlaying)
        {
            return;
        }

        // Apply the settings from the SoundData and play
        bgmSource.clip = s.clip;
        bgmSource.volume = s.volume;
        bgmSource.loop = s.loop; 
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    public void PlaySFX(string name)
    {
        SoundData s = Array.Find(sfxLibrary, sound => sound.soundName == name);
        if (s == null)
        {
            Debug.LogWarning("SFX with name " + name + " is not found in the library.");
            return;
        }

        // PlayOneShot allows sounds to overlap (useful for rapid sound effects)
        sfxSource.PlayOneShot(s.clip, s.volume);
    }
}