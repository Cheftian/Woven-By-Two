using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class HorizontalCameraMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How fast the camera glides across the scene.")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Boundary Settings")]
    [Tooltip("The absolute furthest the camera can look to the left.")]
    [SerializeField] private float leftBoundary = -10f;
    
    [Tooltip("The absolute furthest the camera can look to the right.")]
    [SerializeField] private float rightBoundary = 10f;

    [Header("Mouse Edge Scrolling Settings")]
    [Tooltip("How close to the edge of the screen the cursor needs to be to move the camera (in pixels).")]
    [SerializeField] private float edgeScrollThickness = 50f;

    [Header("Cinematic Intro Settings")]
    [Tooltip("Check this if the camera should pan down from the sky when the game first starts.")]
    [SerializeField] private bool playIntroPan = true;

    [Tooltip("The high Y coordinate the camera will start from before gliding down.")]
    [SerializeField] private float introStartY = 20f;

    [Tooltip("How long it takes for the camera to glide down to its initial position.")]
    [SerializeField] private float introDuration = 4f;

    private bool isAutomaticMode = false;
    private bool isTransitioning = false;
    private Transform targetToFollow;

    private void Start()
    {
        // Jika kisah ini dimulai dengan turunnya pandangan dari langit
        if (playIntroPan)
        {
            StartCoroutine(IntroPanRoutine());
        }
    }

    private void Update()
    {
        // Jangan biarkan input mengganggu saat kamera sedang dalam transisi sinematik
        if (isTransitioning) return;

        Vector3 newPosition = transform.position;

        // If magical automatic mode is active, smoothly follow the target
        if (isAutomaticMode && targetToFollow != null)
        {
            newPosition.x = Mathf.Lerp(newPosition.x, targetToFollow.position.x, moveSpeed * Time.deltaTime);
        }
        else
        {
            float horizontalMovement = 0f;

            // 1. Listen to the gentle press of the keyboard
            horizontalMovement = Input.GetAxisRaw("Horizontal");

            // 2. Listen to the magical cursor touching the edges of the screen
            if (horizontalMovement == 0f)
            {
                if (Input.mousePosition.x >= Screen.width - edgeScrollThickness)
                {
                    horizontalMovement = 1f;
                }
                else if (Input.mousePosition.x <= edgeScrollThickness)
                {
                    horizontalMovement = -1f;
                }
            }

            // 3. Move the camera manually
            if (horizontalMovement != 0f)
            {
                newPosition.x += horizontalMovement * moveSpeed * Time.deltaTime;
            }
        }

        // Always ensure the camera never leaves the designated boundaries
        newPosition.x = Mathf.Clamp(newPosition.x, leftBoundary, rightBoundary);

        // Apply the final magical position
        transform.position = newPosition;
    }

    public void StartAutomaticFollow(Transform target)
    {
        targetToFollow = target;
        isAutomaticMode = true;
    }

    public void StopAutomaticFollow()
    {
        isAutomaticMode = false;
        targetToFollow = null;
    }

    // --- NEW MAGIC: The Skyfall Intro ---
    private IEnumerator IntroPanRoutine()
    {
        isTransitioning = true;
        
        Vector3 finalPos = transform.position; // This is the position set in the Unity Editor
        Vector3 startPos = new Vector3(finalPos.x, introStartY, finalPos.z);
        
        // Snap the camera to the high sky immediately
        transform.position = startPos;
        
        float elapsed = 0f;
        while (elapsed < introDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / introDuration;
            
            // SmoothStep makes the glide start and end very softly
            t = t * t * (3f - 2f * t);

            Vector3 currentPos = Vector3.Lerp(startPos, finalPos, t);
            
            // Ensure it respects horizontal boundaries even while falling
            currentPos.x = Mathf.Clamp(currentPos.x, leftBoundary, rightBoundary);
            transform.position = currentPos;

            yield return null;
        }

        // Ensure it rests perfectly at the final intended position
        transform.position = new Vector3(Mathf.Clamp(finalPos.x, leftBoundary, rightBoundary), finalPos.y, finalPos.z);
        isTransitioning = false;
    }

    // --- MAGIC: Vertical Transition for Prologue (Mid-game) ---
    public void TransitionToNewArea(float targetY, float newLeft, float newRight, float duration)
    {
        StartCoroutine(TransitionRoutine(targetY, newLeft, newRight, duration));
    }

    private IEnumerator TransitionRoutine(float targetY, float newLeft, float newRight, float duration)
    {
        isTransitioning = true;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(startPos.x, targetY, startPos.z);

        // Update the boundaries for the new area
        leftBoundary = newLeft;
        rightBoundary = newRight;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            t = t * t * (3f - 2f * t);

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            currentPos.x = Mathf.Clamp(currentPos.x, leftBoundary, rightBoundary);
            transform.position = currentPos;

            yield return null;
        }

        transform.position = new Vector3(Mathf.Clamp(targetPos.x, leftBoundary, rightBoundary), targetY, startPos.z);
        isTransitioning = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 0.9f, 0.7f); 
        
        Vector3 topPoint = new Vector3(0, 1000f, 0);
        Vector3 bottomPoint = new Vector3(0, -1000f, 0);
        
        Vector3 leftPos = new Vector3(leftBoundary, 0, 0);
        Gizmos.DrawLine(leftPos + topPoint, leftPos + bottomPoint);
        
        Vector3 rightPos = new Vector3(rightBoundary, 0, 0);
        Gizmos.DrawLine(rightPos + topPoint, rightPos + bottomPoint);
    }
}