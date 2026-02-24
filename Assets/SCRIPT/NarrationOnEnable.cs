using UnityEngine;

public class NarrationOnEnable : MonoBehaviour
{
    public enum TriggerMoment
    {
        OnEnable,
        OnStart
    }

    [Header("Narration Settings")]
    [Tooltip("The ID of the narration to play when this magical object is triggered.")]
    [SerializeField] private string narrationIdToPlay;

    [Tooltip("Choose whether the story is told exactly when the scene starts, or when the object is enabled later.")]
    [SerializeField] private TriggerMoment triggerMoment = TriggerMoment.OnEnable;

    // This function is automatically called the moment the GameObject is enabled
    private void OnEnable()
    {
        if (triggerMoment == TriggerMoment.OnEnable)
        {
            TriggerNarration();
        }
    }

    // This function is automatically called on the first frame the script is active
    private void Start()
    {
        if (triggerMoment == TriggerMoment.OnStart)
        {
            TriggerNarration();
        }
    }

    private void TriggerNarration()
    {
        // Whisper the story to the NarrationManager
        if (NarrationManager.Instance != null && !string.IsNullOrEmpty(narrationIdToPlay))
        {
            NarrationManager.Instance.PlayNarration(narrationIdToPlay);
        }
        else
        {
            Debug.LogWarning("Kisah tidak dapat diceritakan: NarrationManager hilang atau ID Narasi kosong di " + gameObject.name);
        }
    }
}