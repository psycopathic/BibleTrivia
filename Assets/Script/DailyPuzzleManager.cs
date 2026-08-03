using System.Collections;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-900)]
public class DailyPuzzleManager : MonoBehaviour
{
    public static DailyPuzzleManager Instance;

    [Header("Prefabs")]
    [SerializeField] private LetterButton letterPrefab;
    [SerializeField] private Slot slotPrefab;
    [SerializeField] private Transform rowPrefab;

    [Header("Containers")]
    [SerializeField] private Transform letterContainer;
    [SerializeField] private Transform slotContainer;

    [Header("Letter Wheel")]
    [SerializeField] private float wheelRadius = 190f;
    [SerializeField] private float shuffleDuration = 0.25f;

    [Header("Drag Line")]
    [SerializeField] private LineRenderer dragLine;

    [Header("UI")]
    [SerializeField] private TMP_Text puzzleCounterText;

    [Header("Calendar")]
    [SerializeField] private TMP_Text monthText;
    [SerializeField] private TMP_Text dayText;
    [SerializeField]
    private VictoryPopup victoryPopup;

    private const string DailyQuestionsResourcePath = "Data/dailyQuestions";

    private DailyQuestionData dailyData;
    private DailyDay todayData;
    private LevelData currentLevel;
    private int currentPuzzleIndex = 0;

    public bool IsDragging { get; private set; }
    private bool isShuffleAnimating;
    private Coroutine shuffleCoroutine;
    private Coroutine returnToMainSceneCoroutine;

    private Dictionary<string, List<Slot>> answerSlots =
        new Dictionary<string, List<Slot>>();

    private List<LetterButton> selectedLetters =
        new List<LetterButton>();

    private List<LetterButton> letterButtons = new();
    private List<Vector2> letterPositions = new();

    private HashSet<string> solvedWords =
        new HashSet<string>();

    private string currentWord = "";

