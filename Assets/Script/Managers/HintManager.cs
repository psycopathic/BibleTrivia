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

        GameManager.Instance.RevealHint();
    }

}