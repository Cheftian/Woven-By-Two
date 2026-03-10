using UnityEngine;
using System;

[System.Serializable]
public class SoundData
{
    [Tooltip("Nama panggilan untuk melodi ini (misal: 'MainTheme' atau 'WindSFX').")]
    public string soundName;
    
    [Tooltip("File audio yang akan dimainkan.")]
    public AudioClip clip;
    
    [Tooltip("Volume khusus untuk audio ini.")]
    [Range(0f, 1f)] 
    public float volume = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("AudioSource khusus untuk memutar musik latar (BGM).")]
    [SerializeField] private AudioSource bgmSource;
    
    [Tooltip("AudioSource khusus untuk memutar efek suara (SFX).")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Melody Library")]
    [Tooltip("Daftar seluruh musik latar yang ada di dalam dunia ini.")]
    [SerializeField] private SoundData[] bgmLibrary;
    
    [Tooltip("Daftar seluruh efek suara yang ada di dalam dunia ini.")]
    [SerializeField] private SoundData[] sfxLibrary;

    private void Awake()
    {
        // Memastikan hanya ada satu konduktor di seluruh semesta
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            // Menjaga agar alunan musik tidak terputus meski berpindah tempat (scene)
            DontDestroyOnLoad(this.gameObject); 
        }

        // Memastikan BGM terus berulang (loop)
        if (bgmSource != null)
        {
            bgmSource.loop = true;
        }
    }

    // --- SIFAT AJAIB UNTUK MEMANGGIL MUSIK ---

    public void PlayBGM(string name)
    {
        SoundData s = Array.Find(bgmLibrary, sound => sound.soundName == name);
        if (s == null)
        {
            Debug.LogWarning("BGM dengan nama " + name + " tidak ditemukan di perpustakaan nada.");
            return;
        }

        // Jika lagu yang sama sudah berputar, biarkan ia terus mengalun tanpa mengulangnya dari awal
        if (bgmSource.clip == s.clip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = s.clip;
        bgmSource.volume = s.volume;
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
            Debug.LogWarning("SFX dengan nama " + name + " tidak ditemukan di perpustakaan nada.");
            return;
        }

        // PlayOneShot memungkinkan suara dimainkan bertumpuk (seperti suara langkah kaki yang cepat)
        sfxSource.PlayOneShot(s.clip, s.volume);
    }
}