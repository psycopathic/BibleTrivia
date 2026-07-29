using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance;

    public int currentChapter = 0;

    public int currentPuzzle = 0;

    public const int puzzlesPerChapter = 3;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool IsLastPuzzle()
    {
        return currentPuzzle >= puzzlesPerChapter - 1;
    }

    public void NextPuzzle()
    {
        currentPuzzle++;

        if (currentPuzzle >= puzzlesPerChapter)
        {
            currentPuzzle = 0;
            currentChapter++;
        }
    }
}