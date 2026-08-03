using UnityEngine;

public class HintManager : MonoBehaviour
{
    public void OnHintButtonClicked()
    {
        if (!CoinManager.Instance.SpendCoins(CoinConstants.HintCost))
        {
            Debug.Log("Not enough coins");
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RevealHint();
            return;
        }

        if (DailyPuzzleManager.Instance != null)
        {
            DailyPuzzleManager.Instance.RevealHint();
            return;
        }

        Debug.LogWarning("No active puzzle manager found for hint action.");
    }

    public void OnShuffleButtonClicked()
    {
        if (!CoinManager.Instance.SpendCoins(CoinConstants.ShuffleCost))
        {
            Debug.Log("Not enough coins");
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShuffleLetters();
            return;
        }

        if (DailyPuzzleManager.Instance != null)
        {
            DailyPuzzleManager.Instance.ShuffleLetters();
            return;
        }

        Debug.LogWarning("No active puzzle manager found for shuffle action.");

        
    }

}
