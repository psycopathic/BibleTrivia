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

    private Dictionary<string, List<Slot>> answerSlots =
        new Dictionary<string, List<Slot>>();

    private List<LetterButton> selectedLetters =
        new List<LetterButton>();

    private HashSet<string> solvedWords =
        new HashSet<string>();

    private string currentWord = "";

    private void Awake()
    {
        Instance = this;
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
            victoryPopup.Show();
             Invoke(nameof(ReturnToMainScene), 5f);
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

        int count = currentLevel.letters.Count;

        for (int i = 0; i < count; i++)
        {
            LetterButton button =
                Instantiate(letterPrefab, letterContainer);

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

                if (solvedWords.Count == currentLevel.answers.Count)
                {
                    Debug.Log("Puzzle Complete");

                    Invoke(nameof(LoadNextPuzzle), 1f);
                }
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
}
