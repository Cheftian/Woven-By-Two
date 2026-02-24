using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This ensures the flower has a collider to detect the touch
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class FlowerInteraction : MonoBehaviour
{
    [Header("Magical Sprites")]
    [Tooltip("The original look of the flower.")]
    [SerializeField] private Sprite spriteA;
    
    [Tooltip("The new look of the flower after being touched.")]
    [SerializeField] private Sprite spriteB;

    [Header("Transition Settings")]
    [Tooltip("How fast the flower blossoms into its new form. Higher means faster.")]
    [SerializeField] private float transitionSpeed = 2f;

    [Tooltip("The tag of the entity that can trigger this magic (e.g., 'Player').")]
    [SerializeField] private string targetTag = "Player";

    private SpriteRenderer baseRenderer;
    private SpriteRenderer fadeRenderer;
    private Collider2D flowerCollider;
    private bool hasBeenTouched = false;
    
    // A filter to sense all overlapping objects flawlessly
    private ContactFilter2D overlapFilter;
    private List<Collider2D> touchingColliders = new List<Collider2D>();

    private void Start()
    {
        baseRenderer = GetComponent<SpriteRenderer>();
        baseRenderer.sprite = spriteA;
        
        flowerCollider = GetComponent<Collider2D>();
        
        // Prepare the filter to ensure it detects everything, including other triggers
        overlapFilter = new ContactFilter2D();
        overlapFilter.NoFilter();
        overlapFilter.useTriggers = true; 

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
        fadeRenderer.sprite = spriteB;
        
        fadeRenderer.sortingLayerID = baseRenderer.sortingLayerID;
        fadeRenderer.sortingOrder = baseRenderer.sortingOrder + 1;

        Color startColor = fadeRenderer.color;
        startColor.a = 0f;
        fadeRenderer.color = startColor;
    }

    private void Update()
    {
        // As long as it hasn't blossomed, keep feeling the surroundings
        if (!hasBeenTouched)
        {
            // This manually checks all colliders touching this flower's area every frame
            int overlapCount = flowerCollider.Overlap(overlapFilter, touchingColliders);
            
            for (int i = 0; i < overlapCount; i++)
            {
                // If any of the touching entities has the right tag, trigger the magic
                if (touchingColliders[i].CompareTag(targetTag))
                {
                    hasBeenTouched = true;
                    StartCoroutine(SmoothTransitionToSpriteB());
                    break; // One touch is enough to start the blossom
                }
            }
        }
    }

    private IEnumerator SmoothTransitionToSpriteB()
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