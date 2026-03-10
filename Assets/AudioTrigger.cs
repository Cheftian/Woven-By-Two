using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    [Header("BGM Settings")]
    [Tooltip("Ketikkan nama BGM persis seperti yang tertulis di AudioManager.")]
    [SerializeField] private string bgmName;

    [Header("Trigger Conditions")]
    [Tooltip("Jika dicentang, BGM akan langsung berganti saat semesta (scene) baru saja dimulai.")]
    [SerializeField] private bool triggerOnStart = false;

    [Tooltip("Jika dicentang, BGM akan berganti setiap kali objek ini diaktifkan/muncul (Enable).")]
    [SerializeField] private bool triggerOnEnable = true;

    private void Start()
    {
        if (triggerOnStart)
        {
            ChangeTheMelody();
        }
    }

    private void OnEnable()
    {
        if (triggerOnEnable)
        {
            ChangeTheMelody();
        }
    }

    private void ChangeTheMelody()
    {
        // Memastikan sang konduktor ada sebelum meminta lagu
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(bgmName))
        {
            AudioManager.Instance.PlayBGM(bgmName);
            Debug.Log("Suasana berubah. Mengalunkan melodi: " + bgmName);
        }
    }
}