using UnityEngine;

// Ensures the bubble can be clicked and felt
[RequireComponent(typeof(Collider2D))]
public class BubbleBehavior : MonoBehaviour
{
    private MusicalBubbleMinigame manager;
    private float lifeTime;
    private float floatSpeed;
    private float swaySpeed = 2f;
    private float swayWidth = 0.5f;

    private SpriteRenderer baseRenderer;
    private SpriteRenderer fadeRenderer;

    private float lifeTimer = 0f;
    private float initialX;
    private float windOffset;
    private bool hasPopped = false;

    // This is called by the Manager when the bubble is born
    public void Initialize(MusicalBubbleMinigame mgr, float life, float speed, Sprite spriteA, Sprite spriteB)
    {
        manager = mgr;
        lifeTime = life;
        floatSpeed = speed;

        baseRenderer = GetComponent<SpriteRenderer>();
        baseRenderer.sprite = spriteA;

        // Create the magical transition layer
        GameObject fadeObj = new GameObject("FadeSprite");
        fadeObj.transform.SetParent(this.transform);
        fadeObj.transform.localPosition = Vector3.zero;
        
        fadeRenderer = fadeObj.AddComponent<SpriteRenderer>();
        fadeRenderer.sprite = spriteB;
        fadeRenderer.sortingLayerID = baseRenderer.sortingLayerID;
        fadeRenderer.sortingOrder = baseRenderer.sortingOrder + 1;
        
        // Start completely transparent
        Color startColor = fadeRenderer.color;
        startColor.a = 0f;
        fadeRenderer.color = startColor;

        initialX = transform.position.x;
        windOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (hasPopped) return;

        lifeTimer += Time.deltaTime;

        // 1. Natural Floating Movement
        float newX = initialX + Mathf.Sin((Time.time + windOffset) * swaySpeed) * swayWidth;
        float newY = transform.position.y + (floatSpeed * Time.deltaTime);
        transform.position = new Vector2(newX, newY);

        // 2. Smooth Transition from Sprite A to Sprite B
        float transitionProgress = lifeTimer / lifeTime;
        Color currentColor = fadeRenderer.color;
        currentColor.a = Mathf.Clamp01(transitionProgress);
        fadeRenderer.color = currentColor;

        // 3. Pop if it fully becomes Sprite B (Time is up)
        if (lifeTimer >= lifeTime)
        {
            hasPopped = true;
            manager.BubbleMissed(this);
            Destroy(gameObject);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (NarrationManager.Instance != null && NarrationManager.Instance.IsNarrationPlaying) return;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (GetComponent<Collider2D>().OverlapPoint(mousePos))
            {
                hasPopped = true;
                manager.BubbleClicked(this);
                Destroy(gameObject);
            }
        }
    }

    // 4. Magic touch to pop the bubble
    private void OnMouseDown()
    {
        if (NarrationManager.Instance != null && NarrationManager.Instance.IsNarrationPlaying) return;
        if (hasPopped) return;
        
        hasPopped = true;
        manager.BubbleClicked(this);
        Destroy(gameObject);
    }
}