    private void Awake()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        if (returnToMainSceneCoroutine != null)
        {
            StopCoroutine(returnToMainSceneCoroutine);
            returnToMainSceneCoroutine = null;
        }
    }

    private void InitializeCurrentLevel()
    {
        Debug.Log($"[DailyPuzzleManager] Starting daily challenge at puzzle index {currentPuzzleIndex}", this);
    }

    private bool TryInitializeLevelData()
    {
        TextAsset json = Resources.Load<TextAsset>(DailyQuestionsResourcePath);

        if (json == null)
        {
            Debug.LogError($"[DailyPuzzleManager] Could not find Resources/{DailyQuestionsResourcePath}.json", this);
            return false;
        }

        dailyData = JsonUtility.FromJson<DailyQuestionData>(json.text);

        if (dailyData == null || dailyData.days == null)
        {
            Debug.LogError("[DailyPuzzleManager] Failed to parse dailyQuestions.json or 'days' is missing.", this);
            return false;
        }

        string today = DateTime.Now.ToString("yyyy-MM-dd");
        todayData = dailyData.days.Find(day => day.date == today);

        if (todayData == null)
        {
            Debug.LogError("No Daily Challenge Found", this);
            return false;
        }

        if (todayData.levels == null || todayData.levels.Count == 0)
        {
            Debug.LogError($"[DailyPuzzleManager] Daily challenge for {today} has no levels.", this);
            return false;
        }

        Debug.Log($"[DailyPuzzleManager] Loaded daily challenge for {today} with {todayData.levels.Count} levels.", this);
        return true;
    }

    private void Start()
    {
        InitializeCurrentLevel();

        if (!TryInitializeLevelData())
        {
            return;
        }
        
        UpdateCalendar();
        LoadCurrentPuzzle();

        if (dragLine != null)
        {
            dragLine.positionCount = 0;
            dragLine.enabled = false;
        }
    }

    private void Update()
    {
        if (!IsDragging)
            return;

        if (selectedLetters.Count == 0)
            return;

        if (dragLine == null)
            return;

        dragLine.positionCount = selectedLetters.Count + 1;

        for (int i = 0; i < selectedLetters.Count; i++)
        {
            dragLine.SetPosition(i, selectedLetters[i].transform.position);
        }

        Vector3 mouse = Input.mousePosition;
        mouse.z = 10f;

        Vector3 world = Camera.main.ScreenToWorldPoint(mouse);

        dragLine.SetPosition(selectedLetters.Count, world);
    }

    private void UpdateCalendar()
   {
    DateTime today = DateTime.Now;

    monthText.text = today.ToString("MMM").ToUpper(); // JAN, FEB, MAR...
    dayText.text = today.Day.ToString();              // 1, 2, 3 ... 31
   }
   
   private void ReturnToMainScene()
     {
     SceneManager.LoadScene("MainScene");
     }

    private IEnumerator ReturnToMainSceneAfterVictory()
    {
        if (victoryPopup == null)
        {
            Debug.LogError("[DailyPuzzleManager] Victory popup is not assigned.", this);
            returnToMainSceneCoroutine = null;
            yield break;
        }

        victoryPopup.Show();
        yield return new WaitForSeconds(victoryPopup.ShowDuration + 5f);
        returnToMainSceneCoroutine = null;
        ReturnToMainScene();
    }
    //---------------------------------------------------------
    // PUZZLE LOADING
    //---------------------------------------------------------

    void LoadCurrentPuzzle()
    {
        if (todayData == null || todayData.levels == null)
        {
            Debug.LogError("[DailyPuzzleManager] Cannot load daily puzzle because today's data is unavailable.", this);
            return;
        }

        if (currentPuzzleIndex < 0 || currentPuzzleIndex >= todayData.levels.Count)
        {
            Debug.LogError($"[DailyPuzzleManager] Daily puzzle index {currentPuzzleIndex} is out of range.", this);
            return;
        }

        currentLevel = todayData.levels[currentPuzzleIndex];

        if (currentLevel == null)
        {
            Debug.LogError($"[DailyPuzzleManager] Daily level data at index {currentPuzzleIndex} is null.", this);
            return;
        }

        Debug.Log($"[DailyPuzzleManager] Loading daily puzzle {currentPuzzleIndex + 1}/{todayData.levels.Count} (level id {currentLevel.id})", this);

        ResetInteractionState();
        solvedWords.Clear();

        SpawnLetters();
        SpawnSlots();

        UpdatePuzzleCounter();
    }

    void UpdatePuzzleCounter()
    {
        if (puzzleCounterText == null)
        {
            Debug.LogError("[DailyPuzzleManager] Puzzle counter text is not assigned.", this);
            return;
        }

        if (todayData == null || todayData.levels == null)
        {
            Debug.LogError("[DailyPuzzleManager] Cannot update puzzle counter because daily data is unavailable.", this);
            return;
        }

        puzzleCounterText.text = $"{currentPuzzleIndex + 1}/{todayData.levels.Count}";
    }

    void LoadNextPuzzle()
    {
        if (todayData == null || todayData.levels == null)
        {
            Debug.LogError("[DailyPuzzleManager] Cannot load the next daily puzzle because daily data is unavailable.", this);
            return;
        }

        currentPuzzleIndex++;

        if (currentPuzzleIndex >= todayData.levels.Count)
        {
            if (returnToMainSceneCoroutine != null)
            {
                StopCoroutine(returnToMainSceneCoroutine);
            }

            returnToMainSceneCoroutine = StartCoroutine(ReturnToMainSceneAfterVictory());
            return;
        }

        Debug.Log($"[DailyPuzzleManager] Advancing to daily puzzle {currentPuzzleIndex + 1}/{todayData.levels.Count}.", this);
        LoadCurrentPuzzle();
    }

    //---------------------------------------------------------
    // LETTER WHEEL
    //---------------------------------------------------------

    void SpawnLetters()
    {
        foreach (Transform child in letterContainer)
            Destroy(child.gameObject);

        letterButtons.Clear();
        letterPositions.Clear();

        int count = currentLevel.letters.Count;

        for (int i = 0; i < count; i++)
        {
            LetterButton button =
                Instantiate(letterPrefab, letterContainer);

            letterButtons.Add(button);

            button.SetLetter(currentLevel.letters[i]);

            RectTransform rect =
                button.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(.5f, .5f);
            rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);

            rect.sizeDelta = new Vector2(100, 100);

            float angle =
                (90f + i * (360f / count)) * Mathf.Deg2Rad;

            rect.anchoredPosition =
                new Vector2(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle)
                ) * wheelRadius;

            letterPositions.Add(rect.anchoredPosition);
        }
    }

    //---------------------------------------------------------
    // WORD SLOTS
    //---------------------------------------------------------

    void SpawnSlots()
    {
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        answerSlots.Clear();

        foreach (string answer in currentLevel.answers)
        {
            Transform row =
                Instantiate(rowPrefab, slotContainer);

            row.name = answer;

            List<Slot> slots = new List<Slot>();

            foreach (char c in answer)
            {
                Slot slot =
                    Instantiate(slotPrefab, row);

                slot.Clear();

                slots.Add(slot);
            }

            answerSlots.Add(answer, slots);
        }
    }

    //---------------------------------------------------------
    // DRAGGING
    //---------------------------------------------------------

    public void StartSelection(LetterButton button)
    {
        if (isShuffleAnimating)
            return;

        IsDragging = true;

        currentWord = "";

        selectedLetters.Clear();

        if (dragLine != null)
        {
            dragLine.positionCount = 0;
            dragLine.enabled = true;
        }

        AddLetter(button);
    }

    public void AddLetter(LetterButton button)
    {
        if (!IsDragging)
            return;

        if (selectedLetters.Contains(button))
            return;

        selectedLetters.Add(button);

        currentWord += button.Letter;

        if (dragLine != null)
        {
            dragLine.positionCount = selectedLetters.Count;

            for (int i = 0; i < selectedLetters.Count; i++)
            {
                dragLine.SetPosition(i, selectedLetters[i].transform.position);
            }
        }

        Debug.Log("Current Word : " + currentWord);
    }

    public void EndSelection()
    {
        IsDragging = false;

        if (answerSlots.ContainsKey(currentWord))
        {
            if (!solvedWords.Contains(currentWord))
            {
                solvedWords.Add(currentWord);

                List<Slot> slots = answerSlots[currentWord];

                for (int i = 0; i < currentWord.Length; i++)
                {
                    slots[i].SetLetter(currentWord[i].ToString());
                }

                Debug.Log("Solved : " + currentWord);
                HandlePuzzleCompletion();
            }
        }

        currentWord = "";
        selectedLetters.Clear();

        if (dragLine != null)
        {
            dragLine.positionCount = 0;
            dragLine.enabled = false;
        }
    }

    public void ShuffleLetters()
    {
        if (IsDragging || isShuffleAnimating || letterButtons.Count <= 1)
            return;

        List<Vector2> shuffled = new List<Vector2>(letterPositions);

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int random = UnityEngine.Random.Range(0, i + 1);

            Vector2 temp = shuffled[i];
            shuffled[i] = shuffled[random];
            shuffled[random] = temp;
        }

        if (shuffleCoroutine != null)
            StopCoroutine(shuffleCoroutine);

        shuffleCoroutine = StartCoroutine(AnimateShuffle(shuffled));
    }

    private IEnumerator AnimateShuffle(List<Vector2> targetPositions)
    {
        isShuffleAnimating = true;

        List<RectTransform> rectTransforms = new List<RectTransform>(letterButtons.Count);
        List<Vector2> startPositions = new List<Vector2>(letterButtons.Count);

        for (int i = 0; i < letterButtons.Count; i++)
        {
            RectTransform rect = letterButtons[i].GetComponent<RectTransform>();
            rectTransforms.Add(rect);
            startPositions.Add(rect.anchoredPosition);
        }

        float elapsed = 0f;

        while (elapsed < shuffleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / shuffleDuration);
            float easedT = 1f - (1f - t) * (1f - t);

            for (int i = 0; i < rectTransforms.Count; i++)
            {
                rectTransforms[i].anchoredPosition = Vector2.LerpUnclamped(
                    startPositions[i],
                    targetPositions[i],
                    easedT
                );
            }

            yield return null;
        }

        for (int i = 0; i < rectTransforms.Count; i++)
            rectTransforms[i].anchoredPosition = targetPositions[i];

        isShuffleAnimating = false;
        shuffleCoroutine = null;
    }

    public void RevealHint()
    {
        foreach (var pair in answerSlots)
        {
            string answer = pair.Key;
            List<Slot> slots = pair.Value;

            if (solvedWords.Contains(answer))
                continue;

            for (int i = 0; i < answer.Length; i++)
            {
                if (!slots[i].IsFilled)
                {
                    slots[i].SetLetter(answer[i].ToString());
                    Debug.Log($"Hint revealed: {answer[i]}");

                    if (TryMarkWordSolved(answer, slots))
                        HandlePuzzleCompletion();

                    return;
                }
            }
        }
    }

    private void ResetInteractionState()
    {
        IsDragging = false;
        currentWord = "";
        selectedLetters.Clear();

        if (shuffleCoroutine != null)
        {
            StopCoroutine(shuffleCoroutine);
            shuffleCoroutine = null;
        }

        isShuffleAnimating = false;

        if (dragLine != null)
        {
            dragLine.positionCount = 0;
            dragLine.enabled = false;
        }

        CancelInvoke(nameof(LoadNextPuzzle));
    }

    private bool TryMarkWordSolved(string answer, List<Slot> slots)
    {
        if (solvedWords.Contains(answer))
            return false;

        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsFilled)
                return false;
        }

        solvedWords.Add(answer);
        Debug.Log("Solved : " + answer);
        return true;
    }

    private void HandlePuzzleCompletion()
    {
        if (solvedWords.Count != currentLevel.answers.Count)
            return;

        Debug.Log("Puzzle Complete");

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.AddCoins(CoinConstants.LevelReward);
        }
        else
        {
            Debug.LogWarning("[DailyPuzzleManager] CoinManager is missing. Skipping coin reward but continuing puzzle flow.", this);
        }

        CancelInvoke(nameof(LoadNextPuzzle));
        Invoke(nameof(LoadNextPuzzle), 1f);
    }
}
