using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    private const string LevelsResourcePath = "Data/levels";
    private const string BonusQuestionsResourcePath = "Data/bonus_questions";

    public LevelsData Levels { get; private set; }
    public BonusQuestionsData BonusQuestions { get; private set; }
    public bool IsDataLoaded { get; private set; }
    public string LastLoadError { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        EnsureInstance();
    }

    public static DataManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        Instance = FindAnyObjectByType<DataManager>();

        if (Instance != null)
        {
            return Instance;
        }

        GameObject dataManagerObject = new GameObject("DataManager");
        Instance = dataManagerObject.AddComponent<DataManager>();
        Debug.Log("[DataManager] Auto-created runtime instance.", Instance);
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[DataManager] Duplicate instance found. Destroying the new instance.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[DataManager] Initialized.", this);

        LoadData();
    }

    private void LoadData()
    {
        IsDataLoaded = false;
        LastLoadError = null;
        Levels = null;
        BonusQuestions = null;

        TextAsset levelsFile = Resources.Load<TextAsset>(LevelsResourcePath);

        if (levelsFile == null)
        {
            LastLoadError = $"Could not find Resources/{LevelsResourcePath}.json";
            Debug.LogError($"[DataManager] {LastLoadError}", this);
            return;
        }

        Debug.Log($"[DataManager] Levels JSON loaded from Resources/{LevelsResourcePath}.json", this);

        Levels = JsonUtility.FromJson<LevelsData>(levelsFile.text);

        if (Levels == null)
        {
            LastLoadError = "Failed to parse levels JSON into LevelsData.";
            Debug.LogError($"[DataManager] {LastLoadError}", this);
            return;
        }

        if (Levels.levels == null)
        {
            LastLoadError = "Levels JSON parsed, but the 'levels' array is missing or invalid.";
            Debug.LogError($"[DataManager] {LastLoadError}", this);
            return;
        }

        Debug.Log($"[DataManager] Loaded {Levels.levels.Count} levels.", this);

        TextAsset bonusFile = Resources.Load<TextAsset>(BonusQuestionsResourcePath);

        if (bonusFile == null)
        {
            LastLoadError = $"Could not find Resources/{BonusQuestionsResourcePath}.json";
            Debug.LogError($"[DataManager] {LastLoadError}", this);
            return;
        }

        Debug.Log($"[DataManager] Bonus questions JSON loaded from Resources/{BonusQuestionsResourcePath}.json", this);

        BonusQuestions = JsonUtility.FromJson<BonusQuestionsData>(bonusFile.text);

        if (BonusQuestions == null)
        {
            LastLoadError = "Failed to parse bonus questions JSON into BonusQuestionsData.";
            Debug.LogError($"[DataManager] {LastLoadError}", this);
            return;
        }

        if (BonusQuestions.bonusRounds == null)
        {
            LastLoadError = "Bonus questions JSON parsed, but the 'bonusRounds' array is missing or invalid.";
            Debug.LogError($"[DataManager] {LastLoadError}", this);
            return;
        }

        Debug.Log($"[DataManager] Loaded {BonusQuestions.bonusRounds.Count} bonus rounds.", this);

        IsDataLoaded = true;
    }
}
