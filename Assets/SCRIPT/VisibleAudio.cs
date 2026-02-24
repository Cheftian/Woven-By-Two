using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VisibleAudio : MonoBehaviour
{
    private AudioSource _audioSource;
    private Camera _mainCamera;

    [Header("Settings")]
    [Tooltip("Volume maksimal saat objek terlihat (0.0 - 1.0)")]
    [Range(0f, 1f)] public float maxVolume = 1.0f;
    
    [Tooltip("Seberapa cepat suara muncul/hilang (Fade In/Out)")]
    public float fadeSpeed = 2.0f;

    [Tooltip("Buffer Area (Supaya suara mulai terdengar dikit sebelum benar-benar masuk layar)")]
    // 0 = Pas banget di pinggir layar. -0.1 = Mulai bunyi pas masih agak di luar.
    public float buffer = 0.1f; 

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _mainCamera = Camera.main; // Pastikan kamera utama punya tag "MainCamera"

        // Mulai dengan volume 0 (diam)
        _audioSource.volume = 0;
        
        // Pastikan Loop nyala kalau ini suara lingkungan (misal: TV Static)
        // _audioSource.loop = true; 
        // _audioSource.Play();
    }

    void Update()
    {
        if (_mainCamera == null) return;

        // 1. Cek Posisi Objek di Layar Kamera (Viewport)
        // Viewport Point mengubah posisi dunia (x,y,z) menjadi koordinat layar (0 sampai 1)
        Vector3 viewPos = _mainCamera.WorldToViewportPoint(transform.position);

        // 2. Logika: Apakah ada di dalam layar?
        // x antara 0-1, y antara 0-1, dan z > 0 (artinya di depan kamera)
        bool isOnScreen = (viewPos.x >= -buffer && viewPos.x <= 1 + buffer) &&
                          (viewPos.y >= -buffer && viewPos.y <= 1 + buffer) &&
                          (viewPos.z > 0);

        // 3. Atur Target Volume
        float targetVolume = isOnScreen ? maxVolume : 0f;

        // 4. Fade Volume (Biar halus, gak kaget putus-putus)
        _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);
    }
}