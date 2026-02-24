using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

[System.Serializable]
public class NarrationData
{
    [Tooltip("The unique ID to call this specific narration group.")]
    public string narrationId;
    
    [Tooltip("The sequence of story texts to be displayed one after another. Use /word/ to color it red!")]
    [TextArea(3, 5)] 
    public string[] narrationLines;
    
    [Tooltip("How long each text stays on screen after it finishes typing before moving to the next line.")]
    public float autoAdvanceDelay = 3f;
}

[RequireComponent(typeof(AudioSource))]
public class NarrationManager : MonoBehaviour
{
    public static NarrationManager Instance { get; private set; }

    // --- KEAJAIBAN BARU: Penanda apakah dunia harus diam ---
    public bool IsNarrationPlaying { get { return isMasterSequenceRunning; } }

    [Header("Narration Database")]
    [SerializeField] private List<NarrationData> narrationDatabase;

    [Header("Cinematic UI Elements")]
    [Tooltip("The top black bar RectTransform. Anchor should be top-stretch.")]
    [SerializeField] private RectTransform topCinematicBar;
    
    [Tooltip("The bottom black bar RectTransform. Anchor should be bottom-stretch.")]
    [SerializeField] private RectTransform bottomCinematicBar;
    
    [Tooltip("The TextMeshPro UI element inside the bottom bar.")]
    [SerializeField] private TextMeshProUGUI narrationTextUI;
    
    [Tooltip("CanvasGroup on the Text object to control fade in/out transparency.")]
    [SerializeField] private CanvasGroup textCanvasGroup;

    [Header("Animation Settings")]
    [Tooltip("How fast the cinematic bars slide in and out.")]
    [SerializeField] private float barSlideSpeed = 500f;
    
    [Tooltip("Delay between each character appearing (lower is faster).")]
    [SerializeField] private float typingSpeed = 0.05f;
    
    [Tooltip("How fast the text fades out before the next line or before bars close.")]
    [SerializeField] private float textFadeOutSpeed = 2f;

    [Header("Audio Settings")]
    [Tooltip("The looping sound effect played while text is typing.")]
    [SerializeField] private AudioClip typingSFX;
    
    private AudioSource audioSource;
    
    // Internal State Variables for Queue and Flow
    private Queue<NarrationData> narrationQueue = new Queue<NarrationData>();
    private bool isMasterSequenceRunning = false;
    
    private bool isTypingText = false;
    private bool isWaitingForNextLine = false;
    private Coroutine typingCoroutine;

    // Target positions for the bars (Y axis)
    private float topBarHiddenY, topBarVisibleY = 0f;
    private float bottomBarHiddenY, bottomBarVisibleY = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        audioSource = GetComponent<AudioSource>();

        topBarHiddenY = topCinematicBar.rect.height;
        bottomBarHiddenY = -bottomCinematicBar.rect.height;

