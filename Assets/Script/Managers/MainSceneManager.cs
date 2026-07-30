using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainSceneManager : MonoBehaviour
{
    [SerializeField] private TMP_Text levelText;

    private void Start()
    {
        int currentLevel = PlayerPrefs.GetInt("CURRENT_LEVEL", 1);
        levelText.text = $"LEVEL {currentLevel}";
    }

    public void PlayLevel()
    {
        GameSession.CurrentMode = GameMode.Normal;
        SceneManager.LoadScene("PuzzleScene");
    }

    public void PlayDailyChallenge()
    {
        GameSession.CurrentMode = GameMode.DailyChallenge;
        SceneManager.LoadScene("PuzzleScene");
    }
}