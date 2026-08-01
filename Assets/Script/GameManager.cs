using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[DefaultExecutionOrder(-900)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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

    // private WordData wordData;
    // private PuzzleData currentPuzzle;

    private LevelsData levelsData;
    private LevelData currentLevel;

    public bool IsDragging { get; private set; }

    private Dictionary<string, List<Slot>> answerSlots =
        new Dictionary<string, List<Slot>>();

    private List<LetterButton> selectedLetters =
        new List<LetterButton>();

    private List<LetterButton> letterButtons = new();
    private List<Vector2> letterPositions = new();

    private HashSet<string> solvedWords =
        new HashSet<string>();

    private string currentWord = "";

    // private int currentStageIndex = 0;
    // private int currentPuzzleIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void InitializeCurrentLevel()
    {
        int level = PlayerPrefs.GetInt("CURRENT_LEVEL", 1);
        Debug.Log($"[GameManager] Current level from PlayerPrefs: {level}", this);
    }

    private bool TryInitializeLevelData()
    {
        DataManager dataManager = DataManager.EnsureInstance();

        if (dataManager == null)
        {
            Debug.LogError("[GameManager] DataManager could not be created or found.", this);
            return false;
        }

        if (!dataManager.IsDataLoaded)
        {
            string reason = string.IsNullOrEmpty(dataManager.LastLoadError)
                ? "DataManager did not finish loading game data."
                : dataManager.LastLoadError;

            Debug.LogError($"[GameManager] Cannot start because DataManager failed to load data: {reason}", this);
            return false;
        }

        levelsData = dataManager.Levels;

        if (levelsData == null)
        {
            Debug.LogError("[GameManager] DataManager returned null LevelsData.", this);
            return false;
        }

        if (levelsData.levels == null)
        {
            Debug.LogError("[GameManager] LevelsData is present, but its 'levels' list is null.", this);
            return false;
        }

        if (levelsData.levels.Count == 0)
        {
            Debug.LogError("[GameManager] LevelsData loaded successfully, but it contains 0 levels.", this);
            return false;
        }

        Debug.Log($"[GameManager] Received level data from DataManager. Total levels: {levelsData.levels.Count}", this);
        return true;
    }

    private void Start()
    {
        InitializeCurrentLevel();

        if (!TryInitializeLevelData())
        {
            return;
        }

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

    //---------------------------------------------------------
    // PUZZLE LOADING
    //---------------------------------------------------------

    void LoadCurrentPuzzle()
    {
        if (levelsData == null || levelsData.levels == null)
        {
            Debug.LogError("[GameManager] Cannot load current puzzle because level data is not available.", this);
            return;
        }

        int savedLevel = PlayerPrefs.GetInt("CURRENT_LEVEL", 1);

        if (savedLevel < 1 || savedLevel > levelsData.levels.Count)
        {
            Debug.LogError($"[GameManager] Saved level {savedLevel} is out of range. Valid range: 1-{levelsData.levels.Count}.", this);
            return;
        }

        currentLevel = levelsData.levels[savedLevel - 1];

        if (currentLevel == null)
        {
            Debug.LogError($"[GameManager] Level data at index {savedLevel - 1} is null.", this);
            return;
        }

        Debug.Log($"[GameManager] Loading level {currentLevel.id}", this);

        solvedWords.Clear();

        SpawnLetters();
        SpawnSlots();

        UpdatePuzzleCounter();
    }

    void UpdatePuzzleCounter()
    {
        if (puzzleCounterText == null)
        {
            Debug.LogError("[GameManager] Puzzle counter text is not assigned.", this);
            return;
        }

        if (levelsData == null || levelsData.levels == null)
        {
            Debug.LogError("[GameManager] Cannot update puzzle counter because level data is unavailable.", this);
            return;
        }

        int currentLevelNumber = PlayerPrefs.GetInt("CURRENT_LEVEL", 1);
        int totalLevels = levelsData.levels.Count;

        puzzleCounterText.text = $"{currentLevelNumber}/{totalLevels}";
    }

    void LoadNextPuzzle()
    {
        if (levelsData == null || levelsData.levels == null)
        {
            Debug.LogError("[GameManager] Cannot load the next puzzle because level data is unavailable.", this);
            return;
        }

        int currentLevel = PlayerPrefs.GetInt("CURRENT_LEVEL", 1);

        currentLevel++;

        PlayerPrefs.SetInt("CURRENT_LEVEL", currentLevel);
        PlayerPrefs.Save();

        if (currentLevel > levelsData.levels.Count)
        {
            Debug.Log("[GameManager] Game completed.", this);
            SceneManager.LoadScene("MainScene");
            return;
        }

        if ((currentLevel - 1) % 2 == 0)
        {
            Debug.Log($"[GameManager] Level {currentLevel - 1} complete. Loading BonusQuestionScene.", this);
            SceneManager.LoadScene("BonusQuestionScene");
            return;
        }

        Debug.Log($"[GameManager] Advancing to level {currentLevel}. Reloading MainScene.", this);
        SceneManager.LoadScene("MainScene");
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
                    CoinManager.Instance.AddCoins(CoinConstants.LevelReward);
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


    public void ShuffleLetters()
{
    if (IsDragging)
        return;

    List<Vector2> shuffled = new List<Vector2>(letterPositions);

    // Fisher-Yates shuffle
    for (int i = shuffled.Count - 1; i > 0; i--)
    {
        int random = Random.Range(0, i + 1);

        Vector2 temp = shuffled[i];
        shuffled[i] = shuffled[random];
        shuffled[random] = temp;
    }

    for (int i = 0; i < letterButtons.Count; i++)
    {
        RectTransform rect = letterButtons[i].GetComponent<RectTransform>();
        rect.anchoredPosition = shuffled[i];
    }
}

    public void RevealHint()
  {
    foreach (var pair in answerSlots)
    {
        string answer = pair.Key;
        List<Slot> slots = pair.Value;

        // Skip already solved words
        if (solvedWords.Contains(answer))
            continue;

        // Reveal the first empty letter
        for (int i = 0; i < answer.Length; i++)
        {
            if (!slots[i].IsFilled)
            {
                slots[i].SetLetter(answer[i].ToString());

                Debug.Log($"Hint revealed: {answer[i]}");

                return;
            }
        }
    }
  }
}
