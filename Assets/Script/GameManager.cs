using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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

    private WordData wordData;
    private PuzzleData currentPuzzle;

    public bool IsDragging { get; private set; }

    private Dictionary<string, List<Slot>> answerSlots =
        new Dictionary<string, List<Slot>>();

    private List<LetterButton> selectedLetters =
        new List<LetterButton>();

    private HashSet<string> solvedWords =
        new HashSet<string>();

    private string currentWord = "";

    private int currentStageIndex = 0;
    private int currentPuzzleIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        TextAsset json = Resources.Load<TextAsset>("words");

        if (json == null)
        {
            Debug.LogError("words.json not found.");
            return;
        }

        wordData = JsonUtility.FromJson<WordData>(json.text);

        if (wordData == null || wordData.stages.Count == 0)
{
    Debug.LogError("No stages found.");
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
    currentPuzzle =
        wordData
        .stages[currentStageIndex]
        .puzzles[currentPuzzleIndex];

        Debug.Log($"Loading Stage {currentStageIndex + 1}, Puzzle {currentPuzzleIndex + 1}");

    solvedWords.Clear();

    SpawnLetters();
    SpawnSlots();

    UpdatePuzzleCounter();
}
    void UpdatePuzzleCounter()
{
    if (puzzleCounterText == null)
    {
        Debug.LogError("Puzzle Counter Text is NULL");
        return;
    }

    int totalPuzzles =
        wordData.stages[currentStageIndex].puzzles.Count;

    puzzleCounterText.text =
        $"{currentPuzzleIndex + 1}/{totalPuzzles}";

    Debug.Log("Counter Updated To : " + puzzleCounterText.text);
}

    void LoadNextPuzzle()
    {
        currentPuzzleIndex++;
         Debug.Log("Current Puzzle Index: " + currentPuzzleIndex);
       int totalPuzzles =
        wordData.stages[currentStageIndex].puzzles.Count;

    if (currentPuzzleIndex >= totalPuzzles)
    {
        SceneManager.LoadScene("BonusQuestionScene");
        return;
    }

        LoadCurrentPuzzle();
    }

    //---------------------------------------------------------
    // LETTER WHEEL
    //---------------------------------------------------------

    void SpawnLetters()
    {
        foreach (Transform child in letterContainer)
            Destroy(child.gameObject);

        int count = currentPuzzle.letters.Count;

        for (int i = 0; i < count; i++)
        {
            LetterButton button =
                Instantiate(letterPrefab, letterContainer);

            button.SetLetter(currentPuzzle.letters[i]);

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

        foreach (string answer in currentPuzzle.answers)
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

                if (solvedWords.Count == currentPuzzle.answers.Count)
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