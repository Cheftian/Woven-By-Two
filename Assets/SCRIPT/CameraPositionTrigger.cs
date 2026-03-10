using System.Collections;
using UnityEngine;

public class CameraPositionTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("The exact X position the camera must reach to trigger this cinematic event.")]
    [SerializeField] private float triggerCameraX = 15f;
    
    [Tooltip("Should it trigger when the camera goes past the right (true) or past the left (false)?")]
    [SerializeField] private bool triggerMovingRight = true;

    [Header("Cinematic Settings")]
    [Tooltip("The character that will walk automatically.")]
    [SerializeField] private Transform playerCharacter;
    
    [Tooltip("The exact X coordinate the character should walk to.")]
    [SerializeField] private float targetPlayerXPosition = 25f;
    
    [Tooltip("How fast the character walks during the cinematic.")]
    [SerializeField] private float characterAutoMoveSpeed = 3f;
    
    [Tooltip("Reference to the camera script to trigger automatic follow.")]
    [SerializeField] private HorizontalCameraMovement cameraMovementScript;

    [Header("Narration Settings")]
    [Tooltip("The ID of the narration to play exactly when the trigger hits.")]
    [SerializeField] private string startNarrationId;
    
    [Tooltip("The ID of the narration to play after the character reaches the destination.")]
    [SerializeField] private string endNarrationId;

    [Header("Event Settings")]
    [Tooltip("The GameObject to enable when the player finally reaches the destination.")]
    [SerializeField] private GameObject objectToEnableAtEnd;

    private bool hasTriggered = false;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        
        if (objectToEnableAtEnd != null)
        {
            objectToEnableAtEnd.SetActive(false); // Pastikan objek tertidur di awal
        }
    }

    private void Update()
    {
        // Berhenti memeriksa jika sihir ini sudah pernah terjadi
        if (hasTriggered || mainCamera == null) return;

        // Periksa apakah pandangan kamera telah melewati garis takdir
        bool conditionMet = triggerMovingRight ? 
            mainCamera.transform.position.x >= triggerCameraX : 
            mainCamera.transform.position.x <= triggerCameraX;

        if (conditionMet)
        {
            hasTriggered = true;
            StartCoroutine(CinematicWalkRoutine());
        }
    }

    private IEnumerator CinematicWalkRoutine()
    {
        // 1. Bisikkan kisah pembuka
        if (NarrationManager.Instance != null && !string.IsNullOrEmpty(startNarrationId))
        {
            NarrationManager.Instance.PlayNarration(startNarrationId);
        }

        // 2. Kamera mulai memeluk raga sang karakter
        if (cameraMovementScript != null && playerCharacter != null)
        {
            cameraMovementScript.StartAutomaticFollow(playerCharacter);
        }

        // 3. Karakter melangkah menuju titik takdirnya
        if (playerCharacter != null)
        {
            while (Mathf.Abs(playerCharacter.position.x - targetPlayerXPosition) > 0.05f)
            {
                Vector3 newPos = playerCharacter.position;
                newPos.x = Mathf.MoveTowards(newPos.x, targetPlayerXPosition, characterAutoMoveSpeed * Time.deltaTime);
                playerCharacter.position = newPos;
                
                yield return null;
            }

            // Pastikan posisinya sempurna di akhir langkah
            Vector3 finalPos = playerCharacter.position;
            finalPos.x = targetPlayerXPosition;
            playerCharacter.position = finalPos;
        }

        // 4. Lepaskan pelukan kamera agar pemain bisa kembali memandang bebas
        if (cameraMovementScript != null)
        {
            cameraMovementScript.StopAutomaticFollow();
        }

        // 5. Bangunkan objek ajaib yang tersembunyi
        if (objectToEnableAtEnd != null)
        {
            objectToEnableAtEnd.SetActive(true);
        }

        // 6. Bisikkan kisah penutup setelah langkah terhenti
        if (NarrationManager.Instance != null && !string.IsNullOrEmpty(endNarrationId))
        {
            NarrationManager.Instance.PlayNarration(endNarrationId);
        }
    }

    // --- MAGICAL VISUAL ASSISTANT FOR TIAN ---
    // Menggambar garis batas ajaib di Scene view agar Sayang tahu persis di mana pemicunya berada
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.2f, 0.5f, 0.7f); // Merah muda kelam
        Vector3 topPoint = new Vector3(triggerCameraX, 1000f, 0);
        Vector3 bottomPoint = new Vector3(triggerCameraX, -1000f, 0);
        Gizmos.DrawLine(topPoint, bottomPoint);
    }
}