using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BonusQuestionManager : MonoBehaviour
{
    [Header("JSON")]
    [SerializeField] private TextAsset wordsJson;

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


[SerializeField] private ProgressManager progressManager;
    private WordData wordData;

    private int currentQuestion = 0;
    private bool answered = false;

    private void Start()
    {
        LoadQuestions();

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => SelectAnswer(index));
        }

        ShowQuestion(currentQuestion);
    }

    void LoadQuestions()
    {
        wordData = JsonUtility.FromJson<WordData>(wordsJson.text);
        Debug.Log("Stages Loaded: " + wordData.stages.Count);
    }

    void ResetButtons()
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

    void ShowQuestion(int stageIndex)
    {
        ResetButtons();

        BonusQuestion q = wordData.stages[stageIndex].bonusQuestion;

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

        BonusQuestion q = wordData.stages[currentQuestion].bonusQuestion;

        // Because options is List<string>
        int correctIndex = q.options.IndexOf(q.correctAnswer);

        if (selectedIndex == correctIndex)
        {
            optionImages[selectedIndex].sprite = correctSprite;
            progressManager.AddCorrectAnswer();
        }
        else
        {
            optionImages[selectedIndex].sprite = wrongSprite;
            optionImages[correctIndex].sprite = correctSprite;
        }

        StartCoroutine(NextQuestion());
    }

    IEnumerator NextQuestion()
    {
        foreach (Button button in optionButtons)
        {
            button.interactable = false;
        }

        yield return new WaitForSeconds(1f);

        currentQuestion++;

        // Show only first 2 bonus questions
        if (currentQuestion >= wordData.stages.Count)
        {
            Debug.Log("Bonus Questions Finished");

            // TODO:
            // SceneManager.LoadScene("PuzzleScene");

            yield break;
        }

        ShowQuestion(currentQuestion);
    }
}