        SetBarsPosition(topBarHiddenY, bottomBarHiddenY);
        textCanvasGroup.alpha = 0f;
        narrationTextUI.text = "";
    }

    private void Update()
    {
        // Detect screen tap or mouse click to interact with the narration
        if (Input.GetMouseButtonDown(0))
        {
            HandleUserInput();
        }
    }

    public void PlayNarration(string idToPlay)
    {
        NarrationData data = narrationDatabase.Find(n => n.narrationId == idToPlay);
        
        if (data != null && data.narrationLines.Length > 0)
        {
            narrationQueue.Enqueue(data);
            
            if (!isMasterSequenceRunning)
            {
                StartCoroutine(MasterNarrationSequence());
            }
        }
        else
        {
            Debug.LogError("Narration ID not found or has no lines: " + idToPlay);
        }
    }

    private void HandleUserInput()
    {
        if (isTypingText)
        {
            if (typingCoroutine != null) 
            {
                StopCoroutine(typingCoroutine);
            }
            
            narrationTextUI.maxVisibleCharacters = 99999; 
            StopTypingAudio();
            isTypingText = false; 
        }
        else if (isWaitingForNextLine)
        {
            isWaitingForNextLine = false; 
        }
    }

    // --- MAIN SEQUENCES ---

    private IEnumerator MasterNarrationSequence()
    {
        isMasterSequenceRunning = true;

        while (narrationQueue.Count > 0)
        {
            if (Mathf.Abs(topCinematicBar.anchoredPosition.y - topBarVisibleY) > 0.1f)
            {
                textCanvasGroup.alpha = 1f;
                yield return StartCoroutine(SlideBars(topBarVisibleY, bottomBarVisibleY));
            }

            NarrationData currentData = narrationQueue.Dequeue();
            yield return StartCoroutine(ProcessNarrationDataRoutine(currentData));
        }

        yield return StartCoroutine(CloseNarrationSequence());

        isMasterSequenceRunning = false;
        
        if (narrationQueue.Count > 0)
        {
            StartCoroutine(MasterNarrationSequence());
        }
    }

    private IEnumerator ProcessNarrationDataRoutine(NarrationData data)
    {
        for (int i = 0; i < data.narrationLines.Length; i++)
        {
            string rawText = data.narrationLines[i];
            string parsedText = Regex.Replace(rawText, @"/(.*?)/", "<color=red>$1</color>");

            textCanvasGroup.alpha = 1f;
            isTypingText = true;
            
            typingCoroutine = StartCoroutine(TypeTextRoutine(parsedText));
            
            while (isTypingText)
            {
                yield return null;
            }

            isWaitingForNextLine = true;
            float waitTimer = 0f;
            
            while (isWaitingForNextLine && waitTimer < data.autoAdvanceDelay)
            {
                waitTimer += Time.deltaTime;
                yield return null;
            }
            
            isWaitingForNextLine = false; 

            bool isLastLineOverall = (i == data.narrationLines.Length - 1) && (narrationQueue.Count == 0);
            if (!isLastLineOverall)
            {
                while (textCanvasGroup.alpha > 0f)
                {
                    textCanvasGroup.alpha -= textFadeOutSpeed * Time.deltaTime;
                    yield return null;
                }
            }
        }
    }

    private IEnumerator CloseNarrationSequence()
    {
        while (textCanvasGroup.alpha > 0f)
        {
            textCanvasGroup.alpha -= textFadeOutSpeed * Time.deltaTime;
            yield return null;
        }
        narrationTextUI.text = "";

        yield return StartCoroutine(SlideBars(topBarHiddenY, bottomBarHiddenY));
    }

    // --- HELPER COROUTINES ---

    private IEnumerator SlideBars(float targetTopY, float targetBottomY)
    {
        while (Mathf.Abs(topCinematicBar.anchoredPosition.y - targetTopY) > 0.1f ||
               Mathf.Abs(bottomCinematicBar.anchoredPosition.y - targetBottomY) > 0.1f)
        {
            Vector2 newTopPos = topCinematicBar.anchoredPosition;
            newTopPos.y = Mathf.MoveTowards(newTopPos.y, targetTopY, barSlideSpeed * Time.deltaTime);
            topCinematicBar.anchoredPosition = newTopPos;

            Vector2 newBottomPos = bottomCinematicBar.anchoredPosition;
            newBottomPos.y = Mathf.MoveTowards(newBottomPos.y, targetBottomY, barSlideSpeed * Time.deltaTime);
            bottomCinematicBar.anchoredPosition = newBottomPos;

            yield return null;
        }

        SetBarsPosition(targetTopY, targetBottomY); 
    }

    private IEnumerator TypeTextRoutine(string parsedText)
    {
        PlayTypingAudio();

        narrationTextUI.text = parsedText;
        narrationTextUI.maxVisibleCharacters = 0;
        
        narrationTextUI.ForceMeshUpdate();
        int totalCharacters = narrationTextUI.textInfo.characterCount;

        for (int i = 0; i <= totalCharacters; i++)
        {
            narrationTextUI.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
        }

        StopTypingAudio();
        isTypingText = false;
    }

    private void PlayTypingAudio()
    {
        if (typingSFX != null && audioSource != null)
        {
            audioSource.clip = typingSFX;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void StopTypingAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void SetBarsPosition(float topY, float bottomY)
    {
        topCinematicBar.anchoredPosition = new Vector2(topCinematicBar.anchoredPosition.x, topY);
        bottomCinematicBar.anchoredPosition = new Vector2(bottomCinematicBar.anchoredPosition.x, bottomY);
    }
}