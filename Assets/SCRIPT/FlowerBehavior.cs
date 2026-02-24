using UnityEngine;

// This ensures the flower has a collider to detect the magical cursor
[RequireComponent(typeof(Collider2D))]
public class FlowerBehavior : MonoBehaviour
{
    [Header("Idle Sway Settings")]
    [Tooltip("How fast the flower sways back and forth.")]
    [SerializeField] private float swaySpeed = 2f;
    
    [Tooltip("The maximum angle the flower reaches during its idle sway.")]
    [SerializeField] private float swayAngle = 15f;

    [Header("Hover Reaction Settings")]
    [Tooltip("How fast the flower reacts and rotates when hovered.")]
    [SerializeField] private float reactionSpeed = 15f;
    
    [Tooltip("How fast the flower returns to its idle sway when untouched.")]
    [SerializeField] private float returnSpeed = 5f;

    [Tooltip("The angle the flower bends to when startled by the mouse.")]
    [SerializeField] private float reactionAngle = 45f;

    private Collider2D flowerCollider;
    private Camera mainCamera;

    private float currentZRotation = 0f;
    private float targetZRotation = 0f;
    
    // A custom timer so we can pause the rhythm when hovered
    private float swayTimer = 0f; 

    private void Start()
    {
        flowerCollider = GetComponent<Collider2D>();
        mainCamera = Camera.main;
        
        // Give each flower a unique starting rhythm
        swayTimer = Random.Range(0f, 100f);
    }

    private void Update()
    {
        // Find where the cursor is in the game world
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        
        bool isBeingHovered = false;

        // 1. Check if the cursor is touching this flower
        if (flowerCollider.OverlapPoint(mouseWorldPosition))
        {
            isBeingHovered = true;

            // 2. MAGIC PRIORITY: Look at all objects under the cursor
            Collider2D[] overlappingObjects = Physics2D.OverlapPointAll(mouseWorldPosition);
            foreach (Collider2D obj in overlappingObjects)
            {
                // If we detect a bubble in the same spot, the flower politely yields focus
                if (obj.GetComponent<BubbleBehavior>() != null)
                {
                    isBeingHovered = false;
                    break;
                }
            }
        }

        if (isBeingHovered)
        {
            // Hovered: Determine direction and freeze at the reaction angle
            if (mouseWorldPosition.x < transform.position.x)
            {
                targetZRotation = -reactionAngle;
            }
            else
            {
                targetZRotation = reactionAngle;
            }
            
            // Quickly move to the reaction angle and hold it there
            currentZRotation = Mathf.Lerp(currentZRotation, targetZRotation, Time.deltaTime * reactionSpeed);
        }
        else
        {
            // Not Hovered: Resume the rhythm exactly where it left off
            swayTimer += Time.deltaTime;
            
            // Calculate the gentle sway
            targetZRotation = Mathf.Sin(swayTimer * swaySpeed) * swayAngle;
            
            // Smoothly return from the reaction angle back into the gentle sway
            currentZRotation = Mathf.Lerp(currentZRotation, targetZRotation, Time.deltaTime * returnSpeed);
        }

        // Apply the final magical rotation
        transform.rotation = Quaternion.Euler(0f, 0f, currentZRotation);
    }
}