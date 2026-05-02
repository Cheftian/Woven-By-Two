using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FlowerDestroyer : MonoBehaviour
{
    [Header("Machine Settings")]
    [Tooltip("Nama SFX mesin penghancur yang ada di AudioManager (misal: 'Machine_Crunch').")]
    [SerializeField] private string destroySFXName = "Machine_Crunch";

    [Tooltip("Efek partikel (seperti asap atau percikan logam) saat bunga hancur.")]
    [SerializeField] private GameObject destructionParticlePrefab;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Memeriksa apakah yang menyentuh mesin adalah bunga
        if (collision.CompareTag("Flower"))
        {
            DestroyFlower(collision.gameObject);
        }
    }

    private void DestroyFlower(GameObject flower)
    {
        // 1. Munculkan efek mesin (asap/uap) jika ada
        if (destructionParticlePrefab != null)
        {
            Instantiate(destructionParticlePrefab, flower.transform.position, Quaternion.identity);
        }

        // 2. Mainkan suara mesin menghancurkan sesuatu
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(destroySFXName))
        {
            AudioManager.Instance.PlaySFX(destroySFXName);
        }

        // 3. Hapus bunga dari semesta
        Destroy(flower);
        
        Debug.Log("Mesin telah memproses satu bunga.");
    }
}