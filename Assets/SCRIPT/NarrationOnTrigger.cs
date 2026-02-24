using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NarrationOnTrigger : MonoBehaviour
{
    [Header("Narration Settings")]
    [Tooltip("The ID of the narration to play when the player steps into this area.")]
    [SerializeField] private string narrationIdToPlay;

    [Tooltip("The tag of the character who is allowed to hear this story.")]
    [SerializeField] private string targetTag = "Player";

    [Tooltip("If true, this story will only be told once, no matter how many times the area is entered.")]
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasToldStory = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the story has been told and it's only meant to be heard once, remain silent
        if (triggerOnlyOnce && hasToldStory) return;

        // Check if the one stepping in is exactly who we are waiting for
        if (collision.CompareTag(targetTag))
        {
            if (NarrationManager.Instance != null && !string.IsNullOrEmpty(narrationIdToPlay))
            {
                NarrationManager.Instance.PlayNarration(narrationIdToPlay);
                hasToldStory = true; // Mark the story as told
            }
        }
    }
}