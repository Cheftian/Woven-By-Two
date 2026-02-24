using UnityEngine;
using System.Collections;

// Ensures the leaf has physics and a collider to feel the touch
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class FallingLeafBehavior : MonoBehaviour
{
    [Header("Falling Physics")]
    [Tooltip("How fast the leaf falls downwards.")]
    [SerializeField] private float fallSpeed = 2f;
    
    [Tooltip("How fast the leaf sways left and right.")]
    [SerializeField] private float swaySpeed = 2f;
    
    [Tooltip("How far the leaf sways to the sides.")]
    [SerializeField] private float swayWidth = 1.5f;

    [Header("Natural Rotation Settings")]
    [Tooltip("Minimum rotation speed of the leaf.")]
    [SerializeField] private float minRotationSpeed = 30f;
    
    [Tooltip("Maximum rotation speed of the leaf.")]
    [SerializeField] private float maxRotationSpeed = 90f;

    [Header("Magical Sprites")]
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite touchedSprite;
    [SerializeField] private float transitionSpeed = 3f;

    [Header("Interaction Tags")]
    [Tooltip("Tag of the player that changes the leaf's color.")]
    [SerializeField] private string playerTag = "Player";
    
    [Tooltip("Tag of the ground that makes the leaf disappear.")]
    [SerializeField] private string groundTag = "Ground";

    private SpriteRenderer baseRenderer;
    private SpriteRenderer fadeRenderer;
    private bool hasBeenTouched = false;
    
    // To anchor the sway movement and handle unique rotation
    private float initialXPosition;
    private float windOffset;
    private float currentRotationSpeed;

    private void Start()
    {
        // Ensure Rigidbody is kinematic so it moves smoothly via code
        GetComponent<Rigidbody2D>().isKinematic = true;
        GetComponent<Collider2D>().isTrigger = true;

        baseRenderer = GetComponent<SpriteRenderer>();
        baseRenderer.sprite = originalSprite;

        // Remember where it started falling to sway around this point
        initialXPosition = transform.position.x;
        
        // Give each leaf a different sway rhythm
        windOffset = Random.Range(0f, 100f);

        // --- NEW MAGIC: Decide the unique rotation style for this leaf ---
        currentRotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        
        // 50% chance to spin the other way for a natural chaotic feel
        if (Random.value > 0.5f)
        {
            currentRotationSpeed = -currentRotationSpeed;
        }

        CreateFadeLayer();
    }

    private void CreateFadeLayer()
    {
        GameObject fadeObject = new GameObject("FadeSpriteLayer");
        fadeObject.transform.SetParent(this.transform);
        fadeObject.transform.localPosition = Vector3.zero;
        fadeObject.transform.localRotation = Quaternion.identity;
        fadeObject.transform.localScale = Vector3.one;

        fadeRenderer = fadeObject.AddComponent<SpriteRenderer>();
        fadeRenderer.sprite = touchedSprite;
        fadeRenderer.sortingLayerID = baseRenderer.sortingLayerID;
        fadeRenderer.sortingOrder = baseRenderer.sortingOrder + 1;

        Color startColor = fadeRenderer.color;
        startColor.a = 0f;
        fadeRenderer.color = startColor;
    }

    private void Update()
    {
        // 1. Calculate the elegant swaying motion
        float newXPosition = initialXPosition + Mathf.Sin((Time.time + windOffset) * swaySpeed) * swayWidth;
        
        // 2. Calculate the constant downward motion
        float newYPosition = transform.position.y - (fallSpeed * Time.deltaTime);

        // Apply the falling and swaying movement
        transform.position = new Vector2(newXPosition, newYPosition);

        // 3. --- NEW MAGIC: Apply the continuous natural rotation ---
        transform.Rotate(0f, 0f, currentRotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If it touches the ground, vanish into nothingness
        if (collision.CompareTag(groundTag))
        {
            Destroy(gameObject);
        }
        // If it touches the player, bloom into the new color
        else if (collision.CompareTag(playerTag) && !hasBeenTouched)
        {
            hasBeenTouched = true;
            StartCoroutine(SmoothTransitionToTouchedSprite());
        }
    }

    private IEnumerator SmoothTransitionToTouchedSprite()
    {
        float currentAlpha = 0f;
        while (currentAlpha < 1f)
        {
            currentAlpha += Time.deltaTime * transitionSpeed;
            Color newColor = fadeRenderer.color;
            newColor.a = Mathf.Clamp01(currentAlpha);
            fadeRenderer.color = newColor;
            yield return null;
        }
    }
}