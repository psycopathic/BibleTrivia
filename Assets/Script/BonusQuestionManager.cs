using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BonusQuestionManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text optionAText;
    [SerializeField] private TMP_Text optionBText;
    [SerializeField] private TMP_Text optionCText;
    [SerializeField] private TMP_Text optionDText;

    [Header("Buttons")]
    [SerializeField] private Button[] optionButtons;

    [Header("Button Images")]
    [SerializeField] private Image[] optionImages;

    [Header("Button Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite correctSprite;
    [SerializeField] private Sprite wrongSprite;

    [Header("Progress")]
    [SerializeField] private ProgressManager progressManager;

    private BonusRound currentBonusRound;
    private int currentQuestion = 0;
    private bool answered = false;

    private void Start()
    {
        LoadBonusRound();

        if (currentBonusRound == null)
        {
            Debug.LogError("No bonus round found.");
            return;
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => SelectAnswer(index));
        }

        ShowQuestion(currentQuestion);
    }

    private void LoadBonusRound()
    {
        DataManager dataManager = DataManager.EnsureInstance();

        if (dataManager == null)
        {
            Debug.LogError("[BonusQuestionManager] DataManager could not be created or found.");
            return;
        }

        if (!dataManager.IsDataLoaded)
        {
            string reason = string.IsNullOrEmpty(dataManager.LastLoadError)
                ? "DataManager did not finish loading game data."
                : dataManager.LastLoadError;

            Debug.LogError($"[BonusQuestionManager] Cannot load bonus round because DataManager failed to load data: {reason}");
            return;
        }

        BonusQuestionsData bonusData = dataManager.BonusQuestions;

        if (bonusData == null)
        {
            Debug.LogError("BonusQuestionsData is null.");
            return;
        }

        int completedLevel = PlayerPrefs.GetInt("CURRENT_LEVEL", 1) - 1;

        foreach (BonusRound round in bonusData.bonusRounds)
        {
            if (round.afterLevel == completedLevel)
            {
                currentBonusRound = round;
                Debug.Log($"Loaded Bonus Round after Level {completedLevel}");
                return;
            }
        }

        Debug.LogError($"No bonus round configured for level {completedLevel}");
    }

    private void ResetButtons()
    {
        answered = false;

        foreach (Image image in optionImages)
        {
            image.sprite = normalSprite;
        }

        foreach (Button button in optionButtons)
        {
            button.interactable = true;
        }
    }

    private void ShowQuestion(int questionIndex)
    {
        ResetButtons();

        BonusRoundQuestion q = currentBonusRound.questions[questionIndex];

        questionText.text = q.question;

        optionAText.text = q.options[0];
        optionBText.text = q.options[1];
        optionCText.text = q.options[2];
        optionDText.text = q.options[3];
    }

    public void SelectAnswer(int selectedIndex)
    {
        if (answered)
            return;

        answered = true;

        BonusRoundQuestion q = currentBonusRound.questions[currentQuestion];

        int correctIndex = q.options.IndexOf(q.correctAnswer);

        if (selectedIndex == correctIndex)
        {
            optionImages[selectedIndex].sprite = correctSprite;

            if (progressManager != null)
            {
                progressManager.AddCorrectAnswer();
            }
        }
        else
        {
            optionImages[selectedIndex].sprite = wrongSprite;
            optionImages[correctIndex].sprite = correctSprite;
        }

        StartCoroutine(NextQuestion());
    }

    private IEnumerator NextQuestion()
    {
        foreach (Button button in optionButtons)
        {
            button.interactable = false;
        }

        yield return new WaitForSeconds(1f);

        currentQuestion++;

        if (currentQuestion >= currentBonusRound.questions.Count)
        {
            Debug.Log("Bonus Round Complete");

            SceneManager.LoadScene("MainScene");
            yield break;
        }

        ShowQuestion(currentQuestion);
    }
}
